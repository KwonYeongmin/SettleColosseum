using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System;
using System.IO;

public class CharacterInfo_UI : MonoBehaviour
{
    public enum Character { Laura = 0, Casey =1};

    // 선택 캐릭터(디폴트 값 로라)
    [HideInInspector] public Character character = Character.Laura;

    // 배경 캐릭터 이미지
    [Header("소스")]
    public Sprite[] characterSprites;

    // 이미지 오브젝트
    [Header("UI")]
    public Image characterImg;

    // 캐릭터 선택 패널
    [Header("Panel")]
    public GameObject Panel_Select;

    // 캐릭터 정보 패널
    public GameObject Panel_Info;
    public GameObject[] Panel_Infos;

    // 대기창 패널
    public GameObject Panel_Waiting;

    // 마우스 사라지게하는 프리팹
    [Header("Mouse Disappear Prefab")]
    public GameObject Prefab_MouseDisappearer;

    // 맵 시작시 존재하는 전경 카메라
    private GameObject camObj;

    void Start()
    {
        camObj = FindObjectOfType<Camera>().gameObject;   
    }

    void Update()
    {
        BackwardToSelect();
        CheckWaitingFinish();
    }

    // 캐릭터 정보창에서 ESC키를 누르면 뒤로 되돌아간다.
    private void BackwardToSelect()
    {
        if(Panel_Info.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Panel_Info.SetActive(false);
            for(int i = 0; i < Panel_Infos.Length; i++)
                Panel_Infos[i].SetActive(false);

            Panel_Select.SetActive(true);
        }
    }

    private void CheckWaitingFinish()
    {
        // 대기중이 아니라면 그냥 리턴한다.
        if(!Panel_Waiting.activeSelf)   return;

        // 다른 유저들의 캐릭터가 스폰됐는지 검사하고 게임을 시작한다.
        Playable[] playables = FindObjectsOfType<Playable>();
        if (playables.Length >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            for (int i = 0; i < playables.Length; i++)
                playables[i].bCanInteract = true;
            Destroy(camObj);
            Destroy(gameObject);
        }
        else
        {
            for (int i = 0; i < playables.Length; i++)
                playables[i].bCanInteract = false;
        }
    }

    // 로라 정보창을 연다
    public void OnClickLaura()
    {
        character = Character.Laura;
        OpenInfo();
    }

    // 케이시 정보창을 연다
    public void OnClickCasey()
    {
        character = Character.Casey;
        OpenInfo();
    }

    // 정보창을 연다
    private void OpenInfo()
    {
        // 이미지 셋팅
        characterImg.sprite = characterSprites[(int)character];
        characterImg.SetNativeSize();

        // 정보 셋팅
        Panel_Info.SetActive(true);
        Panel_Infos[(int)character].SetActive(true);

        // 캐릭터 선택창 비활성화
        Panel_Select.SetActive(false);
    }

    public void OnClickGameStart()
    {
        // 패널 비활성화
        characterImg.gameObject.SetActive(false);
        Panel_Info.SetActive(false);

        // 대기창 패널 활성화
        Panel_Waiting.SetActive(true);

        // 캐릭터를 스폰시키기 위한 스폰 포인트 목록을 가져온다.
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();

        // 소속된 팀에 따라 스폰 포인트를 따로 정해준다.
        int iSpawn = 0;
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[i])
            {
                if (i % 2 == 0) iSpawn = 0;
                else iSpawn = 1;
                break;
            }
        }

        // 선택한 캐릭터를 스폰시켜준다.
        switch (character)
        {
            case Character.Laura:     // 로라 스폰
                PhotonNetwork.Instantiate(
                    Path.Combine("PhotonPrefabs", "RoraController"),
                    spawnPoints[iSpawn].transform.position,
                    spawnPoints[iSpawn].transform.rotation);
                break;
            case Character.Casey:     // 케이시 스폰
                PhotonNetwork.Instantiate(
                    Path.Combine("PhotonPrefabs", "CaseyController"),
                    spawnPoints[iSpawn].transform.position,
                    spawnPoints[iSpawn].transform.rotation);
                break;
        }

        // 마우스를 사라지게해준다.
        Instantiate(Prefab_MouseDisappearer);
    }
}
