using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SceneIndex
{
    Room, Game
}

public class GameState : MonoBehaviour
{
    [Header("�⺻ ����")]
    public StateType StateType;

    [Header("���� ���� & ���� ����")]
    [SerializeField] protected float startDelayTime;
    [SerializeField] protected float finishDelayTime;
    [SerializeField] private GameTimer timerUI;

    [Header("�÷��̾� ���� ����")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector2 bottomLeft;            // ���� ���� ������ ���ϴ� ��ǥ
    [SerializeField] private Vector2 upRight;               // ���� ���� ������ ���� ��ǥ

    protected CoreManager manager;
    protected Dictionary<int, GameObject> playerObjectDic;
    private WaitForSecondsRealtime startDelay;
    private WaitForSeconds finishDelay;

    private void OnEnable()
    {
        // ���� �����̴� ������ ����ߵǴ� ��ɵ� �����ϹǷ� Realtime���� ���
        startDelay = new WaitForSecondsRealtime(startDelayTime);
        finishDelay = new WaitForSeconds(finishDelayTime);
    }

    public virtual void Enter()
    {
        print($"{StateType}�� ����");
        manager = CoreManager.Instance;
        playerObjectDic = new Dictionary<int, GameObject>();
        if (playerPrefab != null)
        {
            var playerKeys = manager.PlayerDic.Keys.ToArray();
            print($"�÷��̾� ��: {playerKeys.Length}");
            var playerSpawnPos = PlayerSpawnStartPositions(bottomLeft, upRight, playerKeys.Length);
            for (int i = 0; i < playerKeys.Length; i++)
            {
                playerObjectDic.Add(playerKeys[i], Instantiate(playerPrefab, playerSpawnPos[i], Quaternion.identity, null));
            }
        }
        StartCoroutine(StartRoutine());
    }
    public virtual void OnUpdate()
    {
        //print($"{StateType}���� ������Ʈ ��");
    }
    public virtual void Exit()
    {
        print($"{StateType}���� Ż��");

        // �⺻������ �ش� ���� ��尡 ������ ���� ��ü�ȴٰ� �����ϰ�, �ʱ�ȭ ����
        var playerObjectKeys = playerObjectDic.Keys.ToArray();
        // �÷��̾� ������Ʈ�� ����
        foreach (var playerID in playerObjectKeys)
        {
            if (playerObjectDic[playerID] != null)
                Destroy(playerObjectDic[playerID]);
        }
        playerObjectDic.Clear();
        manager?.ResetPlayer();

        SceneLoad(SceneIndex.Room);
    }

    /// <summary>
    /// ��� ���� ��, �۵��� Ÿ�̸� ��ƾ
    /// </summary>
    private IEnumerator StartRoutine()
    {
        timerUI.Timer = startDelayTime;
        timerUI.transform.gameObject.SetActive(true);
        Time.timeScale = 0f;
        yield return startDelay;
        timerUI.transform.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// �� ���� ��� ��, FinishRoutine ���� �Լ�
    /// </summary>
    protected virtual IEnumerator FinishRoutine(int playerID)
    {
        timerUI.Timer = finishDelayTime;
        timerUI.transform.gameObject.SetActive(true);
        yield return finishDelay;
        timerUI.transform.gameObject.SetActive(false);
    }

    /// <summary>
    /// FinishRoutine�����, StopCoroutine���� �ѹ� ���İ��� �̵��Լ�
    /// </summary>
    protected void StopFinishRoutine(Coroutine routine)
    {
        // ���� ��, �������� ��ɵ� �ϰ� ó��
        timerUI.transform.gameObject.SetActive(false);
        StopCoroutine(routine);
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

    protected void SceneLoad(SceneIndex sceneIndex)
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        // �� ��ȯ �׽�Ʈ �Ҷ��� �ּ�ó��
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.LoadLevel((int)sceneIndex);
    }
}
