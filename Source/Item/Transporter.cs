using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Transporter : MonoBehaviour
{
    public enum Item
    {
        Heal = 0,
        Shield = 1,
        Ult = 2
    }

    [Tooltip("����")] public GameObject HealPack;
    [Tooltip("HealPack����Ÿ��")] public float defaultHealRegenTime = 10.0f;
    Color healColor = new Color(0, 0.753f, 1, 1);

    [Tooltip("�ǵ���")] public GameObject ShieldPack;
    [Tooltip("Shield����Ÿ��")] public float defaultShieldRegenTime = 10.0f;
    Color shieldColor = new Color(1, 0.969f, 0, 1);

    [Tooltip("�ñر���")] public GameObject UltPack;
    [Tooltip("Ult����Ÿ��")] public float defaultUltRegenTime = 10.0f;
    Color ultColor = new Color(0.847f, 0.176f, 0.188f, 1);

    [Header("============���� ������============")]
    //[Tooltip("Ult����Ÿ��")] public float
    [HideInInspector] public PhotonView pv;
    [Tooltip("������ ����")] public int itemnum;

    [Tooltip("��")] public GameObject pack;
    [Tooltip("��")] public GameObject packname;
    [Tooltip("������ ��ġ")] public GameObject itemPos;
    [Tooltip("����Ÿ��")] public float defaultRegenTime = 0;
    [Tooltip("(look)����Ÿ��")] public float RegenTime = 0;
    [Tooltip("���ǵ�")] public float updateSpeed = 90;
    [Tooltip("���ǵ�")] public float speed = 0;
    Color color;
    bool bRegen = false;

    Material material;

    public void Start()
    {
        material = GetComponent<MeshRenderer>().materials[0];
        material.SetFloat("_Speed", speed);
        pv = GetComponent<PhotonView>();
        
        bRegen = false;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (pack != null) return;

        //�ٽ� ������ ����
        if (!bRegen)
        {
            SelectPack();
            pv.RPC("RPC_SetColor", RpcTarget.AllBuffered);
            pv.RPC("RPC_RegenPack", RpcTarget.AllBuffered);
        }
        RegenTime = (RegenTime - Time.deltaTime > 0) ? RegenTime - Time.deltaTime : 0;

        if (RegenTime > 0) return;

        //������ ����
        pv.RPC("RPC_Respawn", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_SetColor()
    {
        GetComponent<MeshRenderer>().materials[0].SetColor("_Color_Em", color);
    }
    void SelectPack()
    {
        itemnum = Random.Range(0, 3);
        switch (itemnum)
        {
            case (int)Item.Heal:
                {
                    packname = HealPack;
                    defaultRegenTime = defaultHealRegenTime;
                    color = healColor;

                    break;
                }
            case (int)Item.Shield:
                {
                    packname = ShieldPack;
                    defaultRegenTime = defaultShieldRegenTime;
                    color = shieldColor;

                    break;
                }
            case (int)Item.Ult:
                {
                    packname = UltPack;
                    defaultRegenTime = defaultUltRegenTime;
                    color = ultColor;

                    break;
                }
        }
    }

    [PunRPC]
    void RPC_RegenPack()
    {
        RegenTime = defaultRegenTime;
        GetComponent<MeshRenderer>().materials[0].SetFloat("_Speed", updateSpeed);
        bRegen = true;
    }

    [PunRPC]
    void RPC_Respawn()
    {
        bRegen = false;
        GetComponent<MeshRenderer>().materials[0].SetFloat("_Speed", speed);
        pack=PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/ItemInstance", packname.name),
            itemPos.transform.position, itemPos.transform.rotation);
    }
}