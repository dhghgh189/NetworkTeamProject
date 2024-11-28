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
    [HideInInspector] public Dictionary<int, GameObject> towerObjectDic;
    [HideInInspector] public float playerWidth;
    protected PlayerGameCanvasUI playerUI;
    private WaitForSecondsRealtime startDelay;
    private WaitForSeconds finishDelay;

    // Ȱ��ȭ ������ ��� �ʱ�ȭ
    private void OnEnable()
    {
        print("����");
        StartCoroutine(NetworkWaitRoutine());
    }
    protected virtual void Init()
    {
        // ���� �����̴� ������ ����ߵǴ� ��ɵ� �����ϹǷ� Realtime���� ���
        startDelay = new WaitForSecondsRealtime(startDelayTime);
        finishDelay = new WaitForSeconds(finishDelayTime);
        // �÷��̾� ������Ʈ ��ųʸ��� ��� Ŭ���̾�Ʈ�� ������ �ֵ��� ����
        playerObjectDic = new Dictionary<int, GameObject>();
        towerObjectDic = new Dictionary<int, GameObject>();

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
            var towerView = towerObj.GetComponent<PhotonView>();
            photonView.RPC("SetPlayerObjectDic", RpcTarget.All, playerView.ViewID);
            photonView.RPC("SetTowerObjectDic", RpcTarget.All, towerView.ViewID);
            // ���� ������Ʈ�� �����Ǵ� ��쿡�� ���� UI�� ���� ����
            playerUI = Instantiate(uiPrefab);
            playerUI.GetComponent<PlayerGameCanvasUI>().gameState = gameObject;
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

    [PunRPC]
    protected void SetPlayerObjectDic(int viewID)
    {
        var obj = PhotonView.Find(viewID);
        playerObjectDic.Add(obj.Owner.ActorNumber, obj.gameObject);
    }

    [PunRPC]
    protected void SetTowerObjectDic(int viewID)
    {
        var obj = PhotonView.Find(viewID);
        towerObjectDic.Add(obj.Owner.ActorNumber, obj.gameObject);
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
        // ���� �÷��̾� ������ 0.25������ ������ �� �ֵ��� ����
        var rawWidth = MathF.Abs(upRight.x - bottomLeft.x) / playerNum;
        playerWidth = Mathf.Ceil(rawWidth / 0.5f) * 0.5f;
        // ������ width�� ����, ���ϴ� ��ǥ ����, 
        var widthRemain = MathF.Abs(upRight.x - bottomLeft.x) - (playerWidth * playerNum);
        // ��� ����
        bottomLeft = new Vector2(bottomLeft.x + widthRemain / 2, bottomLeft.y);

        // ���� �� �� = �÷��̾� �� + 1
        // ���� �� ��ġ (x��) = bottomLeft.x + ���� �� �ε��� * width
        // ���� �� ��ġ (y��) = bottomLeft.y
        for (int i = 0; i < playerNum+1; i++)
        {
            PhotonNetwork.Instantiate(wallPrefabPath, new Vector2(bottomLeft.x + (i * playerWidth), bottomLeft.y), Quaternion.identity);
        }

        // �÷��̾� ���� ��ġ (x��) =
        // (bottomLeft + ���� �ʺ� * �÷��̾� �ε��� = �� �÷��̾� ������ bottomLeft)
        // + (���γʺ� / 2 = �� �÷��̾� ������ �߽�) 
        // �÷��̾� ���� ��ġ (y��) = bottomLeft.y
        var playerPositions = new Vector2[playerNum];
        for (int i = 0; i < playerPositions.Length; i++)
        {
            playerPositions[i] = new Vector2((bottomLeft.x + playerWidth * i) + (playerWidth / 2), bottomLeft.y);
        }
        return playerPositions;
    }
}
