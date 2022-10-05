using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System;

public class TurretEffect : MonoBehaviour
{
/*
    <���� ���>
        ������Ÿ�� : 5��
        ������ ���� 0.25
        1ƽ�� ������ 5
        ���ο� ���� : 20%
 */


    //========================== ��ž ���� ���� ==========================
    [Header("��ž")]
    [HideInInspector] public GameObject Effect;
    public float Size;
    [Tooltip("��ž �� ����")] public float radius;
    public float height;
    public float Interval;




    //========================== Q��ų ���� ���� ==========================
    List<GameObject> Turrets = new List<GameObject>();
    private float LifeTime = 5;
    private float damage = 5f;
    private float DamageInterval = 0.25f;
    private float SlowValue = 0.8f;
    private float KeepSlowTime = 3.0f;



    //========================== ��Ÿ ���� ==========================
    private Transform character;
    [HideInInspector] public PhotonView pv;
    private float timeSaver;
    private float limitTime = 25f;
    [HideInInspector] public PlayerAudio audioManager;
    private bool bTurretSpawned = false;

    private void Awake()
    {
        pv = this.GetComponent<PhotonView>(); //���� ���� �ʱ�ȭ

        this.GetComponent<CapsuleCollider>().height = height;
        this.GetComponent<CapsuleCollider>().center = new Vector3(0, height / 2, 0);
        Effect.GetComponent<Transform>().localScale = new Vector3(Size, Size, Size);

        // �ͷ��� ��ȯ�� �÷��̾ ã�´�.
        GameObject owner = null;
        Playable[] players = FindObjectsOfType<Playable>();
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<PhotonView>().Controller == pv.Owner)
            {
                owner = players[i].gameObject;
                break;
            }
        }

        // ������Ʈ Ǯ���� �ӽ÷� ������ ��� �׳� return�Ѵ�.
        if (owner == null) return;

        // Owner�κ��� ���� ������ ������ �ʱ�ȭ�Ѵ�.
        character = owner.transform;

        SkillControl sc = owner.GetComponent<SkillControl>();
        damage = sc.QSkill_Damage;
        DamageInterval = sc.QSkill_DamageInterval;
        LifeTime = sc.QSkill_LifeTime;
        SlowValue = sc.QSkill_SlowValue;
        KeepSlowTime = sc.QSkill_KeepSlowTime;
    }


    void Start()
    {
        if(!pv.IsMine)  return;

        float dis = Mathf.Sqrt(3) * radius * 1 / 2;

        StartCoroutine(InstantiateTurrets(new Vector3(0, 0, radius), 0));
        StartCoroutine(InstantiateTurrets(new Vector3(-dis, 0, -dis), Interval));
        StartCoroutine(InstantiateTurrets(new Vector3(dis, 0, -dis), Interval * 2));
    }

    IEnumerator InstantiateTurrets(Vector3 spawnpos, float interval)
    {
        yield return new WaitForSeconds(interval);
        
        // ��ž ���� �� �ʱ�ȭ
        GameObject turret = PhotonNetwork.Instantiate(
                    Path.Combine("PhotonPrefabs/RoraInstance/", "Turret"),
                    spawnpos, character.rotation);
        pv.RPC("RPC_InitTurret", RpcTarget.AllBuffered, 
            turret.GetComponent<PhotonView>().ViewID,
            LifeTime, damage, DamageInterval, SlowValue, KeepSlowTime);
        bTurretSpawned = true;
    }

    [PunRPC]
    private void RPC_InitTurret(
        int viewID, float lifeTime, float damage, float damageInterval, float slowValue, float keepSlowTime)
    {
        Turret turret = PhotonNetwork.GetPhotonView(viewID).GetComponent<Turret>();
        turret.InitTurret(lifeTime, damage, damageInterval, slowValue, keepSlowTime);
        turret.transform.SetParent(this.gameObject.transform, false);
        Turrets.Add(turret.gameObject);
    }

    void Update()
    {
        CheckDestroy();
    }

    private void CheckDestroy()
    {
        if(!bTurretSpawned) return;

        // �ͷ��� �� �ı��ƴ��� �˻��Ѵ�.
        for(int i = 0; i < Turrets.Count; i++)
        {
            if(Turrets[i])  return;
        }

        // �ͷ��� �� �ı��ƴٸ� �� ������Ʈ�� �ı��Ѵ�.
        PhotonNetwork.Destroy(gameObject);
    }
}
