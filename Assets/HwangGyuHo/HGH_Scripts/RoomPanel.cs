using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class RoomPanel : MonoBehaviour
{
    [SerializeField] PlayerEntry[] playerEntries;
    [SerializeField] Button startButton;
    [SerializeField] MainPanel gameMode;
    [SerializeField] TMP_InputField gameModeText;
    [SerializeField] private int gameScene;


    private void OnEnable()
    {
        UpdatePlayer();

        // PlayerNumbering �� �÷��̾� �߰�
        PlayerNumbering.OnPlayerNumberingChanged += UpdatePlayer;

        PhotonNetwork.LocalPlayer.SetReady(false);
    }

    private void OnDisable()
    {
        // PlayerNumbering�� �÷��̾� ����
        PlayerNumbering.OnPlayerNumberingChanged -= UpdatePlayer;
    }

    public void GameModeUI()
    {
        if(gameMode.isMode_0 == true)
        {
            gameModeText.text = "Mode 1";
        }
        else if(gameMode.isMode_1 == true)
        {
            gameModeText.text = "Mode 2";
        }
        else if (gameMode.isMode_2 == true)
        {
            gameModeText.text = "Mode 3";
        }
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

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startButton.interactable = AllPlayerReadyCheck();
        }
        else
        {
            startButton.interactable = false;
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
        // ���� Ŀ���� ������Ƽ�� ������ ���� RREADY Ű�� ����
        // TODO : �÷��̾� �Ӽ��� �ٲ�� �װ��� ������Ʈ
        Debug.Log($"{targetPlayer.NickName} ��������!!");
        if (properties.ContainsKey(CustomPropert.READY))
        {
            UpdatePlayer();
        }
    }

    public void StartGame()
    {
        // �÷��̾�� READY�� ��� �Ǹ� ���ӽ��� ��ư���� ���ӽ���
        // �� �̸��� ��ġ�ؾ� ��
        PhotonNetwork.LoadLevel(gameScene);
        // ���� �÷��� �߿��� �濡 ���� �� ����
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    public void LeaveRoom()
    {
        Debug.Log("���� �������ϴ�");
        PhotonNetwork.LeaveRoom();
    }

    public bool AllPlayerReadyCheck()
    {
        // ��� �÷��̾��� ���� üũ
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetReady() == false)
                return false;
        }
        return true;
    }
}
