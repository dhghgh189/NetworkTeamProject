using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoginPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField idInputField;

    private void Start()
    {
        idInputField.text = $"Player{Random.Range(1000,10000)}";
    }

    public void Login()
    {
        Debug.Log("�α��� ��û");
        if (idInputField.text == "")
        {
            Debug.LogWarning("���̵� �Է��ϼž� ������ �����մϴ�");
        }
        
        PhotonNetwork.LocalPlayer.NickName = idInputField.text;
        PhotonNetwork.ConnectUsingSettings();
    }
}
