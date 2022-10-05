using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TriggerHitEffect : MonoBehaviour
{
    [Tooltip("타격 이펙트")] public GameObject hitEffect;
    [Tooltip("(Look)데미지")] public float damage;
    [Tooltip("데미지 입히기 가능여부->콜라이더가 여러개일 때 한번 충돌시 한번만 데미지 주기위함")] public bool damageable;

    Casey casey;
    [HideInInspector] PlayerAudio AudioManager;

    private GameObject hitObj;
    private Collider other;

    void Start()
    {
        damageable = true;
    }

    private void OnTriggerEnter(Collider collider)
    {
        other = collider;

        // 공격하는 자기 자신을 구한다.
        PhotonView pv_mine = transform.root.GetComponent<PhotonView>();

        // 맞은 오브젝트를 구한다.
        hitObj = collider.transform.root.gameObject;
        PhotonView pv_other = hitObj.GetComponent<PhotonView>();

        // 맞은 오브젝트가 정적 오브젝트라면 그냥 끝낸다.
        if (pv_other == null)
        {
            return;
        }

        if (pv_mine != pv_other)
        {
            if(!damageable) return;
            damageable = false;
            InstanceEffect(collider);
            Damage(hitObj);
        }
    }

    private void OnTriggerEnd(Collider collider)
    {
        damageable = true;
    }

    void InstanceEffect(Collider collider)//이펙트 출력 & 데미지 설정
    {
        if (collider.CompareTag("Body") || collider.CompareTag("Dummy"))
        {
            Instantiate(hitEffect, collider.transform.position, Quaternion.identity);
            {
                if(transform.gameObject.name== "VAttackHitBox")
                    damage = transform.root.gameObject.GetComponent<Casey>().Vdamage;
                else
                    damage = transform.root.gameObject.GetComponent<Casey>().dash_damage;
            }
        }
    }

    void Damage(GameObject hitObj)//데미지 부여
    {
        // 맞은게 상대방인지, 상대방의 총알인지 구분해 데미지를 준다.
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        if (hitPlayer != null)//캐릭터
        {
            Debug.Log("Damage: " + damage);
            if (hitObj.GetComponent<Casey>() != null)//케이시
            {
                hitObj.GetComponent<Casey>().TakeDamage_Sync((int)damage);
            }
            else//로라
            {
                hitObj.GetComponent<Rora>().TakeDamage_Sync((int)damage);
            }
        }

        transform.root.GetComponent<Casey>().UpdateUltgauge((int)damage);
    }

}