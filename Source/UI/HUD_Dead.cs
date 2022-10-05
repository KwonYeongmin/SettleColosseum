using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD_Dead : MonoBehaviour
{
    public TextMeshProUGUI txt_desc;

    private float timer;
    public float Timer { get { return timer; } }

    void OnEnable()
    {
        timer = 5f;
    }

    void Update()
    {
        UpdateTimer();
    }

    void UpdateTimer()
    {
        if(timer > 0f)
            timer -= Time.deltaTime;
        txt_desc.text = "<color=#00FFFF>" + Mathf.Ceil(timer) + "</color>초 후 부활";
    }
}
