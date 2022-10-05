using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignUpUI : BaseUI
{
    public TMP_InputField txtEmail;
    public TMP_InputField txtNickname;
    public TMP_InputField txtPassword;
    public TMP_InputField txtPasswordCheck;
    public TextMeshProUGUI txtAlert;

    string email;
    string nickname;
    string password;
    string passwordCheck;

    public void OnClickComplete()
    {
       

        // 회원 가입을 한다.
        txtAlert.text = null;
        if (txtAlert.text == null && string.IsNullOrEmpty(email))              txtAlert.text = "! 이메일 입력란이 비었습니다.";
        if (txtAlert.text == null && string.IsNullOrEmpty(nickname))           txtAlert.text = "! 닉네임 입력란이 비었습니다.";
        if (txtAlert.text == null && string.IsNullOrEmpty(password))           txtAlert.text = "! 비밀번호 입력란이 비었습니다.";
        if (txtAlert.text == null && string.IsNullOrEmpty(passwordCheck))      txtAlert.text = "! 비밀번호 재확인을 해주세요.";
        if (txtAlert.text == null && password != passwordCheck)     txtAlert.text = "! 비밀번호와 재확인 번호가 서로 다릅니다.";
        if (txtAlert.text == null && password.Length < 6)           txtAlert.text = "! 비밀번호가 6자리 미만입니다.";
        txtAlert.color = Color.red;
        
        if(txtAlert.text == null)
            NetworkManager.Inst.Register(email, nickname, password, txtAlert);
    }

    public void OnClickBackward()
    {
        // 뒤로가기
        StartUI_Manager.Inst.ChangeUI(UI_Type.LOGIN);
    }

    public void OnValueChangedEmail(TMP_InputField txtEmail)
    {
        email = txtEmail.text;
    }

    public void OnValueChangedNickname(TMP_InputField txtNickname)
    {
        nickname = txtNickname.text;
    }

    public void OnValueChangedPassword(TMP_InputField txtPassword)
    {
        password = txtPassword.text;
    }

    public void OnValueChangedPasswordCheck(TMP_InputField txtPasswordCheck)
    {
        passwordCheck = txtPasswordCheck.text;
    }

    public override void Activate()
    {
        base.Activate();

        txtEmail.text = "";
        txtNickname.text = "";
        txtPassword.text = "";
        txtPasswordCheck.text = "";
        txtAlert.text = "";
    }
}
