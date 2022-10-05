using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text;
using Photon.Pun;

public class HUDManager : MonoBehaviour
{
    public enum character{ Rora, Casey};
    public character curCharacter;

    public GameObject Character;

    private SkillControl skillSC;
    private Casey caseySC;

    [Header("HP")]
    public HUD_HealthBar HPBar; 

    [Header("GameTime")]
    public TextMeshProUGUI GameTimer;
    public int MaxGameTime;
    private float startTime;
    public bool bIsTimerStarted = false;

    [Header("GameTimer")]
    public TextMeshProUGUI timer;
    public int Duration { get; private set; }
    private int remainDuration;


    [Header("Character")]
    public TextMeshProUGUI nickname;

    [Header("Magazine")]
    public TextMeshProUGUI txt_remain;
    public TextMeshProUGUI txt_max;
    StringBuilder strB;

    [Header("Cooltime")]
    public Image ShiftSkillCT;
    public Image FSkillCT;
    public Image ESkillCT;
    public Image VSkillCT;

    [Header("±√±ÿ±‚")]
    public Image QSkillGuage;
    public TextMeshProUGUI QSkillGuage_txt;
    public Sprite[] QskillSprite;

    [Header("HitGauge")]
    public Image HitGuage;
    public Sprite[] HitSprite;

    [Header("«œ¿ß HUD")]
    public HUD_Dead DeadHUD;
    public HUD_GameFinish GameFinishHUD;

    void Start()
    {
        if (curCharacter == character.Rora)
        {
            skillSC = Character.GetComponent<SkillControl>();
            VSkillCT.gameObject.SetActive(false);
        }
        else if (curCharacter==character.Casey)
        {
            caseySC = Character.GetComponent<Casey>();
            VSkillCT.gameObject.SetActive(true);
        }

        // timer Ω√¿€
        SetDuration();
        Begin();
        //
    }


    void Initialized()
    {
        strB = new StringBuilder();
        bIsTimerStarted = true;
       
        QSkillGuage.sprite = QskillSprite[0];
    }

    void Update()
    {
        RunGameTimer();
        RunCooltime();
        RunUltimateGauge();
        RunHitGauge();
    }

    private void RunGameTimer()
    {
        if (bIsTimerStarted)
        {
            startTime = Time.time;
            bIsTimerStarted = false;
        }

        float time = Time.time - startTime;
        // GameTimer.text = string.Format("{0:D2}:{1:D2}", (int)time / 60,(int)time % 60);
    }



    private void RunCooltime()
    {
        if(skillSC == null && caseySC == null)
            return;

        if (curCharacter == character.Rora && skillSC)
        {
            ShiftSkillCT.fillAmount = skillSC.GetShiftSkill().GetCurCooltime() / skillSC.Teleport_coolTime;
            FSkillCT.fillAmount = skillSC.GetFskill().GetCurCooltime() / skillSC.ManaBomb_coolTime;
            ESkillCT.fillAmount = skillSC.GetEskill().GetCurCooltime() / skillSC.reflection_coolTime;
        }
        else if (curCharacter == character.Casey && caseySC)
        {
            ShiftSkillCT.fillAmount = (caseySC.defaultDashCoolTime-caseySC.DashCoolTime) / caseySC.defaultDashCoolTime;
            FSkillCT.fillAmount = (caseySC.defaultBarrierCoolTime-caseySC.barrierCoolTime) / caseySC.defaultBarrierCoolTime;
            ESkillCT.fillAmount = (caseySC.defaultESkillCoolTime-caseySC.ESkillCoolTime) / caseySC.defaultESkillCoolTime;
            VSkillCT.fillAmount = (caseySC.defaultVCoolTime-caseySC.VCoolTime) / caseySC.defaultVCoolTime;
        }
       
    }



    private void RunUltimateGauge()
    {
        float QskillCT = 0.0f;
        float HitCT = 0.0f;

        if (curCharacter == character.Rora && skillSC)
        {
            QskillCT= skillSC.GetQskill().GetCurCooltime() / skillSC.QSkill_Cooltime;
           // QSkillGuage.fillAmount = skillSC.GetQskill().GetCurCooltime() / skillSC.QSkill_coolTime;
           // QSkillGuage_txt.text = string.Format("{0:P} %", (Mathf.RoundToInt((skillSC.GetQskill().GetCurCooltime() / skillSC.QSkill_coolTime))));
        }
        else if (curCharacter == character.Casey && caseySC)
        {
            QskillCT = caseySC.CoolTimeUltGauge / (float)caseySC.maxultGauge;//caseySC.defaultQCoolTime - 
        }

        QSkillGuage.fillAmount = QskillCT;
        QSkillGuage_txt.text = string.Format("{0:P}", (QskillCT));

        if (QSkillGuage.fillAmount > 0.99f) { QSkillGuage.sprite = QskillSprite[1]; }
        else { QSkillGuage.sprite = QskillSprite[0]; }
    }

    private void RunHitGauge()
    {
        float HitCT = 0.0f;

        if (curCharacter == character.Rora && skillSC) return;
        else if (curCharacter == character.Casey && caseySC)
        {
            HitCT = caseySC.HUDHitGauge / (float)caseySC.maxhitGauge;
        }

        HitGuage.fillAmount = HitCT;

        if (HitGuage.fillAmount > 0.99f) { HitGuage.sprite = HitSprite[1]; }
        else { HitGuage.sprite = HitSprite[0]; }
    }

    public void SetDuration()
    {
        Duration = remainDuration = MaxGameTime;
    }

    public void Begin()
    {
        StopAllCoroutines();
        StartCoroutine("UpdateTimer");
    }

    private IEnumerator UpdateTimer()
    {
        
        while (remainDuration > 0)
        {
            UpdateUITimer(remainDuration);
            remainDuration--;
            yield return new WaitForSeconds(1f);
        }
        End();
    }


    private void UpdateUITimer(int seconds)
    {
       //  timer.text = string.Format("{0:D2}:{1:D2}", seconds / 60, seconds % 60);  
    }

    public void End()
    {

    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public float GetDeadHUD_Timer()
    {
        return DeadHUD.Timer;
    }

    public float GetGameFinishHUD_Timer()
    {
        return GameFinishHUD.Timer;
    }

    public void SetGameResult(bool bIsVictory)
    {
        GameFinishHUD.SetResult(bIsVictory);
    }
}
