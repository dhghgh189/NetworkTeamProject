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
    [SerializeField] Button[] modeButton;                // ��� �����ϴ� ��ư 3��
    [SerializeField] Button startButton;
    // [SerializeField] MainPanel gameMode;
    // [SerializeField] private TMP_InputField gameModeText;
    [SerializeField] private int gameScene;
    [SerializeField] PhotonView photonView;

    [Header("Mode Select")]
    [SerializeField] private bool isMode_0;           // ���1 ��ư
    [SerializeField] private bool isMode_1;           // ���2 ��ư
    [SerializeField] private bool isMode_2;           // ���3 ��ư


    private void OnEnable()
    {
        UpdatePlayer();

        // PlayerNumbering �� �÷��̾� �߰�
        PlayerNumbering.OnPlayerNumberingChanged += UpdatePlayer;

        PhotonNetwork.LocalPlayer.SetReady(false);

        isMode_0 = false;
        isMode_1 = false;
        isMode_2 = false;
    }

    private void OnDisable()
    {
        // PlayerNumbering�� �÷��̾� ����
        PlayerNumbering.OnPlayerNumberingChanged -= UpdatePlayer;
    }

    // public void GameModeUI()
    // {
    //     if(gameMode.isMode_0 == true)
    //     {
    //         gameModeText.text = "Mode 1";
    //     }
    //     else if(gameMode.isMode_1 == true)
    //     {
    //         gameModeText.text = "Mode 2";
    //     }
    //     else if (gameMode.isMode_2 == true)
    //     {
    //         gameModeText.text = "Mode 3";
    //     }
    // }

    /// <summary>
    /// ��ư1�� ���������� �Լ�
    /// </summary>
    [PunRPC]
    public void SelectModeButton1()
    {
        ColorBlock colorBlock = modeButton[0].colors;
        colorBlock.normalColor = Color.green;
        modeButton[0].colors = colorBlock;

        if (isMode_0 == true)
        {
            // ������ ���������� �� ��ư�� ���� �� �ְ� Ȱ��ȭ
            isMode_0 = false;
            modeButton[0].interactable = true;
            modeButton[1].interactable = true;
            modeButton[2].interactable = true;

            colorBlock.normalColor = Color.white;
            modeButton[0].colors = colorBlock;
            Debug.Log($"isMode_0: {isMode_0}, isMode_1: {isMode_1}, isMode_2: {isMode_2}");

            return;
        }
        // ���2, ���3 ��ư�� true ��� ���1�� ��ư�� �Ͼ��, ismode_1,2 �� false��.
        else if (isMode_1 == true || isMode_2 == true)
        {
            colorBlock.normalColor = Color.white;
            modeButton[1].colors = colorBlock;
            modeButton[2].colors = colorBlock;

            isMode_1 = false;
            isMode_2 = false;

            Debug.Log($"isMode_0: {isMode_0}, isMode_1: {isMode_1}, isMode_2: {isMode_2}");
        }
        isMode_0 = true;
        modeButton[1].interactable = false;
        modeButton[2].interactable = false;
    }

    public void SendSelectMode1()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {            
            photonView.RPC("SelectModeButton1", RpcTarget.All);
        }
    }

    public void SendSelectMode2()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {            
            photonView.RPC("SelectModeButton2", RpcTarget.All);
        }
    }

    public void SendSelectMode3()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {            
            photonView.RPC("SelectModeButton3", RpcTarget.All);
        }
    }

    /// <summary>
    /// ����ư2 �� ��������
    /// </summary>
    [PunRPC]
    public void SelectModeButton2()
    {
        ColorBlock colorBlock = modeButton[1].colors;
        colorBlock.normalColor = Color.green;
        modeButton[1].colors = colorBlock;

        if (isMode_1 == true)
        {
            isMode_1 = false;

            modeButton[0].interactable = true;
            modeButton[1].interactable = true;
            modeButton[2].interactable = true;

            colorBlock.normalColor = Color.white;
            modeButton[1].colors = colorBlock;
            return;
        }
        else if (isMode_0 == true || isMode_2 == true)
        {
            colorBlock.normalColor = Color.white;
            modeButton[0].colors = colorBlock;
            modeButton[2].colors = colorBlock;

            isMode_0 = false;
            isMode_2 = false;
        }
        isMode_1 = true;
        modeButton[0].interactable = false;
        modeButton[2].interactable = false;
        Debug.Log($"isMode_0: {isMode_0}, isMode_1: {isMode_1}, isMode_2: {isMode_2}");
    }

    /// <summary>
    /// ����ư3 �� ��������
    /// </summary>
    [PunRPC]
    public void SelectModeButton3()
    {
        ColorBlock colorBlock = modeButton[2].colors;
        colorBlock.normalColor = Color.green;
        modeButton[2].colors = colorBlock;

        if (isMode_2 == true)
        {
            isMode_2 = false;
            modeButton[0].interactable = true;
            modeButton[1].interactable = true;
            modeButton[2].interactable = true;

            colorBlock.normalColor = Color.white;

            modeButton[2].colors = colorBlock;
            return;
        }
        else if (isMode_0 == true || isMode_1 == true)
        {
            colorBlock.normalColor = Color.white;
            modeButton[0].colors = colorBlock;
            modeButton[1].colors = colorBlock;

            isMode_1 = false;
            isMode_0 = false;
        }

        isMode_2 = true;
        modeButton[0].interactable = false;
        modeButton[1].interactable = false;
        Debug.Log($"isMode_0: {isMode_0}, isMode_1: {isMode_1}, isMode_2: {isMode_2}");
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
