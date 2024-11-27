using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject player;         // ��ȯ�� �÷��̾�
    [SerializeField] private BlockMaxHeightManager blockMaxHeightManager; // BlockMaxHeightManager�� ����

    private float previousHighestBlockY = 0f;  // ���� �ִ� ����(ó�� ������ ��ġ�� ������ ����)

    public void SetMaxHeightManager(BlockMaxHeightManager blockMaxHeightManager)
    {
        this.blockMaxHeightManager = blockMaxHeightManager;
        if (blockMaxHeightManager != null)
        {
            blockMaxHeightManager.OnHeightChanged += SetPlayerPosition; // �̺�Ʈ ����
        }
    }


    private void Start()
    {

    }

    private void OnDestroy()
    {
        if (blockMaxHeightManager != null)
        {
            blockMaxHeightManager.OnHeightChanged -= SetPlayerPosition; // �̺�Ʈ ���� ����
        }
    }

    // �÷��̾� ��ġ�� ����� �ִ� ���̿� �°� �����ϴ� �Լ�
    public void SetPlayerPosition(float highestPoint)
    {
        // �÷��̾��� ��ġ�� ���� ���� ��� ��ġ�� ����
        Vector2 playerNewPosition = new Vector2(player.transform.position.x, highestPoint);

        // �÷��̾��� ��ġ ������Ʈ
        player.transform.position = playerNewPosition;
    }
}
