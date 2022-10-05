using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Itempack : MonoBehaviour
{
    public int healPoint = 50;//플레이어 체력 50올림
    public int shieldPoint = 15;//방어막 수치 15올림
    public float ultPoint = 20;//궁극기 게이지 20%올림
    
    [Tooltip("이펙트")] public GameObject aura;

    [Tooltip("한번만 효과 주기위한 변수")] public bool effectable;

    [HideInInspector] public PhotonView pv;
    //아이템 먹는 효과
    //이펙트1초유지

    GameObject effect;
    GameObject hitObj;

    void Start()
    {
        effectable = true;
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
    }

    private void OnTriggerEnter(Collider collider)
    {
        hitObj = collider.transform.root.gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();

        if (hitPlayer == null) return;

        if (!effectable) return;

        InstanceEffect(hitObj.transform.position);
        effectable = false;
    }

    private void OnTriggerExit(Collider collider)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        effectable = true;
        hitObj = null;
    }
    
    void InstanceEffect(Vector3 pos)//이펙트 출력 & 데미지 설정
    {
        Debug.Log(gameObject.name);
        effect = Instantiate(aura, pos, Quaternion.identity);
        effect.transform.SetParent(hitObj.transform);
        if (gameObject.name == "HealPack(Clone)")
            HealPack();
        else if (gameObject.name == "ShieldPack(Clone)")
            ShieldPack();
        else if (gameObject.name == "UltPack(Clone)")
            UltPack();
        effectable = true;
        hitObj = null;
        Destroy(gameObject);
    }

    [PunRPC]
    void RPC_ItemInactive()//데미지 부여
    {
        effectable = true;
        hitObj = null;
        this.gameObject.SetActive(false);
    }

    void HealPack()
    {
        if (hitObj.GetComponent<Casey>() != null)//케이시
            hitObj.GetComponent<Casey>().IncreaseHP_Sync(healPoint);
        else//로라
            hitObj.GetComponent<Rora>().IncreaseHP_Sync(healPoint);
    }

    void ShieldPack()
    {
        if (hitObj.GetComponent<Casey>() != null)//케이시
            hitObj.GetComponent<Casey>().RPC_IncreaseShield(shieldPoint);
        else
            hitObj.GetComponent<Rora>().RPC_IncreaseShield(shieldPoint);
    }

    void UltPack()
    {
        if (hitObj.GetComponent<Casey>() != null)//케이시
            hitObj.GetComponent<Casey>().UpdatePerUltgauge(ultPoint);
        else//로라
            hitObj.GetComponent<SkillControl>().GetQskill().IncreaseCurCooltime(ultPoint);
    }
}
