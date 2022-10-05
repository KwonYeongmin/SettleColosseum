using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

/*
< 상태 이상 >
기절 (Faint) : 이동, 조작 불가능
수면 (Sleep) :  이동, 조작이 불가능 (해제 가능)
속박(Restraint) : 이동 불가능
침묵(Silence) : 조작 불가능
끌어당김 (Dragged) : 이동 불가능, 끌려감
슬로우 (Slow) : 속도 저하
*/

// == 상태 ==
[HideInInspector]
public enum RoraState
{
    Error = 0,
    Idle,
    Move,
    Jump,
    MainAttack, SubAttack, VSkill,
    QSkill, ESkill, Teleport, FSkill, ESnipeShot, ESnipeMode,
    Reload,
    //CC,
    Faint, Sleep, Restraint, Silence, Attraction, Slow,
    Die, Dragged
}

public class Rora : Playable, IPunObservable
{
    // 상태기계
    [HideInInspector]
    public RoraStateMachine stateMachine;

    // 컴포넌트
    [HideInInspector]
    public CharacterController characterController;
   // [HideInInspector]
    public Animator animTP;
    public Animator animFP;
    [HideInInspector]
    public GameObject cameraObj;
    SkillControl SkillSC;

    // UI
    [Header("UI")]
    private GameObject _HUD;
    public GameObject prefabHUD;
    public GameObject HUD { get { return _HUD; } }

    // 모델
    [Header("모델")]
    [Tooltip("3인칭 모델")] public GameObject[] models;
    [Tooltip("1인칭 팔")] public GameObject fpArm;
    public GameObject tpModel;

    // 체력
    HUD_HealthBar hpBar;
    [Header("체력")]
    [Tooltip("HP 기본 값")] public int DEFAULT_HP = 50;
    [Tooltip("실드 기본 값")] public int DEFAULT_SHIELD = 150;
    int totalHP;
    public int shield;
    float shieldRegenTimer = 0f;
    float shieldRegenValue = 0f;

    //== 이동 ==
    [Header("이동")]
    [Tooltip("기본 이동 속도"), Range(1.0f, 15.0f)] public float defaulMoveSpeed = 7.0f;
    [Tooltip("텔레포트 이동 속도"), Range(50.0f, 100.0f)] public float TeleportSpeed = 15.0f;
    [Tooltip("(Look)이동 속도"), Range(1.0f, 15.0f)] public float MoveSpeed = 7.0f;
    [Tooltip("블랙홀 사용시 느려지는 속도 배율"), Range(1.0f, 15.0f)] public float MoveMultiplierInBlackHole = 0.3f;

    //== 탄창
    HUD_Magazine RoraMagazine;
    [Header("재장전 속도")]
    public float autoReloadSpeed = 1.0f;
    public float manualReloadSpeed = 1.1f;

    // == 카메라 회전 ==
    [HideInInspector] public bool bIsTurnCameraEnable = true;
    [HideInInspector] public bool bIsTurnPlayerEnable = true;

    //==중력==
    [Header("중력")]
    [Tooltip("기본 중력")] public float defaultGravity = 1.0f;
    [Tooltip("공중유지상태 중력")] public float keepAirGravity = 0.1f;
    [Tooltip("텔레포트 이동시 중력")] public float teleportAirGravity = 0.5f;
    [Tooltip("(Look)로라 현재 중력")] public float gravity;
    [Range(-10.0f, 10.0f)] public float VerticalSpeed = 0.0f;
    [Tooltip("(Look)땅인지 체크")] public bool bIsGrounded = true;


    //== 점프 ==
    [Header("점프")]
    [Tooltip("땅점프 최대 횟수")] public int maxGroundJumpCount = 1;
    [Tooltip("공중점프 최대 횟수")] public int maxAirJumpCount = 1;
    [Tooltip("(Look)땅점프 카운트")] public int groundJumpCount = 0;
    [Tooltip("땅 점프 파워")] public float groundJumpPower = 0.5f;
    [HideInInspector] public int airJumpCount;
    [Tooltip("공중 점프 파워")] public float airJumpPower = 0.5f;
    public float yVelocity = 0;
    [HideInInspector] public bool bIsJumping = false;

    //== 공중유지 ==
    [Header("공중 유지")]
    [Tooltip("공중 유지 시작하는 스페이스바 입력 최소값")] public float limitSpaceTime = 0.25f;
    [Tooltip("(Look)스페이스바 입력 시간")] public float spaceSaver = 0;

    // == 상태 관리 변수
    [HideInInspector] public int SkillNumber = 0;
    [HideInInspector] public bool bIsMoving = false;
    [HideInInspector] public bool bEndMainAttack = true;
    [HideInInspector] public bool bEndSubAttack = true;
    [HideInInspector] public bool bEndFSkill = true;
    [HideInInspector] public bool bEndESkill = true;
    [HideInInspector] public bool bEndQSkill = true;
    [HideInInspector] public bool bEndShiftSkill = true;
    [HideInInspector] public bool bEndMeleeAttack = true;
    [HideInInspector] public bool bEndReload = true;
    [HideInInspector] public bool bEndESnipeMode = true;
    [HideInInspector] public bool bEndESnipeShot = true;
    [HideInInspector] public bool bEndDead = true;
    [HideInInspector] public bool bEndDragged = true;

    // 착지 상태 판단 관련 타이머
    const float GROUND_CHECK_TIME = 0.1f;
    float groundCheckTimer = GROUND_CHECK_TIME;
    const float CEIL_CHECK_TIME = 0.1f;
    float ceilCheckTimer = CEIL_CHECK_TIME;

    // 포톤 애니메이션 싱크용 Enum 및 변수
    public enum PhotonAnim
    {
        IDLE =              0b000000000000000001,
        LOWER_IDLE =        0b000000000000000010,
        MOVE =              0b000000000000000100,
        LOWER_MOVE =        0b000000000000001000,
        LOWER_JUMP =        0b000000000000010000,
        LAND =              0b000000000000100000,
        ATTACK =            0b000000000001000000,
        END_ATTACK =        0b000000000010000000,
        RATTACK =           0b000000000100000000,
        MELEE_ATTACK =      0b000000001000000000,
        FSKILL =            0b000000010000000000,
        ESKILL =            0b000000100000000000,
        END_ESKILL =        0b000001000000000000,
        ESKILL_SHOT =       0b000010000000000000,
        QSKILL =            0b000100000000000000,
        SHIFTSKILL =        0b001000000000000000,
        RELOAD =            0b010000000000000000,
        ESKILL_IDLE =       0b100000000000000000,
    }
    [HideInInspector] public PhotonAnim photonAnimSync = 0;

    // 충돌 컬라이더
    //[Header("충돌 컬라이더 모음")]
    [HideInInspector]
    public BoxCollider[] colliders;
    // [0]이 머리

    //<< audioManager
    [HideInInspector] public PlayerAudio audioManager;
    [HideInInspector] public GameObject dragger;

    protected override void OnEnable()
    {
        base.OnEnable();

        // HUD 인스턴스 생성
        pv = GetComponent<PhotonView>();
        _HUD = Instantiate(prefabHUD);
        _HUD.GetComponent<HUDManager>().Character = this.gameObject;
        _HUD.GetComponent<HUDManager>().nickname.text = PhotonNetwork.LocalPlayer.NickName.ToString();
        hpBar = _HUD.GetComponent<HUD_HealthBar>();
        RoraMagazine = _HUD.GetComponent<HUD_Magazine>();
        if (!pv.IsMine ) _HUD.SetActive(false);

        Initialized();
    }

