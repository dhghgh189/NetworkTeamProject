using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera mainCamera;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float maxZoomOut = 10f;
    [SerializeField] private float minZoomIn = 5f;
    [SerializeField] private List<Player> players;
    [SerializeField] public float cameraSize;

    private void Update()
    {
        ModulateCameraZoom();
    }

    private void ModulateCameraZoom()
    {
        float highestPlayerHeight = 10f;
        float lowestPlayerHeight = 1f;

        // ���� ���̰� ������ ����, ũ�� �ܾƿ�
        float heightDifference = highestPlayerHeight - lowestPlayerHeight;
        float targetZoom = Mathf.Lerp(minZoomIn, maxZoomOut, heightDifference / 10f); // ���̿� ����Ͽ� ��

        // ���� ī�޶� ���� ��ǥ ���� �°� õõ�� ��ȭ
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        
        // ���� ī�޶� ����� ������ ����
        cameraSize = Camera.main.orthographicSize;
    }
}
