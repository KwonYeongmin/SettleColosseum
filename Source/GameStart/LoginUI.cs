using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginUI : BaseUI
{
    public TMP_InputField txtEmail;
    public TMP_InputField txtPassword;
    public TextMeshProUGUI txtAlert;

    string email;
    string password;

    private void OnClickLogIn()
    {
        txtAlert.text = null;
        if (txtAlert.text == null && string.IsNullOrEmpty(email))        txtAlert.text = "! 이메일 입력란이 비었습니다.";
        if (txtAlert.text == null && string.IsNullOrEmpty(password))  txtAlert.text = "! 비밀번호 입력란이 비었습니다.";
        txtAlert.color = Color.red;

        if (txtAlert.text == null)
            NetworkManager.Inst.Login(email, password, txtAlert);
    }

    private void OnClickSignUp()
    {
        StartUI_Manager.Inst.ChangeUI(UI_Type.SIGN_UP);
    }

    private void OnClickExit()
    {
        Debug.Log("Application Quit");
        Application.Quit();
    }

    public void OnValueChangedEmail(TMP_InputField txtEmail)
    {
        email = txtEmail.text;
    }

    public void OnValueChangedPassword(TMP_InputField txtPassword)
    {
        password = txtPassword.text;
    }

    public override void Activate()
    {
        base.Activate();

        txtEmail.text = "";
        txtPassword.text = "";
        txtAlert.text = "";
    }
}