    public void Initialized()
    {
        // 오브젝트가 플레이어가 조종하는게 아니라면
        if (!pv.IsMine)
        {
            // 1인칭 시점을 비활성화하고 3인칭 모델을 보이게 해준다.
            GetComponentInChildren<Camera>().enabled = false;
            fpArm.SetActive(false);
            for (int i = 0; i < models.Length; i++)
                models[i].layer = LayerMask.NameToLayer("Enemy");

            // 또한 컬라이더의 레이어를 Enemy로 설정해준다.
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
        else
        {
            // 컬라이더의 레이어를 Player로 설정해준다.
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].gameObject.layer = LayerMask.NameToLayer("Player");
        }

        // ==컴포넌트
        SkillSC = this.GetComponent<SkillControl>();
        characterController = this.GetComponent<CharacterController>();

        //== 중력
        gravity = defaultGravity;

        //== 상태기계
        stateMachine = new RoraStateMachine(this);

        // hp 초기화
        hp = DEFAULT_HP;
        shield = DEFAULT_SHIELD;
        totalHP = DEFAULT_HP + DEFAULT_SHIELD;

        // 래그돌 초기화
        ragdollApplier = GetComponent<RagdollApplier>();

        // 상호작용 가능 초기화
        bCanInteract = true;

        //audioListenr
        if (pv.IsMine) ControlAudioListener(true);
        else ControlAudioListener(false);
    }

    private void ControlAudioListener(bool b)
    {
        camera.GetComponent<AudioListener>().enabled = b;
    }

    protected override void Update()
    {
        Debug.Log(mouseSensitivity);
        // 실드 리젠 체크
        RegenShield();

        if (!pv.IsMine || !bCanInteract)
            return;

        Debug.Log("Current State: " + stateMachine.GetState());
        
        // 죽었는지 체크한다.
        CheckDead();

        // 부모 클래스의 Update 호출
        base.Update();

        // 상태 Update 호출
        stateMachine.UpdateState();

        // 카메라 & 캐릭터 회전 체크
        if (bIsTurnCameraEnable) RotateCamera();
        if (bIsTurnPlayerEnable) RotateCharacter();

        // E스킬의 스나이퍼 모드로 돌입했는지 체크
        CheckESnipeMode();

        //공중유지
        //if(!bIsGrounded)  KeepAir();
        KeepAir();

        // HUD 업데이트
        UpdateHUD();

        // 디버그용
        TestHP();

        //
        audioManager = this.GetComponent<PlayerAudio>();
       
        //
        CurTimeValue = Time.timeScale;
    }

    private void FixedUpdate()
    {
        if (!pv.IsMine || !bCanInteract)
            return;

        ApplyGravity();             // 중력 적용
        CheckOnGround();            // 땅 체크
        CheckCollideWithCeil();     // 천장에 부딫쳤는지 체크
    }

    void TestHP()
    {
        if (Input.GetKeyDown(KeyCode.O)) TakeDamage(10);
        if (Input.GetKeyDown(KeyCode.P)) IncreaseHP(10);
    }

    public void IdleState()
    {
        Vector3 Direction = Vector3.zero;
        Direction = new Vector3(Input.GetAxis("Horizontal"), VerticalSpeed, Input.GetAxis("Vertical"));
    }



    //======================== State Transition ===========================
    public override void Idle()
    {
        if (!bEndESnipeMode)
            return;

        if (
            stateMachine.GetState() == RoraState.Move ||
            (bEndMainAttack && bEndSubAttack && bEndFSkill && bEndESkill && bEndQSkill &&
            bEndShiftSkill && bEndMeleeAttack && bEndReload && bEndESnipeShot && bEndDead && bEndDragged))
        {
            if (bIsGrounded) stateMachine.SetState(RoraState.Idle);
            else stateMachine.SetState(RoraState.Jump);
        }
    }

    public override void Move()
    {
        if (!bEndESnipeMode)
            return;

        if (
            stateMachine.GetState() == RoraState.Idle ||
            (bEndMainAttack && bEndSubAttack && bEndFSkill && bEndESkill && bEndQSkill &&
            bEndShiftSkill && bEndMeleeAttack && bEndReload && bEndESnipeShot && bEndDead && bEndDragged))
        {
            if (bIsGrounded) stateMachine.SetState(RoraState.Move);
            else stateMachine.SetState(RoraState.Jump);
        }
    }

    public override void Jump() //땅 점프 및 공중 점프
    {
        if (!bEndESnipeMode)
            return;

        if (
           stateMachine.GetState() == RoraState.Idle ||
           stateMachine.GetState() == RoraState.Move ||
           (bEndMainAttack && bEndSubAttack && bEndESkill && bEndQSkill && bEndFSkill &&
           bEndShiftSkill && bEndMeleeAttack && bEndReload && bEndESnipeShot && bEndDead && bEndDragged))
        {
            stateMachine.SetState(RoraState.Jump);
        }

    }

    public override void Attack()
    {
        if (stateMachine.GetState() == RoraState.ESnipeMode)
        {
            // 현재 스나이퍼 모드였다면 저격샷 상태로 진입한다.
            stateMachine.SetState(RoraState.ESnipeShot);
            return;
        }

        if (
            stateMachine.GetState() == RoraState.Idle ||
            stateMachine.GetState() == RoraState.Move ||
            stateMachine.GetState() == RoraState.Jump ||
            stateMachine.GetState() == RoraState.MainAttack ||
            (bEndMainAttack && bEndSubAttack && bEndFSkill && bEndESkill && bEndQSkill &&
            bEndShiftSkill && bEndMeleeAttack && bEndReload && bEndESnipeShot && bEndDead && bEndDragged))
        {

            // 총알이 없을때 발사를 하려고 하면 자동으로 재장전 상태로 전환한다.
            if (SkillSC.remainBullet < 1)
            {
                bEndReload = false;
                stateMachine.SetState(RoraState.Reload);
                return;
            }
            if (stateMachine.GetState() != RoraState.MainAttack) { stateMachine.SetState(RoraState.MainAttack); }
        }
    }

    public override void RAttack()
    {
        if (
                stateMachine.GetState() == RoraState.Idle ||
                stateMachine.GetState() == RoraState.Move ||
                stateMachine.GetState() == RoraState.Jump ||
                stateMachine.GetState() == RoraState.SubAttack ||
                (bEndMainAttack && bEndSubAttack && bEndFSkill && bEndESkill && bEndQSkill &&
                bEndShiftSkill && bEndMeleeAttack && bEndReload && bEndESnipeShot && bEndDead && bEndDragged))
        {
            // 총알이 없을때 발사를 하려고 하면 자동으로 재장전 상태로 전환한다.
            if (SkillSC.remainBullet < 1)
            {
                bEndReload = false;
                stateMachine.SetState(RoraState.Reload);
                return;
            }

            FinishESnipeMode();
            if (stateMachine.GetState() != RoraState.SubAttack) stateMachine.SetState(RoraState.SubAttack);
        }
    }

