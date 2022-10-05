using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System;

public class RoomItemUI : MonoBehaviour
{
    public TextMeshProUGUI txtNickName;
    public Image imgReadyState;

    [Header("레디 여부에 사용할 이미지")]
    public Sprite spriteReady;
    public Sprite spriteReadyComplete;
    public Sprite spriteHost;

    [Header("레디 여부에 오브젝트")]
    public GameObject[] lineImg;

    bool bIsReady = false;

    public string Nickname
    {
        get
        {
            return txtNickName.text;
        }
    }

    public bool IsReady
    {
        get
        {
            return bIsReady;
        }
    }

    public void Initialize(Player player)
    {
        Image background = GetComponent<Image>();

        if (player != null)
        {
            // 프로필의 닉네임과 방장 여부에 따라 레디 이미지를 선택한다.
            txtNickName.text = player.NickName;
            if (player.IsMasterClient) imgReadyState.sprite = spriteHost;
            else imgReadyState.sprite = spriteReady;

            background.color = new Color(background.color.r, background.color.g, background.color.b, 0.2f);
            imgReadyState.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else
        {
            // 빈 리스트로 초기화시킨다.
            txtNickName.text = "";
            imgReadyState.sprite = spriteReady;

            background.color = new Color(background.color.r, background.color.g, background.color.b, 0.1f);
            imgReadyState.color = new Color(1.0f, 1.0f, 1.0f, 0.2f);
        }
    }

    public void Initialize(RoomItemUI item)
    {
        Image background = GetComponent<Image>();

        txtNickName.text = item.txtNickName.text;
        if(txtNickName.text == "")
        {
            // 빈 프로필일 경우
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0.1f);
            imgReadyState.color = new Color(1.0f, 1.0f, 1.0f, 0.2f);
        }
        else
        {
            // 프로필이 있는 경우
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0.2f);
            imgReadyState.color = item.imgReadyState.color;
        }
    }

    public void SetReady()
    {
        // 방장인 경우 레디 여부를 따질 필요가 없으므로 넘어간다.
        if(imgReadyState.sprite == spriteHost)  return;

        // 레디 여부에 따라 레디 이미지를 선택한다.
        bIsReady = !bIsReady;
        if (bIsReady)
        {
            for (int i = 0; i < 4; i++) lineImg[i].SetActive(true);
            imgReadyState.sprite = spriteReadyComplete;
            imgReadyState.color = Color.yellow;
        }
        else
        {
            for (int i = 0; i < 4; i++) lineImg[i].SetActive(false);
            imgReadyState.sprite = spriteReady;
            imgReadyState.color = Color.white;
        }
       
    }

    public void SetNewMasterClient()
    {
        imgReadyState.sprite = spriteHost;
    }
}
