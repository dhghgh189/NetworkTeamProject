using Photon.Pun;
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
    private Action<int> hpAction;
    private Action<int> blockCountAction;
    private List<int> FinishPlayers;

    protected override void OnEnable()
    {
        base.OnEnable();
        FinishPlayers = new List<int>();

        if (photonView.IsMine == false) return;

        // ���� �ڽŸ� �̺�Ʈ ���
        var controller = selfPlayer.GetComponent<PlayerController>();
        hpAction = (newHP) => PlayerHPHandle(newHP);
        controller.OnChangeHp += hpAction;
        blockCountAction = (newBlockCount) => PlayerBlockCountHandle(newBlockCount);
        //controller.OnChangeBlockCount += blockCountAction;
    }

    public override void Exit()
    {
        // winRoutine�� ����ǰ� �ִ� ��쿡�� �ش� �ڷ�ƾ�� ����
        StopCoroutine(winRoutine);
        var controller = selfPlayer.GetComponent<PlayerController>();
        controller.OnChangeHp -= hpAction;
        //controller.OnChangeBlockCount -= blockCountAction;

        base.Exit();
    }

    public void PlayerHPHandle(int newHP)
    {
        // �ڽ� �̺�Ʈ�� ��쿡�� ȣ��
        if (photonView.IsMine == false) return;

        if (newHP < 1)
        {
            if (selfPlayer.TryGetComponent<PlayerController>(out var controller))
            {
                controller.IsGoal = true;
                // �÷��̾� ��Ʈ�ѷ� ��, ���� ���� ������Ʈ �� �ڵ� �߰�
                print($"{photonView.Owner.NickName}���� ���� ����� ��� �����Ǿ� ���ӿ����Ǿ����ϴ�.");
                photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient, false, photonView.Owner.ActorNumber);
            }
        }
    }


    public void PlayerBlockCountHandle(int newBlockCount)
    {
        // �ڽ� �̺�Ʈ�� ��쿡�� ȣ��
        if (photonView.IsMine == false) return;

        // ��ǥ �� ������ŭ �׿��� ��, �¸� ��ƾ ����
        if (newBlockCount >= winBlockCount
            && winRoutine == null)
        {
            winRoutine = StartCoroutine(FinishRoutine(photonView.Owner.ActorNumber));
        }
        else
        {
            //����ǰ� �ִ� �¸� ��ƾ�� �����ϸ�, �ش� ��ƾ ����
            if (winRoutine != null)
            {
                StopFinishRoutine(winRoutine);
            }
        }

    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));

        // ���� �ð��� ������
        // �ش� �÷��̾�� �� �̻� ���� �Ұ�
        print($"{playerID}�� ���� ������ �� �����ϴ�.");

        // ��� �÷��̾� ���� üũ ��, ���� ����
        photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient, true, photonView.Owner.ActorNumber);
    }

    [PunRPC]
    private void AllPlayerStateCheck(bool isFinishGame,int playerID = -1)
    {
        // ��ųʸ����� �ʱ�ȭ�� �÷��̾ �ִ� ���, ������ �ʱ�ȭ ����
        if (playerID > -1)
        {
            // ������ Dic�� ��Ͽ��� �ش� �÷��̾� �߰�
            FinishPlayers.Add(playerID);
        }

        if (isFinishGame)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();
            int score = 0;
            for (score = 0; score < FinishPlayers.Count; score++)
            {
                result.Add(new Tuple<int, int>(FinishPlayers[score], score + 1));
            }
            //������������ �� ���� ����
            result.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            //result.ForEach((x) => {
            //    playerUI?.SetResultEntry(x.Item1.ToString(), x.Item2);
            //    playerUI?.SetResult();
            //});
            print($"��� �÷��̾��� �� ���� ���� �� ���� ����");
            print($"{result[0].Item1}�� �����̹� ����� ������Դϴ�!!!");

            var players = new int[result.Count];
            var blockCounts = new int[result.Count];
            for (int i = 0; i < result.Count; i++)
            {
                players[i] = result[i].Item1;
                blockCounts[i] = result[i].Item2;
            }
            Time.timeScale = 0f;

            // UI ������Ʈ �۾� �� ���� ��������� ��� Ŭ���̾�Ʈ ����
            photonView.RPC("UpdateUI", RpcTarget.All, players, blockCounts);
        }
        else
        {
            if (FinishPlayers.Count < playerObjectDic.Count)
            {
                List<Tuple<int, int>> result = new List<Tuple<int, int>>();
                foreach (var id in playerObjectDic.Keys)
                {
                    //if (playerObjectDic[id].TryGetComponent<BlockCountManager>(out var manager))
                    //{
                    //    result.Add(new Tuple<int, int>(id manager.BlockCount));
                    //}

                    //�׽�Ʈ �ڵ�
                    result.Add(new Tuple<int, int>(id, id));
                }
                //������������ �� ���� ����
                result.Sort((x, y) => y.Item2.CompareTo(x.Item2));

                //�� Ŭ���̾�Ʈ����, ������ UI�� �ش� ���� �ݿ��ǵ��� ����
                print($"��� �÷��̾��� �� ���� ���� �� ���� ����");
                print($"������ ����� ������.. {result[0].Item1}�� �����̹� ����� ������Դϴ�!!!");

                var players = new int[result.Count];
                var blockCounts = new int[result.Count];
                for (int i = 0; i < result.Count; i++)
                {
                    players[i] = result[i].Item1;
                    blockCounts[i] = result[i].Item2;
                }
                Time.timeScale = 0f;

                // UI ������Ʈ �۾� �� ���� ��������� ��� Ŭ���̾�Ʈ ����
                photonView.RPC("UpdateUI", RpcTarget.All, players, blockCounts);
            }
        }
    }
}
