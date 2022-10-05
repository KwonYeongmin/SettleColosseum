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
            txt_desc.text = "<color=#00FFFF>" + Mathf.Ceil(timer) + "</color>�� �� �κ�� �̵��մϴ�";
            return;
        }

        // �̹� ���� ������ �ִ� ���̶�� ������.
        if(bIsLeaving)  return;

        // Ÿ�̸Ӱ� 0�� �� ��� ���� ������.
        Debug.Log("Finish Game");
        NetworkManager.Inst.LeaveRoom();
        bIsLeaving = true;
    }

    public void SetResult(bool bIsVictory)
    {
        if(bIsVictory)
        {
            img_highlight.color = Color.yellow;
            txt_result.text = "�� ��";
            txt_result.color = Color.yellow;
            return;
        }

        img_highlight.color = Color.red;
        txt_result.text = "�� ��";
        txt_result.color = Color.red;
        return;
    }
}
