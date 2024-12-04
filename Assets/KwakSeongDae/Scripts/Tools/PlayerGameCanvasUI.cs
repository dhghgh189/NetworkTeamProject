using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public GameObject survivalBlockCount;
    [SerializeField] private TextMeshProUGUI blockCountText;


    [HideInInspector]public GameObject gameState;

    private SurvivalModeState survivalMode;

    private void Start()
    {
        if (gameState.TryGetComponent<SurvivalModeState>(out survivalMode))
        {
            BlockFallenHandle(0);
            survivalMode.selfPlayer.GetComponent<PlayerController>().OnChangeBlockCount += BlockFallenHandle;
            survivalBlockCount.SetActive(true);
        }
        else
        {
            survivalBlockCount.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (survivalMode != null)
        {
            survivalMode.selfPlayer.GetComponent<PlayerController>().OnChangeBlockCount -= BlockFallenHandle;
        }
    }

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

    // (�����̹�) Ÿ���� ���� �� ���� üũ 
    private void BlockFallenHandle(int newBlockCount)
    {
        blockCountText?.SetText((survivalMode.winBlockCount - newBlockCount).ToString());
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
