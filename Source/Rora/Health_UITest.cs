using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health_UITest : MonoBehaviour
{
    public GameObject ShieldBar;
    public GameObject HPBar;
    public Image ShieldMaskImg;
    public Image ShieldBarImg;
    public Image HPBarImg;

    public float HPdefault;
    public float ShieldDefault;
    public float HPMaxAmount;
    public float ShieldMaxAmount;

    [Range(0.0f, 0.2f)]
    public float gaugeAmount;


    private void Start()
    {
        Initialized();
    }
    private void Initialized()
    {
        HPBarImg.fillAmount = HPdefault;
        ShieldBarImg.fillAmount = 0.9f;
    }

    private void Update()
    {
        HealthTest();
    }

    void HealthTest()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //ControlHP(1);
            Control(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            //ControlHP(-1);
            Control(-1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ControlShield(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ControlShield(-1);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        int mode = 2;

        switch (mode)
        {
            case 1:
                {
                    if (other.gameObject.name == "HPAdd")
                    {
                        ControlHP(1);
                        Debug.Log("HPAdd");
                    }
                    if (other.gameObject.name == "HPMinus")
                    {
                        ControlHP(-1);
                        Debug.Log("HPMinus");
                    }
                    if (other.gameObject.name == "ShieldAdd")
                    {
                        ControlShield(1);
                        Debug.Log("ShieldAdd");
                    }
                    if (other.gameObject.name == "ShieldMinus")
                    {
                        ControlShield(-1);
                        Debug.Log("ShieldMinus");
                    }
                }
                break;
            case 2: //수치 조절 하기
                {
                    if (other.gameObject.name == "HPAdd")
                    {
                        ControlHP(1);
                        Debug.Log("HPAdd");
                    }
                    if (other.gameObject.name == "HPMinus")
                    {
                        ControlHP(-1);
                        Debug.Log("HPMinus");
                    }

                }
                break;
        }

    }

    void Control(int p)
    {
        if (p > 0)
        {
            if (HPBarImg.fillAmount < HPMaxAmount)
            {
                HPBarImg.fillAmount += p * gaugeAmount;
                ShieldMaskImg.fillAmount -= p * gaugeAmount;
                ShieldBarImg.fillAmount += p * gaugeAmount;
            }

            else
            {
                ShieldBarImg.fillAmount += gaugeAmount;
            }
        }
        else if (p < 0)
        {
            HPBarImg.fillAmount += p * gaugeAmount;
            ShieldMaskImg.fillAmount -= p * gaugeAmount;
            ShieldBarImg.fillAmount += p * gaugeAmount;
        }
    }

    void ControlHP(int p)
    {
        if (HPBarImg.fillAmount < HPMaxAmount)
            HPBarImg.fillAmount += p * gaugeAmount;

        //shield 자리 옮겨주기
        ShieldMaskImg.fillAmount -= p * gaugeAmount;
        ShieldBarImg.fillAmount += p * gaugeAmount;

    }

    void ControlShield(int p)
    {
        if (p > 0)
        {
            if (HPBarImg.fillAmount > HPMaxAmount)
            {
                ShieldBarImg.fillAmount += p * gaugeAmount;
            }
        }
        else { ShieldBarImg.fillAmount += p * gaugeAmount; }
    }
}