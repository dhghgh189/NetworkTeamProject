using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMaxHeightManager : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float boxCastHeight = 20f;
    [SerializeField] private LayerMask blockLayer;      // ���� ���Ե� ���̾�
    [SerializeField] private Vector2 BoxSize;
    [SerializeField] public float highestPoint = 0f;

    [HideInInspector] public GameState gameState;
    public event Action<float> OnHeightChanged; // ���� ���� �̺�Ʈ

    private void Start()
    {
        BoxSize = new Vector2(gameState.playerWidth, 1f);
    }

    public void UpdateHighestPoint()
    {
        float currentHighest = GetHighestBlockPosition();
        if (currentHighest != highestPoint) // ���̰� ����� ��쿡�� ����
        {
            highestPoint = currentHighest;
            OnHeightChanged?.Invoke(highestPoint); // �̺�Ʈ ȣ��
        }
    }

    // Ÿ������ ���� ���� ���� ã�� �Լ�
    public float GetHighestBlockPosition()
    {
        RaycastHit2D hit = BoxCastToFindHighestBlock();

        if (hit.collider != null)
        {
            // �θ� ������Ʈ���� y���� ������ (�ڽĿ� Collider�� �ִ� ���)
            /*Transform parentTransform = hit.collider.transform.parent;
            if (parentTransform != null)
            {
                return parentTransform.position.y;
            }*/
            return hit.point.y;
        }

        return 0;  // ���� ���� ������ �ſ� ���� ���� ��ȯ(Ÿ���� ������ ���� �޾ƿ� ����)
    }

    private RaycastHit2D BoxCastToFindHighestBlock()
    {
        // Ÿ���� �߽ɿ��� �������� boxCastHeight��ŭ �÷� ����, �Ʒ� �������� ��� ���� ������ ����
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * boxCastHeight;  // Ÿ�� ������ �߻�

        // ������ �Ʒ��� BoxCast�� ���� ù ��°�� ��� ���� ã��
        RaycastHit2D hit = Physics2D.BoxCast(rayOrigin, BoxSize, 0f, Vector2.down, boxCastHeight, blockLayer);

        return hit;
    }

    private void OnDrawGizmos()
    {
        // BoxCast�� ���� ��ġ ���
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * boxCastHeight;
        Vector2 direction = Vector2.down; // �ڽ�ĳ��Ʈ ����
        float distance = boxCastHeight; // �ڽ�ĳ��Ʈ �Ÿ�

        // Gizmos ���� ����
        Gizmos.color = Color.red;

        // �浹 ����� �˻�
        RaycastHit2D hit = BoxCastToFindHighestBlock();

        if (hit.collider != null ) // �浹 �߻� ��
        {
            // �浹 �������� ������ ǥ��
            Gizmos.DrawRay(rayOrigin, direction * hit.distance);

            // �浹 ������ ��Ȯ�� �ڽ��� ǥ��
            Gizmos.DrawWireCube(hit.point, BoxSize);
        }
        else // �浹 ����
        {
            // �ִ� �Ÿ����� ������ ǥ��
            Gizmos.DrawRay(rayOrigin, direction * distance);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(highestPoint);
        }
        else
        {
            highestPoint = (float)stream.ReceiveNext();
            OnHeightChanged?.Invoke(highestPoint);
        }
    }
}
