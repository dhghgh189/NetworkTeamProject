using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraColliderController : MonoBehaviour
{
    public Camera mainCamera; // ī�޶� Inspector���� �Ҵ��ϰų� �ڵ����� ã���ϴ�.

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // ī�޶� �Ҵ���� �ʾ����� MainCamera�� �ڵ����� ã���ϴ�.
        }

        if (mainCamera == null)
        {
            Debug.LogError("���� ī�޶� ��ϵ��� �ʾҽ��ϴ�.");
            return;
        }
    }

    private void Update()
    {
        SetColliderToCamera();
    }

    private void SetColliderToCamera()
    {
        // ī�޶� ũ�� ���
        if (!mainCamera.orthographic)
        {
            Debug.LogError("ī�޶� Projection�� Orthographic�� �ƴմϴ�.");
            return;
        }

        float cameraHeight = mainCamera.orthographicSize * 2f; // ī�޶��� ���� ũ��
        float cameraWidth = cameraHeight * mainCamera.aspect;  // ī�޶��� ���� ũ��

        // �ڽ� ������Ʈ�� �ִ� �ݶ��̴� ��������
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogError("�� ������Ʈ�� Collider2D ������Ʈ�� �����ϴ�.");
            return;
        }

        boxCollider.size = new Vector2(cameraWidth, cameraHeight);


    }
}
