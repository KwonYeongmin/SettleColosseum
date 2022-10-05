using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BlackHole : ObjectWithHP, IPunPrefabPool
{

    //========================== Blackhole 관련 변수 ==========================
    public float maxSize = 3.0f;
    [Range(2, 10)] public float GrowingSpeed = 10.0f;
    [Range(2, 20)] public float DisappearSpeed = 10.0f;



    //========================== 기타 변수 ==========================
    [HideInInspector] public Animator animator;
    private int State = 0;


    //========================== E스킬 관련 변수 ==========================
    public float absorbedDamage = 0f;
    public int GetAbsorbedDamage() { return Mathf.RoundToInt(absorbedDamage * damage_coef); }
    float PercentValue = 0.2f;
    [HideInInspector] public bool bIsCC = false;

    [HideInInspector]
    public PhotonView pv;

    public AudioClip[] BlackholeAudio;
    AudioSource audioSource;

    public float damage_coef = 0.5f;

    //========================== 블랙홀 상태 관련 함수==========================
    public void UpdateState(int value)   {State = value;}
    public int GetBlackMode()  {return State; }

    //========================== 충돌 체크 관련 함수 ==========================
    private Collider Other;
    private GameObject HitObj;

    //========================== Start 및 초기화 ==========================
    void OnEnable()
    {
        pv = GetComponent<PhotonView>(); //포톤 초기화
        absorbedDamage = 0;
        Initialized();
    }

    private void Initialized()
    {
        // 총알을 쏜 플레이어를 찾는다.
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

        // 3인칭 레이저 발사 위치와 부모 오브젝트 transform을 설정한다.
        transform.parent = owner.transform.GetChild(1);
        audioSource = GetComponent< AudioSource > ();
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        // 데미지 흡수율 설정
        damage_coef = owner.GetComponent<SkillControl>().absorbedCoef;

        State = 0;
    }

    void Update()
    {
        switch (State)
        {
            case 0: { Instantiated(); } break; //블랙홀 생성

            case 1: { Grow(); } break; // 블랙홀 증가

            case 2: { UsingSkill(); } break; //블랙홀 스킬 사용
            case 3: { Disappear(); } break; // 블랙홀 감소

            case 4: { PhotonNetwork.Destroy(gameObject); } break; // 블랙홀 파괴
        }
    }

    //블랙홀 생성
    private void Instantiated()
    {
        hp = 0; // 흡수한 데미지 초기화
        audioSource.clip = BlackholeAudio[0];
        audioSource.loop = false;
        audioSource.Play();
        
    }


    //블랙홀 증가
    private void Grow()
    {
        //오디오 재생 
        audioSource.clip = BlackholeAudio[1];
        audioSource.loop = true;
        audioSource.Play();

        float cur_size = transform.localScale.x;

        if (cur_size < maxSize)
        {
            cur_size += Time.deltaTime * GrowingSpeed;
            transform.localScale = new Vector3(cur_size, cur_size, cur_size);
        }
    }

    // E스킬 사용
    private void UsingSkill()
    {
        //  오디오 재생 
        {
           // audioSource.clip = blackHoleSound[1]; 
          //  audioSource.loop = true;
          //  audioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        Other = collision;

        // 공격하는 자기 자신을 구한다
        PhotonView pv_mine = transform.root.GetComponent<PhotonView>();

        // 맞은 오브젝트를 구한다.
        HitObj = collision.transform.root.gameObject;
        PhotonView pv_other = HitObj.GetComponent<PhotonView>();

        //맞은 오브젝트가 정적 오브젝트라면 그냥 끝낸다.
        if (pv_other == null) return;

        if (pv_mine != pv_other)
        {
            pv.RPC("RPC_AbsordDamage", RpcTarget.OthersBuffered, pv_other.ViewID); //공격 흡수하기
        }
    }
    
    [PunRPC]
    private void RPC_AbsordDamage(int viewID)
    {
        GameObject hitObj = PhotonNetwork.GetPhotonView(viewID).gameObject;

        if (!pv.IsMine)
        {
            // 흡수 오디오
            //audioSource.clip = BlackholeAudio[1]; //오디오 재생 
            //audioSource.Play();

            // 파괴 시켜도 되는지 여부
            bool bCanDestroy = true;

            // 케이시 
            {
                if (hitObj.GetComponent<MLEffect>() != null) //케이지 주공격
                {
                    absorbedDamage += (int)hitObj.GetComponent<MLEffect>().damage;
                }
                else if (hitObj.GetComponent<MREffect>() != null) // 케이지 보조공격
                {
                    absorbedDamage += (int)hitObj.GetComponent<MREffect>().damage;
                }
                else if (hitObj.GetComponent<QBullet>() != null) // 케이시 QSkill
                {
                    absorbedDamage += (int)hitObj.GetComponent<QBullet>().damage;
                }
                else
                {
                    bCanDestroy = false;
                }
            }


            // Rora
            {
                if (hitObj.GetComponent<ManaEffect>() != null) // 로라 F스킬
                {
                    absorbedDamage += hitObj.GetComponent<ManaEffect>().damage;
                }
                else
                {
                    bCanDestroy = false;
                }
            }

            // 부딪친 오브젝트를 파괴하거나 비활성한다.
            if (bCanDestroy)    PhotonNetwork.Destroy(hitObj);
        }
    }

    public void Absorb(float damage)
    {
        pv.RPC("RPC_Absorb", RpcTarget.AllBuffered, damage);
    }

    [PunRPC]
    private void RPC_Absorb(float damage)
    {
        absorbedDamage += damage;
    }

    // 블랙홀 감소
    private void Disappear()
    {
        float cur_size = transform.localScale.x;
        if (cur_size >= 0.0f)
        {
            cur_size -= Time.deltaTime * DisappearSpeed;
            transform.localScale = new Vector3(cur_size, cur_size, cur_size);
        }
    }
}