    public override void MeleeAttack()
    {
        if (
              stateMachine.GetState() == RoraState.Idle ||
              stateMachine.GetState() == RoraState.Move ||
              stateMachine.GetState() == RoraState.Jump ||
              (
              bEndFSkill && bEndESkill && bEndQSkill && bEndShiftSkill &&
              bEndMeleeAttack && bEndReload && bEndESnipeShot && bEndDead && bEndDragged))
        {
            FinishESnipeMode();
            stateMachine.SetState(RoraState.VSkill);
        }
    }

    public override void ESkill()
    {
        if (
              stateMachine.GetState() == RoraState.Idle ||
              stateMachine.GetState() == RoraState.Move ||
              stateMachine.GetState() == RoraState.Jump ||
              (
              bEndFSkill && bEndESkill && bEndQSkill && bEndShiftSkill &&
              bEndMeleeAttack && bEndReload && bEndESnipeShot && bEndDead && bEndDragged))
        {
            FinishESnipeMode();
            stateMachine.SetState(RoraState.ESkill);
        }
    }

    public override void FSkill()
    {
        if (SkillSC.ManaBombEnable())
        {
            if (
            stateMachine.GetState() == RoraState.Idle ||
            stateMachine.GetState() == RoraState.Move ||
            stateMachine.GetState() == RoraState.Jump ||
            (
            bEndFSkill && bEndESkill && bEndQSkill && bEndShiftSkill &&
            bEndMeleeAttack && bEndReload && bEndESnipeShot && bEndDead && bEndDragged))
            {
                FinishESnipeMode();
                stateMachine.SetState(RoraState.FSkill);
            }
        }
    }

    public override void ShiftSkill()
    {
        if (SkillSC.TeleportEnable())
        {
            if (
             stateMachine.GetState() == RoraState.Idle ||
             stateMachine.GetState() == RoraState.Move ||
             stateMachine.GetState() == RoraState.Jump ||
             (
             bEndFSkill && bEndESkill && bEndQSkill && bEndShiftSkill &&
             bEndMeleeAttack && bEndReload && bEndESnipeShot && bEndDead && bEndDragged))
            {
                FinishESnipeMode();
                stateMachine.SetState(RoraState.Teleport);
            }
        }

    }

    public override void QSkill()
    {
        if (SkillSC.QskillEnable())
        {
            if (
            stateMachine.GetState() == RoraState.Idle ||
            stateMachine.GetState() == RoraState.Move ||
            (
            bIsGrounded && bEndFSkill && bEndESkill && bEndQSkill && bEndShiftSkill &&
            bEndMeleeAttack && bEndReload && bEndESnipeShot && bEndDead && bEndDragged))
            {
                FinishESnipeMode();
                stateMachine.SetState(RoraState.QSkill);
            }
        }
    }

    public override void Reload()
    {
        if (
            stateMachine.GetState() == RoraState.Idle ||
            stateMachine.GetState() == RoraState.Move ||
            stateMachine.GetState() == RoraState.MainAttack ||
            stateMachine.GetState() == RoraState.SubAttack ||
            (
            bEndESkill && bEndFSkill && bEndESkill && bEndQSkill && bEndShiftSkill &&
            bEndMeleeAttack && bEndReload && bEndESnipeShot && bEndDead && bEndDragged))
        {
            stateMachine.SetState(RoraState.Reload);
        }
    }

    //  여기에 안들어오도록
    void CheckESnipeMode()
    {
        if (stateMachine.GetState() != RoraState.ESnipeMode &&
            !bEndESnipeMode)
        {
            stateMachine.SetState(RoraState.ESnipeMode);
            this.GetComponent<SkillControl>().SniperStart();
        }
    }

    void FinishESnipeMode()
    {
        if (stateMachine.GetState() == RoraState.ESnipeMode)
            SkillSC.SniperEnd();
    }








    //======================== HP, Damage ===========================

    // 로라는 기본 HP에 실드가 포함되어 있으므로 체력 증감 방식이 다르게 처리돼야 한다.
    public override void IncreaseHP(int value)
    {
        hp += value;
        if (hp < DEFAULT_HP) return;

        // 회복량이 최대 체력을 넘어섰을 경우
        shield += hp - DEFAULT_HP;
        hp = DEFAULT_HP;
        if (shield < DEFAULT_SHIELD) return;

        // 회복량이 최대 실드량을 넘어섰을 경우
        shield = DEFAULT_SHIELD;

    }


    public override void TakeDamage(int value)
    {
        if (bEndESkill)
        {
            // 실드 리젠 타이머를 0으로 초기화한다.
            shieldRegenTimer = 0f;

            // 실드가 먼저 까이고 체력이 까여야한다.
            shield -= value;
            if (shield > 0f) return;

            // 실드가 전부 까인 경우
            hp += (int)shield;
            shield = 0;
        }
    }

    public void TakeDamageContinuous(float value)
    {
        if (!bEndESkill)
        {
            // 실드 리젠 타이머를 0으로 초기화한다.
            shieldRegenTimer = 0f;

            // 실드가 먼저 까이고 체력이 까여야한다.
            float floatshield = shield;
            floatshield -= value * Time.deltaTime;
            shield = Mathf.RoundToInt(floatshield);
            if (shield > 0) return;

            // 실드가 전부 까인 경우
            hp += (int)shield;
            shield = 0;
            if (hp > 0) return;
        }
    }

    public void Take_Damage()
    {
        if (colliders[0].isTrigger)
        {
            Debug.Log("HeadDamage");
        }
    }

    public override void TakeDamage_Sync(int damage)
    {
        pv.RPC("RPC_TakeDamage", RpcTarget.AllBuffered, damage);
    }

    [PunRPC]
    public void RPC_TakeDamage(int damage)
    {
        TakeDamage(damage);
    }

    public void IncreaseHP_Sync(int point)
    {
        pv.RPC("RPC_IncreaseHP", RpcTarget.AllBuffered, point);
    }

    [PunRPC]
    public void RPC_IncreaseHP(int point)
    {
        IncreaseHP(point);
    }

    public void IncreaseShield(int point)
    {
        pv.RPC("RPC_IncreaseShield", RpcTarget.AllBuffered, point);
    }

    [PunRPC]
    public void RPC_IncreaseShield(int value)
    {
        shield = (shield + value < DEFAULT_SHIELD) ? shield + value : DEFAULT_SHIELD;
    }
    //======================== ApplyGravity , CheckOnGround===========================

    void ApplyGravity()
    {
        if (!bEndShiftSkill) return;

        if (bIsGrounded && yVelocity < -0.1f) yVelocity = -0.1f;
        else
        {
            yVelocity -= gravity * Time.deltaTime;
        }
        characterController.Move(new Vector3(0f, yVelocity, 0f));
    }


