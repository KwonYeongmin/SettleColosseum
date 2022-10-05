using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
public class HUD_Magazine : MonoBehaviour
{
    public TextMeshProUGUI txt_remain;
    public TextMeshProUGUI txt_max;

    StringBuilder sb;

    private void Awake()
    {
        sb = new StringBuilder();
    }

    public void Init(string maxBullet)
    {
        txt_remain.text = maxBullet;
        txt_max.text = sb.Append("/").Append(maxBullet).ToString();
    }

    public void UpdateRemain(string bulletCount)
    {
        txt_remain.text = bulletCount; 
        
    }
}
