using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class TooltipUI : MonoBehaviour
{
    // ?? ????
    [Header("UI")]
    public Image mapImg;
    public TextMeshProUGUI TXT_mapName;
    

    [Header("?? ????")]
    public Sprite[] maps;
    public string[] mapName;

    [HideInInspector]public int index = 0;
    private int curIndex;
    private int tempIndex;


    [Header("?? ???? ??????")]
    public GameObject[] MapTooltip;
    private GameObject[] TooltipImgs = new GameObject[5];
    private int ImgNum = 5;
    private int ImgIndex = 0;
    public TextMeshProUGUI TXT_map1_info;
    public TextMeshProUGUI TXT_map2_info;
    public string[] map1_info;
    public string[] map2_info;


    [Header("????")]
    public GameObject[] Btn;


    PhotonView pv;
    
    void Start()
    {
        pv = GetComponent<PhotonView>();
        for (int i = 0; i < maps.Length; i++) MapTooltip[i].SetActive(false);
    }


    private void OnEnable()
    {
        // ???????? ?? ???? ??????????
        HashTable ht = PhotonNetwork.CurrentRoom.CustomProperties;

        if ((MapName)ht["MapName"] == MapName.Wegheim)      index = 0;
        else if ((MapName)ht["MapName"] == MapName.Axi)     index = 1;

        if (PhotonNetwork.IsMasterClient) //?????????? ?? ???? ?????? ????????.
            for (int i = 0; i < 2; i++)
                Btn[i].SetActive(true);
        else
            for (int i = 0; i < 2; i++)
                Btn[i].SetActive(false);

      //  Debug.Log("Enter Index: " + index);
    }

    void Update()
    {
     //   Debug.Log("Update Index: " + index);
        mapImg.sprite = maps[index];
        TXT_mapName.text = mapName[index];

        for (int i = 0; i < ImgNum; i++)
        {
            TooltipImgs[i] = 
                MapTooltip[index].transform.GetChild(1).
                gameObject.transform.GetChild(0).
                gameObject.transform.GetChild(i).gameObject;
        }
    }

    public void OnClickMap()
    {
        pv.RPC("RPC_ChangeMapIndex", RpcTarget.All);
    }


    //============================???? ???? ============================
    private bool bIsOpening = false;

    public void OnClickMapTooltip()
    {
        MapTooltip[index].SetActive(true);
        bIsOpening = true;
        if (ImgIndex != 0)  TooltipImgs[ImgIndex].GetComponent<Image>().enabled = false;
        ImgIndex = 0;
        TooltipImgs[ImgIndex].GetComponent<Image>().enabled = true;

        if (index == 0)     TXT_map1_info.text = map1_info[ImgIndex];
        else                TXT_map2_info.text = map2_info[ImgIndex];
    }

    [PunRPC]
    private void RPC_ChangeMapIndex()
    {
        int prevIndex = index; 

        index++;
        index %= 2;
        curIndex = index;

        if (bIsOpening) 
            index = prevIndex;
       
        HashTable ht = PhotonNetwork.CurrentRoom.CustomProperties;
        ht["MapName"] = (MapName)index;
        PhotonNetwork.CurrentRoom.SetCustomProperties(ht);

        NetworkManager.Inst.curMap = (MapName)ht["MapName"];

        Debug.Log("MapName: " + PhotonNetwork.CurrentRoom.CustomProperties["MapName"]);
    }

    public void OnClickMapTooltipExit()
    {
        MapTooltip[index].SetActive(false);
        if (curIndex != index) index = curIndex;
         bIsOpening = false;
        if (ImgIndex!=0) TooltipImgs[ImgIndex].GetComponent<Image>().enabled = false;
        ImgIndex = 0;
        TooltipImgs[ImgIndex].GetComponent<Image>().enabled = true;
    }

    public void OnClickMapImgPrev()
    {
        if (ImgIndex < 1) return;

        TooltipImgs[ImgIndex].GetComponent<Image>().enabled = false;
        TooltipImgs[--ImgIndex].GetComponent<Image>().enabled = true;

        if (index == 0)
        {
            TXT_map1_info.text = map1_info[ImgIndex];
        }
        else
        {
            TXT_map2_info.text = map2_info[ImgIndex];
        }
    }


    public void OnClickMapImgNext()
    {
        if (ImgIndex > ImgNum - 2) return;

        TooltipImgs[ImgIndex].GetComponent<Image>().enabled = false;
        TooltipImgs[++ImgIndex].GetComponent<Image>().enabled = true;

        if (index == 0)
        {
            TXT_map1_info.text = map1_info[ImgIndex];
        }
        else
        {
            TXT_map2_info.text = map2_info[ImgIndex];
        }
    }
}
