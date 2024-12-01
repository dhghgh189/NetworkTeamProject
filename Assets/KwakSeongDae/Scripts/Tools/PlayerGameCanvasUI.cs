using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerGameCanvasUI : MonoBehaviour
{
    [Header("�⺻ UI ����")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject button;
    [SerializeField] private GameTimer gameTimer;

    [Header("���ھ� �г� ����")]
    [SerializeField] private GameObject scoreView;
    [SerializeField] private GameObject resultEntryPrefab;


    [HideInInspector]public GameObject gameState;

    public void AddResultEntry(int playerID, int score)
    {
        //��Ʈ��ũ ������Ʈ�� ����� ������Ʈ�� �ƴ�
        var obj = Instantiate(resultEntryPrefab,scoreView.transform);
        if (obj.TryGetComponent<ResultScoreEntry>(out var entry))
        {
            entry.SetEntry(playerID.ToString(), score);
        }
    }
    public void AddResultEntry(int playerID, float score)
    {
        //��Ʈ��ũ ������Ʈ�� ����� ������Ʈ�� �ƴ�
        var obj = Instantiate(resultEntryPrefab, scoreView.transform);
        if (obj.TryGetComponent<ResultScoreEntry>(out var entry))
        {
            entry.SetEntry(playerID.ToString(), score);
        }
    }

    public void SetTimer(float time)
    {
        if(gameTimer == null) return;

        if (time > 0)
        {
            gameTimer.gameObject.SetActive(true);
            gameTimer.Timer = time;
        }
        else
        {
            gameTimer.Timer = 0f;
        }
    }

    public void SetResult()
    {
        // ���常 ������ ��ư Ȱ��ȭ
        if (PhotonNetwork.IsMasterClient)
            button.SetActive(true);

        scorePanel.SetActive(true); 
    }

    public void ReturnScene()
    {
        gameState?.SetActive(false);
    }
}
