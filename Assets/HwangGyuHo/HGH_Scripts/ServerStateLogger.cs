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
        // ���°� �ٲ� ��Ȳ������ Ȯ���ϴ� �ڵ�
        // ���°� �ȹٲ���ٸ� Ȯ�ξ��Ѵ�
        // Update �����Ӹ��� Ȯ���� ���� �����ϱ�
        // �α׸� ����ִ� �ڵ�
        if (state == PhotonNetwork.NetworkClientState)
            return;

        // �������� ���� ���°� ��� ��û�ϴ� ����
        state = PhotonNetwork.NetworkClientState;
        Debug.Log($"[Pun] {state}");
    }

}
