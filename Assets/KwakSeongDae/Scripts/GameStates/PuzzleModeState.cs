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

    private Coroutine finishRoutine;
    private Dictionary<int, bool> isBlockCheckDic;

    private Coroutine mainCollisionRoutine;

    protected override void OnEnable()
    {
        base.OnEnable();
        // Dictionary �ʱ� ����
        isBlockCheckDic = new Dictionary<int, bool>();
        // �÷��̾� ����ŭ �̸� ��� �߰�
        foreach (var playerID in playerObjectDic.Keys)
        {
            isBlockCheckDic.Add(playerID, false);
        }

        // ���常 �浹 ���� ��ƾ ����
        if (PhotonNetwork.IsMasterClient)
            mainCollisionRoutine = StartCoroutine(CollisionCheckRoutine());
    }

    public override void Exit()
    {
        if (PhotonNetwork.IsMasterClient) 
            StopCoroutine(mainCollisionRoutine);

        // finishRoutine�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        StopCoroutine(finishRoutine);

        isBlockCheckDic.Clear();

        // Exitȣ���� Enter�� ����
        base.Exit();
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
                // ���� �����ϴ� ���, �ش� �������� ���� ������ üũ
                // �浹�� ���� ������, �÷��̾��� �ڷ�ƾ�� ���� �Ǵ� ��, �ڷ�ƾ ����
                if (collision.GetComponent<Blocks>().IsEntered == false
                    && collision.TryGetComponent<PhotonView>(out var block))
                {
                    // �׽�Ʈ ��
                    //int playerID = block.Owner.ActorNumber;
                    int playerID = collision.GetComponent<TestBlocks>().PlayerID;

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
                    photonView.RPC("FinishRoutineMiddleware", RpcTarget.AllViaServer, playerID, true);
                }
                else
                {
                    print($"{playerID} �� ����");
                    photonView.RPC("FinishRoutineMiddleware", RpcTarget.AllViaServer, playerID, false);
                }
            }
            yield return delay;
        }
    }

    [PunRPC]
    private void FinishRoutineMiddleware(int playerID, bool isPlay)
    {
        // �ش�� �÷��̾���� ��ƾ ����
        if (PhotonNetwork.LocalPlayer.ActorNumber != playerID) return;

        if (isPlay)
        {
            // ������ �������̸� ����
            if (finishRoutine == null)
                finishRoutine = StartCoroutine(FinishRoutine(playerID));
        }
        else
        {
            if (finishRoutine != null)
                StopCoroutine(finishRoutine);

            finishRoutine = null;
        }
    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));
        // ���� �ð��� ������ �ش� �÷��̾�� �� �̻� ���� �Ұ�

        // �ش� PlayerStateChange�� ���������� ����
        PlayerStateChange(playerID);

        // ����, ������ ��� �÷��̾� ���� üũ ��, ����
        photonView.RPC("AllPlayerResult",RpcTarget.MasterClient,playerID);
    }

    private void PlayerStateChange(int playerID)
    {
        if (playerObjectDic.ContainsKey(playerID)
            && playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
        {
            controller.IsGoal = true;
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

        if (isBlockCheckDic.Count < 1)
        {
            List<Tuple<int,int>> result = new List<Tuple<int,int>>();
            foreach (var playerKey in playerObjectDic.Keys)
            {
                //if (playerObjectDic[playerKey].TryGetComponent<BlockCountManager>(out var manager))
                //{
                //    result.Add(new Tuple<int, int>(playerKey, manager.BlockCount));
                //}

                //�׽�Ʈ �ڵ�
                result.Add(new Tuple<int, int>(playerKey, playerKey));
            }
            //������������ �� ���� ����
            result.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            result.ForEach((x) => {
                playerUI?.SetResultEntry(x.Item1.ToString(), x.Item2);
                playerUI?.SetResult();
            });

            print($"��� �÷��̾��� �� ���� ���� �� ���� ����");
            print($"{result[0].Item1}�� ���� ����� ������Դϴ�!!!");

            Time.timeScale = 0f;
        }
    }
}
