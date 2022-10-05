using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class QBullet : ObjectWithHP
{
    [Header("Q미사일")]
    [Tooltip("속도")] public float speed = 5.0f;
    [Tooltip("라이프타임")] public float defaultLifetime = 10.0f;
    [Tooltip("(Look)라이프타임")] public float lifeTime = 0f;
    [Tooltip("체력")] public int defaultHp = 10;
    [Tooltip("충돌데미지")] public float crashDamage = 50.0f;
    [Tooltip("(Look)데미지")] public float damage;

    //[Tooltip("총구이펙트")] public GameObject muzzleEffect;
    //[Tooltip("꼬리이펙트")] public GameObject tailEffect;
    [Tooltip("폭발이펙트")] public GameObject hitEffect;
    [Tooltip("몸 타격 이펙트")] public GameObject hitbodyEffect;
    [HideInInspector] public PhotonView pv;

    [Header("audio")]
    public AudioClip[] audioClips;
    private AudioSource audioSource;

    public GameObject owner;
    private GameObject hitObj;
    Collision other;

    Vector3 dir;

    void OnEnable()
    {
        // pv 초기화
        pv = GetComponent<PhotonView>();

        // 오디오 소스 초기화
        audioSource = this.GetComponent<AudioSource>();
        // 오디오 재생
        PlayQSkillAudio(0);


        // 총알을 쏜 플레이어를 찾는다.
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

        GameObject target = owner.transform.GetChild(3).GetChild(1).gameObject;
        dir = target.transform.forward;
        transform.LookAt(target.transform);
        hp = defaultHp;
        damage = crashDamage;
    }

    void Update()
    {
        lifeTime += Time.deltaTime;
        GetComponent<Rigidbody>().AddForce(dir * speed);
        //transform.position += dir * speed * Time.deltaTime;
        if (lifeTime >= defaultLifetime || hp <= 0)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        other = collision;

        // 총알의 pv가 초기화되지 않았다면 초기화시킨다.
        if(pv == null)
            pv = GetComponent<PhotonView>();
        damage = crashDamage;

        // 로컬에서만 피격 판정을 수행한다.
        if(!pv.IsMine)
            return;

        // 맞은 오브젝트를 구한다.
        hitObj = collision.transform.root.gameObject;
        PhotonView pv_other = hitObj.GetComponent<PhotonView>();

        Debug.Log("Hit Obj: " + hitObj.name);
        Debug.Log("Hit PV: " + pv_other);

        // 맞은 오브젝트가 정적 오브젝트라면 폭발 프리팹를 출력한뒤 총알을 파괴하고 끝낸다.
        if (pv_other == null)
        {
            Debug.Log("Hello!");
            PhotonNetwork.Instantiate(
                "PhotonPrefabs/CaseyInstance/BigExplosion", collision.GetContact(0).point, Quaternion.identity);
            Destroy(gameObject);
            return;
        }

        // pv 소유권을 비교해 맞은게 상대편이라면 데미지를 입힌다.
        if (pv_other.Controller != pv.Owner)
        {
            InstanceEffect(collision);
            pv.RPC("RPC_Damage", RpcTarget.AllBuffered, pv_other.ViewID);
            TakeDamage(defaultHp);
            PlayQSkillAudio(1);
        }
    }

    void InstanceEffect(Collision collision)//이펙트 출력 & 데미지 설정
    {
        if (collision.GetContact(0).otherCollider.gameObject.CompareTag("Head") ||
            collision.GetContact(0).otherCollider.gameObject.CompareTag("Body") ||
            collision.GetContact(0).otherCollider.gameObject.CompareTag("Dummy"))
        {
            Instantiate(hitbodyEffect, collision.GetContact(0).point, Quaternion.identity);
        }
        Instantiate(hitEffect, collision.GetContact(0).point, Quaternion.identity);

        Debug.Log("QSkill Hit : " + collision.transform.root.name + " -> " + collision.transform.name);
    }

    [PunRPC]
    void RPC_Damage(int viewID)//데미지 부여
    {
        // 맞은게 상대방인지, 상대방의 총알인지 구분해 데미지를 준다.
        GameObject hitObj = PhotonNetwork.GetPhotonView(viewID).gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        if (hitPlayer != null)//캐릭터
        {
            //Debug.Log("Damage: " + damage);
            if (hitObj.GetComponent<Casey>() != null)//케이시
            {
                hitObj.GetComponent<Casey>().TakeDamage((int)damage);
            }
            else//로라
            {
                hitObj.GetComponent<Rora>().TakeDamage((int)damage);
            }
        }
        else//투사체
        {
            if (other.gameObject.transform.root.GetComponent<ObjectWithHP>())
                other.gameObject.transform.root.GetComponent<ObjectWithHP>().TakeDamage((int)damage);
        }
    }

    private void PlayQSkillAudio(int index)
    {
        audioSource.clip = audioClips[index];
        audioSource.Play();
    }
}