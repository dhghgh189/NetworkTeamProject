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
    [SerializeField] private GameState gameState;
    public float cameraSize;

    [SerializeField] private float highestPlayerHeight = 0f;
    [SerializeField] private float lowestPlayerHeight = 0f;

    private void Update()
    {
        GetHighestTowerHeight();
        ModulateCameraZoom();
        //Debug.Log($"<color=Greed>Camera Pos: {transform.position} </color>");
    }

    public void GetHighestTowerHeight()
    {
        // ���� ó��
        if (gameState == null || gameState.towerObjectDic == null) return;

        // �ʱ�ȭ
        highestPlayerHeight = float.MinValue;
        lowestPlayerHeight = float.MaxValue;

        //�÷��̾� GetCommponent

        foreach (var player in gameState.towerObjectDic.Values)
        {
            var blockMaxHeightManager = player.GetComponent<BlockMaxHeightManager>();
            if (blockMaxHeightManager != null)
            {
                float height = blockMaxHeightManager.highestPoint;

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
    }

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
