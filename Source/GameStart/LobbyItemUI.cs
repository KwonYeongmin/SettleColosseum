using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyItemUI : MonoBehaviour
{
    public TextMeshProUGUI txtRoomName;
    public TextMeshProUGUI txtVersusInfo;
    public TextMeshProUGUI txtGameMode;
    public TextMeshProUGUI txtMapName;
    public TextMeshProUGUI txtUserInfo;

    public void Initialize(string roomName, string versusInfo, string gameMode, string mapName, string userInfo)
    {
        txtRoomName.text = roomName;
        txtVersusInfo.text = versusInfo;
        txtGameMode.text = gameMode;
        txtMapName.text = mapName;
        txtUserInfo.text = userInfo;
    }
}
