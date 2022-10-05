using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class HUD_GameFinish : MonoBehaviour
{
    public TextMeshProUGUI txt_desc;
    public Image img_highlight;
    public TextMeshProUGUI txt_result;

    bool bIsLeaving;

    private float timer;
    public float Timer { get { return timer; } }

    void OnEnable()
    {
        timer = 10f;
        bIsLeaving = false;
    }

    void Update()
    {
        UpdateTimer();
    }

    void UpdateTimer()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            txt_desc.text = "<color=#00FFFF>" + Mathf.Ceil(timer) + "</color>초 후 로비로 이동합니다";
            return;
        }

        // 이미 방을 나가고 있는 중이라면 끝낸다.
        if(bIsLeaving)  return;

        // 타이머가 0이 된 경우 방을 나간다.
        Debug.Log("Finish Game");
        NetworkManager.Inst.LeaveRoom();
        bIsLeaving = true;
    }

    public void SetResult(bool bIsVictory)
    {
        if(bIsVictory)
        {
            img_highlight.color = Color.yellow;
            txt_result.text = "승 리";
            txt_result.color = Color.yellow;
            return;
        }

        img_highlight.color = Color.red;
        txt_result.text = "패 배";
        txt_result.color = Color.red;
        return;
    }
}
