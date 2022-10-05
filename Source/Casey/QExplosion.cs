using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class QExplosion : MonoBehaviour
{
    [Tooltip("폭발데미지")] public float explosionDamage = 40.0f;
    [HideInInspector] public PhotonView pv;
    private GameObject hitObj;
    Collision other;

    void Start()
    {
        // pv 초기화
        pv = GetComponent<PhotonView>();
    }
    
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        other = collision;

        // 총알의 pv가 초기화되지 않았다면 초기화시킨다.
        if (pv == null)
            pv = GetComponent<PhotonView>();

        // 로컬에서만 피격 판정을 수행한다.
        if (!pv.IsMine)
            return;

        // 맞은 오브젝트를 구한다.
        hitObj = collision.transform.root.gameObject;
        PhotonView pv_hit = hitObj.GetComponent<PhotonView>();

        Debug.Log("QExplosion Hit Obj: " + hitObj.name);
        Debug.Log("QExplosion Hit PV: " + pv_hit);

        pv.RPC("RPC_Damage", RpcTarget.AllBuffered, pv_hit.ViewID);
    }

    [PunRPC]
    void RPC_Damage(int viewID)//데미지 부여
    {
        // 맞은게 상대방인지, 상대방의 총알인지 구분해 데미지를 준다.
        GameObject hitObj = PhotonNetwork.GetPhotonView(viewID).gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        if (hitPlayer != null)//캐릭터
        {
            Debug.Log("Damage: " + explosionDamage);
            if (hitObj.GetComponent<Casey>() != null)//케이시
            {
                hitObj.GetComponent<Casey>().TakeDamage((int)explosionDamage);
            }
            else//로라
            {
                hitObj.GetComponent<Rora>().TakeDamage((int)explosionDamage);
            }
        }
        else//투사체
        {
            if (other.gameObject.transform.root.GetComponent<ObjectWithHP>())
                other.gameObject.transform.root.GetComponent<ObjectWithHP>().TakeDamage((int)explosionDamage);
        }
    }
}
