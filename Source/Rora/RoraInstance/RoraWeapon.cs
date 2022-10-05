using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoraWeapon : MonoBehaviour
{
    //================================ 이펙트 ============================
    [Header("이펙트")]
    public GameObject[] PointEffects;



    //================================ 간접공격 관련 ============================
    [Header("간접공격 데미지")]
    public int DefaultDamage;
    [HideInInspector] public int Damage;
    [Range(0.0f, 1.0f)]
    public float Head_coef = 0.5f;

    //================================ 기타변수============================

    [HideInInspector] public bool bOnVAttack = false;
    [HideInInspector] public bool bIsDamaged = false;
    private GameObject HitObj;
    private Collision HitBox;



    //================================ 포톤 ============================
    [HideInInspector] public PhotonView pv;



    //================================ audio ============================
    [HideInInspector] public PlayerAudio AudioManager;


    void Start()
    {
        Damage = DefaultDamage;
        bOnVAttack = false;
        bIsDamaged = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // collision 초기화
        HitBox = collision;

        // 공격하는 자신 구하기
        HitObj = collision.transform.gameObject;
        PhotonView pv_other = HitObj.GetComponent<PhotonView>();

        if (pv_other == null) return;

        if (pv != pv_other)
        {
            // 이미 맞았으면 끝내기
            if (bIsDamaged) return;
            bIsDamaged = true;

            InstaniateEffect(collision); //이펙트 생성, 데미지 계산
            GiveDamage(HitObj);
        }
    }


    // 데미지 입히기
    void GiveDamage(GameObject hitObj)
    {
        Playable hitplayer = hitObj.GetComponent<Playable>();

        if (hitplayer.gameObject.transform.root.GetComponent<Casey>() != null)
        {
            hitplayer.gameObject.transform.root.GetComponent<Casey>().TakeDamage_Sync((int)Damage);
        }
        else if (hitplayer.gameObject.transform.root.GetComponent<Rora>() != null)
        {
            hitplayer.gameObject.transform.root.GetComponent<Rora>().TakeDamage_Sync((int)Damage);
        }
    }

    // 이펙트 생성, damage 계산
    void InstaniateEffect(Collision collision)
    {
        GameObject effect = null;
        if (collision.GetContact(0).thisCollider.transform.tag == "Head")
        {
             effect = Instantiate(PointEffects[0], collision.GetContact(0).point, Quaternion.identity);
            Damage = DefaultDamage + Mathf.RoundToInt(DefaultDamage * Head_coef);

            // Head 판정 시 audio 재생
            /*
            if (collision.collider.gameObject.transform.root.GetComponent<PlayerAudio>() != null)
            {
                collision.collider.gameObject.transform.root.GetComponent<PlayerAudio>().PlayheadShot();
            }*/

        }
        else if (collision.GetContact(0).thisCollider.transform.tag == "Body")
        {
             effect = Instantiate(PointEffects[1], collision.GetContact(0).point, Quaternion.identity);
            Damage = DefaultDamage;
        }
        
        Destroy(effect, 1f);
    }

    // 오디오 플레이
    /*
    void PlayEnemyAudio(Collision other)
    {
        if (!other.collider.gameObject.GetComponent<AudioSource>())
        {
            other.collider.gameObject.AddComponent<AudioSource>().clip = AudioManager.InstanceClip[1];
            other.collider.gameObject.GetComponent<AudioSource>().Play(); //재생
        }
        else
        {
            other.collider.gameObject.GetComponent<AudioSource>().clip = AudioManager.InstanceClip[1];
            other.collider.gameObject.GetComponent<AudioSource>().Play(); //재생
        }
    }*/
}


