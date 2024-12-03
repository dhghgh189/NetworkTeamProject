using Firebase.Auth;
using Firebase.Extensions;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;

public class LoginPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField emailInputField;
    [SerializeField] TMP_InputField passwordInputField;

    [SerializeField] NickNamePanel nickNamePanel;
    [SerializeField] VerifyPanel verifyPanel;

    public void OnEnable()
    {
        // ������� ��� (Login)
        SoundManager.Instance.Play(Enums.ESoundType.BGM, SoundManager.BGM_LOGIN);
    }

    public void Login()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        BackendManager.Auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                        return;
                    }

                    Firebase.Auth.AuthResult result = task.Result;
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        result.User.DisplayName, result.User.UserId);
                CheckUserInfo();
            });
            
   }

    private void CheckUserInfo()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;
        if (user == null)
            return;

        Debug.Log($"DisPlay Name : {user.DisplayName}");
        Debug.Log($"Email : {user.Email}");
        Debug.Log($"Email verified : {user.IsEmailVerified}");
        Debug.Log($"User ID : {user.UserId}");

        if(user.IsEmailVerified == false)
        {
            // �̸��� ���� ����
            verifyPanel.gameObject.SetActive(true);


        }
        else if (user.DisplayName == "")
        {
            // �г��� ���� �ܰ�
            nickNamePanel.gameObject.SetActive(true);

        }
        else
        {
            // ���� ���� �ܰ�
            PhotonNetwork.LocalPlayer.NickName = user.DisplayName;
            PhotonNetwork.ConnectUsingSettings();
        }
        
    }
}
