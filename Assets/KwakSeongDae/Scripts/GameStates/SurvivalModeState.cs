using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalModeState : GameState
{
    [Header("�����̹� ��� ����")]
    [SerializeField] int winBlockCount;

    private Dictionary<int,Action<int>> hpMiddleware;
    private Dictionary<int,Action<int>> blockCountMiddleware;
    private Dictionary<int,Coroutine> winRoutineDic;

    public override void Enter()
    {
        SceneLoad(SceneIndex.Game);
        base.Enter();

        // Dictionary �ʱ� ����
        hpMiddleware = new Dictionary<int,Action<int>>();
        blockCountMiddleware = new Dictionary<int,Action<int>>();
        winRoutineDic = new Dictionary<int, Coroutine>();
        var playerKeys = playerObjectDic.Keys;

        // �÷��̾� ����ŭ �̸� ��� �߰�
        foreach (var playerID in playerKeys)
        {
            winRoutineDic.Add(playerID, null);

            // �� �÷��̾� HP �� BlockCount�̺�Ʈ ���� ����
            if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
            {
                hpMiddleware.Add(playerID, (newHP) => PlayerHPHandle(newHP, playerID));
                controller.OnChangeHp += hpMiddleware[playerID];
            }
            if (playerObjectDic[playerID].TryGetComponent<BlockCountManager>(out var manager))
            {
                blockCountMiddleware.Add(playerID, (newBlockCount) => PlayerBlockCountHandle(newBlockCount, playerID));
                manager.OnChangeBlockCount += blockCountMiddleware[playerID];
            }
        }
    }

    public override void Exit()
    {
        var playerKeys = playerObjectDic.Keys;

        // �÷��̾� ����ŭ �̸� ��� �߰�
        foreach (var playerID in playerKeys)
        {
            if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller)
                && hpMiddleware.ContainsKey(playerID))
            {
                controller.OnChangeHp -= hpMiddleware[playerID];
            }
            if (playerObjectDic[playerID].TryGetComponent<BlockCountManager>(out var manager)
                && blockCountMiddleware.ContainsKey(playerID))
            {
                manager.OnChangeBlockCount -= blockCountMiddleware[playerID];
            }
        }

        hpMiddleware.Clear();
        blockCountMiddleware.Clear();

        // winRoutineDic�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        foreach (int i in winRoutineDic.Keys)
        {
            if (winRoutineDic[i] != null)
            {
                StopFinishRoutine(winRoutineDic[i]);
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
                controller.IsGoal = true;
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
                    winRoutineDic[playerID] = StartCoroutine(FinishRoutine(playerID));
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
                StopFinishRoutine(winRoutineDic[playerID]);
                winRoutineDic[(playerID)] = null;
            }
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

        print($"{playerID}�� ������Դϴ�.");

        manager.CurrentState = StateType.Stop;
    }

    private void PlayerStateChange(int playerID)
    {
        if (playerObjectDic.ContainsKey(playerID)
            && playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
        {
            controller.IsGoal = true;
        }

        // ������ winRoutineDic�� ��Ͽ��� �ش� �÷��̾� ����
        winRoutineDic.Remove(playerID);
        winRoutineDic.Remove(playerID);

        print($"{playerID}�� ���� ������ �� �����ϴ�.");
    }

    private void AllPlayerResult()
    {
        // TODO: ��� �÷��̾ ������ �� ���� ���·� ����
        print("��� �÷��̾��� �ൿ�� �����Ǿ����ϴ�.");
    }
}
