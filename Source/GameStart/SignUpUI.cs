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
       

        // ȸ�� ������ �Ѵ�.
        txtAlert.text = null;
        if (txtAlert.text == null && string.IsNullOrEmpty(email))              txtAlert.text = "! �̸��� �Է¶��� ������ϴ�.";
        if (txtAlert.text == null && string.IsNullOrEmpty(nickname))           txtAlert.text = "! �г��� �Է¶��� ������ϴ�.";
        if (txtAlert.text == null && string.IsNullOrEmpty(password))           txtAlert.text = "! ��й�ȣ �Է¶��� ������ϴ�.";
        if (txtAlert.text == null && string.IsNullOrEmpty(passwordCheck))      txtAlert.text = "! ��й�ȣ ��Ȯ���� ���ּ���.";
        if (txtAlert.text == null && password != passwordCheck)     txtAlert.text = "! ��й�ȣ�� ��Ȯ�� ��ȣ�� ���� �ٸ��ϴ�.";
        if (txtAlert.text == null && password.Length < 6)           txtAlert.text = "! ��й�ȣ�� 6�ڸ� �̸��Դϴ�.";
        txtAlert.color = Color.red;
        
        if(txtAlert.text == null)
            NetworkManager.Inst.Register(email, nickname, password, txtAlert);
    }

    public void OnClickBackward()
    {
        // �ڷΰ���
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
