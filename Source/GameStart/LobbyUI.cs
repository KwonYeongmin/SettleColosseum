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
        // �� ���� UI�� �Ѿ��.
        Debug.Log("�Ѿ��");
        StartUI_Manager.Inst.ChangeUI(UI_Type.ROOMCREATE);
        Debug.Log("�� ����");
    }

    private void OnClickPrev()
    {
        // ���� �� ���
        if(curPage > 0)
        {
            curPage--;
            ShowRoomList();
        }
    }

    private void OnClickNext()
    {
        // ���� �� ���
        if (roomList.Count - ((curPage + 1) * N_ROOMS_IN_PAGE) < 1)
        {
            curPage++;
            ShowRoomList();
        }
    }

    private void OnClickJoin()
    {
        // ���� ����
        int i = (curPage * N_ROOMS_IN_PAGE) + toggleGroup.Index;
        if(i != -1)     NetworkManager.Inst.JoinRoom(roomList[i].Name);
    }

    private void OnClickBackward()
    {
        // �ڷΰ���
        NetworkManager.Inst.LeaveLobby();
    }

    private void ShowRoomList()
    {
        // �� ����� �����ش�.
        for (int i = 0; i < N_ROOMS_IN_PAGE; i++)
        {
            int iRoom = (curPage * N_ROOMS_IN_PAGE) + i;
            if (iRoom < roomList.Count)
            {
                // �����ϴ� ���ΰ��.
                RoomInfo roomInfo = roomList[iRoom];

                // Ŀ���� �����͸� ���´�.
                HashTable ht = roomInfo.CustomProperties;
                Debug.Log("MapName: " + ht["MapName"]);

                string title = (string)ht["Title"];
                if(title == null)   return;

                string gameMode = null;
                switch ((GameMode)ht["GameMode"])
                {
                    case GameMode.DEATH_MATCH: gameMode = "������ġ"; break;
                }

                string mapName = null;
                switch ((MapName)ht["MapName"])
                {
                    case MapName.Wegheim:   mapName = "�������� ��������"; break;
                    case MapName.Axi:       mapName = "��質�� �׽�"; break;
                }

                // �׸��� �ʱ�ȭ�Ѵ�.
                lobbyItem[i].Initialize(
                    title,
                    roomInfo.MaxPlayers / 2 + ":" + roomInfo.MaxPlayers / 2,
                    gameMode,
                    mapName,
                    roomInfo.PlayerCount.ToString() + "/" + roomInfo.MaxPlayers);

                // ����� ��ȣ�ۿ� ���� ���θ� true�� �Ѵ�.
                lobbyItem[i].gameObject.GetComponent<CustomToggle>().SetInteractable(true);
            }
            else
            {
                // �� ���ΰ��.
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
        // �� ������ �޾� UI�� �� ����� �����ϰ�, �ٽ� �׷��ش�.
        roomList = NetworkManager.Inst.roomInfos;
        ShowRoomList();
        UpdatePrevNextBtn();
    }
}
