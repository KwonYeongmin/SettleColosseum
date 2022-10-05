using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class LobbyUI : BaseUI
{
    const int N_ROOMS_IN_PAGE = 6;

    public Button btnPrev, btnNext;
    public LobbyItemUI[] lobbyItem;
    public CustomToggleGroup toggleGroup;


    List<RoomInfo> roomList = new List<RoomInfo>();
    int curPage;

    // Start is called before the first frame update
    void OnEnable()
    {
        curPage = 0;

        for(int i = 0; i < N_ROOMS_IN_PAGE; i++)
        {
            lobbyItem[i].gameObject.GetComponent<CustomToggle>().SetInteractable(false);
        }
    }

    public void OnClickCreate()
    {
        // 방 생성 UI로 넘어간다.
        Debug.Log("넘어가기");
        StartUI_Manager.Inst.ChangeUI(UI_Type.ROOMCREATE);
        Debug.Log("방 생성");
    }

    private void OnClickPrev()
    {
        // 이전 방 목록
        if(curPage > 0)
        {
            curPage--;
            ShowRoomList();
        }
    }

    private void OnClickNext()
    {
        // 다음 방 목록
        if (roomList.Count - ((curPage + 1) * N_ROOMS_IN_PAGE) < 1)
        {
            curPage++;
            ShowRoomList();
        }
    }

    private void OnClickJoin()
    {
        // 게임 참가
        int i = (curPage * N_ROOMS_IN_PAGE) + toggleGroup.Index;
        if(i != -1)     NetworkManager.Inst.JoinRoom(roomList[i].Name);
    }

    private void OnClickBackward()
    {
        // 뒤로가기
        NetworkManager.Inst.LeaveLobby();
    }

    private void ShowRoomList()
    {
        // 방 목록을 보여준다.
        for (int i = 0; i < N_ROOMS_IN_PAGE; i++)
        {
            int iRoom = (curPage * N_ROOMS_IN_PAGE) + i;
            if (iRoom < roomList.Count)
            {
                // 실존하는 방인경우.
                RoomInfo roomInfo = roomList[iRoom];

                // 커스텀 데이터를 얻어온다.
                HashTable ht = roomInfo.CustomProperties;
                Debug.Log("MapName: " + ht["MapName"]);

                string title = (string)ht["Title"];
                if(title == null)   return;

                string gameMode = null;
                switch ((GameMode)ht["GameMode"])
                {
                    case GameMode.DEATH_MATCH: gameMode = "데스매치"; break;
                }

                string mapName = null;
                switch ((MapName)ht["MapName"])
                {
                    case MapName.Wegheim:   mapName = "마법나라 웨그하임"; break;
                    case MapName.Axi:       mapName = "기계나라 액시"; break;
                }

                // 항목을 초기화한다.
                lobbyItem[i].Initialize(
                    title,
                    roomInfo.MaxPlayers / 2 + ":" + roomInfo.MaxPlayers / 2,
                    gameMode,
                    mapName,
                    roomInfo.PlayerCount.ToString() + "/" + roomInfo.MaxPlayers);

                // 토글의 상호작용 가능 여부를 true로 한다.
                lobbyItem[i].gameObject.GetComponent<CustomToggle>().SetInteractable(true);
            }
            else
            {
                // 빈 방인경우.
                lobbyItem[i].Initialize("", "", "", "", "");
                lobbyItem[i].gameObject.GetComponent<CustomToggle>().SetInteractable(false);
            }
        }
    }

    private void UpdatePrevNextBtn()
    {
        if (curPage == 0) btnPrev.interactable = false;
        else btnPrev.interactable = true;

        if (roomList.Count - ((curPage + 1) * N_ROOMS_IN_PAGE) < 1) btnNext.interactable = false;
        else btnNext.interactable = true;
    }

    public void UpdateRoomList()
    {
        // 방 정보를 받아 UI의 방 목록을 갱신하고, 다시 그려준다.
        roomList = NetworkManager.Inst.roomInfos;
        ShowRoomList();
        UpdatePrevNextBtn();
    }
}
