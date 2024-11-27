using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;
using static UnityEditor.Progress;

public enum SceneIndex
{
    Room, Game
}

[RequireComponent(typeof(PhotonView))]
public class GameState : MonoBehaviourPun
{
    private const int maxPlayer = 4;

    [Header("���� ���� & ���� ����")]
    [SerializeField] protected float startDelayTime;
    [SerializeField] protected float finishDelayTime;
    [SerializeField] private PlayerGameCanvasUI uiPrefab;

    [Header("�÷��̾� ���� ����")]
    [SerializeField] private string playerPrefabPath;
    [SerializeField] private string towerPrefabPath;
    [SerializeField] private string wallPrefabPath;
    [SerializeField] private Vector2 bottomLeft;            // ���� ���� ������ ���ϴ� ��ǥ
    [SerializeField] private Vector2 upRight;               // ���� ���� ������ ���� ��ǥ

    [HideInInspector] public Dictionary<int, GameObject> playerObjectDic;
    protected PlayerGameCanvasUI playerUI;
    private WaitForSecondsRealtime startDelay;
    private WaitForSeconds finishDelay;

    // Ȱ��ȭ ������ ��� �ʱ�ȭ
    private void Start()
    {
        StartCoroutine(NetworkWaitRoutine());
    }
    protected virtual void Init()
    {
        print($"���� ��忡 ����");

        // ���� �����̴� ������ ����ߵǴ� ��ɵ� �����ϹǷ� Realtime���� ���
        startDelay = new WaitForSecondsRealtime(startDelayTime);
        finishDelay = new WaitForSeconds(finishDelayTime);
        // �÷��̾� ������Ʈ ��ųʸ��� ��� Ŭ���̾�Ʈ�� ������ �ֵ��� ����
        playerObjectDic = new Dictionary<int, GameObject>();

        if (playerPrefabPath.IsNullOrEmpty() == false
            && uiPrefab != null)
        {
            var players = PhotonNetwork.PlayerList;
            var playerSpawnPos = PlayerSpawnStartPositions(bottomLeft, upRight, players.Length);
            print($"�÷��̾� ��: {players.Length}");

            var playerNum = PhotonNetwork.LocalPlayer.GetPlayerNumber();
            // Ÿ�� ����
            var towerObj = PhotonNetwork.Instantiate(towerPrefabPath, playerSpawnPos[playerNum], Quaternion.identity, data: new object[] { playerNum });
            // ��Ʈ��ũ �÷��̾� ������Ʈ�� �����ϱ�
            var playerObj = PhotonNetwork.Instantiate(playerPrefabPath, playerSpawnPos[playerNum], Quaternion.identity, data: new object[] { players[playerNum].NickName });

            var playerView = playerObj.GetComponent<PhotonView>();
            photonView.RPC("SetPlayerObjectDic", RpcTarget.All, playerView.ViewID);
            // ���� ������Ʈ�� �����Ǵ� ��쿡�� ���� UI�� ���� ����
            playerUI = Instantiate(uiPrefab);
        }

        // RPC�̿��ؼ� ���� �ð� ����ȭ, ������ RPC������
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("StartRoutineWrap", RpcTarget.All);
    }
    
    private IEnumerator NetworkWaitRoutine()
    {
        var delay = new WaitForSeconds(1f);
        yield return delay;
        Init();
    }

    public virtual void Exit()
    {
        print($"���� ��� ����");

        // ��� ��ųʸ� �ʱ�ȭ ���� ���ʿ�
        Time.timeScale = 1f;

        SceneLoad(SceneIndex.Room);
    }

    [PunRPC]
    protected void SetPlayerObjectDic(int viewID)
    {
        var obj = PhotonView.Find(viewID);
        playerObjectDic.Add(obj.Owner.ActorNumber, obj.gameObject);
    }

    [PunRPC]
    protected void StartRoutineWrap()
    {
        StartCoroutine(StartRoutine(PhotonNetwork.Time));
    }

    /// <summary>
    /// ��� ���� ��, �۵��� Ÿ�̸� ��ƾ
    /// </summary>
    protected IEnumerator StartRoutine(double startTime)
    {
        var delay = PhotonNetwork.Time - startTime;
        print($"������ ���� RPC�� ���ű��� ������ {delay}");
        // �������� ����
        playerUI?.SetTimer(startDelayTime - (float)delay);
        Time.timeScale = 0f;
        yield return startDelay;
        playerUI?.SetTimer(0);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// �� ���� ��� ��, FinishRoutine ���� �Լ�
    /// </summary>
    protected virtual IEnumerator FinishRoutine(int playerID)
    {
        playerUI?.SetTimer(finishDelayTime);
        yield return finishDelay;
        playerUI?.SetTimer(0);
    }

    /// <summary>
    /// FinishRoutine�����, StopCoroutine���� �ѹ� ���İ��� �̵��Լ�
    /// </summary>
    protected void StopFinishRoutine(Coroutine routine)
    {
        playerUI?.SetTimer(0);
        StopCoroutine(routine);
    }

    /// <summary>
    /// �÷��̾ ������ ��ġ ��ȯ �� �� ���ѱ��� ����
    /// </summary>
    /// <param name="bottomLeft"> �� ���ϴ� ��ġ</param>
    /// <param name="upRight"> �� ���� ��ġ</param>
    /// <param name="playerNum"> �� �÷��̾� ��</param>
    /// <returns></returns>
    private Vector2[] PlayerSpawnStartPositions(Vector2 bottomLeft, Vector2 upRight, int playerNum)
    {
        if (playerNum < 1 || playerNum >= maxPlayer) return null;

        // ���� �÷��̾� �ʺ� = ��ü �ʺ� / �÷��̾� ��
        var width = MathF.Abs(upRight.x - bottomLeft.x) / playerNum;

        // ���� �� �� = �÷��̾� �� + 1
        // ���� �� ��ġ (x��) = bottomLeft.x + ���� �� �ε��� * width
        // ���� �� ��ġ (y��) = bottomLeft.y
        for (int i = 0; i < playerNum+1; i++)
        {
            PhotonNetwork.Instantiate(wallPrefabPath, new Vector2(bottomLeft.x + (i * width), bottomLeft.y), Quaternion.identity);
        }

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
