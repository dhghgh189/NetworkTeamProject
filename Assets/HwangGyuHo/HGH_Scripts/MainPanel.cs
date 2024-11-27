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

    private bool isColored_0;
    private bool isColored_1;
    private bool isColored_2;
    
    /// <summary>
    /// ������� �ο��� ���ϴ� �г��� OFF
    /// </summary>
    private void OnEnable()
    {
        createRoomPanel.SetActive(false);
        isColored_0 = false;
        isColored_1 = false;
        isColored_2 = false;

        modeButton[0].gameObject.SetActive(true);
        modeButton[1].gameObject.SetActive(true);
        modeButton[2].gameObject.SetActive(true);
    }

    /// <summary>
    /// 
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
        isColored_0 = true;
        ButtonColor();
    }

    public void SelectModeButton2()
    {
        isColored_1 = true;
        ButtonColor();
    }
    public void SelectModeButton3()
    {
        isColored_2 = true;
        ButtonColor();
    }


    public void ButtonColor()
    {
        ColorBlock colorBlock = new ColorBlock();
        colorBlock.selectedColor = Color.green;
        colorBlock.colorMultiplier = 3;
        if (isColored_0)
        {

            modeButton[0].colors = colorBlock;
            modeButton[1].gameObject.SetActive(false);
            modeButton[2].gameObject.SetActive(false);
        }else if (isColored_1)
        {
            modeButton[1].colors= colorBlock;
            modeButton[0].gameObject.SetActive(false);
            modeButton[2].gameObject.SetActive(false);
        }else if (isColored_2)
        {
            modeButton[2].colors= colorBlock;
            modeButton[0].gameObject.SetActive(false);
            modeButton[1].gameObject.SetActive(false);
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
