using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleModeState : GameState
{
    [Header("���� ��� ����")]
    [SerializeField] private float finishTimer;
    [SerializeField] private BoxCollider2D boxDetector;
    private Dictionary<int, Coroutine> finishRoutineDic;
    private Dictionary<int, bool> isBlockCheckDic;
    private WaitForSeconds timer;

    private Coroutine mainCollisionRoutine;

    public override void Enter()
    {
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

        // Timer �ʱ� ����
        timer = new WaitForSeconds(finishTimer);

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
                StopCoroutine(finishRoutineDic[i]);
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
            Collider2D[] cols = Physics2D.OverlapBoxAll(detectorPos, detectorScale, 0);
            print("�浹 ���� ��");
            foreach (var collision in cols)
            {
                // TODO: ���� �ش��ϴ� �±׷� �ٲ��ֱ�

                // ���� �����ϴ� ���, �ش� �������� ���� ������ üũ
                // �浹�� ���� ������, �÷��̾��� �ڷ�ƾ�� ���� �Ǵ� ��, �ڷ�ƾ ����
                if (collision.CompareTag("Player")
                    && collision.GetComponent<Blocks>().IsEntered == false
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
                            StopCoroutine(finishRoutineDic[playerID]);
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

    private IEnumerator FinishRoutine(int playerID)
    {
        // ���� �ð��� ������
        yield return timer;

        // �� �̻� �÷��̾ ������ �� ���� ���·� ����

        // ������ finishRoutineDic�� ��Ͽ��� �ش� �÷��̾� ����
        finishRoutineDic.Remove(playerID);
        isBlockCheckDic.Remove(playerID);
        print($"{playerID}�� ���� ������ �� �����ϴ�.");

        // ��� �÷��̾ ����Ǿ����� üũ
        AllPlayerStateCheck();
    }

    private void AllPlayerStateCheck()
    {
        if (finishRoutineDic.Count < 1)
        {
            //TODO: �� �÷��̾ ���� ���� ������ �����ϴ� �ڵ� �ʿ�
            print($"��� �÷��̾��� �� ���� ���� �� ���� ����");
            manager.CurrentState = StateType.Stop;
        }
    }
}
