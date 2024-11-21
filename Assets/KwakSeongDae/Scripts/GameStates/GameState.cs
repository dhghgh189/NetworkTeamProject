using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [Header("�⺻ ����")]
    public StateType StateType;

    protected CoreManager manager;

    public virtual void Enter() 
    {
        print($"{StateType}�� ����");
        manager = CoreManager.Instance;
    }
    public virtual void OnUpdate() 
    {
        //print($"{StateType}���� ������Ʈ ��");
    }
    public virtual void Exit() 
    {
        print($"{StateType}���� Ż��");
    }
}
