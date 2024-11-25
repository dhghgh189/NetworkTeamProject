using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ServerStateLogger : MonoBehaviourPunCallbacks
{
    // ���� Ŭ���̾�Ʈ�� ���¸� �� �� �ִ� Ŭ����
    [SerializeField] ClientState state;

    // ���� ��Ȳ�� �˷��ִ� �ڵ�
    private void Update()
    {
        // ���°� �ٲ�� ���� Ȯ�εǸ� �α׸� ����ش�
        if (state == PhotonNetwork.NetworkClientState)
            return;

        // �������� ���� ���°� ��� ��û�ϴ� ����
        state = PhotonNetwork.NetworkClientState;
        Debug.Log($"[Pun] {state}");
    }

}
