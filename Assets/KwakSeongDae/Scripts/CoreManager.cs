using Photon.Pun;
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
    public Dictionary<int, GameObject> PlayerDic;

    protected override void Init()
    {
        stateDic = new Dictionary<StateType, GameState>();
        PlayerDic = new Dictionary<int, GameObject>();
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
    /// ���� �䰡 ������ �÷��̾ �÷��̾�� �߰� ����
    /// </summary>
    public void SetPlayer(GameObject player)
    {
        if (player.TryGetComponent<PhotonView>(out var view))
        {
            PlayerDic.Add(view.Owner.ActorNumber, player);
        }
    }
    public void SetPlayer(GameObject[] player)
    {
        foreach (var p in player)
        {
            if (p.TryGetComponent<PhotonView>(out var view))
            {
                PlayerDic.Add(view.Owner.ActorNumber, p);
            }
        }
    }

    /// <summary>
    /// ����, �� �÷��̾ ���� �ʱ�ȭ�� ���� �ʿ��� ���, PlayerDic�� �̿��� ���� �ʱ�ȭ ����
    /// </summary>
    public void ResetPlayer()
    {
        PlayerDic.Clear();
    }
}
