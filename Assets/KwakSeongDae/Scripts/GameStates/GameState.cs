using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SceneIndex
{
    Room, Game
}

/* ��ųʸ� ����: ���常
 * �浹 ���� �� �� �� ����: ���常 
 * UI ����: �� �÷��̾�
 */
public class GameState : MonoBehaviourPunCallbacks
{
    [Header("�⺻ ����")]
    public StateType StateType;

    [Header("���� ���� & ���� ����")]
    [SerializeField] protected float startDelayTime;
    [SerializeField] protected float finishDelayTime;
    [SerializeField] private PlayerGameCanvasUI uiPrefab;

    [Header("�÷��̾� ���� ����")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector2 bottomLeft;            // ���� ���� ������ ���ϴ� ��ǥ
    [SerializeField] private Vector2 upRight;               // ���� ���� ������ ���� ��ǥ

    [HideInInspector] public Dictionary<int, GameObject> playerObjectDic;
    protected PlayerGameCanvasUI playerUI;
    private WaitForSecondsRealtime startDelay;
    private WaitForSeconds finishDelay;

    // Ȱ��ȭ ������ ��� �ʱ�ȭ
    protected virtual void OnEnable()
    {
        print($"{StateType}�� ����");

        // ���� �����̴� ������ ����ߵǴ� ��ɵ� �����ϹǷ� Realtime���� ���
        startDelay = new WaitForSecondsRealtime(startDelayTime);
        finishDelay = new WaitForSeconds(finishDelayTime);
        playerObjectDic = new Dictionary<int, GameObject>();

        if (playerPrefab != null
            && uiPrefab != null)
        {
            var players = PhotonNetwork.PlayerList;
            var playerSpawnPos = PlayerSpawnStartPositions(bottomLeft, upRight, players.Length);
            print($"�÷��̾� ��: {players.Length}");

            for (int i = 0; i < players.Length; i++)
            {
                var playerObj = Instantiate(playerPrefab, playerSpawnPos[i], Quaternion.identity, null);
                // ���� ������Ʈ�� �����Ǵ� ��쿡�� ���� UI�� ���� ����
                if (players[i] == PhotonNetwork.LocalPlayer)
                    playerUI = Instantiate(uiPrefab, playerObj.transform);

                playerObjectDic.Add(players[i].ActorNumber, playerObj);
            }
        }

        // RPC�̿��ؼ� ���� �ð� ����ȭ, ������ RPC������
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("StartRoutineMiddleware", RpcTarget.AllViaServer);
    }

    public virtual void Exit()
    {
        print($"{StateType}���� Ż��");

        // ��� ��ųʸ� �ʱ�ȭ ���� ���ʿ�
        Time.timeScale = 1f;

        SceneLoad(SceneIndex.Room);
    }

    [PunRPC]
    private void StartRoutineMiddleware()
    {
        // ���� �ڽ��� State������ ó��
        if(photonView.IsMine)
            StartCoroutine(StartRoutine(PhotonNetwork.Time));
    }

    /// <summary>
    /// ��� ���� ��, �۵��� Ÿ�̸� ��ƾ
    /// </summary>
    private IEnumerator StartRoutine(double startTime)
    {
        var delay = PhotonNetwork.Time - startTime;
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
        // ���� ��, �������� ��ɵ� �ϰ� ó��
        playerUI?.SetTimer(0);

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
