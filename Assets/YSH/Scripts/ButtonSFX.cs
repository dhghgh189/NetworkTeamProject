using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
    // ��ư ������Ʈ
    Button buttonComponent;

    void Awake()
    {
        // ��ư ������Ʈ�� ã�´�.
        buttonComponent = GetComponent<Button>();
    }

    private void Start()
    {
        // ��ư Ŭ���� ȿ���� ����ϵ��� �ݹ� �߰�
        buttonComponent.onClick.AddListener(() => {
            SoundManager.Instance.Play(Enums.ESoundType.SFX, SoundManager.SFX_CLICK);
        });
    }
}
