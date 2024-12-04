using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

[RequireComponent(typeof(PhotonView))]
public class GameState : MonoBehaviourPunCallbacks
{
    private const int maxPlayer = 4;

    [Header("���� ���� & ���� ����")]
    [SerializeField] protected float startDelayTime;
    [SerializeField] protected float finishDelayTime;
    [SerializeField] private PlayerGameCanvasUI uiPrefab;
    [SerializeField] private int returnSceneIndex;

    [Header("�÷��̾� ���� ����")]
    [SerializeField] private string playerPrefabPath;
    [SerializeField] private string towerPrefabPath;
    [SerializeField] private string wallPrefabPath;
    [SerializeField] private float maxLeft;                // ���� ���� ������ ���ϴ� ��ǥ
    [SerializeField] private float maxRight;               // ���� ���� ������ ���� ��ǥ
    [SerializeField] private float playerSpaceMaxWidth;    // �÷��̾� ���� ���� �ִ�ġ ����
    [SerializeField] private float initY;

    [HideInInspector] public Dictionary<int, GameObject> playerObjectDic;
    [HideInInspector] public Dictionary<int, GameObject> towerObjectDic;
    [HideInInspector] public float playerWidth;
    protected PlayerGameCanvasUI playerUI;
    private WaitForSecondsRealtime startDelay;
    private WaitForSeconds finishDelay;

    // Ȱ��ȭ ������ ��� �ʱ�ȭ
    public override void OnEnable()
    {
        base.OnEnable();
        StartCoroutine(NetworkWaitRoutine());
    }

    private void OnDestroy()
    {

        // ���常 ���� �� ���� �۾� ����
        if (!PhotonNetwork.IsMasterClient) return;

        // ��� �÷��̾ ������ ��Ʈ��ũ ������Ʈ�� ���� (�� �� RPC ����)
        PhotonNetwork.DestroyAll();
        print("��Ʈ��ũ ������Ʈ�� ���� ����");
        playerObjectDic.Clear();
        towerObjectDic.Clear();

    }
    public override void OnDisable()
    {
        // ���常 ���� �� �ε� �۾� ����
        if (PhotonNetwork.IsMasterClient == false) return;

        PhotonNetwork.CurrentRoom.IsOpen = true;
        print("������ �������Ƿ� ���� ��ȯ�˴ϴ�.");
        PhotonNetwork.LoadLevel(returnSceneIndex);

        base.OnDisable();
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
            var playerSpawnPos = PlayerSpawnStartPositions(initY, players.Length);
            print($"�÷��̾� ��: {players.Length}"); 

            var playerNum = PhotonNetwork.LocalPlayer.GetPlayerNumber();
            // Ÿ�� ����
            print(playerSpawnPos[playerNum]);

            var towerObj = PhotonNetwork.Instantiate(towerPrefabPath, playerSpawnPos[playerNum], Quaternion.identity, data: new object[] { playerNum, photonView.ViewID });
            // ��Ʈ��ũ �÷��̾� ������Ʈ�� �����ϱ�
            var playerPath = playerPrefabPath + playerNum.ToString();
            var playerObj = PhotonNetwork.Instantiate(playerPath, playerSpawnPos[playerNum], Quaternion.identity, data: new object[] { players[playerNum].NickName });

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
        //print($"������ ���� RPC�� ���ű��� ������ {delay}");
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
    private Vector2[] PlayerSpawnStartPositions(float initY,int playerNum)
    {
        if (playerNum < 1 || playerNum > maxPlayer) return null;

        // ���� �÷��̾� �ʺ� = ��ü �ʺ� / �÷��̾� ��
        // ���� �÷��̾� ������ 0.25������ ������ �� �ֵ��� ����
        var rawWidth = MathF.Abs(maxRight - maxLeft) / playerNum;
        playerWidth = Mathf.Ceil(rawWidth / 0.5f) * 0.5f;

        // �÷��̾� ������ �ִ� ���� �ʺ� ����� ���ϵ��� ����
        if (playerWidth > playerSpaceMaxWidth) playerWidth = playerSpaceMaxWidth;

        // ������ width�� ����, ���ϴ� ��ǥ ����, 
        var widthRemain = MathF.Abs(maxRight - maxLeft) - (playerWidth * playerNum);
        // ��� ����
        var bottomLeft = new Vector2(maxLeft + widthRemain / 2, initY);

        // ���� �� �� = �÷��̾� �� + 1
        // ���� �� ��ġ (x��) = bottomLeft.x + ���� �� �ε��� * width
        // ���� �� ��ġ (y��) = bottomLeft.y
        for (int i = 0; i < playerNum + 1; i++)
        {
            PhotonNetwork.Instantiate(wallPrefabPath, new Vector2(bottomLeft.x + (i * playerWidth), initY), Quaternion.identity);
        }

        // �÷��̾� ���� ��ġ (x��) =
        // (bottomLeft + ���� �ʺ� * �÷��̾� �ε��� = �� �÷��̾� ������ bottomLeft)
        // + (���γʺ� / 2 = �� �÷��̾� ������ �߽�) 
        // �÷��̾� ���� ��ġ (y��) = bottomLeft.y
        var playerPositions = new Vector2[playerNum];
        for (int i = 0; i < playerPositions.Length; i++)
        {
            playerPositions[i] = new Vector2((bottomLeft.x + playerWidth * i) + (playerWidth / 2), initY);
        }
        return playerPositions;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        print("������ ������ ���� ������ ����˴ϴ�.");

        // ������ �ٲ�� ��� (�������� �� ���)
        // 1. ���� ���� ��� ������Ʈ ���� 
        gameObject.SetActive(false);
        // 2. �� ������
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        // 3. �κ� ������ ����
        PhotonNetwork.LoadLevel(returnSceneIndex);
    }
}
