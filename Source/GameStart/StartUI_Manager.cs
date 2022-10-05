using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum UI_Type
{
    LOGIN,
    SIGN_UP,
    TITLE,
    LOBBY,
    ROOMCREATE,
    ROOM
}

public class StartUI_Manager : MonoBehaviour
{
    public static StartUI_Manager uiManager;

    public static StartUI_Manager Inst
    {
        get
        {
            return uiManager;
        }
    }

    private BaseUI curUI;

    public LoginUI logInUI;
    public SignUpUI signUpUI;
    public TitleUI titleUI;
    public LobbyUI lobbyUI;
    public RoomCreateUI roomCreateUI;
    public RoomUI roomUI;

    public GameObject cover;

    // Start is called before the first frame update
    void Start()
    {
        InitManager();
    }

    private void InitManager()
    {
        if(uiManager)
        {
            Destroy(this);
            return;
        }
        uiManager = this;

        logInUI.Deactivate();
        signUpUI.Deactivate();
        titleUI.Deactivate();
        lobbyUI.Deactivate();
        roomCreateUI.Deactivate();
        roomUI.Deactivate();

        if(PhotonNetwork.IsConnected)   curUI = titleUI;
        else                            curUI = logInUI;
        curUI.Activate();

        NetworkManager.Inst.lobbyUI = lobbyUI;
        NetworkManager.Inst.roomUI = roomUI;
    }

    public void ChangeUI(UI_Type type)
    {
        curUI.Deactivate();

        switch(type)
        {
            case UI_Type.LOGIN:
                curUI = logInUI;
                break;
            case UI_Type.SIGN_UP:
                curUI = signUpUI;
                break;
            case UI_Type.TITLE:
                curUI = titleUI;
                break;
            case UI_Type.LOBBY:
                curUI = lobbyUI;
                break;
            case UI_Type.ROOMCREATE:
                curUI = roomCreateUI;
                break;
            case UI_Type.ROOM:
                curUI = roomUI;
                break;
        }

        curUI.Activate();
    }

    public void SetCover(bool bCoverOn)
    {
        // UI를 클릭하지 못하게하는 커버를 활성화 or 비활성화 한다.
        cover.SetActive(bCoverOn);
    }
}
