using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum StateType
{
    Stop,Puzzle,Race,Survival,Size
}

public class CoreManager : Singleton<CoreManager>
{
    [Header("���� ���� ����")]
    [SerializeField] GameState[] states;
    [SerializeField] private StateType currentState;
    public StateType CurrentState
    {
        get => currentState;
        set
        {
            currentState = value;
            ChangeState(value);
        }
    }

    private GameState state;
    private Dictionary<StateType, GameState> stateDic;

    [Header("���� �÷��̾� ����")]
    public Dictionary<int, Player> PlayerDic;

    protected override void Init()
    {
        stateDic = new Dictionary<StateType, GameState>();
        PlayerDic = new Dictionary<int, Player>();
        foreach (var s in states)
        {
            stateDic.Add(s.StateType, s);
        }
        print("INIT");
        CurrentState = StateType.Stop;
    }

    private void Update()
    {
        state?.OnUpdate();
    }

    private void ChangeState(StateType changeType)
    {
        if (stateDic != null 
            && stateDic.ContainsKey(changeType))
        {
            state?.Exit();
            state?.gameObject.SetActive(false);

            state = stateDic[changeType];

            state.gameObject.SetActive(true);
            state?.Enter();
        }
    }

    /// <summary>
    /// �����⿡�� State�� ���Ƿ� �ٲ��� ��, ����ǵ��� ����
    /// </summary>
    private void OnValidate()
    {
        ChangeState(currentState);
    }

    /// <summary>
    /// ���� �÷��̾ �÷��̾�� �߰� ����
    /// </summary>
    public void SetPlayer(Player player)
    {
        PlayerDic.Add(player.ActorNumber, player);
    }
    public void SetPlayer(Player[] players)
    {
        foreach (var player in players)
        {
            PlayerDic.Add(player.ActorNumber, player);
        }
    }

    /// <summary>
    /// ����, �� �÷��̾ ���� �ʱ�ȭ�� ���� �ʿ��� ���, PlayerDic�� �̿��� ���� �ʱ�ȭ ����
    /// </summary>
    public void ResetPlayer()
    {
        PlayerDic.Clear();
        PlayerDic = new Dictionary<int, Player>();
        // �׽�Ʈ �ڵ�
        PlayerDic.Add(0, null);
        PlayerDic.Add(1, null);
        PlayerDic.Add(2, null);
    }
}
