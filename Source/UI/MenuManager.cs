using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MenuManager : MonoBehaviour
{
    // 싱글턴 정의
    public static MenuManager menuManager;
    public static MenuManager Inst { get { return menuManager; } }

    void Start()
    {
        if (menuManager)
        {
            Destroy(this);
            return;
        }
        menuManager = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("OptionWindows")]
    public GameObject MenuUI;
    public GameObject MenuPanel;
    public GameObject OptionWindow;
    public GameObject GiveupPopup;
    public GameObject Panel;

    [HideInInspector] public int windowToggleState = 0;

    [Header("Audio")]
    public AudioMixer[] Mixers;
    public Slider[] VolumeSlider;
   // public int MixerNum = 3;
    private float[] Value = new float[3];
    public PlayAudio2D PlayUIAudio;

    [Header("Mouse")]
    public Slider MouseSlider;
    private float MouseValue = 0;


    [Header("Toggle")]
    [Tooltip("true면 근접 V, F스킬 F / False면 근접 F, F스킬 V")]
    [HideInInspector]public int bToggle = 0;
    public Sprite[] ToggleImages;
    public Image ToggleImg;


    // ============================= 함수 =============================


    private void OnEnable()
    {
        MenuUI.SetActive(true);

        if (GetComponent<PhotonView>().ViewID != 990)
            GetComponent<PhotonView>().ViewID = 990;

        for (int i = 0; i < 3; i++) Value[i] = VolumeSlider[i].value;
        MouseValue = MouseSlider.value;
        ToggleImg.sprite = ToggleImages[bToggle];
        
    }


    private void Update()
    {
        if(GetComponent<PhotonView>().ViewID != 990)
            GetComponent<PhotonView>().ViewID = 990;

        if (Input.GetKeyDown(KeyManager.Inst.Menu))
        {
            windowToggleState++;
            PlayUIAudio.PlaySound(3);
        }
       
        windowToggleState %= 2;

        if (windowToggleState % 2 == 0) SetActiveMode(false);
        else
        {
            SetActiveMode(true);
        }

        if (MenuPanel.activeSelf) Time.timeScale = 0;
        else
        {
            if (!bTest)
            {
                StartCoroutine(ReturnTimescale());
                bTest = true;
            }
            // 
            Time.timeScale = 1;
        }
    }
    private bool bTest=false;
    IEnumerator ReturnTimescale()
    {
        yield return new WaitForSeconds(0.01f);
        Time.timeScale = 1;
        bTest = false;
    }

    private void SetActiveMode(bool b)
    {
        MenuPanel.SetActive(b);
        Panel.SetActive(b);
    }

    public void OnClickExit(GameObject UI)
    {
        UI.SetActive(false);
    }


    // ============================= 전투재개 =============================
    public void OnGameContinue()
    {
        windowToggleState++;
    }



    // ============================= 옵션 =============================
    public void OnActiveOption()
    {
        // 옵션창 켜기
        OptionWindow.SetActive(true);
        // 팝업창 닫기
        GiveupPopup.SetActive(false);
    }

    // ================ 사운드 볼륨 조절 ================

    //BGMVolume
    public void ControlBGMVolume()
    {
        Value[0] = VolumeSlider[0].value;
        Mixers[0].SetFloat("BGMVolume", Mathf.Log10(Value[0]) * 20);
    }

    // SFXVolume
    public void ControlSFXVolume()
    {
        Value[1] = VolumeSlider[1].value;
        Mixers[0].SetFloat("SFXVolume", Mathf.Log10(Value[1]) * 20);
    }

    // CharacterVolume
    public void ControlCharacterVolume()
    {
        Value[2] = VolumeSlider[2].value;
        Mixers[1].SetFloat("CharacterVolume", Mathf.Log10(Value[2]) * 20);
    }

    // ================ 마우스 민감도 조절 ================

    public void ControlMouseSensibility()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            Debug.Log("태그로 Player 찾음");
            if (player.GetComponent<Rora>() != null)
            {
                MouseValue = MouseSlider.value;
                Debug.Log("로라 찾음 : "+ player.GetComponent<Rora>().mouseSensitivity.ToString());
                player.GetComponent<Rora>().mouseSensitivity = MouseValue*4;
            }
            else if (player.GetComponent<Casey>() != null)
            {
                Debug.Log("Casey 찾음 : " + player.GetComponent<Casey>().mouseSensitivity.ToString());
                MouseValue = MouseSlider.value;
                player.GetComponent<Casey>().mouseSensitivity = MouseValue*4;
            }
        }
    }

    // ================ 토글 키 ================

    public void ControlToggleKey()
    {
        bToggle++;
        bToggle %= 2;
        KeyManager.Inst.ChangeKey();
        ToggleImg.sprite = ToggleImages[bToggle];
    }




    // ============================= 게임 종료 팝업창 =============================

    public void OnActiveGiveupPopup()
    {
        // 옵션창 닫기
        OptionWindow.SetActive(false);
        // 팝업창 닫기
        GiveupPopup.SetActive(true);
    }

    public void OnClickExit()
    {
        Application.Quit();
    }
}