    private void CheckOnGround()
    {
        //Debug.Log("bIsGrounded: " + bIsGrounded);

        // 땅위에 있는지 판단한다.
        List<RaycastHit> hitInfos;
        Vector3 center = transform.position;
        hitInfos = Physics.SphereCastAll(center, characterController.radius, Vector3.down, 0.001f).ToList();

        // 캐릭터의 콜라이더와 부딪친 경우는 제외한다.
        hitInfos.RemoveAll(hit => (hit.transform.root.GetComponent<Playable>() != null));

        // 총알과 부딪친 경우도 제외한다.
        hitInfos.RemoveAll(hit => (hit.transform.root.gameObject.layer == LayerMask.NameToLayer("Projectiles")));

        // 충돌한게 없다면 끝낸다.
        if (hitInfos.Count == 0)
        {
            groundCheckTimer = GROUND_CHECK_TIME;
            bIsGrounded = false;
            return;
        }

        // Landable 오브젝트 위에 착지했는지 검사한다.
        for (int i = 0; i < hitInfos.Count; i++)
        {
            //Debug.Log("Hit Object Name: " + hitInfos[i].collider.gameObject.name);
            if (hitInfos[i].collider.tag == "Landable")
            {
                // 점프 애니메이션 출력 중이었을시 착지 애니메이션 출력
                if (Utilities.IsAnimationPlaying(animTP, "Jump", 1) || Utilities.IsAnimationPlaying(animTP, "Fall", 1))
                {
                    animTP.SetTrigger("Land");
                    photonAnimSync |= PhotonAnim.LAND;
                }
                airJumpCount = 0;
                bIsGrounded = true;
                gravity = defaultGravity;
                return;
            }
        }

        // 착지한 곳이 Landable이 아니라면 일정 시간 이상 해당 발판 위에 있었을때 착지 상태로 변환한다.
        if (groundCheckTimer > 0.0f && !bIsGrounded)
        {
            groundCheckTimer -= Time.fixedDeltaTime;
        }
        else if (!bIsGrounded)
        {
            groundCheckTimer = GROUND_CHECK_TIME;
            airJumpCount = 0;
            bIsGrounded = true;
            gravity = defaultGravity;
        }
    }

    void CheckCollideWithCeil()
    {
        // 천장과 부딪쳤는지 구한다.
        List<RaycastHit> hitInfos;
        Vector3 center = transform.position + new Vector3(0f, characterController.height / 2f, 0f);
        hitInfos = Physics.SphereCastAll(
            center, characterController.radius * 1.5f, Vector3.up, characterController.height / 1.8f).ToList();

        // 캐릭터의 콜라이더와 부딪친 경우는 제외한다.
        hitInfos.RemoveAll(hit => (hit.transform.root.GetComponent<Playable>() != null));

        // 충돌한게 없다면 끝낸다.
        if (hitInfos.Count == 0) return;

        // 천장과 일정시간 이상 부딪쳤다면 아래 방향으로 떨어지게 중력을 갱신한다.
        if (ceilCheckTimer > 0.0f)
        {
            ceilCheckTimer -= Time.fixedDeltaTime;
        }
        else if (yVelocity > 0)
        {
            ceilCheckTimer = CEIL_CHECK_TIME;
            yVelocity = 0f;
        }
    }











    //======================== Activation ===========================
    public void TeleportMove()
    {
        Vector3 Direction = Vector3.zero;
        Direction = cameraObj.transform.forward * TeleportSpeed * Time.deltaTime;
        // Debug.Log("y값: " + cameraObj.transform.forward.y);

        gravity = teleportAirGravity;

        yVelocity = 0;
        if (cameraObj.transform.forward.y > 0.1f)
        {
            characterController.Move(new Vector3(Direction.x, cameraObj.transform.forward.y, Direction.z));
        }
        else
        {
            characterController.Move(Direction);
        }
    }


    public void TryJump()
    {

        if (Input.GetKeyDown(KeyManager.Inst.Jump)) //스페이스키 누르면
        {
            if (maxAirJumpCount > airJumpCount) //점프 가능
            {
                if (bIsGrounded)
                {
                    animTP.SetTrigger("Lower_Jump");
                    photonAnimSync |= PhotonAnim.LOWER_JUMP;
                    yVelocity = groundJumpPower;  //땅 점프 파워
                    audioManager.PlayFootstepSound(2); // << 땅점프 audio추가
                }
                else
                {
                    yVelocity = airJumpPower; //공중점프 파워
                    ++airJumpCount;
                    audioManager.PlayFootstepSound(3); // << audio추가
                }
            }
        }
    }

    public void TryMove(float multiplier = 1f)
    {
        // 플레이어 입력 받기 & 이동
        float horizontal = Input.GetAxis("Horizontal") * multiplier;
        float vertical = Input.GetAxis("Vertical") * multiplier;

        Vector3 Direction = new Vector3(horizontal, 0, vertical);
        if (Direction.sqrMagnitude > 1.0f) //  대각선 이동이 더 빠른걸 막는다.
            Direction.Normalize();
        Direction = Direction * MoveSpeed * Time.deltaTime * slowValue;
        Direction = transform.TransformDirection(Direction);
        characterController.Move(Direction);

        // 하체 애니메이션 결정
        if (bIsGrounded)     // 공중에 있는지 검사
        {
            // 땅위에 있다면 이동 여부에 따라 Idle 또는 Move애니메이션을 출력한다.
            if (Mathf.Abs(horizontal) <= 0.01f && Mathf.Abs(vertical) <= 0.01f)
            {
                if (!Utilities.IsAnimationPlaying(animTP, "Idle", 1))
                {
                    animTP.SetTrigger("Lower_Idle");
                    photonAnimSync |= PhotonAnim.LOWER_IDLE;
                }
            }
            else
            {
                animTP.SetFloat("Move_FB", vertical);
                animTP.SetFloat("Move_RL", horizontal);
                if (!Utilities.IsAnimationPlaying(animTP, "Move", 1))
                {
                    animTP.SetTrigger("Lower_Move");
                    photonAnimSync |= PhotonAnim.LOWER_MOVE;
                }
            }
        }
    }

    void KeepAir() //공중유지
    {
        if (Input.GetKey(KeyManager.Inst.Jump) && yVelocity < 0f)//스페이스 누르고 있고 하강중
        {
            spaceSaver += Time.deltaTime;

            if (spaceSaver > limitSpaceTime)//0.25초 동안 스페이스 입력 확인
            {
                gravity = keepAirGravity; //공중유지 중력
                if(keepAirGravity <0.0f) keepAirGravity*=-1;
                audioManager.FootstepSrc.clip = audioManager.FootstepClip[4]; // 공중유지 중력 제어 사운드
                spaceSaver = 0;

            }

        }

        if (Input.GetKeyUp(KeyManager.Inst.Jump) || bIsGrounded)
        {
            spaceSaver = 0;
            gravity = defaultGravity;//원래 중력으로
        }
    }

    void RegenShield()
    {
        // 3초이상 피격되지 않았을시 실드를 재생한다.
        shieldRegenTimer += Time.deltaTime;
        if (shieldRegenTimer > 3.0f)
        {
            shieldRegenValue += Time.deltaTime;//(50f * Time.deltaTime);
            shield += (int)shieldRegenValue;
            if(shieldRegenValue > 1.0f) 
                shieldRegenValue -= (int)(shieldRegenValue);

            if (shield > DEFAULT_SHIELD) shield = DEFAULT_SHIELD;
        }
    }

