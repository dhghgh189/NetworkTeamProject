using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[Serializable]
//public enum StateType
//{
//    Stop,Puzzle,Race,Survival,Size
//}

public class CoreManager : Singleton<CoreManager>
{
//    [Header("���� ���� ����")]
//    [SerializeField] GameState[] states;
//    [SerializeField] private StateType currentState;
//    public StateType CurrentState
//    {
//        get => currentState;
//        set
//        {
//            currentState = value;
//            // ��� Ŭ���̾�Ʈ ���� �Ŵ������� ���� ���� ����
//            photonView.RPC("ChangeState",RpcTarget.All,value);
//        }
//    }

//    [HideInInspector]public GameState state;
//    private Dictionary<StateType, GameState> stateDic;

//    [Header("���� �÷��̾� ����")]
//    public Dictionary<int, Player> PlayerDic;

//    protected override void Init()
//    {
//        stateDic = new Dictionary<StateType, GameState>();
//        PlayerDic = new Dictionary<int, Player>();
//        foreach (var s in states)
//        {
//            stateDic.Add(s.StateType, s);
//        }
//        print("INIT");
//        CurrentState = StateType.Stop;
//    }

//    private void Update()
//    {
//        state?.OnUpdate();
//    }

//    [PunRPC]
//    private void ChangeState(StateType changeType)
//    {
//        if (stateDic != null 
//            && stateDic.ContainsKey(changeType))
//        {
//            state?.Exit();
//            state?.gameObject.SetActive(false);

//            state = stateDic[changeType];

//            state.gameObject.SetActive(true);
//            state?.Enter();
//        }
//    }

//    /// <summary>
//    /// �ν����Ϳ��� State�� ���Ƿ� �ٲ��� ��, ����ǵ��� ����.
//    /// ����,������ ���� �ʿ�
//    /// </summary>
//    private void OnValidate()
//    {
//#if UNITY_EDITOR
//        // ��� Ŭ���̾�Ʈ ���� �Ŵ������� ���� ���� ����
//            photonView.RPC("ChangeState",RpcTarget.All,currentState);
//#endif
//    }
}
