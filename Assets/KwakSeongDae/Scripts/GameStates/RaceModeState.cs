using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceModeState : GameState
{
    [SerializeField] private float goalAcceptTime;
    private Dictionary<int, Coroutine> goalRoutineDic;
    private Dictionary<int, int> collisionBlockCountDic;
    private WaitForSeconds timer;

    public override void Enter()
    {
        base.Enter();
        // Dictionary �ʱ� ����
        goalRoutineDic = new Dictionary<int, Coroutine>();
        collisionBlockCountDic = new Dictionary<int, int>();
        // Timer �ʱ� ����
        timer = new WaitForSeconds(goalAcceptTime);
    }

    public override void Exit()
    {
        base.Exit();
        // finishRoutine�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        foreach (int i in goalRoutineDic.Keys)
        {
            if (goalRoutineDic[i] != null)
            {
                StopCoroutine(goalRoutineDic[i]);
            }
        }
        goalRoutineDic.Clear();
        collisionBlockCountDic.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // TODO: ���� �ش�Ǵ� �±׷� ��ü,
        // ���� �����ϴ� ���, ���� �����ͳ��� �浹 �����Ǹ� BlockCount ����
        // �浹�� ���� ������, �÷��̾��� �ڷ�ƾ�� ���� �Ǵ� ��, �ڷ�ƾ ����
        if (collision.CompareTag("Block")
            && collision.TryGetComponent<PhotonView>(out var block))
        {
            int playerID = block.Owner.ActorNumber;

            if (collisionBlockCountDic.ContainsKey(playerID))
                collisionBlockCountDic[playerID]++;
            else
                collisionBlockCountDic.Add(playerID, 1);

            if (goalRoutineDic.ContainsKey(playerID) == false)
                goalRoutineDic.Add(playerID, StartCoroutine(FinishRoutine(playerID)));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // TODO: ���� �����ϴ� ���, �ش� �±׸� ���� ������Ʈ���� �浹 ���� ó��
        if (collision.CompareTag("Block") && collision.TryGetComponent<PhotonView>(out var block))
        {
            int playerID = block.Owner.ActorNumber;
            if (collisionBlockCountDic.ContainsKey(playerID))
            {
                collisionBlockCountDic[playerID]--;

                if (collisionBlockCountDic[playerID] < 1
                    && goalRoutineDic.ContainsKey(playerID))
                {
                    StopCoroutine(goalRoutineDic[playerID]);
                    goalRoutineDic.Remove(playerID);
                }
            }
            // blockCount�� ������� �ʴ� ���� ���������� ����� �Ǵ��ϰ� finishRoutine�� ����
            // ������ ���� ����: ���� �߰����� �ʰ�, ���ڱ� ���� Exit�ǰų�, �� Enter�� ���������� ó������ ����
            else
            {
                print("���������� ��");
                if (goalRoutineDic.ContainsKey(playerID))
                {
                    StopCoroutine(goalRoutineDic[playerID]);
                    goalRoutineDic.Remove(playerID);
                }
            }
        }
    }

    private IEnumerator FinishRoutine(int playerID)
    {
        // ���� �ð��� ������
        yield return timer;

        // TODO: ��� �÷��̾ ������ �� ���� ���·� ���� �� �ش� ��� ����
        print($"{playerID}�� ������Դϴ�.");
        
        manager.CurrentState = StateType.Stop;
    }
}
