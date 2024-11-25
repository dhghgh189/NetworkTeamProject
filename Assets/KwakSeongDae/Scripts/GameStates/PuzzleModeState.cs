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

    private Dictionary<int, Coroutine> finishRoutineDic;
    private Dictionary<int, bool> isBlockCheckDic;

    private Coroutine mainCollisionRoutine;

    public override void Enter()
    {
        SceneLoad(SceneIndex.Game);
        base.Enter();
        // Dictionary �ʱ� ����
        finishRoutineDic = new Dictionary<int, Coroutine>();
        isBlockCheckDic = new Dictionary<int, bool>();
        // �÷��̾� ����ŭ �̸� ��� �߰�
        foreach (var playerID in playerObjectDic.Keys)
        {
            finishRoutineDic.Add(playerID, null);
            isBlockCheckDic.Add(playerID, false);
        }

        // �浹 ���� ��ƾ ����
        mainCollisionRoutine = StartCoroutine(CollisionCheckRoutine());
    }

    public override void Exit()
    {
        StopCoroutine(mainCollisionRoutine);
        // finishRoutine�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        foreach (int i in finishRoutineDic.Keys)
        {
            if (finishRoutineDic[i] != null)
            {
                StopFinishRoutine(finishRoutineDic[i]);
            }
        }
        finishRoutineDic.Clear();
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
            var isBlockCheckKeys = isBlockCheckDic.Keys.ToArray();
            foreach (var playerID in isBlockCheckKeys)
            {
                isBlockCheckDic[playerID] = false;
            }

            // 2. Physics2D�� �浹ü �˻�
            // isEntered�� �� ���� �����ؼ� ���� FInish ���� ���� ������Ʈ
            Collider2D[] cols = Physics2D.OverlapBoxAll(detectorPos, detectorScale, 0, LayerMask.GetMask("Blocks"));
            print("�浹 ���� ��");
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

            // 3. ���� �浹�� ���� �ִ� �÷��̾�鸸 FInishRoutine ����
            // �浹�� ���� ���� �÷��̾���� ���� ����Ǵ� ��ƾ�� ����
            var finishRoutineKeys = finishRoutineDic.Keys.ToArray();
            foreach (var playerID in finishRoutineKeys)
            {
                // ��üũ�� �ش� �÷��̾ �����鼭 true�� ��� => ���� FInish������ ���� ����
                if (isBlockCheckDic.ContainsKey(playerID))
                {
                    if (isBlockCheckDic[playerID] == true)
                    {
                        print($"{playerID} �� ����");
                        if (finishRoutineDic[playerID] == null)
                            finishRoutineDic[playerID] = StartCoroutine(FinishRoutine(playerID));
                    }
                    else
                    {
                        print($"{playerID} �� ����");
                        if (finishRoutineDic[playerID] != null)
                        {
                            StopFinishRoutine(finishRoutineDic[playerID]);
                            finishRoutineDic[playerID] = null;
                        }
                    }
                }
                else
                {
                    print($"{playerID}�� ���������� ����");
                }
            }
            yield return delay;
        }
    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));
        // ���� �ð��� ������
        // �ش� �÷��̾�� �� �̻� ���� �Ұ�
        PlayerStateChange(playerID);

        // ��� �÷��̾� ���� üũ ��, ���� ����
        AllPlayerResult();
    }

    private void PlayerStateChange(int playerID)
    {
        if (playerObjectDic.ContainsKey(playerID)
            && playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
        {
            controller.IsGoal = true;
        }

        // ������ finishRoutineDic�� ��Ͽ��� �ش� �÷��̾� ����
        finishRoutineDic.Remove(playerID);
        isBlockCheckDic.Remove(playerID);

        print($"{playerID}�� ���� ������ �� �����ϴ�.");
    }

    private void AllPlayerResult()
    {
        if (finishRoutineDic.Count < 1)
        {
            //TODO: �� �÷��̾ ���� ���� ������ �����ϴ� �ڵ� �ʿ�
            List<Tuple<int,int>> result = new List<Tuple<int,int>>();
            foreach (var playerID in playerObjectDic.Keys)
            {
                //if (playerObjectDic[playerID].TryGetComponent<BlockCountManager>(out var manager))
                //{
                //    result.Add(new Tuple<int, int>(playerID, manager.BlockCount));
                //}

                //�׽�Ʈ �ڵ�
                result.Add(new Tuple<int, int>(playerID, playerID));
            }
            //������������ �� ���� ����
            result.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            result.ForEach(x => print($"{x.Item1}�� ������: {x.Item2}"));

            print($"��� �÷��̾��� �� ���� ���� �� ���� ����");
            print($"{result[0].Item1}�� ���� ����� ������Դϴ�!!!");

            manager.CurrentState = StateType.Stop;
        }
    }
}
