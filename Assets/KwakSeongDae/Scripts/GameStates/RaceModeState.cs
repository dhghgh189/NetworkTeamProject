using Photon.Pun;
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

        // goalRoutine�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        StopCoroutine(goalRoutine);

        isBlockCheckDic.Clear();

        // Exitȣ���� Enter�� ����
        base.Exit();
    }
    private IEnumerator CollisionCheckRoutine()
    {
        var detectorPos = (Vector2)boxDetector.transform.position + boxDetector.offset;
        var detectorScale = Vector2.Scale(boxDetector.transform.localScale, boxDetector.size);
        var delay = new WaitForSeconds(0.1f);
        // �ڷ�ƾ ���� ��� ���� ���� ���� => �αװ� ��������
        yield return null;

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

                // TODO: ���� ������츦 üũ�ϴ� ���� �ƴ�, ��Ʈ�� ���θ� üũ�ؾ���
                if (block.IsEntered == false
                    //&& block.IsControllable
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

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));
        // ���� �ð��� ������
        // ��� �÷��̾� �۵� ���߰� ����
        AllPlayerStateChange();
        AllPlayerResult();
    }

    private void AllPlayerStateChange()
    {
        foreach (var playerID in playerObjectDic.Keys)
        {
            if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controlller))
            {
                controlller.IsGoal = true;
            }
            // ������ finishRoutineDic�� ��Ͽ��� �ش� �÷��̾� ����
            goalRoutineDic.Remove(playerID);
            isBlockCheckDic.Remove(playerID);
            print($"{playerID}�� ���� ������ �� �����ϴ�.");
        }
        print("��� �÷��̾��� �ൿ�� �����Ǿ����ϴ�.");
    }

    private void AllPlayerResult()
    {
        if (goalRoutineDic.Count < 1)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();
            foreach (var playerID in playerObjectDic.Keys)
            {
                //TODO: �� �÷��̾��� ���� ���� ���� �����ϴ� �ڵ� �ʿ�

                //�׽�Ʈ �ڵ�
                result.Add(new Tuple<int, int>(playerID, playerID));
            }
            //������������ �� ���� ����
            result.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            //result.ForEach((x) => {
            //    playerUI?.SetResultEntry(x.Item1.ToString(), x.Item2);
            //    playerUI?.SetResult();
            //});

            print($"��� �÷��̾��� �� ���� ���� �� ���� ����");
            print($"{result[0].Item1}�� ���� ����� ������Դϴ�!!!");

            Time.timeScale = 0f;
        }
    }
}
