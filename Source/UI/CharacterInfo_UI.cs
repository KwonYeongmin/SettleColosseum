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

    // ���� ĳ����(����Ʈ �� �ζ�)
    [HideInInspector] public Character character = Character.Laura;

    // ��� ĳ���� �̹���
    [Header("�ҽ�")]
    public Sprite[] characterSprites;

    // �̹��� ������Ʈ
    [Header("UI")]
    public Image characterImg;

    // ĳ���� ���� �г�
    [Header("Panel")]
    public GameObject Panel_Select;

    // ĳ���� ���� �г�
    public GameObject Panel_Info;
    public GameObject[] Panel_Infos;

    // ���â �г�
    public GameObject Panel_Waiting;

    // ���콺 ��������ϴ� ������
    [Header("Mouse Disappear Prefab")]
    public GameObject Prefab_MouseDisappearer;

    // �� ���۽� �����ϴ� ���� ī�޶�
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

    // ĳ���� ����â���� ESCŰ�� ������ �ڷ� �ǵ��ư���.
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
        // ������� �ƴ϶�� �׳� �����Ѵ�.
        if(!Panel_Waiting.activeSelf)   return;

        // �ٸ� �������� ĳ���Ͱ� �����ƴ��� �˻��ϰ� ������ �����Ѵ�.
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

    // �ζ� ����â�� ����
    public void OnClickLaura()
    {
        character = Character.Laura;
        OpenInfo();
    }

    // ���̽� ����â�� ����
    public void OnClickCasey()
    {
        character = Character.Casey;
        OpenInfo();
    }

    // ����â�� ����
    private void OpenInfo()
    {
        // �̹��� ����
        characterImg.sprite = characterSprites[(int)character];
        characterImg.SetNativeSize();

        // ���� ����
        Panel_Info.SetActive(true);
        Panel_Infos[(int)character].SetActive(true);

        // ĳ���� ����â ��Ȱ��ȭ
        Panel_Select.SetActive(false);
    }

    public void OnClickGameStart()
    {
        // �г� ��Ȱ��ȭ
        characterImg.gameObject.SetActive(false);
        Panel_Info.SetActive(false);

        // ���â �г� Ȱ��ȭ
        Panel_Waiting.SetActive(true);

        // ĳ���͸� ������Ű�� ���� ���� ����Ʈ ����� �����´�.
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();

        // �Ҽӵ� ���� ���� ���� ����Ʈ�� ���� �����ش�.
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

        // ������ ĳ���͸� ���������ش�.
        switch (character)
        {
            case Character.Laura:     // �ζ� ����
                PhotonNetwork.Instantiate(
                    Path.Combine("PhotonPrefabs", "RoraController"),
                    spawnPoints[iSpawn].transform.position,
                    spawnPoints[iSpawn].transform.rotation);
                break;
            case Character.Casey:     // ���̽� ����
                PhotonNetwork.Instantiate(
                    Path.Combine("PhotonPrefabs", "CaseyController"),
                    spawnPoints[iSpawn].transform.position,
                    spawnPoints[iSpawn].transform.rotation);
                break;
        }

        // ���콺�� ����������ش�.
        Instantiate(Prefab_MouseDisappearer);
    }
}
