using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
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
    [SerializeField] TMP_Text roomTitle;

    //[Header("Mode Select")]
    private bool[] isMode;
    //[SerializeField] private bool isMode_0;           // ���1 ��ư
    //[SerializeField] private bool isMode_1;           // ���2 ��ư
    //[SerializeField] private bool isMode_2;           // ���3 ��ư

    private void Awake()
    {
        isMode = new bool[3];
    }

    private void OnEnable()
    {
        UpdatePlayer();

        // PlayerNumbering �� �÷��̾� �߰�
        PlayerNumbering.OnPlayerNumberingChanged += UpdatePlayer;

        PhotonNetwork.LocalPlayer.SetReady(false);
        int mode = PhotonNetwork.CurrentRoom.GetMode();

        if (PhotonNetwork.IsMasterClient == true)
        {
            for (int i = 0; i < modeButton.Length; i++)
            {
                modeButton[i].animationTriggers.highlightedTrigger = "Highlighted";
            }
            ModeSelctWrapper(mode);
        }
        else
        {
            for (int i = 0; i < modeButton.Length; i++)
            {
                print($"{i}��°: {isMode[i]}");
                modeButton[i].animationTriggers.highlightedTrigger = "";
            }
            ModeSelect(mode);
        }

        roomTitle?.SetText(PhotonNetwork.CurrentRoom.Name);

    }

    
    public void ModeSelctWrapper(int modeIndex)
    {
        if (PhotonNetwork.IsMasterClient == false) return;

        photonView.RPC("ModeSelect", RpcTarget.AllBuffered, modeIndex);
    }

    [PunRPC]
    public void ModeSelect(int modeIndex)
    {
        gameScene = modeIndex + 1;

        for (int i = 0; i < modeButton.Length; i++)
        {
            if (i == modeIndex)
            {
                isMode[modeIndex] = !isMode[modeIndex];
                PhotonNetwork.CurrentRoom.SetMode(modeIndex);
            }
            else
            {
                isMode[modeIndex] = false;
            }
        }

        for (int i = 0; i < modeButton.Length; i++)
        {
            if (i == modeIndex)
            {
                modeButton[i].animationTriggers.normalTrigger = "";
                modeButton[i].animator.CrossFade("Highlighted", 0.1f);
            }
            else
            {
                modeButton[i].animationTriggers.normalTrigger = "Normal";
                modeButton[i].animator.CrossFade("Disabled", 0.1f);
            }
        }
        print($"{PhotonNetwork.LocalPlayer.NickName}: {modeIndex.ToString()}\n {modeButton[modeIndex].animationTriggers.highlightedTrigger.ToString()}");
    }

    private void OnDisable()
    {
        // PlayerNumbering�� �÷��̾� ����
        PlayerNumbering.OnPlayerNumberingChanged -= UpdatePlayer;

        for (int i = 0; i < modeButton.Length; i++)
        {
            modeButton[i].animator.writeDefaultValuesOnDisable = true;
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
        Debug.Log($"LocalPlayer: {PhotonNetwork.LocalPlayer.NickName}");
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startButton.interactable = AllPlayerReadyCheck();
            Debug.Log($"photonView:{photonView.ViewID}");
        }
        else
        {
            startButton.interactable = false;
            //for (int i = 0; i < modeButton.Length; i++)
            //{
            //    modeButton[i].gameObject.SetActive(false);
            //}
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

    public void PlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable properties)
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