    void UpdateHUD()
    {
        int remainBullet = (int)SkillSC.remainBullet;

        hpBar.UpdateHP(hp, (int)shield, totalHP);
        RoraMagazine.UpdateRemain(remainBullet.ToString());
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 멀티플레이 애니메이션 싱크
        if (stream.IsWriting)
        {
            stream.SendNext(photonAnimSync);
            if (photonAnimSync > 0) photonAnimSync = 0;
        }
        else if (stream.IsReading)
        {
            photonAnimSync = (PhotonAnim)stream.ReceiveNext();

            if ((photonAnimSync & PhotonAnim.IDLE) != 0)            animTP.SetTrigger("Idle");
            if ((photonAnimSync & PhotonAnim.LOWER_IDLE) != 0)      animTP.SetTrigger("Lower_Idle");
            if ((photonAnimSync & PhotonAnim.MOVE) != 0)            animTP.SetTrigger("Move");
            if ((photonAnimSync & PhotonAnim.LOWER_MOVE) != 0)      animTP.SetTrigger("Lower_Move");
            if ((photonAnimSync & PhotonAnim.LOWER_JUMP) != 0)      animTP.SetTrigger("Lower_Jump");
            if ((photonAnimSync & PhotonAnim.LAND) != 0)            animTP.SetTrigger("Land");
            if ((photonAnimSync & PhotonAnim.ATTACK) != 0)          animTP.SetTrigger("Attack");
            if ((photonAnimSync & PhotonAnim.END_ATTACK) != 0)      animTP.SetTrigger("EndAttack");
            if ((photonAnimSync & PhotonAnim.RATTACK) != 0)         animTP.SetTrigger("RAttack");
            if ((photonAnimSync & PhotonAnim.MELEE_ATTACK) != 0)    animTP.SetTrigger("MeleeAttack");
            if ((photonAnimSync & PhotonAnim.FSKILL) != 0)          animTP.SetTrigger("FSkill");
            if ((photonAnimSync & PhotonAnim.ESKILL) != 0)          animTP.SetTrigger("ESkill");
            if ((photonAnimSync & PhotonAnim.END_ESKILL) != 0)      animTP.SetTrigger("EndESkill");
            if ((photonAnimSync & PhotonAnim.ESKILL_SHOT) != 0)     animTP.SetTrigger("ESkillShot");
            if ((photonAnimSync & PhotonAnim.QSKILL) != 0)          animTP.SetTrigger("QSkill");
            if ((photonAnimSync & PhotonAnim.SHIFTSKILL) != 0)      animTP.SetTrigger("ShiftSkill");
            if ((photonAnimSync & PhotonAnim.RELOAD) != 0)          animTP.SetTrigger("Reload");
            if ((photonAnimSync & PhotonAnim.ESKILL_IDLE) != 0)     animTP.SetTrigger("ESkillIdle");

            if (photonAnimSync > 0) photonAnimSync = 0;
        }
    }

    public override void CheckDead()
    {
        if(hp <= 0)
            stateMachine.SetState(RoraState.Die);
    }

    public void EnterDead()
    {
        ragdollApplier.ActiveRagdoll();
        fpArm.SetActive(false);
        pv.RPC("RPC_SetActiveModelTP", RpcTarget.AllBuffered, false);
        HUD.transform.GetChild(0).gameObject.SetActive(false);
    }

    public override bool IsDead()
    {
        if (stateMachine == null || !pv.IsMine) return false;
        return (stateMachine.GetState() == RoraState.Die);
    }

    public override bool IsMine()
    {
        return pv.IsMine;
    }

    public override void ShowDeadUI()
    {
        if (!pv.IsMine) return;
        HUD.transform.GetChild(1).gameObject.SetActive(true);
    }

    public override void ShowResultUI(bool bIsVictory)
    {
        if (!pv.IsMine) return;
        HUD.transform.GetChild(0).gameObject.SetActive(false);
        HUD.transform.GetChild(2).gameObject.SetActive(true);
        HUD.transform.GetChild(2).GetComponent<HUD_GameFinish>().SetResult(bIsVictory);
    }

    public override void SendRespawn()
    {
        Destroy(HUD.gameObject);
        ragdollApplier.DeactiveRagdoll();
        base.SendRespawn();
    }

    [PunRPC]
    private void RPC_SetActiveModelTP(bool bActive)
    {
        tpModel.SetActive(bActive);
    }


    //=================== slow ==================//
    bool bIsStopped = false;
    [Header("슬로우 시간")]
    public float RoraSlowTime = 3f;
    [HideInInspector]
    public float  CurTimeValue;

    public void StartSlow(float slowValue)
    { 
        if (!bIsStopped)
        {
            bIsStopped = true;
            Time.timeScale = slowValue;
            StartCoroutine(Slowing());
        }
    }

    IEnumerator Slowing()
    {
        yield return new WaitForSecondsRealtime(RoraSlowTime);
        Time.timeScale = 1;
        bIsStopped = false;
    }

