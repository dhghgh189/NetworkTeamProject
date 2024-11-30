using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SurvivalModeState : GameState
{
    [Header("�����̹� ��� ����")]
    [SerializeField] int winBlockCount;

    private Coroutine winRoutine;
    private Coroutine deadRoutine;
    private Action<int> hpAction;
    private Action<int> blockCountAction;
    private List<int> DeadPlayers;

    private GameObject selfPlayer;

    protected override void Init()
    {
        base.Init();
        DeadPlayers = new List<int>();
        var playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        selfPlayer = playerObjectDic[playerID];

        // ���� �ڽŸ� �̺�Ʈ ���
        var controller = selfPlayer.GetComponent<PlayerController>();
        hpAction = (newHP) => PlayerHPHandle(newHP, playerID);
        controller.OnChangeHp += hpAction;


        blockCountAction = (newBlockCount) => PlayerBlockCountHandle(newBlockCount, playerID);
        controller.OnChangeBlockCount += blockCountAction;
    }
    private void OnDisable()
    {
        // Routine�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        if (deadRoutine != null)
            StopCoroutine(deadRoutine);
        if (winRoutine != null)
            StopCoroutine(winRoutine);

        ReturnScene();
        Time.timeScale = 1f;
    }

    public void PlayerHPHandle(int newHP, int playerID)
    {
        print($"{playerID} ü�� ����: {newHP}");
        // �ڽ� �̺�Ʈ�� ��쿡�� ȣ��
        if (PhotonNetwork.LocalPlayer.ActorNumber != playerID) return;

        // ���� ���ÿ� ������ ��, �ߺ� ȣ�� ����
        if (deadRoutine == null)
            deadRoutine = StartCoroutine(DeadRoutine(newHP));
    }


    public void PlayerBlockCountHandle(int newBlockCount, int playerID)
    {
        // �ڽ� �̺�Ʈ�� ��쿡�� ȣ��
        if (PhotonNetwork.LocalPlayer.ActorNumber != playerID) return;

        // ��ǥ �� ������ŭ �׿��� ��, �¸� ��ƾ ����
        if (newBlockCount >= winBlockCount)
        {
            if (winRoutine == null)
                winRoutine = StartCoroutine(FinishRoutine(photonView.Owner.ActorNumber));
        }
        else
        {
            //����ǰ� �ִ� �¸� ��ƾ�� �����ϸ�, �ش� ��ƾ ����
            if (winRoutine != null)
            {
                StopFinishRoutine(winRoutine);
                winRoutine = null;
            }
        }

    }

    private IEnumerator DeadRoutine(int newHP)
    {
        if (newHP < 1)
        {
            if (selfPlayer.TryGetComponent<PlayerController>(out var controller))
            {
                // �ش� �÷��̾�� �� �̻� ���� �Ұ�
                controller.ReachGoal();
                controller.OnChangeHp -= hpAction;
                controller.OnChangeBlockCount -= blockCountAction;

                print($"{photonView.Owner.NickName}���� ���� ����� ��� �����Ǿ� ���ӿ����Ǿ����ϴ�.");
                photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient, false, photonView.Owner.ActorNumber);
            }
        }
        // ���� ���ÿ� �������� ���, �ߺ� ���� ����
        yield return new WaitForSeconds(1f);
        deadRoutine = null;
    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));

        // ���� �ð��� ������
        // �ش� �÷��̾�� �� �̻� ���� �Ұ�
        if (selfPlayer.TryGetComponent<PlayerController>(out var controller))
        {
            controller.ReachGoal();
            controller.OnChangeHp -= hpAction;
            controller.OnChangeBlockCount -= blockCountAction;

            print($"{playerID}�� ���� ������ �� �����ϴ�.");

            // �̹� �¸��� �÷��̾ ��Ÿ�����Ƿ�, ���� ��
            // ��� �÷��̾� ���� üũ ��, ���� ����
            photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient, true, photonView.Owner.ActorNumber);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // ������ ���� ���� ���� ó��
        if (PhotonNetwork.IsMasterClient == false) return;
        playerObjectDic.Remove(otherPlayer.ActorNumber);
        towerObjectDic.Remove(otherPlayer.ActorNumber);

        AllPlayerStateCheck(false);
    }

    [PunRPC]
    private void AllPlayerStateCheck(bool isFinishGame,int playerID = -1)
    {
        // ���� �÷��̾ �ִ� ���, ������ �ʱ�ȭ ����
        if (playerID > -1)
        {
            // ���� Dic�� ��Ͽ��� �ش� �÷��̾� �߰�
            DeadPlayers.Add(playerID);
        }

        if (isFinishGame)
        {
            foreach (var player in playerObjectDic.Keys)
            {
                if(!DeadPlayers.Contains(player))
                {
                    DeadPlayers.Add(player);
                }
            }
        }

        if (DeadPlayers.Count >= playerObjectDic.Count)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();
            foreach (var id in playerObjectDic.Keys)
            {
                if (playerObjectDic[id].TryGetComponent<PlayerController>(out var controller))
                {
                    result.Add(new Tuple<int, int>(id,controller.BlockCount));
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
    private void UpdateUI(int[] playerIDs, int[] blockCounts)
    {
        for (int i = 0; i < playerIDs.Length; i++)
        {
            playerUI?.AddResultEntry(playerIDs[i], blockCounts[i]);
        }
        playerUI?.SetResult();

        print($"��� �÷��̾��� �� ���� ���� �� ���� ����");
        print($"{playerIDs[0]}�� �����̹� ����� ������Դϴ�!!!");

        Time.timeScale = 0f;
    }
}
