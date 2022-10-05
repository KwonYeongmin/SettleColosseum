using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD_HealthBar : MonoBehaviour
{
    public Image hpBarImg;
    public Image shieldBarImg;

    public TextMeshProUGUI HP;
    public TextMeshProUGUI Total;

    public void UpdateHP(int hp, int shield, int total)
    {
        hpBarImg.fillAmount = hp / (float)total;
        shieldBarImg.fillAmount = (hp + shield) / (float)total;

        HP.text = (hp+shield).ToString();
        Total.text = total.ToString();
    }
}
