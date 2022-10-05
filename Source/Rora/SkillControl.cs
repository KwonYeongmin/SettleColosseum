using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

class SkillControl : MonoBehaviour
{

    //===== Audio =====
    [HideInInspector]public PlayerAudio audioManager;

    //== 무기, 팔 고정
    //  [HideInInspector]
    public bool bIsFixedWeaponEnable = true; // 팔, 지팡이 둘다 고정
                                             // [HideInInspector]
    public bool bIsFixedWand = true; // 팔 고정
                                     // [HideInInspector]
    public bool bIsFixedArm = true; // 지팡이 고정

    private float posWeight = 1;
    private float rotWeight = 1;
    private Rora RoraSC;

    // ==탄창==
    [Header("탄창")]

    [Tooltip("탄창")] public int maxMagazine = 100;
    [Tooltip("(Look)탄약")] public float remainBullet = 0;

    //== 게임 컴포넌트 및 오브젝트 
    [HideInInspector]
    public Transform cameraObjTransform;
    [HideInInspector]
    public Transform wandObjTransform;
    // [HideInInspector]
    public GameObject wandTP; // 간접 공격
    public GameObject wandFP;
    [HideInInspector]
    public Camera mCamera;
    [HideInInspector]
    public GameObject[] characterModels;
    public Transform SkillPointFP;
    [HideInInspector] public Transform SkillPointTP; //camera
    // [HideInInspector]
    public Transform SkillPointTPModel;
    private Animator animator;
    private float cameraDefaultFOV;
    [HideInInspector] public PhotonView pv;

    // IK 동기화 관련
    Transform tf_cam;
    Transform tf_wand;

    // == 공격 이펙트 프리팹
    public GameObject[] Lasers; //3개
    [Tooltip("0:블랙홀 1:총알 2: 아우라 3: 필터")]
    public GameObject blackhole;
    public GameObject reflectionAura;
    public Image FilterScreen;
    public GameObject manaEffects;
    public GameObject teleportparticle1;
    public GameObject teleportparticle2;
    public GameObject QSkillEffect;

    private bool bIsTeleportEnd = true;

    private ShiftSkill TeleportSkill;
    private Attack mainSkill;
    private RAttack subSkill;
    private MeleeAttack meleeAttack;
    private Reflection reflection;
    private FSkill manaBomb;
    private QSkill Qskill;
    private SniperShot SnipperShot;

    // ==UI test용
    public Attack GetAttack() { return mainSkill; }
    public RAttack GetRAttack() { return subSkill; }
    public FSkill GetFskill() { return manaBomb; }
    public ShiftSkill GetShiftSkill() { return TeleportSkill; }
    public Reflection GetEskill() { return reflection; }
    public QSkill GetQskill() { return Qskill; }


    // == 주공격 관련 변수
    [Header("주공격")]
    public int Attack_damage;
    public float Attack_headCoef;
    [Tooltip("초당 탄약 소모량")] public float Attack_bulletsPerShots;
    private bool bOnAttack = false;

    // == 보조 공격 관련 변수
    [Header("보조 공격")]
    [Tooltip("반동 스피드")] public float RAttack_ReboundSpeed = 5.0f;
    public int RAttack_Damage;
    public float RAttack_HeadCoef;
    public float RAttack_cooltime;
    public int RAttack_bulletsPerShots;
    public float RAttack_spreadRange;
    public bool RAttackEnable() { return subSkill.FillCooltime(); }


    // == 간접 공격 관련 변수
    [Header("간접 공격")]
    public int Melee_damage;
    public float Melee_headCoef;


    // == 텔레포트 관련 변수
    [Header("텔레포트 스킬")]
    public float Teleport_coolTime;
    [Tooltip("텔레포트할 때 카메라의 FOV스피드")]
    public float cameraFOVSpeed = 2.0f;
    public bool TeleportEnable() { return TeleportSkill.FillCooltime(); }

    // == 마나 산탄총 관련 변수
    [Header("마나 응축탄")]
    public float ManaBomb_coolTime;
    public int ManaBomb_PerShots;
    public float ManaBomb_Interval;
    public float ManaBomb_headCoef;
    public int ManaBomb_damage;
    public bool ManaBombEnable() { return manaBomb.FillCooltime(); }


    [Header("궁극기")]
    public int QSkill_Damage;
    public float QSkill_DamageInterval;
    public float QSkill_SlowValue;
    public float QSkill_KeepSlowTime;
    public float QSkill_Cooltime;
    public float QSkill_LifeTime;
    public bool QskillEnable() { return Qskill.FillCooltime(); }


    // == 리플렉션 관련 변수
    [Header("리플렉션 스킬")]
    public float reflection_headCoef;
    public int reflection_maxLength;
    public int reflection_damage;
    public float reflection_coolTime;
    public int reflection_bulletsPerShots;
    public float snipperTime;
    public bool reflectionEnable() { return reflection.FillCooltime(); }
    public float absorbedCoef = 0.5f;
    [HideInInspector] public int AbsoredDamage;





    //======================================== start ========================================
    private void Start()
    {
        Initialized();
    }


