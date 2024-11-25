using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : MonoBehaviourPunCallbacks
{
    public enum Panel { Login, Menu, Lobby, Room }

    [SerializeField] LoginPanel loginPanel;
    [SerializeField] MainPanel menuPanel;
    [SerializeField] RoomPanel roomPanel;
    [SerializeField] LobbyPanel lobbyPanel;

    private void Start()
    {
        SetActivePanel(Panel.Login);
    }
    /// <summary>
    /// ������ ������ ������ ����ش޶�� ��û�� ���� ���� ����
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("���ӿ� �����ߴ�!");
        SetActivePanel(Panel.Menu);
    }

    /// <summary>
    /// ���� ������ �������� �� ������ ����
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"������ �����. cause : {cause}");
        SetActivePanel(Panel.Login);
    }

    /// <summary>
    /// �� ���� ��û�� ���� ����
    /// </summary>
    public override void OnCreatedRoom()
    {
        Debug.Log("�� ���� ����");
    }

    /// <summary>
    /// �� ������ �������� �� �����ִ� ����
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"�� ���� ����, ���� : {message}");
    }

    /// <summary>
    /// ���� �����ϴµ� ���������� ������ ����
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log("�� ���� ����");
        SetActivePanel(Panel.Room);
    }

    /// <summary>
    /// �ٸ� �÷��̾ ������ �������� �� ������ ����
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomPanel.EnterPlayer(newPlayer);
    }

    /// <summary>
    /// �ٸ� �÷��̾ ���� �������� ������ ����
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomPanel.ExitPlayer(otherPlayer);
    }

    /// <summary>
    /// �÷��̾��� ���°� ��������� �� ������ �ٸ� �÷��̾�� ����ȭ ��Ű�� ����
    /// </summary>
    /// <param name="targetPlayer"></param>
    /// <param name="changedProps"></param>
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        roomPanel.PlayerPropertiesUpdate(targetPlayer, changedProps);
    }

    // public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    // {
    //    
    // }

    /// <summary>
    /// ���� �����ϴµ� ���������� ������ ����
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"�� ���� ����, ���� : {message}");
    }

    /// <summary>
    /// ������Ī�� ���������� ������ ����
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"���� ��Ī ����, ���� : {message}");
    }

    /// <summary>
    /// �ڽ��� ���� �������� ������ ����
    /// </summary>
    public override void OnLeftRoom()
    {
        Debug.Log("�� ���� ����");
        SetActivePanel(Panel.Menu);
    }

    /// <summary>
    /// �κ�� �������� ������ ����
    /// </summary>
    public override void OnJoinedLobby()
    {
        Debug.Log("�κ� ���� ����");
        SetActivePanel(Panel.Lobby);
    }

    /// <summary>
    /// �κ� �������� ������ ����
    /// </summary>
    public override void OnLeftLobby()
    {
        Debug.Log("�κ� ���� ����");
        lobbyPanel.ClearRoomEntries();
        SetActivePanel(Panel.Menu);
    }

    /// <summary>
    /// �� ����Ʈ�� ������ �־����� ������ ����
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // ���� ����� ������ �ִ� ��� �������� ������ ������
        // ���ǻ���
        // 1. ó�� �κ� ���� �� : ��� �� ����� ����
        // 2. ���� �� �� ����� ����Ǵ� ��� : ����� �� ��ϸ� ����
        lobbyPanel.UpdateRoomList(roomList);
    }

    /// <summary>
    /// ������ �������� �� ������ �ΰ�޴� ����
    /// </summary>
    /// <param name="newMasterClient"></param>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"{newMasterClient.NickName} �÷��̾ ������ �Ǿ����ϴ�.");

         // PhotonNetwork.SetMasterClient(newMasterClient); �� ������� ���� �ֱ�, ������ Ŭ���̾�Ʈ�� �ȴ�
    }

    /// <summary>
    /// Ȱ��ȭ ��ų���� �г��� ������ Ȱ��ȭ �����ְ�, �ȸ����� ��Ȱ��ȭ �����ִ� �Լ�
    /// </summary>
    /// <param name="panel"></param>
    private void SetActivePanel(Panel panel)
    {
        loginPanel.gameObject.SetActive(panel == Panel.Login);
        menuPanel.gameObject.SetActive(panel == Panel.Menu);
        roomPanel.gameObject.SetActive(panel == Panel.Room);
        lobbyPanel.gameObject.SetActive(panel == Panel.Lobby);
    }
}
