using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState : MonoBehaviour
{
    
    [Header("�⺻ ����")]
    public StateType StateType;

    [Header("�÷��̾� ���� ����")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector2 bottomLeft;            // ���� ���� ������ ���ϴ� ��ǥ
    [SerializeField] Vector2 upRight;               // ���� ���� ������ ���� ��ǥ

    protected CoreManager manager;
    protected Dictionary<int, GameObject> playerObjectDic;

    public virtual void Enter() 
    {
        print($"{StateType}�� ����");
        manager = CoreManager.Instance;
        playerObjectDic = new Dictionary<int, GameObject>();
        if (playerPrefab != null )
        {
            var playerKeys = manager.PlayerDic.Keys.ToArray();
            print($"�÷��̾� ��: {playerKeys.Length}");
            var playerSpawnPos = PlayerSpawnStartPositions(bottomLeft, upRight, playerKeys.Length);
            for (int i = 0; i < playerKeys.Length; i++)
            {
                playerObjectDic.Add(playerKeys[i], Instantiate(playerPrefab, playerSpawnPos[i], Quaternion.identity, null));
            }
        }
    }
    public virtual void OnUpdate() 
    {
        //print($"{StateType}���� ������Ʈ ��");
    }
    public virtual void Exit() 
    {
        print($"{StateType}���� Ż��");
        // TODO: ���� ��� ������ ��, ��� ó��?
        // �⺻������ �ش� ���� ��尡 ������ ���� ��ü�ȴٰ� �����ϰ�, �ʱ�ȭ ����
        var playerObjectKeys =playerObjectDic.Keys.ToArray();
        // �÷��̾� ������Ʈ�� ����
        foreach (var key in playerObjectKeys)
        {
            if (playerObjectDic[key] != null)
                Destroy(playerObjectDic[key]);
        }
        playerObjectDic.Clear();
        manager?.ResetPlayer();
    }

    private Vector2[] PlayerSpawnStartPositions(Vector2 bottomLeft, Vector2 upRight, int playerNum)
    {
        if (playerNum < 1 || playerNum > 4) return null;

        // ���� �÷��̾� �ʺ� = ��ü �ʺ� / �÷��̾� ��
        var width = MathF.Abs(upRight.x - bottomLeft.x) / playerNum;

        // �÷��̾� ���� ��ġ (x��) =
        // (bottomLeft + ���� �ʺ� * �÷��̾� �ε��� = �� �÷��̾� ������ bottomLeft)
        // + (���γʺ� / 2 = �� �÷��̾� ������ �߽�) 
        // �÷��̾� ���� ��ġ (y��) = bottomLeft.y
        var playerPositions = new Vector2[playerNum];
        for (int i = 0; i < playerPositions.Length; i++)
        {
            playerPositions[i] = new Vector2((bottomLeft.x + width * i) + (width / 2), bottomLeft.y);
        }
        return playerPositions;
    }
}
