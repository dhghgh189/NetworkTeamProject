using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalModeState : GameState
{
    [Header("�����̹� ��� ����")]
    [SerializeField] int winBlockCount;

    private Dictionary<int,Action<int>> hpWrap;
    private Dictionary<int,Action<int>> blockCountWrap;
    private Dictionary<int,Coroutine> winRoutineDic;

    //public override void Enter()
    //{
    //    SceneLoad(SceneIndex.Game);
    //    base.Enter();

    //    // Dictionary �ʱ� ����
    //    hpWrap = new Dictionary<int,Action<int>>();
    //    blockCountWrap = new Dictionary<int,Action<int>>();
    //    winRoutineDic = new Dictionary<int, Coroutine>();
    //    var playerKeys = playerObjectDic.Keys;

    //    // �÷��̾� ����ŭ �̸� ��� �߰�
    //    foreach (var playerID in playerKeys)
    //    {
    //        winRoutineDic.Add(playerID, null);

    //        // �� �÷��̾� HP �� BlockCount�̺�Ʈ ���� ����
    //        if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
    //        {
    //            hpWrap.Add(playerID, (newHP) => PlayerHPHandle(newHP, playerID));
    //            controller.OnChangeHp += hpWrap[playerID];
    //        }
    //        if (playerObjectDic[playerID].TryGetComponent<BlockCountManager>(out var manager))
    //        {
    //            blockCountWrap.Add(playerID, (newBlockCount) => PlayerBlockCountHandle(newBlockCount, playerID));
    //            manager.OnChangeBlockCount += blockCountWrap[playerID];
    //        }
    //    }
    //}

    //public override void Exit()
    //{
    //    var playerKeys = playerObjectDic.Keys;

    //    // �÷��̾� ����ŭ �̸� ��� �߰�
    //    foreach (var playerID in playerKeys)
    //    {
    //        if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller)
    //            && hpWrap.ContainsKey(playerID))
    //        {
    //            controller.OnChangeHp -= hpWrap[playerID];
    //        }
    //        if (playerObjectDic[playerID].TryGetComponent<BlockCountManager>(out var manager)
    //            && blockCountWrap.ContainsKey(playerID))
    //        {
    //            manager.OnChangeBlockCount -= blockCountWrap[playerID];
    //        }
    //    }

    //    hpWrap.Clear();
    //    blockCountWrap.Clear();

    //    // winRoutineDic�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
    //    foreach (int i in winRoutineDic.Keys)
    //    {
    //        if (winRoutineDic[i] != null)
    //        {
    //            StopFinishRoutine(winRoutineDic[i]);
    //        }
    //    }
    //    winRoutineDic.Clear();

    //    base.Exit();
    //}

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
        if (winRoutineDic.Count < 1)
        {
             List<Tuple<int, int>> result = new List<Tuple<int, int>>();
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