    //======================================== Initialized ========================================


    private void Initialized()
    {
        audioManager = this.GetComponent<PlayerAudio>();

        SkillPointTP = mCamera.transform;

        remainBullet = maxMagazine;
        cameraDefaultFOV = mCamera.fieldOfView;

        RoraSC = GetComponent<Rora>();
        animator = GetComponent<Animator>();

        pv = GetComponent<PhotonView>();

        FilterScreen = RoraSC.HUD.transform.GetChild(0).GetChild(5).GetComponent<Image>(); //<< 추가

        mainSkill = gameObject.AddComponent<Attack>();
        mainSkill.Init(this);

        subSkill = gameObject.AddComponent<RAttack>();
        subSkill.Init(this);

        meleeAttack = gameObject.AddComponent<MeleeAttack>();
        meleeAttack.Init(this);

        TeleportSkill = gameObject.AddComponent<ShiftSkill>();
        TeleportSkill.Init(this,RoraSC);

        reflection = gameObject.AddComponent<Reflection>();
        reflection.Init(this, RoraSC,animator);

        SnipperShot = gameObject.AddComponent<SniperShot>();
        SnipperShot.Init(this);


        manaBomb = gameObject.AddComponent<FSkill>();
        manaBomb.Init(this);

        Qskill = gameObject.AddComponent<QSkill>();
        Qskill.Init(this,animator);


     

        

      
    }


    //======================================== IK ========================================


    private void OnAnimatorIK(int layerIndex)
    {
        if (bIsFixedWeaponEnable) ControlWand();
    }

    void ControlWand()
    {
        //SetEachWeight
        if (bIsFixedArm)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, posWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rotWeight);

            animator.SetIKPosition(AvatarIKGoal.RightHand, cameraObjTransform.position);

