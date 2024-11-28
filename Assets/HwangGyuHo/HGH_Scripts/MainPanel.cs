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
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject createRoomPanel;
    [SerializeField] Button[] modeButton;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField maxPlayerInputField;

    [Header("��弱�� ��ư")]
    [SerializeField] private bool isColored_0;
    [SerializeField] private bool isColored_1;
    [SerializeField] private bool isColored_2;

    ColorBlock colorBlock = new ColorBlock();

    /// <summary>
    /// ������� �ο��� ���ϴ� �г��� OFF
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

    public void CreateRoomConfirm()
    {
        Debug.Log("�� ����� �����߽��ϴ�.");
        string roomName = roomNameInputField.text;
        if (roomName == "")
        {
            Debug.LogWarning("�� �̸��� �Է��ؾ� ���� ������ �� �ֽ��ϴ�.");
            return;
        }
        int maxPlayer = int.Parse(maxPlayerInputField.text);
        maxPlayer = Mathf.Clamp(maxPlayer,1,4);

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = maxPlayer;
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void CreateRoomCancel()
    {
        Debug.Log("�� ����⸦ ����߽��ϴ�");
        createRoomPanel.SetActive(false);
    }

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

    public void JoinLobby()
    {
        Debug.Log("�κ� ���� ��û");
        PhotonNetwork.JoinLobby();
    }

    public void Logout()
    {
        Debug.Log("�α׾ƿ� ��û");
        PhotonNetwork.Disconnect();
    }

    public void SelectModeButton1()
    {
        if (isColored_0 == true)
        {
            colorBlock.selectedColor = Color.white;
            colorBlock.normalColor = Color.white;
            colorBlock.highlightedColor = Color.white;

            modeButton[0].colors = colorBlock;
            colorBlock.colorMultiplier = 5;

            isColored_0 = false;

            return;
        }
        else if (isColored_1 == true || isColored_2 == true)
        {
            modeButton[0].interactable = false;
            return;
        }
        
        isColored_0 = true;
        ButtonColor();
    }

    public void SelectModeButton2()
    {
        if (isColored_1 == true)
        {
            isColored_1 = false;
            colorBlock.selectedColor = Color.white;
            colorBlock.normalColor = Color.white;
            colorBlock.highlightedColor = Color.white;

            modeButton[1].colors = colorBlock;
            colorBlock.colorMultiplier = 5;

            return;
        }
        else if (isColored_0 == true || isColored_2 == true)
        {
            modeButton[1].interactable = false;
            return;
        }

        isColored_1 = true;
        ButtonColor();
    }
    public void SelectModeButton3()
    {
        if (isColored_2 == true)
        {
            colorBlock.selectedColor = Color.white;
            colorBlock.normalColor = Color.white;
            colorBlock.highlightedColor = Color.white;

            modeButton[2].colors = colorBlock;
            colorBlock.colorMultiplier = 5;

            isColored_2 = false;

            return;
        }
        else if (isColored_0 == true || isColored_1 == true)
        {
            modeButton[2].interactable = false;
            return;
        }

        isColored_2 = true;
        ButtonColor();
    }


    public void ButtonColor()
    {
        ColorBlock colorBlock = new ColorBlock();
        colorBlock.selectedColor = Color.green;
        colorBlock.normalColor = Color.green;
        colorBlock.highlightedColor = Color.green;
        colorBlock.colorMultiplier = 3;

        if (isColored_0 == true)
        {
            modeButton[0].colors = colorBlock;
        }
        else if (isColored_1 == true)
        {
            modeButton[1].colors= colorBlock;
            
        }
        else if (isColored_2 == true)
        {
            modeButton[2].colors= colorBlock;
        }
    }

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
