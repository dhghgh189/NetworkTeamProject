using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMaxHeightManager : MonoBehaviour
{
    public float boxCastHeight = 20f;                      
    public LayerMask blockLayer;                     // ���� ���Ե� ���̾�
    public Vector2 BoxSize = new Vector2(5f, 1f);

    // Ÿ������ ���� ���� ���� ã�� �Լ�
    public float GetHighestBlockPosition()
    {
        RaycastHit2D hit = BoxCastToFindHighestBlock();

        if (hit.collider != null)
        {
            // �θ� ������Ʈ���� y���� ������ (�ڽĿ� Collider�� �ִ� ���)
            Transform parentTransform = hit.collider.transform.parent;
            if (parentTransform != null)
            {
                return parentTransform.position.y;
            }
        }

        return 0;  // ���� ���� ������ �ſ� ���� ���� ��ȯ(Ÿ���� ������ ���� �޾ƿ� ����)
    }

    private RaycastHit2D BoxCastToFindHighestBlock()
    {
        // Ÿ���� �߽ɿ��� �������� BoxCast�� ��� ���� ������ ����
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * boxCastHeight;  // Ÿ�� ������ �߻�

        // ������ �Ʒ��� BoxCast�� ���� ù ��°�� ��� ���� ã��
        RaycastHit2D hit = Physics2D.BoxCast(rayOrigin, BoxSize, 0f, Vector2.down, boxCastHeight, blockLayer);

        return hit;
    }

    private void OnDrawGizmos()
    {
        // BoxCast�� ���� ��ġ ��� (���ʿ��� ����)
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * boxCastHeight;

        // Gizmos�� BoxCast�� ��θ� �ð�ȭ�Ͽ� �����
        Gizmos.color = Color.red;

        // BoxCast�� ũ�⿡ ���缭 �ڽ��� �׷���
        // "rayOrigin"�� BoxCast�� �����ϴ� ��ġ, Size�� �ڽ��� ũ��
        Gizmos.DrawWireCube(rayOrigin + Vector2.down * boxCastHeight / 2, BoxSize);
    }

    private void Update()
    {
        float highestBlockY = GetHighestBlockPosition();
        Debug.Log("���� ���� ���� y��: " + highestBlockY);
    }
}
