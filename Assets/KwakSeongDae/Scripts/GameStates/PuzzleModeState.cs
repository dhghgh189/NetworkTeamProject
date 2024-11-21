using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleModeState : GameState
{
    [Header("���� ��� ����")]
    [SerializeField] private float finishAcceptTime;
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
        foreach (var playerID in manager.PlayerDic.Keys)
        {
            finishRoutineDic.Add(playerID, null);
            isBlockCheckDic.Add(playerID, false);
        }
        // �׽�Ʈ ������ �ش� �ڵ� �߰�
        finishRoutineDic.Add(0, null);
        isBlockCheckDic.Add(0, false);

        // Timer �ʱ� ����
        timer = new WaitForSeconds(finishAcceptTime);

        // �浹 ���� ��ƾ ����
        mainCollisionRoutine = StartCoroutine(CollisionCheckRoutine());
    }

    public override void Exit()
    {
        base.Exit();
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
    }
    private IEnumerator CollisionCheckRoutine()
    {
        var detectorPos = (Vector2)boxDetector.transform.position + boxDetector.offset;
        var detectorScale = Vector2.Scale(boxDetector.transform.localScale, boxDetector.size);
        var delay = new WaitForSeconds(0.1f);
        while (true)
        {
            // ���� �� �浹 Check�� False�� �ʱ�ȭ
            var isBlockCheckKeys = isBlockCheckDic.Keys.ToArray();
            foreach (var key in isBlockCheckKeys)
            {
                isBlockCheckDic[key] = false;
            }

            // Physics2D�� �浹ü �˻�
            // isEntered�� �� ���� �����ؼ� ���� FInish ���� ���� ������Ʈ
            Collider2D[] cols = Physics2D.OverlapBoxAll(detectorPos, detectorScale, 0);
            print("�浹 ���� ��");
            foreach (var collision in cols)
            {
                // TODO: ���� �ش��ϴ� �±׷� �ٲ��ֱ�

                // ���� �����ϴ� ���, �ش� �������� ���� ������ üũ
                // �浹�� ���� ������, �÷��̾��� �ڷ�ƾ�� ���� �Ǵ� ��, �ڷ�ƾ ����
                if (collision.CompareTag("Player")
                    //&& collision.GetComponent<Block>().isEntered == true
                    && collision.TryGetComponent<PhotonView>(out var block))
                {
                    //int playerID = block.Owner.ActorNumber;
                    int playerID = 0;
                    print("�浹 ��");
                    if (isBlockCheckDic.ContainsKey(playerID))
                        isBlockCheckDic [playerID] = true;
                }
            }

            // ���� �浹�� ���� �ִ� �÷��̾�鸸 FInishRoutine ����
            // �浹�� ���� ���� �÷��̾���� ���� ����Ǵ� ��ƾ�� ����
            var finishRoutineKeys = finishRoutineDic.Keys.ToArray();
            foreach (var playerID in finishRoutineKeys)
            {
                // ��üũ�� �ش� �÷��̾ �����鼭 true�� ��� => ���� FInish������ ���� ����
                if (isBlockCheckDic.ContainsKey(playerID))
                {
                    if (isBlockCheckDic[playerID] == true)
                    {
                        finishRoutineDic[playerID] = StartCoroutine(FinishRoutine(playerID));
                        print($"{playerID} �� ����");
                    }
                    else
                    {
                        if (finishRoutineDic[playerID] != null)
                        {
                            // �ڷ�ƾ�� ���������� ������� �ʴ� ���� �߻�
                            print(finishRoutineDic[playerID]);
                            StopCoroutine(finishRoutineDic[playerID]);
                            print(finishRoutineDic[playerID]);
                            finishRoutineDic[playerID] = null;
                            print($"{playerID} �� ����");
                        }
                        else
                        {
                            print($"{playerID} �ڷ�ƾ ����");
                        }
                    }
                }
                else
                {
                    print(playerID);
                    print("������ �������� ���� �÷��̾ ����");
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
            print($"��� �÷��̾ ����ʿ� ���� ���� ����");
            manager.CurrentState = StateType.Stop;
        }
    }
}
