using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PuzzleModeState : GameState
{
    [Header("���� ��� ����")]
    [SerializeField] private BoxCollider2D boxDetector;
    [SerializeField] float towerHeightStep;

    private Coroutine finishRoutine;
    private Dictionary<int, bool> isBlockCheckDic;
    private Coroutine mainCollisionRoutine;

    private Action<int> hpAction;
    private GameObject selfPlayer;
    private Coroutine panaltyRoutine;

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

        var playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        selfPlayer = playerObjectDic[playerID];
        hpAction = (newHP) => PlayerHPHandle(newHP, playerID);
        selfPlayer.GetComponent<PlayerController>().OnChangeHp += hpAction;

        // ���常 �浹 ���� ��ƾ ����
        if (PhotonNetwork.IsMasterClient)
            mainCollisionRoutine = StartCoroutine(CollisionCheckRoutine());
    }

    private void OnDisable()
    {
        // ���� �۾� ������
        if (PhotonNetwork.IsMasterClient
            && mainCollisionRoutine != null)
            StopCoroutine(mainCollisionRoutine);

        // finishRoutine�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        if (finishRoutine != null)
            StopCoroutine(finishRoutine);

        isBlockCheckDic?.Clear();

        selfPlayer.GetComponent<PlayerController>().OnChangeHp -= hpAction;

        Time.timeScale = 1f;
    }

    private void PlayerHPHandle(int newHP, int playerID)
    {
        print("ü�� ��ȭ");
        // �ڽ� �̺�Ʈ�� ��쿡�� ȣ��
        if (PhotonNetwork.LocalPlayer.ActorNumber != playerID) return;

        // ���� ���ÿ� ������ ��, �ߺ� ȣ�� ����
        if (panaltyRoutine == null)
            panaltyRoutine = StartCoroutine(PanaltyRoutine(playerID));
    }

    private IEnumerator PanaltyRoutine(int playerID)
    {
        // �� Ÿ���� ���̸� towerHegithStep��ŭ ���
        // �ش� Ÿ���� photonView transform�̹Ƿ� �ڵ����� ��ġ ����ȭ
        towerObjectDic[playerID].transform.Translate(0, towerHeightStep, 0);

        // ���� ���ÿ� �������� ���, �ߺ� ���� ����
        yield return new WaitForSeconds(1f);
        panaltyRoutine = null;
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
                        isBlockCheckDic [playerID] = true;
                }
            }

            // 3. ���� �浹�� ���� �ִ� �÷��̾�� FInishRoutine�� RPC�� �����ϵ��� �����
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
            if (finishRoutine == null)
                finishRoutine = StartCoroutine(FinishRoutine(playerID));

            //// �׽�Ʈ: Ÿ�� �г�Ƽ ���
            //PlayerHPHandle(0,playerID);
        }
        else
        {
            if (finishRoutine != null)
                StopFinishRoutine(finishRoutine);

            finishRoutine = null;
        }
    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));
        // ���� �ð��� ������ �ش� �÷��̾�� �� �̻� ���� �Ұ�

        // �ش� PlayerStateChange�� ���������� ����
        PlayerStateChange(playerID);

        // ����, ��� �÷��̾�� ���� üũ ��, ������� ����
        photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient,playerID);
    }

    private void PlayerStateChange(int playerID)
    {
        if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
        {
            controller.ReachGoal();
        }

        print($"{playerID}�� ���� ������ �� �����ϴ�.");
    }

    [PunRPC]
    private void AllPlayerStateCheck(int playerID = -1)
    {
        // ��ųʸ����� �ʱ�ȭ�� �÷��̾ �ִ� ���, ������ �ʱ�ȭ ����
        if (playerID > -1)
        {
            // ������ Dic�� ��Ͽ��� �ش� �÷��̾� ����
            isBlockCheckDic.Remove(playerID);
        }

        // ���� ���: ��� �÷��̾ ���� ��쿡�� ���� ���� 
        if (isBlockCheckDic.Count < 1)
        {
            List<Tuple<int,int>> result = new List<Tuple<int,int>>();
            foreach (var playerKey in playerObjectDic.Keys)
            {
                if (playerObjectDic[playerKey].TryGetComponent<PlayerController>(out var controller))
                {
                    print(controller.BlockCount);
                    result.Add(new Tuple<int, int>(playerKey, controller.BlockCount));
                }
            }

            //������������ �� ���� ����
            result.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            //�� Ŭ���̾�Ʈ����, ������ UI�� �ش� ���� �ݿ��ǵ��� ����
            var players = new int[result.Count];
            var blockCounts = new int[result.Count];
            for (int i = 0; i < result.Count; i++)
            {
                players[i] = result[i].Item1;
                blockCounts[i] = result[i].Item2;
            }

            // UI ������Ʈ �۾� �� ���� ��������� ��� Ŭ���̾�Ʈ ����
            photonView.RPC("UpdateUI", RpcTarget.All, players, blockCounts);
        }
    }

    [PunRPC]
    private void UpdateUI(int[] playerIDs,int[] blockCounts)
    {
        for (int i = 0; i < playerIDs.Length; i++)
        {
            playerUI?.AddResultEntry(playerIDs[i], blockCounts[i]);
        }
        playerUI?.SetResult();

        print($"��� �÷��̾��� �� ���� ���� �� ���� ����");
        print($"{playerIDs[0]}�� ���� ����� ������Դϴ�!!!");

        Time.timeScale = 0f;
    }
}
