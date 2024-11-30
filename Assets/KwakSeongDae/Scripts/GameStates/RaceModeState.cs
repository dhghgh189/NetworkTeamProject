using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceModeState : GameState
{
    [Header("���̽� ��� ����")]
    [SerializeField] private BoxCollider2D boxDetector;
    
    private Coroutine goalRoutine;
    private Dictionary<int, bool> isBlockCheckDic;

    private Coroutine mainCollisionRoutine;

    protected override void Init()
    {
        base.Init();
        // Dictionary �ʱ� ����
        isBlockCheckDic = new Dictionary<int, bool>();
        // �÷��̾� ����ŭ �̸� ��� �߰�
        foreach (var player in PhotonNetwork.PlayerList)
        {
            isBlockCheckDic.Add(player.ActorNumber, false);
        }

        // ���常 �浹 ���� ��ƾ ����
        if (PhotonNetwork.IsMasterClient)
            mainCollisionRoutine = StartCoroutine(CollisionCheckRoutine());
    }

    private void OnDisable()
    {
        if (PhotonNetwork.IsMasterClient
            && mainCollisionRoutine != null)
            StopCoroutine(mainCollisionRoutine);

        // goalRoutine�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        if (goalRoutine != null)
            StopCoroutine(goalRoutine);

        isBlockCheckDic.Clear();
        ReturnScene();
        Time.timeScale = 1f;
    }

    private IEnumerator CollisionCheckRoutine()
    {
        var detectorPos = (Vector2)boxDetector.transform.position + boxDetector.offset;
        var detectorScale = Vector2.Scale(boxDetector.transform.localScale, boxDetector.size);
        var delay = new WaitForSeconds(0.1f);

        while (true)
        {
            // 1. ���� �� �浹 Check�� False�� �ʱ�ȭ
            var playerIDs = isBlockCheckDic.Keys.ToArray();
            foreach (var playerID in playerIDs)
            {
                isBlockCheckDic[playerID] = false;
            }

            // 2. Physics2D�� �浹ü �˻�
            // isEntered�� �� ���� �����ؼ� ���� FInish ���� ���� ������Ʈ
            Collider2D[] cols = Physics2D.OverlapBoxAll(detectorPos, detectorScale, 0, LayerMask.GetMask("Blocks"));
            print("���� �浹 ���� ��");
            foreach (var collision in cols)
            {
                var blockTrans = collision.transform.parent;
                var block = blockTrans.GetComponent<Blocks>();
                // ���� �����ϴ� ���, �ش� �������� ���� ������ üũ
                // �浹�� ���� ������, �÷��̾��� �ڷ�ƾ�� ���� �Ǵ� ��, �ڷ�ƾ ����
                if (block.IsControllable == false
                    && blockTrans.TryGetComponent<PhotonView>(out var view))
                {
                    int playerID = view.Owner.ActorNumber;

                    if (isBlockCheckDic.ContainsKey(playerID))
                        isBlockCheckDic[playerID] = true;
                }
            }

            // 3. ���� �浹�� ���� �ִ� �÷��̾�鸸 FInishRoutine ����
            // �浹�� ���� ���� �÷��̾���� ���� ����Ǵ� ��ƾ�� ����
            playerIDs = isBlockCheckDic.Keys.ToArray();
            foreach (var playerID in playerIDs)
            {
                // ��üũ�� �ش� �÷��̾ �����鼭 true�� ��� => ���� FInish������ ���� ����
                if (isBlockCheckDic[playerID] == true)
                {
                    print($"{playerID} �� ����");
                    photonView.RPC("FinishRoutineWrap", RpcTarget.AllViaServer, playerID, true);
                }
                else
                {
                    print($"{playerID} �� ����");
                    photonView.RPC("FinishRoutineWrap", RpcTarget.AllViaServer, playerID, false);
                }
            }
            yield return delay;
        }
    }

    [PunRPC]
    private void FinishRoutineWrap(int playerID, bool isPlay)
    {
        // �ش� �÷��̾���� ��ƾ ����
        if (PhotonNetwork.LocalPlayer.ActorNumber != playerID) return;

        if (isPlay)
        {
            // ������ �������̸� ����
            if (goalRoutine == null)
                goalRoutine = StartCoroutine(FinishRoutine(playerID));
        }
        else
        {
            if (goalRoutine != null)
                StopFinishRoutine(goalRoutine);

            goalRoutine = null;
        }
    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));
        // ���� �ð��� ������ ��� �÷��̾� ���� üũ ��, ������� ����
        photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // ������ ���� ���� ���� ó��
        if (PhotonNetwork.IsMasterClient == false) return;

        if (playerObjectDic[otherPlayer.ActorNumber].TryGetComponent<PlayerController>(out var controller))
        {
            controller.ReachGoal();
        }

        playerObjectDic.Remove(otherPlayer.ActorNumber);
        towerObjectDic.Remove(otherPlayer.ActorNumber);
    }

    [PunRPC]
    private void AllPlayerStateCheck()
    {
        foreach (var playerID in playerObjectDic.Keys)
        {
            if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controlller))
            {
                controlller.ReachGoal();
            }
            // ������ finishRoutineDic�� ��Ͽ��� �ش� �÷��̾� ����
            isBlockCheckDic.Remove(playerID);
            print($"{playerID}�� ���� ������ �� �����ϴ�.");
        }
        print("��� �÷��̾��� �ൿ�� �����Ǿ����ϴ�.");

        List<Tuple<int, float>> result = new List<Tuple<int, float>>();
        foreach (var playerID in towerObjectDic.Keys)
        {
            //TODO: �� �÷��̾��� ���� ���� ���� �����ϴ� �ڵ� �ʿ�
            if (towerObjectDic[playerID].TryGetComponent<BlockMaxHeightManager>(out var manager))
            {
                result.Add(new Tuple<int, float>(playerID, manager.highestPoint));
            }
        }

        //������������ �� ���� ����
        result.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        //�� Ŭ���̾�Ʈ����, ������ UI�� �ش� ���� �ݿ��ǵ��� ����
        var players = new int[result.Count];
        var blockHeights = new float[result.Count];
        for (int i = 0; i < result.Count; i++)
        {
            players[i] = result[i].Item1;
            blockHeights[i] = result[i].Item2;
        }

        // UI ������Ʈ �۾� �� ���� ��������� ��� Ŭ���̾�Ʈ ����
        photonView.RPC("UpdateUI", RpcTarget.All, players, blockHeights);
    }

    [PunRPC]
    private void UpdateUI(int[] playerIDs, float[] blockHeights)
    {
        for (int i = 0; i < playerIDs.Length; i++)
        {
            playerUI?.AddResultEntry(playerIDs[i], blockHeights[i]);
        }
        playerUI?.SetResult();

        print($"��� �÷��̾��� �� ���� ���� �� ���� ����");
        print($"{playerIDs[0]}�� ���̽� ����� ������Դϴ�!!!");

        Time.timeScale = 0f;
    }
}
