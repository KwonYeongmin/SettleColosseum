using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System;

public class TurretEffect : MonoBehaviour
{
/*
    <변수 목록>
        라이프타임 : 5초
        데미지 간격 0.25
        1틱당 데미지 5
        슬로우 강도 : 20%
 */


    //========================== 포탑 관련 변수 ==========================
    [Header("포탑")]
    [HideInInspector] public GameObject Effect;
    public float Size;
    [Tooltip("포탑 간 간격")] public float radius;
    public float height;
    public float Interval;




    //========================== Q스킬 관련 변수 ==========================
    List<GameObject> Turrets = new List<GameObject>();
    private float LifeTime = 5;
    private float damage = 5f;
    private float DamageInterval = 0.25f;
    private float SlowValue = 0.8f;
    private float KeepSlowTime = 3.0f;



    //========================== 기타 변수 ==========================
    private Transform character;
    [HideInInspector] public PhotonView pv;
    private float timeSaver;
    private float limitTime = 25f;
    [HideInInspector] public PlayerAudio audioManager;
    private bool bTurretSpawned = false;

    private void Awake()
    {
        pv = this.GetComponent<PhotonView>(); //포톤 변수 초기화

        this.GetComponent<CapsuleCollider>().height = height;
        this.GetComponent<CapsuleCollider>().center = new Vector3(0, height / 2, 0);
        Effect.GetComponent<Transform>().localScale = new Vector3(Size, Size, Size);

        // 터렛을 소환한 플레이어를 찾는다.
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

        // 오브젝트 풀에서 임시로 생성한 경우 그냥 return한다.
        if (owner == null) return;

        // Owner로부터 각종 정보를 가져와 초기화한다.
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
        
        // 포탑 생성 및 초기화
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

        // 터렛이 다 파괴됐는지 검사한다.
        for(int i = 0; i < Turrets.Count; i++)
        {
            if(Turrets[i])  return;
        }

        // 터렛이 다 파괴됐다면 이 오브젝트를 파괴한다.
        PhotonNetwork.Destroy(gameObject);
    }
}
