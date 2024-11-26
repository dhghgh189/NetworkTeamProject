using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WebSocketSharp;

public class SignUpPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField emailInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] TMP_InputField passwordConfirmField;

    /// <summary>
    /// Sign Up ��ư�� �������� ������ �Լ�
    /// </summary>
    public void SignUp()
    {
        string email = emailInputField.text;
        string pass = passwordInputField.text;
        string confirm = passwordConfirmField.text;

        if (email.IsNullOrEmpty())
        {
            Debug.LogWarning("�̸����� �Է����ּ���.");
            return;
        }

        if (pass != confirm)
        {
            Debug.LogWarning("�н����尡 ��ġ���� �ʽ��ϴ�.");
            return;
        }

        // ���̾�̽� ��������
        BackendManager.Auth.CreateUserWithEmailAndPasswordAsync(email, pass)
            .ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            AuthResult result = task.Result;
            Debug.Log($"Firebase user created successfully: {result.User.DisplayName} ({result.User.UserId})");

            gameObject.SetActive(false);
        });
    }



}