            Quaternion handRotation = Quaternion.LookRotation(cameraObjTransform.position - transform.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, handRotation);
        }

        //LookAtObj
        {
            animator.SetLookAtWeight(1.0f);
            animator.SetLookAtPosition(wandObjTransform.position);
        }

        //지팡이
        if (bIsFixedWand)
        {
            wandTP.GetComponent<Transform>().LookAt(wandObjTransform.position - new Vector3(-1, 90, 2));
        }

    }



    private void EnableCharaterTurn(bool b)
    {
        RoraSC.bIsTurnCameraEnable = b;
        RoraSC.bIsTurnPlayerEnable = b;
    }


    //======================================== FixedUpdate && OnDestroy ========================================

    private void FixedUpdate()
    {
        if(!pv.IsMine)  return;

        RAttackUpdate();
        AttackUpdate();
        TeleportUpdate();
        FSkillUpdate();
        QSkillUpdate();
        ESkillUpdate();
        SniperUpdate();
    }

    private void OnDestroy()
    {
        // 파괴되기 전에 FixedUpdate()를 무조건 한번 실행해서 파괴되지 않은 오브젝트가 있다면 파괴시켜준다.
        FixedUpdate();
    }



    //======================================== ReduceMagazine ========================================


    public void ReduceMagazine(int bulletPerShot) // 투사체
    {
        remainBullet = (remainBullet - bulletPerShot <= 0) ? 0 : remainBullet - bulletPerShot;

    }

    public void ReduceMagazineContinuous(float bulletPerShot) //레이저
    {
        if (remainBullet < 1f) remainBullet = 0f;
        else remainBullet -= bulletPerShot * Time.deltaTime;
    }



    //======================================== Idle AnimationEvent ========================================

    public void IdleStart()
    {
        bIsFixedArm = true;
    }

    //======================================== Attack ========================================

    public void AttackStart() // Rora상태머신  AttackStart
    {
        if(!pv.IsMine)  return;
        if(!GetComponent<Rora>().IsInLAttack())  return;

        bOnAttack = true;
        mainSkill.OnSkillStart();
    }

    public void AttackUpdate() //Update
    {
        if (!pv.IsMine) return;

        mainSkill.OnSkillUpdate();
    }

    public void AttackEnd() //Rora상태머신  AttackEnd
    {
        if (!pv.IsMine) return;

        bOnAttack = false; //
        mainSkill.OnSkillEnd();
    }


    //======================================== RAttack  ========================================

    public void RAttackStart() //animationEvent
    {
        if (!pv.IsMine) return;

        subSkill.OnSkillStart();
        audioManager.PlayWeaponSound(0);
    }

    private void RAttackUpdate() //Update
    {
        if (!pv.IsMine) return;
        subSkill.OnSkillUpdate();
    }

    public void RAttackEnd() //Rora상태머신  RAttackEnd
    {
        if (!pv.IsMine) return;
        subSkill.OnSkillEnd();
    }


    //======================================== Teleport AnimationEvent ========================================

    public void TeleportStart() //AnimationEvent
    {
        if (!pv.IsMine) return;

        if (bOnAttack) mainSkill.OnSkillEnd();
        TeleportSkill.OnSkillStart();

        audioManager.PlayWeaponSound(3);
    }

    private void TeleportUpdate() //Update
    {
        if (!pv.IsMine) return;
        TeleportSkill.OnSkillUpdate();
    }

    public void TeleportEvent() //AnimationEvent
    {
        if (!pv.IsMine) return;
        TeleportSkill.OnSkillEvent();
       // audioManager.PlayModelSound(1);
    }

    public void TeleportEnd() //AnimationEvent
    {
        Debug.Log("ShiftEnd");
        TeleportSkill.OnSkillEnd();
        audioManager.PlayWeaponSound(4);
    }


    //======================================== VAttack AnimationEvent ========================================

    public void VSkillStart() //AnimationEvent
    {
        if (!pv.IsMine) return;

        if (bOnAttack) mainSkill.OnSkillEnd();
        bIsFixedArm = false;
        meleeAttack.OnSkillStart();
        wandTP.GetComponent<RoraWeapon>().bIsDamaged = false; // >> 근접 공격 시작때마다 꺼주기
        audioManager.PlayWeaponSound(1);
    }

    public void VSkillEnd() //AnimationEvent
    {
        if (!pv.IsMine) return;

        bIsFixedArm = true;
        meleeAttack.OnSkillEnd();
    }


    //======================================== FSkill AnimationEvent ========================================

    IEnumerator InstantiateMana(float interval)
    {
        yield return new WaitForSeconds(interval);
        manaBomb.InstantiateMana();
        audioManager.PlayWeaponSound(5);
    }

    public void FSkillStart() //AnimationEvent
    {
        if (!pv.IsMine) return;

        if (bOnAttack) mainSkill.OnSkillEnd();
        manaBomb.OnSkillStart();

        for (int i = 0; i < ManaBomb_PerShots; i++)
        {
            StartCoroutine(InstantiateMana(ManaBomb_Interval * i));
            
        }

        
    }

    public void FSkillEnd()
    {
        if (!pv.IsMine) return;
        manaBomb.OnSkillEnd();
    }

    private void FSkillUpdate() //Update
    {
        if (!pv.IsMine) return;
        manaBomb.OnSkillUpdate();
    }


    //======================================== QSkill AnimationEvent ========================================

    public void QSkillStart() 
    {
        if (!pv.IsMine) return;

        if (bOnAttack) mainSkill.OnSkillEnd();
        pv.RPC("RPC_SetIK", RpcTarget.AllBuffered, false);
        Qskill.OnSkillStart();
        audioManager.PlayWeaponSound(3);
        audioManager.PlayVoiceSound(0);
    }

    public void QSkillUpdate() 
    {
        if (!pv.IsMine) return;
        Qskill.OnSkillUpdate();
    }

    public void QSkillEnd() //AnimationEvent
    {
        if (!pv.IsMine) return;

        pv.RPC("RPC_SetIK", RpcTarget.AllBuffered, true);
        Qskill.OnSkillEnd();
    }


    //======================================== ESkill AnimationEvent ========================================

    public void ESkillStart() //AnimationEvent
    {
        if (!pv.IsMine) return;

        if (bOnAttack) mainSkill.OnSkillEnd();
        reflection.OnSkillStart();
        audioManager.PlayWeaponSound(2);
    }

    public void ESkillEventUpdate(int state)  //AnimationEvent
    {
        if (!pv.IsMine) return;
        reflection.UpdateState(state);
    }

    public void ESkillEnd() //AnimationEvent
    {
        if (!pv.IsMine) return;

        reflection.OnSkillEnd();
        Debug.Log("Get Damage: " + reflection.GetDamage());
        SnipperShot.absorbedDamage = reflection.GetDamage();
    }

    private void ESkillUpdate() //Update
    {
        if (!pv.IsMine) return;
        reflection.OnSkillUpdate();
    }


    //========================================SniperShot AnimationEvent ========================================

    public void SniperStart() 
    {
        if (!pv.IsMine) return;

        if (bOnAttack) mainSkill.OnSkillEnd();
        SnipperShot.OnSkillStart();
        
        Debug.Log("SniperStart");
    }

    public void ShootSniperShot()  //Rora 
    {
        if (!pv.IsMine) return;

        SnipperShot.Shoot();
        audioManager.PlayWeaponSound(2);
        Debug.Log("Sniper Shoot");

    }

    public void SniperUpdate()
    {
        if (!pv.IsMine) return;
        SnipperShot.OnSkillUpdate();
    }

    public void SniperEnd() //Rora상태머신End
    {
        if (!pv.IsMine) return;

        reflection.SetBIsShotStart(false);
        SnipperShot.OnSkillEnd();
    }


    //========================================Reload AnimationEvent ========================================

    public void ReloadEventStart()
    {
        if (!pv.IsMine) return;

        if (bOnAttack) mainSkill.OnSkillEnd();
        // bIsFixedArm = false;
        // 
    }

    public void ReloadEventEnd()
    {
        if (!pv.IsMine) return;

        bIsFixedArm = true;
        remainBullet = maxMagazine;
    }

    //======================================== RPC ========================================
    [PunRPC]
    private void RPC_SetIK(bool bActive)
    {
        bIsFixedWeaponEnable = bActive;
    }
}