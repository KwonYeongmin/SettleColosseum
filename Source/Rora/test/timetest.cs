using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timetest : MonoBehaviour
{

    public bool bIsStopped;
    [Header("슬로우 시간")]
    public float SlowTime = 3f;
    public float speed = 0.5f;
    public float CurTimeValue;
    public int HP = 100;

    private void Start()
    {
        // Debug.Log("슬로우 상태 시작");
        CurTimeValue = Time.timeScale;
    }
    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log(other.gameObject.name);

    }

    

    private void Update()
    {
        Vector3 positionY = this.GetComponent<Transform>().position;
        positionY.y += Time.deltaTime * speed;
        this.GetComponent<Transform>().position = positionY;

        CurTimeValue = Time.timeScale;
        //Debug.Log("CurTimeValue: " +CurTimeValue);
        //Debug.Log("HP: " +HP.ToString());

        RunTimer();
        TimerReset();
        IncreaseDamage();
    }

    // ====================== timer 테스트
    public float timersaver = 0;
    private float timeLimit = 0.25f;
    public bool bIsTimerStop = true;
    public bool bIsTimerReset = false;
    public float damagesaver = 0;

    private void TimerReset()
    {
        if (bIsTimerReset)
        {
            timersaver = 0;
            bIsTimerReset = false;
        }
    }

    private void RunTimer()
    {
       if(!bIsTimerStop) timersaver += Time.deltaTime;
    }

    private void IncreaseDamage()
    {
        if (timersaver > timeLimit)
        {
            damagesaver += 5;
            bIsTimerReset = true;
        }
    }
    
    // ======================

    public void StartSlow(float slowValue)
    {
        if (!bIsStopped)
        {
            bIsStopped = true;
            Time.timeScale = slowValue;
            StartCoroutine(Slowing());
            Debug.Log("큐브 슬로우 상태 시작 : " + Time.timeScale);
        }
    }

    IEnumerator Slowing()
    {
        yield return new WaitForSecondsRealtime(SlowTime);
        Time.timeScale = 1;
        Debug.Log("큐브 슬로우 상태 끝 : " + Time.timeScale);
        bIsStopped = false;
    }

    public void TakeDamageContinuous(int d)
    {
        if (HP < 0)
        { Debug.Log("Dead"); return; }

        float floatHP = HP;
        floatHP -=Time.deltaTime * d;
        HP = Mathf.RoundToInt( floatHP);
    }
}
