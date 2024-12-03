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

    float elapsedTime;

    public float Timer
    {
        get
        {
            return timer;
        }
        set
        {
            timer = value;
            //timerText?.SetText($"{(int)timer+1}");            
            //if (timer < 0.01f)
            //{
            //    gameObject.SetActive(false);
            //}

            timerText?.SetText($"{(int)timer}");

            if (timer > 0)
            {
                SoundManager.Instance.Play(Enums.ESoundType.SFX, SoundManager.SFX_COUNTING);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        timerText?.SetText($"{timer}");
        elapsedTime = 0;
    }

    private void Update()
    {
        elapsedTime += Time.unscaledDeltaTime;
        if (elapsedTime >= 1.0f)
        {
            Timer--;
            elapsedTime = 0;
        }

        // ������ �Ͻ������� ���þ��� ����ǵ��� ����
        //Timer -= Time.unscaledDeltaTime;
    }
}
