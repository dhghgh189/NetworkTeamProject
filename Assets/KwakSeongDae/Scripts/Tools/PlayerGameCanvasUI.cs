using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerGameCanvasUI : MonoBehaviour
{
    [Header("�⺻ UI ����")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] GameTimer gameTimer;

    [Header("���ھ� �г� ����")]
    [SerializeField] private GameObject scoreView;
    [SerializeField] private GameObject resultEntryPrefab;

    public void SetResultEntry(string name, int score)
    {
        var obj = Instantiate(resultEntryPrefab,scoreView.transform);
        if (obj.TryGetComponent<ResultScoreEntry>(out var entry))
        {
            entry.SetEntry(name, score);
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
        scorePanel.SetActive(true);
    }
}
