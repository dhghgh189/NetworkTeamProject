using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceModeState : GameState
{
    [Header("���̽� ��� ����")]
    [SerializeField] private BoxCollider2D boxDetector;
    
    private Dictionary<int, Coroutine> goalRoutineDic;
    private Dictionary<int, bool> isBlockCheckDic;

    private Coroutine mainCollisionRoutine;

    public override void Enter()
    {
        SceneLoad(SceneIndex.Game);
        base.Enter();
        // Dictionary �ʱ� ����
        goalRoutineDic = new Dictionary<int, Coroutine>();
        isBlockCheckDic = new Dictionary<int, bool>();
        // �÷��̾� ����ŭ �̸� ��� �߰�
        foreach (var playerID in playerObjectDic.Keys)
        {
            goalRoutineDic.Add(playerID, null);
            isBlockCheckDic.Add(playerID, false);
        }

        // �浹 ���� ��ƾ ����
        mainCollisionRoutine = StartCoroutine(CollisionCheckRoutine());
    }

    public override void Exit()
    {
        StopCoroutine(mainCollisionRoutine);
        // finishRoutine�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        foreach (int i in goalRoutineDic.Keys)
        {
            if (goalRoutineDic[i] != null)
            {
                StopFinishRoutine(goalRoutineDic[i]);
            }
        }
        goalRoutineDic.Clear();
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
                    // �׽�Ʈ�� 
                    //int playerID = block.Owner.ActorNumber;
                    int playerID = collision.GetComponent<TestBlocks>().PlayerID;

                    if (isBlockCheckDic.ContainsKey(playerID))
                        isBlockCheckDic[playerID] = true;
                }
            }

            // 3. ���� �浹�� ���� �ִ� �÷��̾�鸸 FInishRoutine ����
            // �浹�� ���� ���� �÷��̾���� ���� ����Ǵ� ��ƾ�� ����
            var goalRoutineKeys = goalRoutineDic.Keys.ToArray();
            foreach (var playerID in goalRoutineKeys)
            {
                // ��üũ�� �ش� �÷��̾ �����鼭 true�� ��� => ���� FInish������ ���� ����
                if (isBlockCheckDic.ContainsKey(playerID))
                {
                    if (isBlockCheckDic[playerID] == true)
                    {
                        print($"{playerID} �� ����");
                        if (goalRoutineDic[playerID] == null)
                            goalRoutineDic[playerID] = StartCoroutine(FinishRoutine(playerID));
                    }
                    else
                    {
                        print($"{playerID} �� ����");
                        if (goalRoutineDic[playerID] != null)
                        {
                            StopFinishRoutine(goalRoutineDic[playerID]);
                            goalRoutineDic[playerID] = null;
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
        // ��� �÷��̾� �۵� ���߰� ����
        AllPlayerResult();
        manager.CurrentState = StateType.Stop;
    }

    private void AllPlayerResult()
    {
        // TODO: ��� �÷��̾ ������ �� ���� ���·� ����
        print("��� �÷��̾��� �ൿ�� �����Ǿ����ϴ�.");

        print($"�� ������Դϴ�.");
    }
}
