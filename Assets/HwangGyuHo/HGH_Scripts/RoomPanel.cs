using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class RoomPanel : MonoBehaviour
{
    [SerializeField] PlayerEntry[] playerEntries;
    [SerializeField] Button startButton;


    private void OnEnable()
    {
        // PlayerNumbering �� �÷��̾� �߰�
        PlayerNumbering.OnPlayerNumberingChanged += UpdatePlayer;
        PhotonNetwork.LocalPlayer.SetReady(false);
    }

    private void OnDisable()
    {
        // PlayerNumbering�� �÷��̾� ����
        PlayerNumbering.OnPlayerNumberingChanged -= UpdatePlayer;
    }

    public void UpdatePlayer()
    {
       
        foreach (PlayerEntry entry in playerEntries)
        {
            entry.SetEmpty();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetPlayerNumber() == -1)
                return;

            int number = player.GetPlayerNumber();
            playerEntries[number].SetPlayer(player);
        }
    }

    public void EnterPlayer(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} ����!");
        UpdatePlayer();
    }

    public void ExitPlayer(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} ����!");
        UpdatePlayer();
    }

    public void PlayerPropertiesUpdate(Player targetPlayer, Hashtable properties)
    {
        // TODO : �÷��̾� �Ӽ��� �ٲ�� �װ��� ������Ʈ
        Debug.Log($"{targetPlayer.NickName} ��������!!");
    }

    public void StartGame()
    {
        // TODO : �÷��̾�� READY�� ��� �Ǹ� ���ӽ��� ��ư���� ���ӽ���
    }

    public void LeaveRoom()
    {
        Debug.Log("���� �������ϴ�");
        PhotonNetwork.LeaveRoom();
    }

    public void AllPlayerReadyCheck()
    {
        // TODO : ��� �÷��̾��� ���� üũ
    }
}
