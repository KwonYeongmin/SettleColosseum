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

        // �������� ���� ���� ��ġ�� ���´�.
        spawnPoints = FindObjectsOfType<SpawnPoint>();
        
        // ������Ʈ �ʱ�ȭ
        pv = GetComponent<PhotonView>();

        // ����Ʈ �ʱ�ȭ
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
        // ���� ����� ��� �����Ѵ�
        players = FindObjectsOfType<Playable>();

        // ���� �÷��̾ �ִ��� �˻��ϰ� ����� ���(indices_dead)�� �����Ѵ�.
        int iDead = -1;
        for(int i = 0; i < players.Length; i++)
        {
            if(players[i].IsDead())
            {
                // �÷��̾ ��ϵǾ����� ���� ó������ ���� ������� �ѱ�� �ƴ϶�� ���� �߰��Ѵ�.
                if (indices_dead.Contains(i))    continue;
                indices_dead.Add(i);
                iDead = i;
                break;
            }
            else
            {
                // ����� ��Ͽ� ��ϵǾ��ִ� ������ �������� ���¶�� ��Ͽ��� �����.
                if(indices_dead.Contains(i))
                    indices_dead.Remove(i);
            }
        }

        // ���� �÷��̾ ������ �Լ��� �����Ѵ�.
        if(iDead == -1)
            return;

        // ���� �÷��̾��� ���� ���� �ش� ���� ����� �����Ѵ�.
        int iTeam = players[iDead].GetPlayerTeamNumber();
        pv.RPC("RPC_DecreaseTeamLive", RpcTarget.AllBuffered, iTeam);

        // �ºΰ� �������� �ƴ϶�� ���� �÷��̾�� ��� UI�� ����ش�.
        if (lives_team0 > 0 && lives_team1 > 0)
            players[iDead].ShowDeadUI();
    }

    private void MonitorGameFinish()
    {
        // ���� ��� ��Ȳ�� ���� ������ ������.
        if (lives_team0 == 0 || lives_team1 == 0)
        {
            bIsGameEnd = true;

            // � ���� ����� 0�̵ƴٸ� ���� �������� �й� Ȥ�� �¸� UI�� ����ش�.
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
        // �������� �÷��̾��� �ε����� ���Ѵ�.
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

        // �������� �÷��̾��� ó���� �ʿ��� ������ ��´�.
        int iTeam = players[iRespawn].GetPlayerTeamNumber();
        bool bIsCasey = (players[iRespawn].GetComponent<Casey>() != null) ? true : false;

        // ���� ��Ʈ�ѷ��� �����Ѵ�.
        PhotonNetwork.Destroy(players[iRespawn].gameObject);

        // ���ο� ��Ʈ�ѷ��� �˸��� ���� ������ ������ ��Ų��.
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
