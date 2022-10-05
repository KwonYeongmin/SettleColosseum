using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MenuManager : MonoBehaviour
{
    // �̱��� ����
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
    [Tooltip("true�� ���� V, F��ų F / False�� ���� F, F��ų V")]
    [HideInInspector]public int bToggle = 0;
    public Sprite[] ToggleImages;
    public Image ToggleImg;


    // ============================= �Լ� =============================


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


    // ============================= �����簳 =============================
    public void OnGameContinue()
    {
        windowToggleState++;
    }



    // ============================= �ɼ� =============================
    public void OnActiveOption()
    {
        // �ɼ�â �ѱ�
        OptionWindow.SetActive(true);
        // �˾�â �ݱ�
        GiveupPopup.SetActive(false);
    }

    // ================ ���� ���� ���� ================

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

    // ================ ���콺 �ΰ��� ���� ================

    public void ControlMouseSensibility()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            Debug.Log("�±׷� Player ã��");
            if (player.GetComponent<Rora>() != null)
            {
                MouseValue = MouseSlider.value;
                Debug.Log("�ζ� ã�� : "+ player.GetComponent<Rora>().mouseSensitivity.ToString());
                player.GetComponent<Rora>().mouseSensitivity = MouseValue*4;
            }
            else if (player.GetComponent<Casey>() != null)
            {
                Debug.Log("Casey ã�� : " + player.GetComponent<Casey>().mouseSensitivity.ToString());
                MouseValue = MouseSlider.value;
                player.GetComponent<Casey>().mouseSensitivity = MouseValue*4;
            }
        }
    }

    // ================ ��� Ű ================

    public void ControlToggleKey()
    {
        bToggle++;
        bToggle %= 2;
        KeyManager.Inst.ChangeKey();
        ToggleImg.sprite = ToggleImages[bToggle];
    }




    // ============================= ���� ���� �˾�â =============================

    public void OnActiveGiveupPopup()
    {
        // �ɼ�â �ݱ�
        OptionWindow.SetActive(false);
        // �˾�â �ݱ�
        GiveupPopup.SetActive(true);
    }

    public void OnClickExit()
    {
        Application.Quit();
    }
}
