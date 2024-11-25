using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    [Header("Ÿ�̾� �⺻ ����")]
    [SerializeField] private float timer;
    [SerializeField] private TextMeshProUGUI timerText;
    public float Timer
    {
        get
        {
            return timer;
        }
        set
        {
            timer = value;
            timerText?.SetText($"{(int)timer+1}");
            if (timer < 0.01f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        timerText?.SetText($"{timer}");
    }

    private void Update()
    {
        // ������ �Ͻ������� ���þ��� ����ǵ��� ����
        Timer -= Time.unscaledDeltaTime;
    }
}
