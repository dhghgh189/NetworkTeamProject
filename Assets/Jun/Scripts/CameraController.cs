using Photon.Pun.Demo.PunBasics;
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

    [SerializeField] private float highestPlayerHeight = 5f;
    [SerializeField] private float lowestPlayerHeight = 5f;

    private void Update()
    {
        ModulateCameraZoom();
    }

    /*public void GetHighestTowerHeight()
    {
        // �ʱ�ȭ
        highestPlayerHeight = float.MinValue;
        lowestPlayerHeight = float.MaxValue;

        foreach (var player in PlayerDic.Values)
        {
            if (player.BlockMaxHeightManager != null)
            {
                float height = player.BlockMaxHeightManager.highestPoint;

                if (height > highestPlayerHeight)
                {
                    highestPlayerHeight = height;
                }
                if ( height < lowestPlayerHeight)
                {
                    lowestPlayerHeight = height;
                }
            }
        }
    }*/

    private void ModulateCameraZoom()
    {
        // ���� ���̰� ������ ����, ũ�� �ܾƿ�
        float heightDifference = highestPlayerHeight - lowestPlayerHeight;
        float targetZoom = Mathf.Lerp(minZoomIn, maxZoomOut, heightDifference / 10f); // ���̿� ����Ͽ� ��

        // ���� ī�޶� ���� ��ǥ ���� �°� õõ�� ��ȭ
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        
        // ���� ī�޶� ����� ������ ����
        cameraSize = Camera.main.orthographicSize;
    }
}
