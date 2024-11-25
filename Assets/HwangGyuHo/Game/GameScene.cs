using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : MonoBehaviourPunCallbacks
{
    [SerializeField] Button gameoverButton;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameoverButton.interactable = true;
        }
        else
        {
            gameoverButton.interactable = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("HGH_Scene");
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("HGH_Scene");
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameoverButton.interactable = true;
        }
        else
        {
            gameoverButton.interactable = false;
        }
    }
    public void GameOver()
    {
        // �ڽ��� ������ �ƴ϶�� ������Ѵ�
        if (PhotonNetwork.IsMasterClient == false)
            return;

        // ������ ������ �� ������ ���� �� �ְ� �Ѵ�
        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.LoadLevel("HGH_Scene");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
