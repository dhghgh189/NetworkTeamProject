using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PuzzleModeState : GameState
{
    [Header("���� ��� ����")]
    [SerializeField] private BoxCollider2D boxDetector;
    [SerializeField] float towerHeightStep;
    [SerializeField] float towerSpeed;

    private Coroutine finishRoutine;
    private Dictionary<int, bool> isBlockCheckDic;
    private Coroutine mainCollisionRoutine;

    private Action<int> hpAction;
    private GameObject selfPlayer;
    private Coroutine panaltyRoutine;
    private Vector2 targetTowerPos;
    private bool isMoveTower;

    private int playerID;

    public override void OnEnable()
    {
        base.OnEnable();

        // ������� ��� (Puzzle)
        SoundManager.Instance.Play(Enums.ESoundType.BGM, SoundManager.BGM_PUZZLE);
    }

    protected override void Init()
    {
        base.Init();

        // Dictionary �ʱ� ����
        isBlockCheckDic = new Dictionary<int, bool>();
        
        // �÷��̾� ����ŭ �̸� ��� �߰�
        foreach (var player in PhotonNetwork.PlayerList)
        {
            isBlockCheckDic.Add(player.ActorNumber, false);
        }

        playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        selfPlayer = playerObjectDic[playerID];
        //hpAction = (newHP) => PlayerHPHandle(newHP, playerID);
        selfPlayer.GetComponent<PlayerController>().OnChangeHp += PlayerHPHandle;


        // ���常 �浹 ���� ��ƾ ����
        if (PhotonNetwork.IsMasterClient)
            mainCollisionRoutine = StartCoroutine(CollisionCheckRoutine());
    }

    private void Finish()
    {
        // ���� �۾� ������
        if (PhotonNetwork.IsMasterClient
            && mainCollisionRoutine != null)
            StopCoroutine(mainCollisionRoutine);

        // finishRoutine�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        if (finishRoutine != null)
            StopCoroutine(finishRoutine);

        isBlockCheckDic?.Clear();

        selfPlayer.GetComponent<PlayerController>().OnChangeHp -= PlayerHPHandle;
    }

    private void PlayerHPHandle(int newHP)
    {
        print("ü�� ��ȭ");
        //// �ڽ� �̺�Ʈ�� ��쿡�� ȣ��
        //if (PhotonNetwork.LocalPlayer.ActorNumber != playerID) return;

        // ���� ���ÿ� ������ ��, �ߺ� ȣ�� ����
        if (panaltyRoutine == null)
            panaltyRoutine = StartCoroutine(PanaltyRoutine());
    }

    private IEnumerator PanaltyRoutine()
    {
        // �� Ÿ���� ���̸� towerHegithStep��ŭ ���
        // �ش� Ÿ���� photonView transform�̹Ƿ� �ڵ����� ��ġ ����ȭ
        targetTowerPos = (Vector2)towerObjectDic[playerID].transform.position + Vector2.up * towerHeightStep;
        towerObjectDic[playerID].GetComponent<Rigidbody2D>().velocity = Vector2.up * towerSpeed;
        // �÷��̾ �г�Ƽ ó�� �˸�
        playerObjectDic[playerID].GetComponent<PlayerController>().ProcessPanalty(true);
        isMoveTower = true;
        // ���� ���ÿ� �������� ���, �ߺ� ���� ����
        yield return new WaitForSeconds(1f);
        panaltyRoutine = null;
    }

    private void FixedUpdate()
    {
        if (towerObjectDic != null && towerObjectDic.ContainsKey(playerID))
        {
            if (isMoveTower)
            {
                var curPos = (Vector2)towerObjectDic[playerID].transform.position;
                var dif = targetTowerPos - curPos;
                // �Ÿ� ���̰� �̹������ų�, ���� y���� ��ǥġ�� y���� �Ѿ ��쿡�� ��ġ ���� �� �ӵ� 0
                if (Vector2.Distance(targetTowerPos, curPos) < 0.01f
                    || dif.y < -0.01f)
                {
                    towerObjectDic[playerID].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    towerObjectDic[playerID].transform.position = targetTowerPos;
                    isMoveTower = false;
                    // �÷��̾ �г�Ƽ ó�� ���� �˸�
                    playerObjectDic[playerID].GetComponent<PlayerController>().ProcessPanalty(false);
                }
            }
            else
            {
                // Ÿ�� Ÿ�� ��ġ �ʱ�ȭ
                targetTowerPos = towerObjectDic[playerID].transform.position;
            }
        }
    }

    private IEnumerator CollisionCheckRoutine()
    {
        var detectorPos = (Vector2)boxDetector.transform.position + boxDetector.offset;
        var detectorScale = Vector2.Scale(boxDetector.transform.localScale, boxDetector.size);
        var delay = new WaitForSeconds(0.1f);

        while (true)
        {
            // 1. ���� �� �浹 Check�� False�� �ʱ�ȭ
            var IDs = isBlockCheckDic.Keys.ToArray();
            foreach (var ID in IDs)
            {
                isBlockCheckDic[ID] = false;
            }

            // 2. Physics2D�� �浹ü �˻�
            // isEntered�� �� ���� �����ؼ� ���� FInish ���� ���� ������Ʈ
            Collider2D[] cols = Physics2D.OverlapBoxAll(detectorPos, detectorScale, 0, LayerMask.GetMask("Blocks"));
            print("���� �浹 ���� ��");
            foreach (var collision in cols)
            {
                var blockTrans = collision.transform.parent;
                var block = blockTrans.GetComponent<Blocks>();
                // ���� �����ϴ� ���, �ش� �������� ���� ������ üũ
                // �浹�� ���� ������, �÷��̾��� �ڷ�ƾ�� ���� �Ǵ� ��, �ڷ�ƾ ����
                if (block.IsControllable == false
                    && blockTrans.TryGetComponent<PhotonView>(out var view))
                {
                    int ID = view.Owner.ActorNumber;

                    if (isBlockCheckDic.ContainsKey(ID))
                        isBlockCheckDic [ID] = true;
                }
            }

            // Ÿ���� �浹�� ��쵵 ����
            Collider2D[] towerCols = Physics2D.OverlapBoxAll(detectorPos, detectorScale, 0, LayerMask.GetMask("Ground"));
            foreach (var collision in towerCols)
            {
                var towerTrans = collision.transform.parent;
                var tower = towerTrans.GetComponent<Tower>();
                // Ÿ���� �浹�� ���� �� ��� ����
                if (towerTrans.TryGetComponent<PhotonView>(out var view))
                {
                    int ID = view.Owner.ActorNumber;

                    // �ش� PlayerStateChange�� ���������� ����
                    PlayerStateChange(ID);

                    // ����, ��� �÷��̾�� ���� üũ ��, ������� ����
                    AllPlayerStateCheck(ID);
                }
            }

            // 3. ���� �浹�� ���� �ִ� �÷��̾�� FInishRoutine�� RPC�� �����ϵ��� �����
            // �浹�� ���� ���� �÷��̾���� ���� ����Ǵ� ��ƾ�� ����
            IDs = isBlockCheckDic.Keys.ToArray();
            foreach (var ID in IDs)
            {
                // ��üũ�� �ش� �÷��̾ �����鼭 true�� ��� => ���� FInish������ ���� ����
                if (isBlockCheckDic[ID] == true)
                {
                    print($"{ID} �� ����");
                    photonView.RPC("FinishRoutineWrap", RpcTarget.AllViaServer, ID, true);
                }
                else
                {
                    print($"{playerID} �� ����");
                    photonView.RPC("FinishRoutineWrap", RpcTarget.AllViaServer, ID, false);
                }
            }
            yield return delay;
        }
    }

    [PunRPC]
    private void FinishRoutineWrap(int ID, bool isPlay)
    {
        // �ش� �÷��̾���� ��ƾ ����
        if (playerID != ID) return;

        if (isPlay)
        {
            // ������ �������̸� ����
            if (finishRoutine == null)
                finishRoutine = StartCoroutine(FinishRoutine(ID));
        }
        else
        {
            if (finishRoutine != null)
                StopFinishRoutine(finishRoutine);

            finishRoutine = null;
        }
    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));
        // ���� �ð��� ������ �ش� �÷��̾�� �� �̻� ���� �Ұ�

        // �ش� PlayerStateChange�� ���������� ����
        PlayerStateChange(playerID);

        // ����, ��� �÷��̾�� ���� üũ ��, ������� ����
        photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient,playerID);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        print($"{otherPlayer.ActorNumber}�� ����");

        // ������ ���� ���� ���� ó��
        if (PhotonNetwork.IsMasterClient == false) return;

        if (playerObjectDic.ContainsKey(otherPlayer.ActorNumber)
            && playerObjectDic[otherPlayer.ActorNumber].TryGetComponent<PlayerController>(out var controller))
        {
            controller.ReachGoal();
            playerObjectDic.Remove(otherPlayer.ActorNumber);
        }

        if (towerObjectDic.ContainsKey(otherPlayer.ActorNumber))
            towerObjectDic.Remove(otherPlayer.ActorNumber);

        AllPlayerStateCheck(otherPlayer.ActorNumber);
    }

    private void PlayerStateChange(int playerID)
    {
        if (playerObjectDic.ContainsKey(playerID)
            && playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
        {
            controller.ReachGoal();
        }

        print($"{playerID}�� ���� ������ �� �����ϴ�.");
    }

    [PunRPC]
    private void AllPlayerStateCheck(int playerID = -1)
    {
        // ��ųʸ����� �ʱ�ȭ�� �÷��̾ �ִ� ���, ������ �ʱ�ȭ ����
        if (playerID > -1)
        {
            // ������ Dic�� ��Ͽ��� �ش� �÷��̾� ����
            isBlockCheckDic.Remove(playerID);
        }

        // ���� ���: ��� �÷��̾ ���� ��쿡�� ���� ���� 
        if (isBlockCheckDic.Count < 1)
        {
            List<Tuple<int,int>> result = new List<Tuple<int,int>>();
            foreach (var playerKey in playerObjectDic.Keys)
            {
                if (playerObjectDic[playerKey].TryGetComponent<PlayerController>(out var controller))
                {
                    result.Add(new Tuple<int, int>(playerKey, controller.BlockCount));
                }
            }

            //������������ �� ���� ����
            result.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            //�� Ŭ���̾�Ʈ����, ������ UI�� �ش� ���� �ݿ��ǵ��� ����
            var players = new int[result.Count];
            var blockCounts = new int[result.Count];
            for (int i = 0; i < result.Count; i++)
            {
                players[i] = result[i].Item1;
                blockCounts[i] = result[i].Item2;
            }

            // UI ������Ʈ �۾� �� ���� ��������� ��� Ŭ���̾�Ʈ ����
            photonView.RPC("UpdateUI", RpcTarget.All, players, blockCounts);
        }
    }

    [PunRPC]
    private void UpdateUI(int[] playerIDs,int[] blockCounts)
    {
        for (int i = 0; i < playerIDs.Length; i++)
        {
            playerUI?.AddResultEntry(playerIDs[i], blockCounts[i]);
        }
        playerUI?.SetResult();

        // ������ �۾�
        Finish();

        print($"��� �÷��̾��� �� ���� ���� �� ���� ����");
        print($"{playerIDs[0]}�� ���� ����� ������Դϴ�!!!");

        //Time.timeScale = 0f;
    }
}
