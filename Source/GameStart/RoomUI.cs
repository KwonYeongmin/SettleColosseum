using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using HashTable = ExitGames.Client.Photon.Hashtable;
using System;

public class RoomUI : BaseUI
{
    const int MAX_PLAYER_COUNT = 12;

    public TextMeshProUGUI txtRoomName;
    public TextMeshProUGUI txtUsers;
    public TextMeshProUGUI txtAlert;
    public RoomItemUI[] leftProfiles;
    public RoomItemUI[] rightProfiles;
    public Image imgMap;
    public TextMeshProUGUI txtBtnReadyOrStart;
    public GameObject UI_Loading;

    PhotonView pv;
    public TooltipUI MapInfo;

    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        ShowRoomUI();
    }

    private void OnClickReadyOrStart()
    {
        // 준비 혹은 게임 시작
        if(PhotonNetwork.IsMasterClient)
        {
            // 게임 시작
            // 모든 유저가 레디했는지 체크한다.
            for(int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                // 방장은 레디 체크를 하지 않는다.
                if(PhotonNetwork.PlayerList[i].IsMasterClient)  
                    continue;

                bool bIsReady = true;
                if (i % 2 == 0)
                {
                    if (!leftProfiles[i / 2].IsReady) 
                        bIsReady = false;
                }
                else
                {
                    if (!rightProfiles[i / 2].IsReady) 
                        bIsReady = false;
                }

                // 레디하지 않은 유저가 있다면 경고 알림을 띄우고 종료한다.
                if(!bIsReady)
                {
                    txtAlert.color = Color.red;
                    txtAlert.text = "! 모든 유저가 레디하지 않았습니다.";
                    return;
                }
            }

            // 로딩창을 띄우고 맵을 로딩한다.
            pv.RPC("RPC_ActiveLoadingUI", RpcTarget.AllBuffered);
            Debug.Log("Sync Scene: " + PhotonNetwork.AutomaticallySyncScene);
            
            if (MapInfo.index == 0)
            {
                PhotonNetwork.LoadLevel("02_Wegheim");
            }
            else if(MapInfo.index == 1)
            {
                PhotonNetwork.LoadLevel("03_Exe");
            }
        }
        else
        {
            // 준비
            // 리스트에서 로컬 플레이어의 닉네임과 일치하는 유저를 찾아 레디 상태를 변경한다.
            string myName = PhotonNetwork.LocalPlayer.NickName;
            for (int i = 0; i < leftProfiles.Length; i++)
            {
                if(leftProfiles[i].Nickname == myName)
                    pv.RPC("RPC_SetReady", RpcTarget.AllBuffered, true, i);
            }

            for (int i = 0; i < rightProfiles.Length; i++)
            {
                if (rightProfiles[i].Nickname == myName)
                    pv.RPC("RPC_SetReady", RpcTarget.AllBuffered, false, i);
            }
        }
    }

    [PunRPC]
    private void RPC_SetReady(bool bLeft, int i)
    {
        if(bLeft)   leftProfiles[i].SetReady();
        else        rightProfiles[i].SetReady();
    }

    [PunRPC]
    private void RPC_ActiveLoadingUI()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.AutomaticallySyncScene = true;
        NetworkManager.Inst.bIsInGame = true;
        UI_Loading.SetActive(true);
    }

    private void OnClickRoomExit()
    {
        // 나갈 유저가 방장일 경우 다른 유저를 방장으로 설정한다.
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            SetNewMaster();

        // 뒤로가기
        NetworkManager.Inst.LeaveRoom();
    }

    private void SetNewMaster()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            if (PhotonNetwork.LocalPlayer != PhotonNetwork.PlayerList[0])   PhotonNetwork.SetMasterClient(PhotonNetwork.PlayerList[0]);
            else                                                            PhotonNetwork.SetMasterClient(PhotonNetwork.PlayerList[1]);
        }
        else
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;   // 방이 삭제되기 전에 다른 유저들이 못 들어오게 막는다.
        }
    }

    private void InitPlayerList()
    {
        for (int i = 0; i < MAX_PLAYER_COUNT; i++)
        {
            // 인덱스가 현재 방의 플레이어 수보다 작은지 큰지에 따라 리스트를 비워둘건지 채울건지 결정한다.
            if (i < PhotonNetwork.CurrentRoom.PlayerCount)
            {
                // 플레이어의 정보를 리스트에 채워둔다.
                if (i % 2 == 0) leftProfiles[i / 2].Initialize(PhotonNetwork.PlayerList[i]);
                else            rightProfiles[i / 2].Initialize(PhotonNetwork.PlayerList[i]);
            }
            else
            {
                // 빈 리스트로 초기화한다.
                if (i % 2 == 0) leftProfiles[i / 2].Initialize(player: null);
                else            rightProfiles[i / 2].Initialize(player: null);
            }
        }
    }

    public void ShowRoomUI()
    {
        HashTable ht = PhotonNetwork.CurrentRoom.CustomProperties;
        
        // 방 제목 보여주기
        txtRoomName.text = (string)ht["Title"];

        // 인원수 보여주기
        txtUsers.text = "인원수 " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        // 플레이어 프로필 리스트 초기화
        InitPlayerList();

        // 시작 & 레디 버튼 텍스트 설정
        if (PhotonNetwork.IsMasterClient)   txtBtnReadyOrStart.text = "시작하기";
        else                                txtBtnReadyOrStart.text = "레디";
    }

    public void OnEnterUser(Player player)
    {
        // 인원수 업데이트.
        txtUsers.text = "인원수 " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        // 프로필 리스트 업데이트
        int userCount = PhotonNetwork.CurrentRoom.PlayerCount;
        if (userCount % 2 != 0) leftProfiles[userCount / 2].Initialize(player);
        else                    rightProfiles[(userCount / 2) - 1].Initialize(player);
    }

    public void OnLeftUser(Player player)
    {
        Debug.Log("OnLeftUser");

        // 인원수 업데이트.
        txtUsers.text = "인원수 " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        // 프로필 리스트 업데이트
        Debug.Log("Remain Player: " + PhotonNetwork.CurrentRoom.PlayerCount);
        for(int i = 0; i < leftProfiles.Length; i++)
        {
            // 현재 자리가 방을 나간 유저가 있던 자리라면 그 다음 항목에 있는 유저로 덮어씌운다.
            if (leftProfiles[i].Nickname == player.NickName)
            {
                int j = i;
                for (; j < leftProfiles.Length - 1; j++)
                {
                    leftProfiles[j].Initialize(leftProfiles[j + 1]);
                }
                leftProfiles[j].Initialize(player: null);
                return;
            }
        }

        for (int i = 0; i < rightProfiles.Length; i++)
        {
            if (rightProfiles[i].Nickname == player.NickName)
            {
                int j = i;
                for (; j < rightProfiles.Length - 1; j++)
                {
                    rightProfiles[j].Initialize(rightProfiles[j + 1]);
                }
                rightProfiles[j].Initialize(player: null);
                return;
            }
        }
    }
}
