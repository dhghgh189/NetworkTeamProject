using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject player;         // ��ȯ�� �÷��̾�
    [SerializeField] private BlockMaxHeightManager blockMaxHeightManager; // BlockMaxHeightManager�� ����

    private float previousHighestBlockY = 0f;  // ���� �ִ� ����(ó�� ������ ��ġ�� ������ ����)

    private void Update()
    {
        if (player != null)
        {
            // ���� ���� ���� y���� BlockMaxHeightManager���� ��������
            float currentHighestBlockY = blockMaxHeightManager.GetHighestBlockPosition();

            // �ִ� ���̰� ����Ǿ��� ���� ��ġ ������Ʈ
            if (currentHighestBlockY != previousHighestBlockY)
            {
                SetPlayerPosition(currentHighestBlockY);  // ��ġ ������Ʈ
                previousHighestBlockY = currentHighestBlockY; // ���� ���� ����
            }
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
