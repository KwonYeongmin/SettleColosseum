using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MREffect : ObjectWithHP
{
    [Tooltip("체력")] public int defaultHp = 3;
    [Tooltip("데미지")] public float defaultDamage = 60f;
    [Tooltip("(Look)데미지")] public float damage;
    [Tooltip("기본 타격 이펙트")] public GameObject hiteffect;
    [Tooltip("몸 타격 이펙트")] public GameObject hitbodyeffect;
    [Tooltip("데미지 입히기 가능여부->콜라이더가 여러개일 때 한번 충돌시 한번만 데미지 주기위함")] public bool damageable;
    public PhotonView pv;
    float speed;
    RaycastHit rayHit;
    GameObject hitObj;
    Collision other;

    void OnEnable()
    {
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        hp = defaultHp;
        damage = defaultDamage;
        damageable = true;
        speed = transform.root.gameObject.GetComponent<MRBullet>().speed;
    }

    void Update()
    {
        SetInActive_TP();
    }

    private void OnCollisionEnter(Collision collision)
    {
        other = collision;

        // 총알의 pv가 초기화되지 않았다면 초기화시킨다.
        if(pv == null)
            pv = GetComponent<PhotonView>();
        damage = defaultDamage;

        // 로컬에서만 피격 판정을 수행한다.
        if(!pv.IsMine)
            return;

        // 맞은 오브젝트를 구한다.
        hitObj = collision.transform.root.gameObject;
        PhotonView pv_other = hitObj.GetComponent<PhotonView>();

        // 맞은 오브젝트가 정적 오브젝트라면 총알을 파괴하고 끝낸다.
        if (pv_other == null)
        {
            Destroy(gameObject);
            return;
        }

        // pv 소유권을 비교해 맞은게 상대편이라면 데미지를 입힌다.
        if (pv_other.Controller != pv.Owner)
        {
            if (!damageable) return;
            damageable = false;
            InstanceEffect(collision);
            pv.RPC("RPC_Damage", RpcTarget.AllBuffered, pv_other.ViewID); 
            TakeDamage(defaultHp);
        }
    }

    void InstanceEffect(Collision collision)//이펙트 출력 & 데미지 설정
    {
        if (collision.GetContact(0).otherCollider.gameObject.CompareTag("Head") ||
            collision.GetContact(0).otherCollider.gameObject.CompareTag("Body") ||
            collision.GetContact(0).otherCollider.gameObject.CompareTag("Dummy"))
        {
            Instantiate(hitbodyeffect, collision.GetContact(0).point, Quaternion.identity);
        }
        Instantiate(hiteffect, collision.GetContact(0).point, Quaternion.identity);

       // Debug.Log("RAttack Hit : " + collision.transform.root.name + " -> " + collision.transform.name);
    }

    [PunRPC]
    void RPC_Damage(int viewID)//데미지 부여
    {
        // 맞은게 상대방인지, 상대방의 총알인지 구분해 데미지를 준다.
        GameObject hitObj = PhotonNetwork.GetPhotonView(viewID).gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        if (hitPlayer != null)//캐릭터
        {
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

        // 궁극기 게이지를 채운다
        Casey owner = transform.parent.GetComponent<MRBullet>().Owner.GetComponent<Casey>();
        if (owner.pv.IsMine)
        {
            owner.UpdateUltgauge((int)damage);
        }
    }

    void SetInActive_TP()
    {
        if (hp <= 0)
            Destroy(gameObject);
    }

}