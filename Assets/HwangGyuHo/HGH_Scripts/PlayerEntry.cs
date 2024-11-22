using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text readyText;
    [SerializeField] Button readyButton;

    public void SetPlayer(Player player)
    {
        // �濡 ������
        // �г����� nameText.text��
        player.NickName = nameText.text;
        // �����ư�� Ȱ��ȭ ��Ű��
        readyText.gameObject.SetActive(true);
        // ��ȣ�ۿ��� ��ü�� �ڱ��ڽ�, LocalPlayer�϶� ����
        readyButton.interactable = player == PhotonNetwork.LocalPlayer;
    }

    public void SetEmpty()
    {
        readyText.text = "";
        nameText.text = "None";
        readyButton.gameObject.SetActive(false);
    }

    public void Ready()
    {
        bool ready = PhotonNetwork.LocalPlayer.GetReady();
        if (ready)
        {
            PhotonNetwork.LocalPlayer.SetReady(false);
        }
        else
        {
            PhotonNetwork.LocalPlayer.SetReady(true);
        }
    }
}
