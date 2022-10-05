using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum MaxPlayer
{
    TWO,
    FOUR,
    SIX
}

public enum GameMode
{
    DEATH_MATCH
}

public enum MapName
{
    Wegheim,
    Axi
}

public class RoomCreateUI : BaseUI
{
    public TMP_InputField txtRoomName;
    public TMP_Dropdown dropUsers;
    public TMP_Dropdown dropGameMode;
    public TMP_Dropdown dropMapName;
    public TextMeshProUGUI txtAlert;

    private void OnClickCreateRoom()
    {
        if(txtRoomName.text == "")
        {
            txtAlert.text = "! 방 제목이 비어있습니다.";
            txtAlert.color = Color.red;
            return;
        }

        // 주어진 정보를 토대로 방을 생성한다.
        NetworkManager.Inst.CreateRoom(
            txtRoomName.text, 
            (MaxPlayer)dropUsers.value, 
            (GameMode)dropGameMode.value, 
            (MapName)dropMapName.value);
    }

    private void OnClickBackward()
    {
        // 뒤로가기
        NetworkManager.Inst.LeaveLobby();
        NetworkManager.Inst.JoinLobby();
    }

    public override void Activate()
    {
        base.Activate();

        txtRoomName.text = "";
        dropUsers.value = 0;
        dropGameMode.value = 0;
        dropMapName.value = 0;
        txtAlert.text = "";
    }
}