    public override void Dragged(GameObject dragger)
    {
        Debug.Log("Dragged");
        pv.RPC("RPC_Dragged", RpcTarget.AllBuffered, dragger.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    private void RPC_Dragged(int viewID)
    {
        Debug.Log("RPC_Dragged");
        dragger = PhotonNetwork.GetPhotonView(viewID).gameObject;
        stateMachine.SetState(RoraState.Dragged);
    }

    public bool IsInLAttack()
    {
        return (stateMachine.GetState() == RoraState.MainAttack);
    }
}















//======================== RoraStateMachine ===========================
public class RoraStateMachine
{
    // 멤버 변수
    PlayableState state;
    Rora rora;

    // 생성자
    public RoraStateMachine(Rora _rora)
    {
        // 생성될때 기본 상태는 Idle
        rora = _rora;
        state = new RoraIdle(_rora);
        state.Enter();
    }

    // 멤버 함수
    public void SetState(RoraState newState)
    {
        // 중복으로 상태를 설정하는지 검사
        if (GetState() == newState)
            return;

        state.Exit();

        // newState에 따라 상태를 설정한다.
        switch (newState)
        {
            case RoraState.Idle: { state = new RoraIdle(rora); } break;
            case RoraState.Move: { state = new RoraMove(rora); } break;
            case RoraState.Jump: { state = new RoraJump(rora); } break;
            case RoraState.MainAttack: { state = new RoraAttack(rora); } break;
            case RoraState.SubAttack: { state = new RoraRAttack(rora); } break;
            case RoraState.VSkill: { state = new RoraVSkill(rora); } break;
            case RoraState.QSkill: { state = new RoraQSkill(rora); } break;
            case RoraState.ESkill: { state = new RoraESkill(rora); } break;
            case RoraState.Teleport: { state = new RoraShiftSkill(rora); } break;
            case RoraState.FSkill: { state = new RoraFSkill(rora); } break;
            case RoraState.Reload: { state = new RoraReload(rora); } break;
            case RoraState.ESnipeMode: { state = new RoraESnipeMode(rora); } break;
            case RoraState.ESnipeShot: { state = new RoraESnipeShot(rora); } break;
            case RoraState.Die: { state = new RoraDead(rora); } break;
            
                // 상태 이상
            case RoraState.Faint: { state = new RoraFaint(rora); } break;
            case RoraState.Sleep: { state = new RoraSleep(rora); } break;
            case RoraState.Restraint: { state = new RoraRestraint(rora); } break;
            case RoraState.Silence: { state = new RoraSilence(rora); } break;
            case RoraState.Dragged: { state = new RoraDragged(rora); } break;
            case RoraState.Slow: { state = new RoraSlow(rora); } break;
        }

        state.Enter();
    }

    public void UpdateState()
    {
        state.Update();
    }

    public RoraState GetState()
    {
        RoraState curState = RoraState.Error;

        string typeStr = null;
        typeStr = state.Type;
        switch (typeStr)
        {
            case "RoraIdle": curState = RoraState.Idle; break;
            case "RoraMove": curState = RoraState.Move; break;
            case "RoraJump": curState = RoraState.Jump; break;
            case "RoraAttack": curState = RoraState.MainAttack; break;
            case "RoraRAttack": curState = RoraState.SubAttack; break;
            case "RoraVSkill": curState = RoraState.VSkill; break;
            case "RoraQSkill": curState = RoraState.QSkill; break;
            case "RoraESkill": curState = RoraState.ESkill; break;
            case "RoraShiftSkill": curState = RoraState.Teleport; break;
            case "RoraFSkill": curState = RoraState.FSkill; break;
            case "RoraReload": curState = RoraState.Reload; break;
            case "RoraESnipeMode": curState = RoraState.ESnipeMode; break;
            case "RoraESnipeShot": curState = RoraState.ESnipeShot; break;
            case "RoraDead": curState = RoraState.Die; break;

            // 상태 이상
            case "RoraFaint": curState = RoraState.Faint; break;
            case "RoraSleep": curState = RoraState.Sleep; break;
            case "RoraRestraint": curState = RoraState.Restraint; break;
            case "RoraSilence": curState = RoraState.Silence; break;
            case "RoraAttraction": curState = RoraState.Attraction; break;
            case "RoraSlow": curState = RoraState.Slow; break;
        }

        return curState;
    }
}

public abstract class RoraBase : PlayableState
{
    protected Rora rora;
    protected Animator animTP;
    protected Animator animFP;

    public RoraBase(Rora _rora) { rora = _rora; animTP = rora.animTP; animFP = rora.animFP; }
}

public class RoraIdle : RoraBase
{
    public override string Type { get { return "RoraIdle"; } }

    public RoraIdle(Rora rora) : base(rora) { }

    public override void Enter()
    {
        Debug.Log("Enter Idle");

        // 이미 Idle 애니메이션이 재생중인데 다시 재생되는것을 막는다.
        if (!Utilities.IsAnimationPlaying(animFP, "RoraFP_Idle")) animFP.SetTrigger("Idle");
        if (!Utilities.IsAnimationPlaying(animTP, "Idle"))
        {
            animTP.SetTrigger("Idle");
            rora.photonAnimSync |= Rora.PhotonAnim.IDLE;
        }
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        rora.TryMove();
    }
}

public class RoraMove : RoraBase
{
    public override string Type { get { return "RoraMove"; } }

    public RoraMove(Rora rora) : base(rora) { }

    public override void Enter()
    {
        Debug.Log("Enter Move");
        if (!Utilities.IsAnimationPlaying(animFP, "RoraFP_Idle")) animFP.SetTrigger("Idle");
        if (!Utilities.IsAnimationPlaying(animTP, "Idle"))
        {
            animTP.SetTrigger("Idle");
            rora.photonAnimSync |= Rora.PhotonAnim.IDLE;
        }
    }

    public override void Exit()
    {
        // empty
    }

    public override void Update()
    {
        rora.TryMove();
    }
}

public class RoraJump : RoraBase
{
    public override string Type { get { return "RoraJump"; } }

    public RoraJump(Rora rora) : base(rora) { }

    public override void Enter()
    {
        Debug.Log("Enter Jump");

        // 땅점프 시도
        rora.TryJump();

        if (!Utilities.IsAnimationPlaying(animFP, "RoraFP_Idle")) animFP.SetTrigger("Idle");
        if (!Utilities.IsAnimationPlaying(animTP, "Idle"))
        {
            animTP.SetTrigger("Idle");
            rora.photonAnimSync |= Rora.PhotonAnim.IDLE;
        }

    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        rora.TryJump();
        rora.TryMove();
    }
}

public class RoraAttack : RoraBase
{
    public override string Type { get { return "RoraAttack"; } }

    public RoraAttack(Rora rora) : base(rora) { }

    public override void Enter()
    {
        Debug.Log("Enter Attack");
        rora.bEndMainAttack = false;
        animFP.SetTrigger("Attack");
        animTP.SetTrigger("Attack");
        rora.photonAnimSync |= Rora.PhotonAnim.ATTACK;
    }

    public override void Exit()
    {
        Debug.Log("Exit Attack");
        if (!rora.bEndMainAttack) rora.bEndMainAttack = true;
        rora.GetComponent<SkillControl>().AttackEnd();
    }

    public override void Update()
    {
        // 총알이 없을때 발사를 하려고 하면 자동으로 재장전 상태로 전환한다.
        if (rora.GetComponent<SkillControl>().remainBullet < 1)
        {
            rora.bEndReload = false;
            rora.bEndMainAttack = true;
            rora.stateMachine.SetState(RoraState.Reload);
            return;
        }

        if(!Input.GetKey(KeyManager.Inst.Attack))
        {
            animTP.SetTrigger("EndAttack");
            rora.photonAnimSync |= Rora.PhotonAnim.END_ATTACK;
        }

        if (Utilities.IsAnimationFinish(animTP, "MainAttack2"))
            rora.bEndMainAttack = true;

        rora.TryJump();
        rora.TryMove();
    }
}

public class RoraRAttack : RoraBase
{
    public override string Type { get { return "RoraRAttack"; } }

    public RoraRAttack(Rora rora) : base(rora) { }

    private const float ATTACK_INTERVAL = 0.6f;
    static private float timer = 0f;

    public override void Enter()
    {
        Debug.Log("Enter RAttack");

        rora.bEndSubAttack = false;
        animFP.SetTrigger("RAttack");
        animTP.SetTrigger("RAttack");
        rora.photonAnimSync |= Rora.PhotonAnim.RATTACK;
    }

    public override void Exit()
    {
        if (!rora.bEndSubAttack) rora.bEndSubAttack = true;
    }

    public override void Update()
    {
        // 총알이 없을때 발사를 하려고 하면 자동으로 재장전 상태로 전환한다.
        if (rora.GetComponent<SkillControl>().remainBullet < 1)
        {
            rora.bEndReload = false;
            rora.bEndMainAttack = true;
            rora.stateMachine.SetState(RoraState.Reload);
            return;
        }

        //Debug.Log("Update RAttack");
        rora.TryJump();
        rora.TryMove();

        if (!Input.GetKey(KeyManager.Inst.RAttack))
        {
            rora.bEndSubAttack = true;
        }
    }
}

public class RoraVSkill : RoraBase
{
    public override string Type { get { return "RoraVSkill"; } }

    public RoraVSkill(Rora rora) : base(rora) { }

    public override void Enter()
    {
        rora.bEndMeleeAttack = false;
        animFP.SetTrigger("MeleeAttack");
        animTP.SetTrigger("MeleeAttack");
        rora.photonAnimSync |= Rora.PhotonAnim.MELEE_ATTACK;
    }

    public override void Exit()
    {
        if (!rora.bEndMeleeAttack) rora.bEndMeleeAttack = true;
    }

    public override void Update()
    {
        rora.TryJump();
        rora.TryMove();

        // 현재 애니메이션이 근접 공격 관련 애니메이션이고 해당 애니메이션이 끝났다면
        // 근접 공격 상태를 끝내기 위한 bool값을 설정한다.
        if (
            Utilities.IsAnimationPlaying(animFP, "RoraFP_MeleeAttack") &&
            Utilities.IsAnimationFinish(animFP, "RoraFP_MeleeAttack"))
        {
            rora.bEndMeleeAttack = true;
        }
    }
}

public class RoraFSkill : RoraBase
{
    bool bIsKeyUp;

    public override string Type { get { return "RoraFSkill"; } }

    public RoraFSkill(Rora rora) : base(rora) { }

    public override void Enter()
    {
        bIsKeyUp = false;
        rora.bEndFSkill = false;
        animFP.SetTrigger("FSkill");
        animTP.SetTrigger("FSkill");
        rora.photonAnimSync |= Rora.PhotonAnim.FSKILL;
    }

    public override void Exit()
    {
        if (!rora.bEndFSkill) rora.bEndFSkill = true;
    }

    public override void Update()
    {
        rora.TryJump();
        rora.TryMove();

        if (Input.GetKeyUp(KeyManager.Inst.FSkill))
            bIsKeyUp = true;

        if (bIsKeyUp &&
            Utilities.IsAnimationPlaying(animTP, "Fskill 2") &&
            Utilities.IsAnimationFinish(animTP, "Fskill 2"))
        {
            rora.bEndFSkill = true;
        }
    }
}

public class RoraESkill : RoraBase
{
    float blackholeLifetime;
    float timer;

    public override string Type { get { return "RoraESkill"; } }

    public RoraESkill(Rora rora) : base(rora) {}

    public override void Enter()
    {
        rora.bEndESkill = false;

        timer = 0f;
        blackholeLifetime = 3.0f;

        animFP.SetTrigger("ESkill");
        animTP.SetTrigger("ESkill");
        rora.photonAnimSync |= Rora.PhotonAnim.ESKILL;
    }

    public override void Exit()
    {
        if (!rora.bEndESkill) rora.bEndESkill = true;
        rora.GetComponent<SkillControl>().ESkillEnd();
    }

    public override void Update()
    {
        rora.TryJump();
        rora.TryMove(rora.MoveMultiplierInBlackHole);

        // 유지 시간 동안 블랙홀 스킬 유지
        timer += Time.deltaTime;
        //Debug.Log("Timer: " + timer);
        if (timer >= blackholeLifetime)
        {
            // 종료 애니메이션 출력후 저격 모드로 들어간다.
            if (!Utilities.IsAnimationPlaying(animTP, "BlackHole3"))
            {
                animTP.SetTrigger("EndESkill");
                rora.photonAnimSync |= Rora.PhotonAnim.END_ESKILL;
            }
            else if (
                Utilities.IsAnimationPlaying(animTP, "BlackHole3") &&
                Utilities.IsAnimationFinish(animTP, "BlackHole3"))
            {

                rora.bEndESkill = true;
                rora.bEndESnipeMode = false;
            }
        }
    }
}

public class RoraESnipeMode : RoraBase
{
    public override string Type { get { return "RoraESnipeMode"; } }

    public RoraESnipeMode(Rora rora) : base(rora) { }

    public override void Enter()
    {
        animFP.SetTrigger("Idle");
        animTP.SetTrigger("ESkillIdle");
        rora.photonAnimSync |= Rora.PhotonAnim.ESKILL_IDLE;
        rora.audioManager.PlayModelSound(0);
    }

    public override void Exit()
    {
        rora.bEndESnipeMode = true;
        rora.GetComponent<SkillControl>().SniperEnd();
    }

    public override void Update()
    {
        rora.TryJump();
        rora.TryMove();
    }
}

public class RoraESnipeShot : RoraBase
{
    public override string Type { get { return "RoraESnipeShot"; } }

    public RoraESnipeShot(Rora rora) : base(rora) { }

    public override void Enter()
    {
        rora.bEndESnipeShot = false;
        animFP.SetTrigger("ESkillShot");
        animTP.SetTrigger("ESkillShot");
        rora.photonAnimSync |= Rora.PhotonAnim.ESKILL_SHOT;
        
    }

    public override void Exit()
    {
        if (!rora.bEndESnipeShot) rora.bEndESnipeShot = true;
        rora.GetComponent<SkillControl>().SniperEnd();
    }

    public override void Update()
    {
        rora.TryJump();
        rora.TryMove();

        if (
            Utilities.IsAnimationPlaying(animTP, "RoraTP_SnipeShot2") &&
            Utilities.IsAnimationFinish(animTP, "RoraTP_SnipeShot2"))
        {
            rora.bEndESnipeShot = true;
        }
    }
}

public class RoraQSkill : RoraBase
{
    public override string Type { get { return "RoraQSkill"; } }

    public RoraQSkill(Rora rora) : base(rora) { }

    bool bIsInTransition = false;

    public override void Enter()
    {
        Debug.Log("Enter QSkill");

        rora.bEndQSkill = false;

        rora.bIsTurnCameraEnable = false;
        rora.bIsTurnPlayerEnable = false;

        // 주공격, 보조 공격의 전환 중에 트리거가 발생하면 트리거가 씹히므로 Update에서 트리거 시켜준다.
        if(animTP.IsInTransition(0))
        {
            bIsInTransition = true;
        }
        else
        {
            animFP.SetTrigger("QSkill");
            animTP.SetTrigger("QSkill");
        }
        rora.photonAnimSync |= Rora.PhotonAnim.QSKILL;
    }

    public override void Exit()
    {
        if (!rora.bEndQSkill) rora.bEndQSkill = true;
        rora.bIsTurnCameraEnable = true;
        rora.bIsTurnPlayerEnable = true;
    }

    public override void Update()
    {
        if(bIsInTransition && !animTP.IsInTransition(0))
        {
            animFP.SetTrigger("QSkill");
            animTP.SetTrigger("QSkill");
            bIsInTransition = false;
        }

        if (
            Utilities.IsAnimationPlaying(animTP, "QSkill 2") &&
            Utilities.IsAnimationFinish(animTP, "QSkill 2"))
        {
            rora.bEndQSkill = true;
        }
    }
}

public class RoraShiftSkill : RoraBase
{
    public override string Type { get { return "RoraShiftSkill"; } }

    public RoraShiftSkill(Rora rora) : base(rora) { }

    bool bIsInTransition = false;

    public override void Enter()
    {
        rora.bEndShiftSkill = false;

        // 주공격, 보조 공격의 전환 중에 트리거가 발생하면 트리거가 씹히므로 Update에서 트리거 시켜준다.
        if (animTP.IsInTransition(0))
        {
            bIsInTransition = true;
        }
        else
        {
            animFP.SetTrigger("Idle");
            animTP.SetTrigger("ShiftSkill");
        }
        rora.photonAnimSync |= Rora.PhotonAnim.SHIFTSKILL;
    }

    public override void Exit()
    {
        if (!rora.bEndShiftSkill) rora.bEndShiftSkill = true;
        rora.gravity = rora.teleportAirGravity;
    }

    public override void Update()
    {
        if (bIsInTransition && !animTP.IsInTransition(0))
        {
            animFP.SetTrigger("Idle");
            animTP.SetTrigger("ShiftSkill");
            bIsInTransition = false;
        }

        if (
            Utilities.IsAnimationPlaying(animTP, "Teleport") &&
            Utilities.IsAnimationFinish(animTP, "Teleport"))
        {
            rora.bEndShiftSkill = true;
        }
    }
}

public class RoraReload : RoraBase
{
    public override string Type { get { return "RoraReload"; } }

    public RoraReload(Rora rora) : base(rora) { }

    public override void Enter()
    {
        animFP.SetTrigger("Reload");
        animTP.SetTrigger("Reload");
        rora.photonAnimSync |= Rora.PhotonAnim.RELOAD;
        rora.audioManager.PlayWeaponSound(6); //리로드
        Debug.Log("리로드");

        // 수동 재장전인 경우 0.1배 더 빠르게 재장전을 한다.

        if (rora.bEndReload)
        {
            rora.bEndReload = false;
            animFP.speed = rora.manualReloadSpeed;
            animTP.speed = rora.manualReloadSpeed;
        }
    }

    public override void Exit()
    {
        if (!rora.bEndReload) rora.bEndReload = true;
    }

    public override void Update()
    {
        rora.TryJump();
        rora.TryMove();

        // 재장전 애니메이션이 끝났다면 탄창을 꽉 채운다.
        if (
            Utilities.IsAnimationPlaying(animFP, "RoraFP_Reload") &&
            Utilities.IsAnimationFinish(animFP, "RoraFP_Reload"))
        {
            //casey.remainBullet = casey.maxMagazine;
            rora.bEndReload = true;

            // 애니메이터 속도를 정상으로 되돌린다.
            rora.animFP.speed = 1.0f;
            rora.animTP.speed = 1.0f;
        }

    }
}

public class RoraDead : RoraBase
{
    public override string Type { get { return "RoraDead"; } }

    public RoraDead(Rora rora) : base(rora) { }

    HUDManager hudManager;

    public override void Enter()
    {
        rora.bEndDead = false;
        rora.bIsTurnCameraEnable = false;
        rora.bIsTurnPlayerEnable = false;
        rora.EnterDead();
        rora.audioManager.PlayVoiceSound(1); // 죽음 사운드 추가

        hudManager = rora.HUD.GetComponent<HUDManager>();
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        if (!hudManager.DeadHUD.isActiveAndEnabled)
            return;

        float timer = hudManager.GetDeadHUD_Timer();
        if (timer <= float.Epsilon)
        {
            rora.audioManager.PlayVoiceSound(2); //부활 사운드 재생 추가
            rora.SendRespawn();
        }

        if (!hudManager.GameFinishHUD.isActiveAndEnabled)
            return;

        timer = hudManager.GetGameFinishHUD_Timer();
        if(timer <= 0.1)
        {
            PhotonNetwork.Destroy(rora.gameObject);
        }
    }
}

//============================= 상태이상 =========================

public class RoraFaint  : RoraBase
{
    public override string Type { get { return "RoraFaint"; } }

    public RoraFaint (Rora rora) : base(rora) { }

    public override void Enter()
    {
       // rora.bEndShiftSkill = false;

       // animFP.SetTrigger("Idle");
      //  animTP.SetTrigger("ShiftSkill");
    }

    public override void Exit()
    {
      //  if (!rora.bEndShiftSkill) rora.bEndShiftSkill = true;
       // rora.gravity = rora.teleportAirGravity;
    }

    public override void Update()
    {
        if (true
           // Utilities.IsAnimationPlaying(animTP, "Teleport") &&
           // Utilities.IsAnimationFinish(animTP, "Teleport")
           )
        {
          //  rora.bEndShiftSkill = true;
        }
    }
}

public class RoraSleep : RoraBase
{
    public override string Type { get { return "RoraSleep"; } }

    public RoraSleep(Rora rora) : base(rora) { }

    public override void Enter()
    {
        // rora.bEndShiftSkill = false;

        // animFP.SetTrigger("Idle");
        //  animTP.SetTrigger("ShiftSkill");
    }

    public override void Exit()
    {
        //  if (!rora.bEndShiftSkill) rora.bEndShiftSkill = true;
        // rora.gravity = rora.teleportAirGravity;
    }

    public override void Update()
    {
        if (true
           // Utilities.IsAnimationPlaying(animTP, "Teleport") &&
           // Utilities.IsAnimationFinish(animTP, "Teleport")
           )
        {
            //  rora.bEndShiftSkill = true;
        }
    }
}

public class RoraRestraint : RoraBase
{
    public override string Type { get { return "RoraRestraint"; } }

    public RoraRestraint(Rora rora) : base(rora) { }

    public override void Enter()
    {
        // rora.bEndShiftSkill = false;

        // animFP.SetTrigger("Idle");
        //  animTP.SetTrigger("ShiftSkill");
    }

    public override void Exit()
    {
        //  if (!rora.bEndShiftSkill) rora.bEndShiftSkill = true;
        // rora.gravity = rora.teleportAirGravity;
    }

    public override void Update()
    {
        if (true
           // Utilities.IsAnimationPlaying(animTP, "Teleport") &&
           // Utilities.IsAnimationFinish(animTP, "Teleport")
           )
        {
            //  rora.bEndShiftSkill = true;
        }
    }
}


public class RoraSilence : RoraBase
{
    public override string Type { get { return "RoraSilence"; } }

    public RoraSilence(Rora rora) : base(rora) { }

    public override void Enter()
    {
        // rora.bEndShiftSkill = false;

        // animFP.SetTrigger("Idle");
        //  animTP.SetTrigger("ShiftSkill");
    }

    public override void Exit()
    {
        //  if (!rora.bEndShiftSkill) rora.bEndShiftSkill = true;
        // rora.gravity = rora.teleportAirGravity;
    }

    public override void Update()
    {
        if (true
           // Utilities.IsAnimationPlaying(animTP, "Teleport") &&
           // Utilities.IsAnimationFinish(animTP, "Teleport")
           )
        {
            //  rora.bEndShiftSkill = true;
        }
    }
}

public class RoraDragged : RoraBase
{
    public override string Type { get { return "RoraDragged"; } }

    public RoraDragged(Rora rora) : base(rora) { }

    const float LIFE_TIME = 0.3f;
    float t;
    Vector3 startPos;

    public override void Enter()
    {
        Debug.Log("Drag Enter");

        t = 0f;
        startPos = rora.transform.position;
        rora.bEndDragged = false;
        rora.bIsTurnCameraEnable = false;
        rora.bIsTurnPlayerEnable = false;

        animFP.SetTrigger("Idle");
        animTP.SetTrigger("Idle");
        rora.photonAnimSync |= Rora.PhotonAnim.IDLE;
    }

    public override void Exit()
    {
        if(!rora.bEndDragged)   rora.bEndDragged = true;
        rora.bIsTurnCameraEnable = true;
        rora.bIsTurnPlayerEnable = true;
    }

    public override void Update()
    {
        Debug.Log("Dragged Update");

        if (t < LIFE_TIME)
        {
            t += Time.deltaTime;
            rora.characterController.Move(
                (rora.dragger.transform.position - startPos) * (Time.deltaTime / (LIFE_TIME + 0.1f)));
        }
        else
        {
            rora.bEndDragged = true;
        }
    }
}

public class RoraSlow : RoraBase
{
    public override string Type { get { return "RoraSlow"; } }

    public RoraSlow(Rora rora) : base(rora) { }

    public override void Enter()
    {
        // rora.bEndShiftSkill = false;

        // animFP.SetTrigger("Idle");
        //  animTP.SetTrigger("ShiftSkill");
    }

    public override void Exit()
    {
        //  if (!rora.bEndShiftSkill) rora.bEndShiftSkill = true;
        // rora.gravity = rora.teleportAirGravity;
    }

    public override void Update()
    {
        if (true
           // Utilities.IsAnimationPlaying(animTP, "Teleport") &&
           // Utilities.IsAnimationFinish(animTP, "Teleport")
           )
        {
            //  rora.bEndShiftSkill = true;
        }
    }
}