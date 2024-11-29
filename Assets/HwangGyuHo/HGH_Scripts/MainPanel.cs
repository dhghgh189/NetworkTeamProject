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
    [SerializeField] TMP_InputField roomNameInputField;  // ���̸� ���� InputField
    [SerializeField] TMP_InputField maxPlayerInputField; // �ִ� �÷��̾�� ���� InputField

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