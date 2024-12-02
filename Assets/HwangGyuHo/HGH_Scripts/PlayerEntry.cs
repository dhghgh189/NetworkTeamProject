using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text readyText;
    [SerializeField] Toggle readyToggle;

    public void SetPlayer(Player player)
    {
        if (player.IsMasterClient)
        {
            // �����̸� �̷��� ǥ�õǰ�
            nameText.text = $"zZ{player.NickName}Zz";
        }
        else
        {
            // �濡 ������
            // �г����� nameText.text��
            nameText.text = player.NickName;
        }

        // �����ư�� Ȱ��ȭ ��Ű��
        readyToggle.gameObject.SetActive(true);
        // ��ȣ�ۿ��� ��ü�� �ڱ��ڽ�, LocalPlayer�϶� ����
        readyToggle.interactable = player == PhotonNetwork.LocalPlayer;

        if (player.GetReady())
        {
            readyText.text = "Ready";
        }
        else
        {
            readyText.text = "";
        }
    }

    public void SetEmpty()
    {
        readyText.text = "";
        nameText.text = "None";
        readyToggle.gameObject.SetActive(false);
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
