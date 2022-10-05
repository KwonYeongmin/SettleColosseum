using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoraShowUI : MonoBehaviour
{ //cooltime, bullet
    public Text[] ManaShotgun;
    public Text Reflection;
    public Text Teleport;
    public Text Turret;

    public GameObject Rora;
    private SkillControl skillSC;

    void Start()
    {
        skillSC = Rora.GetComponent<SkillControl>();
    }

    
    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {

        ManaShotgun[0].text = "Cooltime : " + skillSC.GetFskill().GetCurCooltime().ToString();

        Reflection.text = "Cooltime : " + skillSC.GetEskill().GetCurCooltime().ToString();

        Teleport.text = "Cooltime : " + skillSC.GetShiftSkill().GetCurCooltime().ToString();

        Turret.text = "Cooltime : " + skillSC.GetQskill().GetCurCooltime().ToString();
    }

}
