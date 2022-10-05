using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager gameStateManager;

    public static GameStateManager Inst
    {
        get
        {
            return gameStateManager;
        }
    }

    private Playable[] players;
    private SpawnPoint[] spawnPoints;
    private int lives_team0 = 3;
    private int lives_team1 = 3;
    private PhotonView pv;
    private List<int> indices_dead;
    private bool bIsGameEnd = false;

    // Start is called before the first frame update
    void Start()
    {
        InitManager();
    }

    private void InitManager()
    {
        gameStateManager = this;

        // 리스폰을 위한 스폰 위치를 얻어온다.
        spawnPoints = FindObjectsOfType<SpawnPoint>();
        
        // 컴포넌트 초기화
        pv = GetComponent<PhotonView>();

        // 리스트 초기화
        indices_dead = new List<int>();
        players = FindObjectsOfType<Playable>();
    }

    void Update()
    {
        MonitorPlayerDead();
        MonitorGameFinish();
    }

    private void MonitorPlayerDead()
    {
        // 유저 목록을 계속 갱신한다
        players = FindObjectsOfType<Playable>();

        // 죽은 플레이어가 있는지 검사하고 사망자 목록(indices_dead)을 갱신한다.
        int iDead = -1;
        for(int i = 0; i < players.Length; i++)
        {
            if(players[i].IsDead())
            {
                // 플레이어가 등록되었지만 아직 처리되지 않은 유저라면 넘기고 아니라면 새로 추가한다.
                if (indices_dead.Contains(i))    continue;
                indices_dead.Add(i);
                iDead = i;
                break;
            }
            else
            {
                // 사망자 목록에 등록되어있는 유저가 리스폰된 상태라면 목록에서 지운다.
                if(indices_dead.Contains(i))
                    indices_dead.Remove(i);
            }
        }

        // 죽은 플레이어가 없으면 함수를 종료한다.
        if(iDead == -1)
            return;

        // 죽은 플레이어의 팀을 구해 해당 팀의 목숨을 차감한다.
        int iTeam = players[iDead].GetPlayerTeamNumber();
        pv.RPC("RPC_DecreaseTeamLive", RpcTarget.AllBuffered, iTeam);

        // 승부가 결정난게 아니라면 죽은 플레이어에게 사망 UI를 띄워준다.
        if (lives_team0 > 0 && lives_team1 > 0)
            players[iDead].ShowDeadUI();
    }

    private void MonitorGameFinish()
    {
        // 팀의 목숨 상황에 따라 게임을 끝낸다.
        if (lives_team0 == 0 || lives_team1 == 0)
        {
            bIsGameEnd = true;

            // 어떤 팀의 목숨이 0이됐다면 속한 팀에따라 패배 혹은 승리 UI를 띄워준다.
            for (int i = 0; i < players.Length; i++)
            {
                if(!players[i].IsMine())
                    continue;

                if (lives_team0 == 0)
                {
                    if(players[i].GetPlayerTeamNumber() == 0)
                        players[i].ShowResultUI(false);
                    else
                        players[i].ShowResultUI(true);
                }
                else
                {
                    if (players[i].GetPlayerTeamNumber() == 0)
                        players[i].ShowResultUI(true);
                    else
                        players[i].ShowResultUI(false);
                }

                break;
            }
        }
    }

    [PunRPC]
    private void RPC_DecreaseTeamLive(int iTeam)
    {
        if(iTeam == 0)          lives_team0--;
        else if(iTeam == 1)     lives_team1--;
    }

    public void Respawn(GameObject playerObj)
    {
        // 리스폰될 플레이어의 인덱스를 구한다.
        int iRespawn = -1;
        for (int i = 0; i < indices_dead.Count; i++)
        {
            if(players[indices_dead[i]] == playerObj.GetComponent<Playable>())
            {
                iRespawn = indices_dead[i];
                break;
            }
        }
        if(iRespawn == -1)  return;

        // 리스폰된 플레이어의 처리에 필요한 정보를 얻는다.
        int iTeam = players[iRespawn].GetPlayerTeamNumber();
        bool bIsCasey = (players[iRespawn].GetComponent<Casey>() != null) ? true : false;

        // 기존 컨트롤러를 삭제한다.
        PhotonNetwork.Destroy(players[iRespawn].gameObject);

        // 새로운 컨트롤러를 알맞은 스폰 지점에 리스폰 시킨다.
        if (iTeam == 0)
        {
            Debug.Log("SpawnPoints[0] position: " + spawnPoints[0].transform.position);

            if (bIsCasey)
            {
                players[iRespawn] = PhotonNetwork.Instantiate(
                "PhotonPrefabs/CaseyController",
                spawnPoints[0].transform.position,
                spawnPoints[0].transform.rotation).GetComponent<Playable>();
                players[iRespawn].SetRotationY(spawnPoints[0].transform.rotation.eulerAngles.y);
            }
            else
            {
                players[iRespawn] = PhotonNetwork.Instantiate(
                "PhotonPrefabs/RoraController",
                spawnPoints[0].transform.position,
                spawnPoints[0].transform.rotation).GetComponent<Playable>();
                players[iRespawn].SetRotationY(spawnPoints[0].transform.rotation.eulerAngles.y);
            }
        }
        else
        {
            Debug.Log("SpawnPoints[1] position: " + spawnPoints[1].transform.position);

            if (bIsCasey)
            {
                players[iRespawn] = PhotonNetwork.Instantiate(
                "PhotonPrefabs/CaseyController",
                spawnPoints[1].transform.position,
                spawnPoints[1].transform.rotation).GetComponent<Playable>();
                players[iRespawn].SetRotationY(spawnPoints[1].transform.rotation.eulerAngles.y);
            }
            else
            {
                players[iRespawn] = PhotonNetwork.Instantiate(
                "PhotonPrefabs/RoraController",
                spawnPoints[1].transform.position,
                spawnPoints[1].transform.rotation).GetComponent<Playable>();
                players[iRespawn].SetRotationY(spawnPoints[1].transform.rotation.eulerAngles.y);
            }
        }
    }
}
