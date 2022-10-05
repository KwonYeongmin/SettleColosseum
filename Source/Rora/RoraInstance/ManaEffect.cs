using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ManaEffect : ObjectWithHP
{
    //==========================Effect==========================
    public GameObject impactParticle;
    public GameObject[] PointEffect;

    //========================== 기타 변수 ==========================
    [HideInInspector] public GameObject owner;
    private bool bIsDamaged = false;
    private Collider other;
    private GameObject hitObj;

    //========================== Mana 투사체 값 ==========================
    [Header("투사체 HP 관련 설정")]
    public int DefaultHP = 1;
    public float HPDamage = 1f;

    [Header("투사체 데미지, 헤드 계수 설정")]
    public int defaultDamage;
    [Range(0.0f, 2.0f)]
    public float Head_coef;
    [HideInInspector] public int damage;

    //========================== 방향 변수 ============================
    Vector3 origin;
    Vector3 dir;
    public float Speed;

    //========================== 유지 시간 ============================
    float lifeTime = 0f;
    public float defaultLifetime = 10f;

    //========================== 포톤 변수 ============================
    [HideInInspector] public PhotonView pv;

    //========================== 오디오 변수 ==========================
    [HideInInspector] public PlayerAudio audioManager;
    private AudioSource audioSource;

    void OnEnable()
    {
        // pv 초기화
        pv = GetComponent<PhotonView>();

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

        // 위치를 초기화한다.
        transform.position = owner.transform.GetChild(1).position + (owner.transform.GetChild(1).forward * 3f);

        // 총알이 날아갈 방향을 구한다.
        dir = owner.transform.GetChild(1).GetComponent<Camera>().transform.forward;

        // 일부 레이어를 레이캐스트에서 제외한다.
        int layerMask = (1 << 11) + (1 << 12) + (1 << 14);
        layerMask = ~layerMask;

        audioSource = this.GetComponent<AudioSource>();
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        hp = DefaultHP;
        damage = defaultDamage;
        bIsDamaged = false;
    }

    void Update()
    {
        // 유지 시간이 전부 지났다면 파괴한다.
        lifeTime += Time.deltaTime;
        if (lifeTime >= defaultLifetime)
            Destroy(gameObject);

        // 총알을 날아가게 한다.
        Trajectory_TP();
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("FSkill Collision With: " + collider.name);

        // 콜리전 초기화
        other = collider;

        //damage 초기화
        damage = defaultDamage;

        //맞은게 자기 자신이라면 무시한다.
        if(collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            return;

        //맞은 오브젝트 구하기
        hitObj = collider.transform.gameObject;
        PhotonView pv_other = hitObj.GetComponent<PhotonView>();

        //맞은 오브젝트가 정적 오브젝트(포톤뷰로 생성된 오브젝트가 아니)라면 총알을 파괴하고 끝난다.
        if (pv_other == null) { Destroy(this.gameObject); return; }

        if (bIsDamaged) return; //이미 맞은 오브젝트라면 리턴

        bIsDamaged = true;

        audioSource.Play();

        InstantiateEffect(collider); //이펙트 생성 및 데미지 계산
        pv.RPC("RPC_GiveDamage", RpcTarget.AllBuffered, pv_other.ViewID); //HP 동기화
        CheckProjectileHP();
        //PlayEnemyAudio(collision);
    }


    private void CheckProjectileHP()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);

            ParticleSystem[] trails = GetComponentsInChildren<ParticleSystem>();
            //Component at [0] is that of the parent i.e. this object (if there is any)
            for (int i = 1; i < trails.Length; i++)
            {
                ParticleSystem trail = trails[i];
                if (!trail.gameObject.name.Contains("Trail"))
                    continue;

                trail.transform.SetParent(null);
                Destroy(trail.gameObject, 2);
            }
        }
        
    }

    // 데미지 입히기
    [PunRPC]
    void RPC_GiveDamage(int ViewID)
    {
        GameObject hitObj = PhotonNetwork.GetPhotonView(ViewID).gameObject;
        Playable hitplayer = this.hitObj.GetComponent<Playable>();

        if (hitplayer != null)// 캐릭터라면
        {
            if (hitObj.CompareTag("BlackHole"))    // 맞춘게 블랙홀일 경우
            {
                hitObj.GetComponent<BlackHole>().Absorb(damage);
                Debug.Log("Hit Absorbed Damage: " + hitObj.GetComponent<BlackHole>().absorbedDamage);
                return;
            }

            if (hitObj.GetComponent<Casey>() != null) hitObj.GetComponent<Casey>().TakeDamage((int)damage); //케이시라면
            if (hitObj.GetComponent<Rora>() != null) hitObj.GetComponent<Rora>().TakeDamage((int)damage); //로라라면
            Destroy(gameObject);
        }
        else // 투사체라면
        {
            if (other.gameObject.transform.root.GetComponent<ObjectWithHP>() != null)
                other.gameObject.transform.root.GetComponent<ObjectWithHP>().TakeDamage((int)damage);
        }
    }

    // 이펙트 생성 및 데미지 갱신 및 오디오 갱신
    void InstantiateEffect(Collider collider)
    {
        GameObject effect = null;

        effect = Instantiate(
            impactParticle, 
            collider.transform.position, 
            Quaternion.identity);

        if (collider.gameObject.CompareTag("Head"))
        {
            damage = defaultDamage + Mathf.RoundToInt(defaultDamage * Head_coef);
        }
       else if (collider.gameObject.CompareTag("Body"))
        {
            damage = defaultDamage;
        }
        Destroy(effect, 1f);
    }

    // 오디오 플레이
    /*
    void PlayEnemyAudio(Collision collision)
    {
        if (!collision.collider.gameObject.GetComponent<AudioSource>())
        {
        collision.collider.gameObject.AddComponent<AudioSource>().clip = audioManager.InstanceClip[1];
        collision.collider.gameObject.GetComponent<AudioSource>().Play(); //재생
        }
        else
        {
        collision.collider.gameObject.GetComponent<AudioSource>().clip = audioManager.InstanceClip[1];
        collision.collider.gameObject.GetComponent<AudioSource>().Play(); //재생
        }
    }*/

    void Trajectory_TP()
    {
        transform.position += dir * Speed * Time.deltaTime;
    }
}


