using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalModeState : GameState
{
    [Header("�����̹� ��� ����")]
    [SerializeField] float winTimer;
    [SerializeField] int winBlockCount;

    private Action<int> hpMiddleware;
    private Action<int> bulletCountMiddleware;
    private Dictionary<int,Coroutine> winRoutineDic;
    private WaitForSeconds timer;

    public override void Enter()
    {
        base.Enter();

        // Dictionary �ʱ� ����
        winRoutineDic = new Dictionary<int, Coroutine>();
        var playerKeys = playerObjectDic.Keys;

        // �÷��̾� ����ŭ �̸� ��� �߰�
        foreach (var playerID in playerKeys)
        {
            winRoutineDic.Add(playerID, null);

            if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
            {
                // TODO: ��Ʈ�ѷ� ��, ���� �̺�Ʈ ������ ���� ����
                hpMiddleware = (newHP) => PlayerHPHandle(newHP,playerID);
                bulletCountMiddleware = (newBlockCount) => PlayerBlockCountHandle(newBlockCount, playerID);
                // controller.OnChangeHp += hpMiddleware;
                // controller.OnChangeBlockCount += bulletCountMiddleware;
                print($"{playerID}�� ���� HP �� BlockCount�̺�Ʈ ���� ����");
            }
        }

        // Timer �ʱ� ����
        timer = new WaitForSeconds(winTimer);
    }

    public override void Exit()
    {
        // �̺�Ʈ ���� ����
        // controller.OnChangeHp -= hpMiddleware;
        // controller.OnChangeBlockCount -= bulletCountMiddleware;

        // winRoutineDic�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        foreach (int i in winRoutineDic.Keys)
        {
            if (winRoutineDic[i] != null)
            {
                StopCoroutine(winRoutineDic[i]);
            }
        }
        winRoutineDic.Clear();

        base.Exit();
    }

    public void PlayerHPHandle(int newHP, int playerID)
    {
        if(newHP < 1)
        {
            if (playerObjectDic.ContainsKey(playerID)
                && playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
            {
                // TODO: �÷��̾ ���� ��Ȳ�� ��, �ش� �÷��̾�� ���� ���ϵ��� ����
                // �÷��̾� ��Ʈ�ѷ� ��, ���� ���� ������Ʈ �� �ڵ� �߰�
                print($"{playerID}���� ���� ����� ��� �����Ǿ� ���ӿ����Ǿ����ϴ�.");
            }
        }
    }


    public void PlayerBlockCountHandle(int newBlockCount, int playerID) 
    { 
        // ��ǥ �� ������ŭ �׿��� ��, �¸� ��ƾ ����
        if (newBlockCount >= winBlockCount)
        {
            if (winRoutineDic.ContainsKey(playerID))
            {
                if (winRoutineDic[playerID] == null)
                    winRoutineDic[playerID] = StartCoroutine(WinRoutine(playerID));
            }
            else
            {
                print("���������� �ʱ�ȭ���� ���� �÷��̾ ����");
            }
        }
        else
        {
            //����ǰ� �ִ� �¸� ��ƾ�� �����ϸ�, �ش� ��ƾ ����
            if (winRoutineDic[playerID] != null)
            {
                StopCoroutine(winRoutineDic[playerID]);
                winRoutineDic[(playerID)] = null;
            }
        }

    }

    private IEnumerator WinRoutine(int playerID)
    {
        // ���� �ð��� ������
        yield return timer;

        AllPlayerStop();

        print($"{playerID}�� ������Դϴ�.");

        manager.CurrentState = StateType.Stop;
    }

    private void AllPlayerStop()
    {
        // TODO: ��� �÷��̾ ������ �� ���� ���·� ����
        print("��� �÷��̾��� �ൿ�� �����Ǿ����ϴ�.");
    }
}
