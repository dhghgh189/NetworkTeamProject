using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine.UI;
using UnityEditor;
using Unity.VisualScripting;

public class MainPanel : MonoBehaviour
{
    [SerializeField] GameObject menuPanel;               // �޴��г� ������Ʈ
    [SerializeField] GameObject createRoomPanel;         // �游��� ������Ʈ
    [SerializeField] Button[] modeButton;                // ��� �����ϴ� ��ư 3��
    [SerializeField] TMP_InputField roomNameInputField;  // ���̸� ���� InputField
    [SerializeField] TMP_InputField maxPlayerInputField; // �ִ� �÷��̾�� ���� InputField

    [Header("��弱��")]
    [SerializeField] public bool isMode_0;           // ���1 ��ư�� ����
    [SerializeField] public bool isMode_1;           // ���2 ��ư�� ����
    [SerializeField] public bool isMode_2;           // ���3 ��ư�� ����



    /// <summary>
    /// �� �����â ON
    /// </summary>
    private void OnEnable()
    {
        createRoomPanel.SetActive(false);
    }

    /// <summary>
    /// �� ����� â �޴�
    /// </summary>
    public void CreateRoomMenu()
    {
        createRoomPanel.SetActive(true);
        roomNameInputField.text = "";
        maxPlayerInputField.text = $"4";
    }

    /// <summary>
    /// �� �����
    /// </summary>
    public void CreateRoomConfirm()
    {
        if (isMode_0 == true)
        {
            Debug.Log("�� ����� �����߽��ϴ�.");
            string roomName = roomNameInputField.text;
            if (roomName == "")
            {
                Debug.LogWarning("�� �̸��� �Է��ؾ� ���� ������ �� �ֽ��ϴ�.");
                return;
            }
            int maxPlayer = int.Parse(maxPlayerInputField.text);
            maxPlayer = Mathf.Clamp(maxPlayer, 1, 4);

            RoomOptions options = new RoomOptions();
            options.MaxPlayers = maxPlayer;
            PhotonNetwork.CreateRoom(roomName, options);
        }
        else if (isMode_1 == true)
        {
            Debug.Log("�� ����� �����߽��ϴ�.");
            string roomName = roomNameInputField.text;
            if (roomName == "")
            {
                Debug.LogWarning("�� �̸��� �Է��ؾ� ���� ������ �� �ֽ��ϴ�.");
                return;
            }
            int maxPlayer = int.Parse(maxPlayerInputField.text);
            maxPlayer = Mathf.Clamp(maxPlayer, 1, 4);

            RoomOptions options = new RoomOptions();
            options.MaxPlayers = maxPlayer;
            PhotonNetwork.CreateRoom(roomName, options);
        }
        else if (isMode_2 == true)
        {
            Debug.Log("�� ����� �����߽��ϴ�.");
            string roomName = roomNameInputField.text;
            if (roomName == "")
            {
                Debug.LogWarning("�� �̸��� �Է��ؾ� ���� ������ �� �ֽ��ϴ�.");
                return;
            }
            int maxPlayer = int.Parse(maxPlayerInputField.text);
            maxPlayer = Mathf.Clamp(maxPlayer, 1, 4);

            RoomOptions options = new RoomOptions();
            options.MaxPlayers = maxPlayer;
            PhotonNetwork.CreateRoom(roomName, options);
        }
        else
            return;
    }

    /// <summary>
    /// �� ���鶧 ����ϱ�
    /// </summary>
    public void CreateRoomCancel()
    {
        Debug.Log("�� ����⸦ ����߽��ϴ�");
        createRoomPanel.SetActive(false);
    }

    /// <summary>
    /// ������Ī
    /// </summary>
    public void RandomMatching()
    {
        Debug.Log("������Ī ��û");
        // ����ִ� ���� ������ ���� �ʴ� ���
        // PhotonNetwork.JoinRandomRoom();

        // ����ִ� ���� ������ ���� ���� ���� ���� ���
        // �׷��� ��� ���� ����� ���� ���뵵 ����� �Ѵ�
        string Name = $"Room {Random.Range(1000, 10000)}";
        RoomOptions options = new RoomOptions() { MaxPlayers = 4 };
        PhotonNetwork.JoinRandomOrCreateRoom(roomName: name, roomOptions: options);
    }

    /// <summary>
    /// �κ� ����
    /// </summary>
    public void JoinLobby()
    {
        Debug.Log("�κ� ���� ��û");
        PhotonNetwork.JoinLobby();
    }

    /// <summary>
    /// �α׾ƿ�
    /// </summary>
    public void Logout()
    {
        Debug.Log("�α׾ƿ� ��û");
        PhotonNetwork.Disconnect();
    }

    /// <summary>
    /// ��ư1�� ���������� �Լ�
    /// </summary>
    public void SelectModeButton1()
    {
        ColorBlock colorBlock = modeButton[0].colors;
        if (isMode_0 == true)
        {
            // ������ ���������� �� ��ư�� ���� �� �ְ� Ȱ��ȭ
            isMode_0 = false;
            modeButton[0].interactable = true;
            modeButton[1].interactable = true;
            modeButton[2].interactable = true;

            colorBlock.normalColor = Color.white;

            modeButton[0].colors = colorBlock;
            return;
        }
        else if (isMode_1 == true || isMode_2 == true)
        {
            colorBlock.normalColor = Color.white;
            colorBlock.highlightedColor = Color.white;
            modeButton[0].colors = colorBlock;
        }

        isMode_1 = false;
        isMode_2 = false;
        isMode_0 = true;
    }

    /// <summary>
    /// ����ư2 �� ��������
    /// </summary>
    public void SelectModeButton2()
    {
        ColorBlock colorBlock = modeButton[1].colors;
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
            colorBlock.highlightedColor = Color.white;
            modeButton[1].colors = colorBlock;
        }

        isMode_0 = false;
        isMode_2 = false;
        isMode_1 = true;
    }

    /// <summary>
    /// ����ư3 �� ��������
    /// </summary>
    public void SelectModeButton3()
    {
        ColorBlock colorBlock = modeButton[2].colors;
        if (isMode_2 == true)
        {
            isMode_2 = false;
            modeButton[0].interactable = true;
            modeButton[1].interactable = true;
            modeButton[2].interactable = true;

            colorBlock.normalColor = Color.white;
            colorBlock.highlightedColor = Color.white;
            modeButton[2].colors = colorBlock;
            return;
        }
        else if (isMode_0 == true || isMode_1 == true)
        {
            colorBlock.normalColor = Color.white;
            colorBlock.highlightedColor = Color.green;
            modeButton[2].colors = colorBlock;
        }

        isMode_1 = false;
        isMode_0 = false;
        isMode_2 = true;
    }

    /// <summary>
    /// ����ID�� ���̾�̽����� �����ϴ� �Լ�
    /// </summary>
    public void DeleteUser()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;
        user.DeleteAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("DeleteAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("DeleteAsync encountered an error: " + task.Exception);
                return;
            }

            Debug.Log("User deleted successfully.");
            PhotonNetwork.Disconnect();
        });
    }
}