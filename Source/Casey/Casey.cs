using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/*상태이상 - 3가지까지 중첩 가능, 4번째 상태이상 중첩시 맨 처음 발생한 상태이상은 해제, 효과 동일한 상태이상 중첩시 먼저 발생한 효과 해제 후 효과 추가
            <조작>
            기절 : 이동 및 조작 불가능, 스킬,공격 불가능, 공격 스킬 중간에 취소
            수면 : 이동 및 조작 불가능, 스킬,공격 불가능, 공격 스킬 중간에 취소, HP변화가 발생하면 수면상태 해제/ 수면상태 중 기절상태 중첩시 수면상채 해제 후 기절상태로 초기화 
            속박 : 이동 불가능, 스킬, 주, 보조공격 시정가능/이동스킬?
            침묵 : 조작 불능, 스킬, 공격 불가능, 중간에 취소, 이동은 가능/침묵상태에서 수면, 기절상태 중첩시 침묵해제후 상태 돌입
            끌어당김 : 기절상태로 설정된 위치값으로 이동하고 몇초 후 기절상태 해제/이동기 스킬 해제,점멸과 같은 특수 이동기에는 우위를 가지지못함
            에어본
            넘기기
            <디버프>
            슬로우 : 이동 및 조작이 가능, 기존 이동, 공격, 스킬 시전 속도 저하/속도 버프 받으면 슬로우 해제, 버프중 슬로우걸리면 버프 해제, 스킬 시전중 상태이상 걸리면 현재 스킬 끝낸 후 부터 상태이상
            블라인드(시야 차단)
            방어력 감소
            자속피해(도트데미지)
            치료효과 감소
            */
public enum CaseyState
{
    // 케이시의 각 상태
    Error,
    Idle, Move, Jump,
    Attack, RAttack, MeleeAttack,   // 오리사 평타, 시메트라 우클릭,      궁극기 : 파라 궁 느낌
    ESkill, ShiftSkill, QSkill,     // ※ F스킬은 상태를 만들 필요가 없을 정도로 간단하므로 제외한다.
    Reload, CC, Die, Dragged
}
//bisGrounded, bReroading, bisJumping
public class Casey : Playable, IPunObservable
{
    // 상태기계
    CaseyStateMachine stateMachine;
    // 체력
    [Header("체력")]
    [Header("============캐릭터============")]
    HUD_HealthBar hpBar;
    [Tooltip("HP 최대 값")] public const int MAX_HP = 300;
    [Tooltip("실드 최대 값")] public const int MAX_SHIELD = 100;
    int totalHP;
    [Tooltip("(Look)실드")] public int shield;

    //탄창
    HUD_Magazine caseyMagazine;
    [Tooltip("탄창")] public int maxMagazine = 100;
    [Tooltip("(Look)탄약")] public int remainBullet;

    //타격게이지
    [Header("케이시 게이지")]
    [Tooltip("쿨타임 타격 게이지")] public float HUDHitGauge;
    [Tooltip("최대 타격 게이지")] public int maxhitGauge = 100;
    [Tooltip("(Look)타격 게이지")] public int hitGauge;
    [Tooltip("최대 궁극기 게이지")] public int maxultGauge = 100;//입힌 데미지 만큼 증가
    [Tooltip("(Look)궁극기 게이지")] public int ultGauge;

    //== 이동 ==
    [Header("이동")]
    [Header("============기동============")]
    [Tooltip("기본 이동 속도")] public float defaultrunSpeed = 6.0f;
    [Tooltip("(Look)이동 속도")] public float runSpeed;

    //== 중력 ==
    [Header("중력")]
    [Tooltip("기본 중력")] public float defaultGravity = -1.5f;
    [Tooltip("공중유지상태 중력")] public float keepAirGravity = -0.01f;
    [Tooltip("(Look)케이시 현재 중력")] public float caseyGravity;
    [Tooltip("(Look)땅인지 체크")] public bool bIsGrounded = true;
    [HideInInspector] public float jumpInterval = 0;

    //== 점프 ==
    [Header("점프")]
    [Tooltip("공중점프 최대 횟수")] public int maxAirJumpCount = 4;
    [Tooltip("(Look)공중 점프 카운트")] public int airJumpCount;
    [Tooltip("땅 점프 파워")] public float groundJumpPower = 0.8f;
    [Tooltip("공중 점프 파워")] public float airJumpPower = 0.8f;
    [Tooltip("(Look)현재 수직 속력")] public float yVelocity;

    //== 공중유지 ==
    [Header("공중 유지")]
    [Tooltip("공중 유지 시작하는 스페이스바 입력 최소값")] public float limitSpaceTime = 0.25f;
    [Tooltip("(Look)스페이스바 입력 시간")] public float spaceTime = 0;

    //===== 발사 ======
    [Header("발사")]
    [Header("============공격============")]
    [Tooltip("(Look)발사 시간")] public float fireTime;
    [Tooltip("총구")] public GameObject FP_Muzzle;
    [Tooltip("총구")] public GameObject TP_Muzzle;
    [Tooltip("총구")] public GameObject FP_RMuzzle;
    [Tooltip("헤드계수")] public float head_coef = 1.5f;

    //===== 재장전 =====
    [Header("재장전")]
    [Tooltip("자동 장전 속도")] public float autoReloadSpeed = 1.0f;
    [Tooltip("수동 장전 속도")] public float manualReloadSpeed = 1.1f;

    //===== MouseL 공격 =====
    [Header("주공격")]
    [Tooltip("주공격 총알")] public GameObject MLbullet;
    [Tooltip("ML 연속발사 간격")] public float MLautoFireRate = 0.3f;
    [Tooltip("ML소모 탄약")] public int MLattackAmmo = 1;

    //===== MouseR 공격 =====
    [Header("보조공격")]
    [Tooltip("보조공격 총알")] public GameObject MRbullet;
    [Tooltip("주공격 차징 효과")] public GameObject MRcharging;
    [Tooltip("MR 최대 입력 시간")] public float maxMRpushTime = 1.5f;
    [Tooltip("MR 최소 입력 시간")] public float minMRpushTime = 1.2f;
    [Tooltip("(Look)MR누르고 있는 시간")] public float MRpushTime;
    [Tooltip("차징 최대 크기")] public float MaxMRchargingSize = 1f;
    [Tooltip("(Look)차징 크기 변화")] public float MRchargingSize;
    [Tooltip("MR소모 탄약")] public int MRattackAmmo = 25;

    //===== V 공격 =====
    [Header("근접 공격")]
    [Tooltip("히트박스")] public GameObject VAttackHitBox;
    [Tooltip("재사용 대기 시간")] public float defaultVCoolTime = 3.0f;
    [Tooltip("(Look)재사용 대기 시간")] public float VCoolTime;
    [Tooltip("V공격 시간")] public float defaultVTime = 1.0f;
    [Tooltip("(Look)V공격 시간")] public float VTime;
    [Tooltip("V데미지")] public int VdefaultDamage = 60;
    [Tooltip("V근접 강화 계수")] public float V_coef = 1.5f;
    [Tooltip("(Look)V데미지")] public int Vdamage;

    //===== F 스킬 =====
    [Header("방어기")]
    [Header("============스킬============")]
    [Tooltip("베리어")] public GameObject barrier;
    [Tooltip("재사용 대기 시간")] public float defaultBarrierCoolTime = 10.0f;
    [Tooltip("(Look)재사용 대기 시간")] public float barrierCoolTime;
    [Tooltip("총 베리어 유지 시간")] public float maxBarrierTime = 6.0f;
    [Tooltip("남은 베리어 유지 시간")] public float barrierTime;
    [Tooltip("베리어 체력")] public int defaultbarrierHp = 150;
    [Tooltip("남은 베리어 체력")] public int barrierHp;
    [Tooltip("감소 속도(%)"), Range(1, 50)] public int reduceSpeed = 10;

    //===== E 스킬 =====
    [Header("적 끌어오기")]
    [Tooltip("끌어오기 검출 영역 오브젝트")] public GameObject ConeSpace;
    [Tooltip("원뿔 반지름")] public float coneRadius = 1.0f;
    [Tooltip("원뿔 거리")] public float coneDistance = 1.0f;
    [Tooltip("재사용 대기 시간")] public float defaultESkillCoolTime = 4.0f;
    [Tooltip("(Look)재사용 대기 시간")] public float ESkillCoolTime;
    [Tooltip("E선 딜레이")] public float Edelay = 0.2f;
    [Tooltip("E 유지 시간")] public float defaultESkillTime = 1.0f;
    [Tooltip("(Look) E 유지 시간")] public float ESkillTime;


    //===== Q 스킬 =====
    [Header("궁극기")]
    [Tooltip("궁극기 총알")] public GameObject QBullet;
    [Tooltip("총구1")] public GameObject QMuzzle1;
    [Tooltip("총구2")] public GameObject QMuzzle2;
    [Tooltip("Q선 딜레이")] public float Qdelay = 0.5f;
    [Tooltip("(Look)재사용 대기 시간 + 게이지")] public float CoolTimeUltGauge = 0;
    [Tooltip("재사용 대기 시간")] public float defaultQSkillCoolTime = 1.0f;
    [Tooltip("(Look)재사용 대기 시간")] public float QSkillCoolTime;
    [Tooltip("스킬 시전 시간")] public float defaultQSkillTime = 3.0f;
    [Tooltip("(Look)스킬 시전 시간")] public float QSkillTime;
    [Tooltip("총알 발사 간격")] public float defaultQFireRate = 0.2f;
    [Tooltip("(Look)총알 발사 간격")] public float QFireRate;
    [Tooltip("미사일 수")] public int dsfaultQMissileNum = 12;
    [Tooltip("(Look)남은 미사일 수")] public int QMissileNum;

    //===== Shift 스킬 =====
    [Header("대쉬")]
    [Tooltip("대쉬 히트 박스")] public GameObject DashHitBox;
    [Tooltip("대쉬 엔진")] public GameObject DashEngine;
    [Tooltip("대쉬 이펙트")] public GameObject DashEffect;
    [Tooltip("선 딜레이 시간")] public float ShiftDelayTime = 0.2f;
    [Tooltip("재사용 대기 시간")] public float defaultDashCoolTime = 3.5f;
    [Tooltip("(Look)재사용 대기 시간")] public float DashCoolTime;
    [Tooltip("대쉬 스피드")] public float dashSpeed = 28.0f;
    [Tooltip("대쉬 시간")] public float defaultDashTime = 1.5f;
    [Tooltip("(Look)대쉬 시간")] public float dashTime;
    [Tooltip("대쉬 데미지")] public int dash_damage = 10;

    //===== 컴포넌트 =====
    [Header("컴포넌트")]
    public GameObject[] models;
    public GameObject fpArm;
    public GameObject tpModel;
    public Animator animTP;
    public Animator animFP;
    public BoxCollider[] colliders;
    [HideInInspector] public CharacterController characterController;

    //===== 기타 변수=====
    [HideInInspector] public Vector3 direction;
    public GameObject GenericIK;
    public GameObject prefabHUD;
    [HideInInspector] public GameObject HUD;
    const float GROUND_CHECK_TIME = 0.1f;
    float groundCheckTimer = GROUND_CHECK_TIME;
    const float CEIL_CHECK_TIME = 0.1f;
    float ceilCheckTimer = CEIL_CHECK_TIME;

    // 포톤 애니메이션 싱크용 Enum 및 변수
    public enum PhotonAnim
    {
        IDLE =          0b000000000000001,
        LOWER_IDLE =    0b000000000000010,
        MOVE =          0b000000000000100,
        JUMP =          0b000000000001000,
        FALL =          0b000000000010000,
        RELOAD =        0b000000000100000,
        ATTACK =        0b000000001000000,
        RATTACK =       0b000000010000000,
        VATTACK =       0b000000100000000,
        FSKILL =        0b000001000000000,
        ESKILL =        0b000010000000000,
        SHIFTSKILL =    0b000100000000000,
        QSKILL =        0b001000000000000
    }
    [HideInInspector] public PhotonAnim photonAnimSync = 0;

    //IK ik;//ik변수 사용

    //===== 상태 관리 변수 =====
    [HideInInspector] public bool bEndMainAttack = true;
    [HideInInspector] public bool bEndSubAttack = true;
    [HideInInspector] public bool bEndESkill = true;
    [HideInInspector] public bool bEndFSkill = true;
    [HideInInspector] public bool bEndQSkill = true;
    [HideInInspector] public bool bEndShiftSkill = true;
    [HideInInspector] public bool bEndMeleeAttack = true;
    [HideInInspector] public bool bEndReload = true;
    [HideInInspector] public bool bEndDead = true;
    [HideInInspector] public bool bEndDragged = true;
    [HideInInspector] public bool bIsTurnEnable = true;

    public AudioClip itemClip;

    //===== Audio =====
   [HideInInspector] public PlayerAudio audioManager;
    private AudioSource audioSource;
    [HideInInspector] public bool bDashAudioPlay = false;

    // 상태이상 관련
    /*[HideInInspector] */public GameObject dragger;

    protected override void OnEnable()
    {
        base.OnEnable();

        bCanInteract = true;

           // 오브젝트가 플레이어가 조종하는게 아니라면
           pv = GetComponent<PhotonView>();
        if (!pv.IsMine)
        {
            // 1인칭 시점을 비활성화하고 3인칭 모델을 보이게 만들어준다.
            GetComponentInChildren<Camera>().enabled = false;
            fpArm.SetActive(false);
            for (int i = 0; i < models.Length; i++)
                models[i].layer = LayerMask.NameToLayer("Enemy");

            // 또한 컬라이더의 레이어를 Enemy로 설정해준다.
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].gameObject.layer = LayerMask.NameToLayer("Enemy");
        }

        // 컴포넌트 초기화
        characterController = GetComponent<CharacterController>();

        // 중력 초기화
        caseyGravity = defaultGravity;
        runSpeed = defaultrunSpeed;

        // 상태기계 초기화
        stateMachine = new CaseyStateMachine(this);

        // HUD 인스턴스 생성
        HUD = Instantiate(prefabHUD);
        HUD.GetComponent<HUDManager>().Character = this.gameObject;
        HUD.GetComponent<HUDManager>().nickname.text = PhotonNetwork.LocalPlayer.NickName.ToString();
        Debug.Log(PhotonNetwork.LocalPlayer.NickName.ToString());
        hpBar = HUD.GetComponent<HUD_HealthBar>();
        caseyMagazine = HUD.GetComponent<HUD_Magazine>();
        if (!pv.IsMine) HUD.SetActive(false);

        // hp 초기화
        hp = MAX_HP;
        shield = 0;
        barrierHp = 0;
        totalHP = MAX_HP;

        //탄창 초기화
        remainBullet = maxMagazine;
        caseyMagazine.Init(maxMagazine.ToString());

        // audioManager
        audioSource = GetComponent<AudioSource>();
        audioManager = this.GetComponent<PlayerAudio>();

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


    private void FixedUpdate()
    {
        if (!pv.IsMine || !bCanInteract)
            return;

        ApplyGravity();             // 중력 적용
        CheckOnGround();            // 땅인지 체크
        CheckCollideWithCeil();     // 천장에 부딫쳤는지 체크
    }

    protected override void Update()
    {
        if (!pv.IsMine || !bCanInteract)
            return;

        CheckDead();

        Debug.Log("Update State: " + stateMachine.GetState());
        base.Update();
        stateMachine.UpdateState();

        if (bIsTurnEnable)
        {
            RotateCamera();     // 마우스를 위아래(Y) 움직임에 따라 카메라 X 축 회전 
            RotateCharacter();  // 마우스 좌우(X) 움직임에 따라 캐릭터 Y 축 회전
        }
        KeepAir();

        UpdateHUD();        // HUD 업데이트

        // UpdateBarrier 추가
        if (barrier.activeSelf == true) UpdateBarrier();

        // 디버그용
        TestHP();

        // 쿨타임 감소
        CoolTimeManager();
    }

    //============================================================================
    void CoolTimeManager()
    {
        if(fireTime < MLautoFireRate)
        {
            fireTime += Time.deltaTime;
        }

        if(!bEndDead)
            return;

        if (VCoolTime > 0)
            VCoolTime = (VCoolTime - Time.deltaTime) <= 0 ? 0 : (VCoolTime - Time.deltaTime);

        if (VTime > 0)
        {
            VTime = (VTime - Time.deltaTime) <= 0 ? 0 : (VTime - Time.deltaTime);
        }

        if (VTime <= 0 && PhotonNetwork.NetworkClientState == ClientState.Joined)
        {
            //+상태이상 면역 해제
            bEndMeleeAttack = true;
        }

        //Eskill
        if (ESkillTime <= 0 && PhotonNetwork.NetworkClientState == ClientState.Joined)    //상태이상이면 끌어당김 멈춤
        {
            bEndESkill = true;
        }

        if (ESkillCoolTime > 0)
            ESkillCoolTime = (ESkillCoolTime - Time.deltaTime) <= 0 ? 0 : (ESkillCoolTime - Time.deltaTime);

        if (ESkillTime > 0)
            ESkillTime = (ESkillTime - Time.deltaTime) <= 0 ? 0 : (ESkillTime - Time.deltaTime);

        //FSkill
        if (barrierCoolTime > 0)
            barrierCoolTime = (barrierCoolTime - Time.deltaTime) <= 0 ? 0 : (barrierCoolTime - Time.deltaTime);

        if (barrierTime > 0)
        {
            barrierTime = (barrierTime - Time.deltaTime) <= 0 ? 0 : (barrierTime - Time.deltaTime);
            bEndFSkill = true;
        }

        if ((barrierTime <= 0 || barrierHp <= 0) && PhotonNetwork.NetworkClientState == ClientState.Joined)
        {
            //+상태이상 면역 해제
            pv.RPC("RPC_Barrier_SetActive", RpcTarget.AllBuffered, false);
        }

        //ShiftSkill
        if (DashCoolTime > 0)
            DashCoolTime = (DashCoolTime - Time.deltaTime) <= 0 ? 0 : (DashCoolTime - Time.deltaTime);

        if (dashTime > 0)
        {
            dashTime = (dashTime - Time.deltaTime) <= 0 ? 0 : (dashTime - Time.deltaTime);
        }

        if (dashTime <= 0 && PhotonNetwork.NetworkClientState == ClientState.Joined)
        {
            bEndShiftSkill = true;
            //caseyGravity = defaultGravity;
        }

        //QSkill
        if (HUDHitGauge < hitGauge)
            HUDHitGauge = (HUDHitGauge + (hitGauge / 2)*Time.deltaTime) >= maxhitGauge ? maxhitGauge : (HUDHitGauge + (hitGauge / 2) * Time.deltaTime);

        if (CoolTimeUltGauge < ultGauge)
            CoolTimeUltGauge = (CoolTimeUltGauge + (ultGauge/2) * Time.deltaTime) >= (float)ultGauge ? (float)ultGauge : (CoolTimeUltGauge + (ultGauge / 2) * Time.deltaTime);

        if (QSkillCoolTime > 0)
            QSkillCoolTime = (QSkillCoolTime - Time.deltaTime) <= 0 ? 0 : (QSkillCoolTime - Time.deltaTime);

        if (QSkillTime > 0)
        {
            QSkillTime = (QSkillTime - Time.deltaTime) <= 0 ? 0 : (QSkillTime - Time.deltaTime);
        }

        if (QSkillTime <= 0)
        {
            bEndQSkill = true;
            //Debug.Log("조작 가능 상태");
        }

        if (QFireRate > 0)
        {
            QFireRate = (QFireRate - Time.deltaTime) <= 0 ? 0 : (QFireRate - Time.deltaTime);
        }
    }

    public override void IncreaseHP(int value)
    {
        hp += value;
        if (hp < MAX_HP)
        {
            if (totalHP < hp + shield) totalHP = hp + shield;
            return;
        }

        // 회복량이 최대 체력을 넘어섰을 경우
        hp = MAX_HP;
        if (totalHP < hp + shield) totalHP = hp + shield;
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
    public void RPC_IncreaseShield(int point)
    {
        shield = (shield + point < MAX_SHIELD) ? shield + point : MAX_SHIELD;
        if (totalHP < hp + shield) totalHP = hp + shield;
    }

    public override void TakeDamage(int damage)
    {
        // 베리어->실드->체력순으로 줄어 들어야 한다.
        //베리어 감소
        barrierHp -= damage;
        if (barrierHp > 0) return;

        // 실드 감소
        shield += barrierHp;
        barrierHp = 0;
        if((hp + shield > 200) && (totalHP < hp + shield))
            totalHP = hp + shield;
        if(shield > 0) return;

        // 체력 감소
        if(totalHP > MAX_HP)    totalHP = MAX_HP;
        hp += shield;
        shield = 0;
        if(hp > 0) return;
            
        hp = 0;
    }

    public void TakeDamageContinuous(float value)
    {
        float floatBarrierHp = barrierHp;
        floatBarrierHp = ((float)barrierHp - value*Time.deltaTime) > 0 ? (float)barrierHp - value* Time.deltaTime : 0;
        barrierHp = Mathf.RoundToInt( floatBarrierHp);

        // 실드가 먼저 까이고 체력이 까여야한다.
        float floatshield = shield;
        floatshield -= value*Time.deltaTime;
        shield = Mathf.RoundToInt(floatshield);

        if (shield > 0) return;

        // 실드가 전부 까인 경우
        hp += shield;
        shield = 0;
        if (hp > 0) return;

        // hp도 전부 까인 경우 사망 처리를 해준다.
        CheckDead();
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

    public override void Idle()
    {
        if (
            stateMachine.GetState() == CaseyState.Move ||
            (
            bEndMainAttack && bEndSubAttack && bEndESkill && bEndQSkill && bEndShiftSkill &&
            bEndMeleeAttack && bEndReload && bEndDead && bEndDragged))
        {
            if (!bIsGrounded) stateMachine.SetState(CaseyState.Jump);
            else stateMachine.SetState(CaseyState.Idle);
        }
    }

    public override void Move() // 키보드 입력에 따라 이동
    {
        if (
            stateMachine.GetState() == CaseyState.Idle ||
            (
            bEndMainAttack && bEndSubAttack && bEndESkill && bEndQSkill && bEndShiftSkill &&
            bEndMeleeAttack && bEndReload && bEndDead && bEndDragged))
        {
            if (!bIsGrounded) stateMachine.SetState(CaseyState.Jump);
            else stateMachine.SetState(CaseyState.Move);
        }
    }

    public override void Jump() // 땅점프 및 공중점프
    {
        if (
            stateMachine.GetState() == CaseyState.Idle ||
            stateMachine.GetState() == CaseyState.Move ||
            (
            bEndMainAttack && bEndSubAttack && bEndESkill && bEndQSkill && bEndShiftSkill &&
            bEndMeleeAttack && bEndReload && bEndDead && bEndDragged))
        {
            stateMachine.SetState(CaseyState.Jump);
        }
    }

    public override void Attack()
    {
        if (
             stateMachine.GetState() == CaseyState.Idle ||
             stateMachine.GetState() == CaseyState.Move ||
             stateMachine.GetState() == CaseyState.Attack ||
             (
             bEndMainAttack && bEndSubAttack && bEndESkill && bEndQSkill && bEndShiftSkill &&
             bEndMeleeAttack && bEndReload && bEndDead && bEndDragged))
        {

            // 총알이 없을때 발사를 하려고 하면 자동으로 재장전 상태로 전환한다.
            if (remainBullet < 1)
            {
                bEndReload = false;
                stateMachine.SetState(CaseyState.Reload);
                return;
            }

            // 중복 상태 설정이 아니라면 공격 상태로 전환한다.
            if (stateMachine.GetState() != CaseyState.Attack)
            {
                Debug.Log("Attack!");
                stateMachine.SetState(CaseyState.Attack);
            }
        }
    }

    public override void RAttack()
    {
        // 우클릭 공격이 끝난후에도 계속 우클릭을 누르고 있어서 우클릭 상태가 끝나지 않는 경우
        if (
            stateMachine.GetState() == CaseyState.RAttack &&
            bEndSubAttack)
        {
            // 일단 상태를 Idle로 전환한다.
            stateMachine.SetState(CaseyState.Idle);
        }

        if (
             stateMachine.GetState() == CaseyState.Idle ||
             stateMachine.GetState() == CaseyState.Move ||
             stateMachine.GetState() == CaseyState.RAttack ||
             (
             bEndMainAttack && bEndSubAttack && bEndESkill && bEndQSkill && bEndShiftSkill &&
             bEndMeleeAttack && bEndReload && bEndDead && bEndDragged))
        {
            if (remainBullet < 1)
            {
                bEndReload = false;
                stateMachine.SetState(CaseyState.Reload);
                return;
            }

            if (stateMachine.GetState() != CaseyState.RAttack)
                stateMachine.SetState(CaseyState.RAttack);
        }
    }

    public override void MeleeAttack()
    {
        if (
              stateMachine.GetState() == CaseyState.Idle ||
              stateMachine.GetState() == CaseyState.Move ||
              (bEndESkill && bEndQSkill && bEndShiftSkill && bEndMeleeAttack && bEndReload && bEndDead && bEndDragged))
        {
            if (VCoolTime > 0) return;
            stateMachine.SetState(CaseyState.MeleeAttack);
        }
    }

    public override void ESkill()//+상태이상 아니고 다른스킬을 사용하고 있지 않다면(주, 보조공격 캔슬 가능)
    {
        if (
              stateMachine.GetState() == CaseyState.Idle ||
              stateMachine.GetState() == CaseyState.Move ||
              (bEndESkill && bEndQSkill && bEndShiftSkill && bEndMeleeAttack && bEndReload && bEndDead && bEndDragged))
        {
            if (ESkillCoolTime > 0 || ESkillTime > 0) return;
            stateMachine.SetState(CaseyState.ESkill);
        }
    }

    public override void FSkill()
    {
        // 배리어는 기절등의 상태이상이 아닌 이상 언제든지 사용할 수 있다.
        if (!bEndDead)
            return;

        if (barrierCoolTime > 0 || barrierTime > 0) return;//+상태이상아니라면
        StartBarrier();
    }

    public override void ShiftSkill()
    {
        if (
            stateMachine.GetState() == CaseyState.Idle ||
            stateMachine.GetState() == CaseyState.Move ||
            (bEndESkill && bEndQSkill && bEndShiftSkill && bEndMeleeAttack && bEndReload && bEndDead && bEndDragged))
        {
            if (DashCoolTime > 0 || dashTime > 0) return;
            stateMachine.SetState(CaseyState.ShiftSkill);
        }
    }

    // >> 오디오 함수 추가 
    /*
    private void OnCollisionEnter(Collision collision)
    {
        if (bDashAudioPlay)
        {
            if (collision.gameObject.transform.root.GetComponent<Casey>() != null)
            {
                if (!collision.gameObject.GetComponent<AudioSource>())
                {
                    collision.gameObject.AddComponent<AudioSource>().clip = audioManager.ModelClip[4];
                    collision.gameObject.GetComponent<AudioSource>().Play();
                }
                else
                {
                    collision.gameObject.GetComponent<AudioSource>().clip = audioManager.ModelClip[4];
                    collision.gameObject.GetComponent<AudioSource>().Play();
                }
                audioManager.PlayModelSound(4); // 케이시 자신에게 소리
            }
            else if (collision.gameObject.transform.root.GetComponent<Rora>() != null)
            {
                if (!collision.gameObject.GetComponent<AudioSource>())
                {
                    collision.gameObject.AddComponent<AudioSource>().clip = audioManager.ModelClip[4];
                    collision.gameObject.GetComponent<AudioSource>().Play();
                }
                else
                {
                    collision.gameObject.GetComponent<AudioSource>().clip = audioManager.ModelClip[4];
                    collision.gameObject.GetComponent<AudioSource>().Play();
                }
                audioManager.PlayModelSound(4); // 케이시 자신에게 소리
            }
        }
    }
    // >>*/

    public override void QSkill()
    {
        if (
             stateMachine.GetState() == CaseyState.Idle ||
             stateMachine.GetState() == CaseyState.Move ||
             (bEndESkill && bEndQSkill && bEndShiftSkill && bEndMeleeAttack && bEndReload && bEndDead && bEndDragged))
        {
            if (QSkillCoolTime > 0) return;
            stateMachine.SetState(CaseyState.QSkill);
        }
    }

    public override void Reload()
    {
        if (
            remainBullet < maxMagazine && (
            stateMachine.GetState() == CaseyState.Idle ||
            stateMachine.GetState() == CaseyState.Move ||
            stateMachine.GetState() == CaseyState.Attack ||
            stateMachine.GetState() == CaseyState.RAttack ||
            (bEndESkill && bEndQSkill && bEndShiftSkill && bEndMeleeAttack && bEndDead && bEndDragged)))
        {
            stateMachine.SetState(CaseyState.Reload);
        }
    }

    //=====================================================================================

    void ApplyGravity() //중력 적용
    {
        //Debug.Log("UsingGravity");
        if (!bEndShiftSkill || !bEndQSkill) return;
        if (bIsGrounded && yVelocity < -0.1f) yVelocity = -0.1f;
        else yVelocity += (caseyGravity * Time.deltaTime);
        animTP.SetFloat("Jump_Y", yVelocity);
        characterController.Move(new Vector3(0f, yVelocity, 0f));
    }

    void CheckOnGround() //땅인지 체크
    {
        //Debug.Log("bIsGrounded: " + bIsGrounded);

        // 땅위에 있는지 판단한다.
        List<RaycastHit> hitInfos;
        Vector3 center = transform.position + new Vector3(0f, characterController.height / 2f, 0f);
        hitInfos = Physics.SphereCastAll(
            center, characterController.radius * 1.5f, Vector3.down, characterController.height / 15.0f).ToList();

        // 캐릭터의 콜라이더와 부딪친 경우는 제외한다.
        hitInfos.RemoveAll(hit => (hit.transform.root.GetComponent<Playable>() != null));

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
                }
                airJumpCount = 0;
                jumpInterval = 0;
                bIsGrounded = true;
                caseyGravity = defaultGravity;
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
            jumpInterval = 0;
            bIsGrounded = true;
            caseyGravity = defaultGravity;
        }
    }

    void CheckCollideWithCeil()
    {
        // 천장과 부딪쳤는지 구한다.
        List<RaycastHit> hitInfos;
        Vector3 center = transform.position + new Vector3(0f, characterController.height / 2f, 0f);
        hitInfos = Physics.SphereCastAll(
            center, characterController.radius * 1.5f, Vector3.up, characterController.height / 1.4f).ToList();

        // 캐릭터의 콜라이더와 부딪친 경우는 제외한다.
        hitInfos.RemoveAll(hit => (hit.transform.root.GetComponent<Playable>() != null));

        // 총알과 부딪친 경우도 제외한다.
        hitInfos.RemoveAll(hit => (hit.transform.root.gameObject.layer == LayerMask.NameToLayer("Projectiles")));

        // 충돌한게 없다면 끝낸다.
        if (hitInfos.Count == 0)    return;

        // 천장과 일정시간 이상 부딪쳤다면 아래 방향으로 떨어지게 중력을 갱신한다.
        if (ceilCheckTimer > 0.0f)
        {
            ceilCheckTimer -= Time.fixedDeltaTime;
        }
        else if(yVelocity > 0)
        {
            ceilCheckTimer = CEIL_CHECK_TIME;
            yVelocity = 0f;
        }
    }

    public void KeepAir() //공중유지
    {
        //if (bIsGrounded) return;
        if (Input.GetKey(KeyManager.Inst.Jump) && yVelocity < 0)//스페이스 누르고 있고 하강중
        {
            spaceTime += Time.deltaTime;
            if (spaceTime > limitSpaceTime)//0.25초 동안 스페이스 입력 확인
            {
                caseyGravity = keepAirGravity;//공중유지 중력
                audioManager.FootstepSrc.clip = audioManager.FootstepClip[4]; // 중력제어 (공중 유지) 오디오
            }
        }
        else
        {
            spaceTime = 0;
            caseyGravity = defaultGravity;//원래 중력으로
        }
        if (Input.GetKeyUp(KeyManager.Inst.Jump))
        {
            spaceTime = 0;
            caseyGravity = defaultGravity;//원래 중력으로
        }
    }

    void UpdateHUD() // HUD 업데이트
    {
        hpBar.UpdateHP(hp, shield, totalHP);
        caseyMagazine.UpdateRemain(remainBullet.ToString());
    }

    void TestHP()
    {
        if (Input.GetKeyDown(KeyCode.O)) TakeDamage(10);
        if (Input.GetKeyDown(KeyCode.P)) IncreaseHP(10);
    }

    void ReduceMaggazine(int bulletnum)//총알 감소, 히트 게이지 증가
    {
        //remainBullet = (remainBullet - bulletnum <= 0) ? 0 : remainBullet - bulletnum;
        //if (remainBullet == 0)
        //    Reload();

        remainBullet = (remainBullet - bulletnum <= 0) ? 0 : remainBullet - bulletnum;
        hitGauge = ((hitGauge + bulletnum) >= maxhitGauge) ? maxhitGauge : hitGauge + bulletnum;
    }

    public void UpdateUltgauge(int gauge)
    {
        ultGauge = ((ultGauge + gauge) >= maxultGauge) ? maxultGauge : ultGauge + gauge;
    }

    public void UpdatePerUltgauge(float per)//총알 감소, 히트 게이지 증가
    {
        int gauge = (int)(maxultGauge * per / 100);
        ultGauge = ((ultGauge + gauge) >= maxultGauge) ? maxultGauge : ultGauge + gauge;
    }

    public void TryJump()
    {
        if (Input.GetKeyDown(KeyManager.Inst.Jump)) //스페이스키 누르면
        {
            if (maxAirJumpCount > airJumpCount) //점프 가능
            {
                if (bIsGrounded)
                {
                    animTP.SetTrigger("Jump");
                    photonAnimSync |= PhotonAnim.JUMP;
                    yVelocity = groundJumpPower;  //땅 점프 파워
                    audioManager.PlayFootstepSound(2); // <<땅 점프 오디오
                    bIsGrounded = false;
                }
                else
                {
                    yVelocity = airJumpPower; //공중점프 파워
                    audioManager.PlayFootstepSound(3); // << 공중 점프 오디오
                    ++airJumpCount;
                }
            }
        }
    }

    public void TryMove(float multiplier = 1f)
    {
        //키보드 입력 값
        float h = KeyManager.Inst.GetAxisRawHorizontal() * multiplier;
        float v = KeyManager.Inst.GetAxisRawVertical() * multiplier;

        // 움직일 벡터값을 설정하고 캐릭터를 이동시킨다.
        direction = new Vector3(h, 0, v);
        direction.Normalize();    // 대각선 이동이 더 빠른걸 막기 위해 정규화한다.
        direction *= (runSpeed * Time.deltaTime * slowValue);
        direction = transform.TransformDirection(direction);    // 월드 좌표 기준으로 방향 변환
        characterController.Move(direction);

        // 하체 애니메이션을 결정한다.
        if (bIsGrounded)     // 공중에 있는지 검사
        {
            // 땅위에 있다면 이동 여부에 따라 Idle 또는 Move애니메이션을 출력한다.
            if (Mathf.Abs(h) <= 0.01f && Mathf.Abs(v) <= 0.01f)
            {
                if (!Utilities.IsAnimationPlaying(animTP, "Idle", 0))
                {
                    //Debug.Log("TryIdle");
                    animTP.SetTrigger("Lower_Idle");
                    photonAnimSync |= PhotonAnim.LOWER_IDLE;
                }
            }
            else
            {
                animTP.SetFloat("MoveV", v);
                animTP.SetFloat("MoveH", h);
                if (!Utilities.IsAnimationPlaying(animTP, "Move", 0))
                {
                    //Debug.Log("TryMove");
                    animTP.SetTrigger("Move");
                    photonAnimSync |= PhotonAnim.MOVE;
                }
            }
        }
    }

    [PunRPC]
    public void RPC_SetIK(bool bActive)
    {
        GenericIK.SetActive(bActive);
    }

    [PunRPC]
    void RPC_MRcharging_SetActive(bool set)
    {
        MRcharging.SetActive(set);
    }

    [PunRPC]
    void RPC_VHitBox_SetActive(bool set)
    {
        VAttackHitBox.SetActive(set);
        VAttackHitBox.GetComponent<TriggerHitEffect>().damage = Vdamage;
    }

    [PunRPC]
    void RPC_Cone_SetActive(bool set)
    {
        ConeSpace.SetActive(set);
    }

    [PunRPC]
    void RPC_Dash_SetActive(bool set)
    {
        DashHitBox.SetActive(set);
        DashEngine.SetActive(set);
        DashEffect.SetActive(set);
    }

    public void UpdateLAttack()
    {
        TryJump();
        TryMove();

        if (Input.GetKeyDown(KeyManager.Inst.Attack))
        {
            //+상태이상이 아니라면, 다른스킬사용중 아니라면
            if (fireTime < MLautoFireRate) return;
            animTP.SetTrigger("Attack");
            photonAnimSync |= PhotonAnim.ATTACK;
            GameObject bullet = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "MLBullet"),
                FP_Muzzle.transform.position, transform.rotation);
            audioManager.PlayWeaponSound(0);
            ReduceMaggazine(MLattackAmmo);
            return;
        }

        if (Input.GetKey(KeyManager.Inst.Attack))
        {
            if (fireTime < MLautoFireRate) return;
            GameObject bullet = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/CaseyInstance", "MLBullet"),
                FP_Muzzle.transform.position, transform.rotation);
            audioManager.PlayWeaponSound(0);
            ReduceMaggazine(MLattackAmmo);
            fireTime = 0;
        }

        //head_coef
        if (Input.GetKeyUp(KeyManager.Inst.Attack))
        {
            Debug.Log("End LAttack!");
            bEndMainAttack = true;
        }
    }

    public void UpdateRAttack()
    {
        TryJump();
        TryMove();

        // 1인칭 발사 애니메이션이 나오고 있다면 아래 코드를 무시한다.
        if (animFP.GetCurrentAnimatorStateInfo(0).IsName("CaseyFP_RAttack"))
        {
            // 발사 애니메이션이 끝났다면 다른 애니메이션으로 전환한다.
            if (animFP.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.98f)
                animFP.SetTrigger("Idle");
        }

        if (Input.GetKeyDown(KeyManager.Inst.RAttack))
        {
            //+상태이상 아니라면, 다른 스킬 사용중 아니라면
            animTP.SetTrigger("RAttack");
            photonAnimSync |= PhotonAnim.ATTACK;
            
            pv.RPC("RPC_MRcharging_SetActive", RpcTarget.AllBuffered, true);
            audioManager.PlayObjectSound(MRcharging, 2); // 차지 소리
        }

        if (Input.GetKey(KeyManager.Inst.RAttack))
        {
            MRpushTime += Time.deltaTime;

            if (MRpushTime < minMRpushTime)//차징 이펙트 출력
            {
                MRchargingSize += MaxMRchargingSize / minMRpushTime * Time.deltaTime;
                MRcharging.transform.localScale = new Vector3(MRchargingSize, MRchargingSize, MRchargingSize);
            }
            else //(MRPushTime >= MaxMRPushTime)자동발사
            {
                MRResetCharging();
                GameObject instance = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/CaseyInstance", "MRBullet"),
                    FP_Muzzle.transform.position, transform.rotation);
                ReduceMaggazine(MRattackAmmo);
                audioManager.PlayWeaponSound(1);
                pv.RPC("RPC_MRcharging_SetActive", RpcTarget.AllBuffered, true);
                animFP.SetTrigger("RAttack");
               
            }
        }

        if (Input.GetKeyUp(KeyManager.Inst.RAttack))
        {
            if (MRpushTime >= maxMRpushTime)
            {
                GameObject instance = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs/CaseyInstance", "MRBullet"),
                    FP_Muzzle.transform.position, transform.rotation);
                audioManager.PlayObjectSound(instance, 3); // MRbullet 날아가는 소리
                ReduceMaggazine(MRattackAmmo);
            }
            MRResetCharging();
            bEndSubAttack = true;
        }
    }

    void MRResetCharging()
    {
        MRpushTime = 0;
        MRchargingSize = 0;
        MRcharging.transform.localScale = new Vector3(MRchargingSize, MRchargingSize, MRchargingSize);
        pv.RPC("RPC_MRcharging_SetActive", RpcTarget.AllBuffered, false);
    }
    
    public void StartVattack()
    {
        //if (Input.GetKeyDown(KeyManager.Inst.MeleeAttack))
        //{

        //}
        if (HUDHitGauge == maxhitGauge)
        {
            hitGauge = 0;
            HUDHitGauge = 0;
            Vdamage = (int)(VdefaultDamage * V_coef);
        }
        else
        {
            Vdamage = (int)VdefaultDamage;
        }
        VTime = defaultVTime;
        VCoolTime = defaultVCoolTime;

        pv.RPC("RPC_SetIK", RpcTarget.AllBuffered, false);

        animFP.SetTrigger("MeleeAttack");
        animTP.SetTrigger("VAttack");
        photonAnimSync |= PhotonAnim.VATTACK;
        pv.RPC("RPC_VHitBox_SetActive", RpcTarget.AllBuffered, true);
        VAttackHitBox.GetComponent<TriggerHitEffect>().damageable = true;
    }

    public void StartBarrier()
    {
        pv.RPC("RPC_Barrier_SetActive", RpcTarget.AllBuffered, true);
        barrierCoolTime = defaultBarrierCoolTime;
        barrierTime = maxBarrierTime;
    }

    public void UpdateBarrier()
    {
        if (barrierHp / 100 > 0.3)
            audioManager.PlayObjectSound(0, 0);
        else
            audioManager.PlayObjectSound(0, 1);
    }

    [PunRPC]
    void RPC_Barrier_SetActive(bool set)
    {
        barrier.SetActive(set);
        if (set)
        {
            barrierHp = defaultbarrierHp;
            runSpeed -= runSpeed * ((float)reduceSpeed / 100.0f);
           audioManager.PlayModelSound(4);
        }
        else
        {
            barrierHp = 0;
            runSpeed = defaultrunSpeed;
        }
    }

    public void StartESkill()//+피격 대상 기절
    {
        if (Input.GetKeyDown(KeyManager.Inst.ESkill))//확인하고 지우기
        {
            ESkillCoolTime = defaultESkillCoolTime;
            ESkillTime = defaultESkillTime;
            animFP.SetTrigger("ESkill");
            animTP.SetTrigger("ESkill");
            photonAnimSync |= PhotonAnim.ESKILL;
            Debug.Log("ESkill");
            Invoke("ActiveConeSpace", Edelay);
        }
    }

    void ActiveConeSpace()
    {
        pv.RPC("RPC_Cone_SetActive", RpcTarget.AllBuffered, true);
        ConeSpace.transform.localScale = new Vector3(coneRadius, coneRadius, coneDistance);
        ConeSpace.GetComponentInChildren<CaseyESkillCollision>().damageable = true;
    }

    public void StartUlt()
    {
        if (Input.GetKeyDown(KeyManager.Inst.QSkill))
        {
            if (ultGauge != maxultGauge) return;
            ultGauge = 0;
            CoolTimeUltGauge = 0;

            pv.RPC("RPC_SetIK", RpcTarget.AllBuffered, false);
            QFireRate = defaultQFireRate;
            QSkillTime = defaultQSkillTime;
            QMissileNum = dsfaultQMissileNum;
            QSkillCoolTime = defaultQSkillCoolTime;

            animFP.SetTrigger("QSkill");
            animTP.SetTrigger("QSkill");
            photonAnimSync |= PhotonAnim.QSKILL;
        }
    }

    public void UpdateQSkill()
    {
        if (QMissileNum > 0)
        {
            if (QFireRate <= 0)
            {
                if (QMissileNum % 2 == 1)
                    PhotonNetwork.Instantiate("PhotonPrefabs/CaseyInstance/QRocket", 
                        QMuzzle1.transform.position, transform.rotation);
                else
                    PhotonNetwork.Instantiate("PhotonPrefabs/CaseyInstance/QRocket", 
                        QMuzzle2.transform.position, transform.rotation);

                --QMissileNum;
                QFireRate = defaultQFireRate;
            }
        }
    }

    //스킬사용 불가능 상태 : 상태이상, 근접공격, 끌어당기기, 궁극기, 재장전
    //+콜라이더
    public void StartDash()
    {
        if (Input.GetKeyDown(KeyManager.Inst.ShiftSkill))
        {
            dashTime = defaultDashTime;
            DashCoolTime = defaultDashCoolTime;
            caseyGravity = 0;
            pv.RPC("RPC_Dash_SetActive", RpcTarget.AllBuffered, true);
            DashHitBox.GetComponent<TriggerHitEffect>().damageable = true;

            pv.RPC("RPC_SetIK", RpcTarget.AllBuffered, false);

            animFP.SetTrigger("Idle");
            animTP.SetTrigger("ShiftSkill");
            photonAnimSync |= PhotonAnim.SHIFTSKILL;
            bDashAudioPlay = true;
        }
    }

    public void UpdateDash()
    {
        direction = transform.GetChild(3).GetChild(1).forward;
        direction *= (dashSpeed * Time.deltaTime);
        characterController.Move(direction);
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

            if ((photonAnimSync & PhotonAnim.IDLE) != 0) animTP.SetTrigger("Idle");
            if ((photonAnimSync & PhotonAnim.LOWER_IDLE) != 0) animTP.SetTrigger("Lower_Idle");
            if ((photonAnimSync & PhotonAnim.MOVE) != 0) animTP.SetTrigger("Move");
            if ((photonAnimSync & PhotonAnim.JUMP) != 0) animTP.SetTrigger("Jump");
            if ((photonAnimSync & PhotonAnim.FALL) != 0) animTP.SetTrigger("Fall");
            if ((photonAnimSync & PhotonAnim.RELOAD) != 0) animTP.SetTrigger("Reload");
            if ((photonAnimSync & PhotonAnim.ATTACK) != 0) animTP.SetTrigger("Attack");
            if ((photonAnimSync & PhotonAnim.RATTACK) != 0) animTP.SetTrigger("RAttack");
            if ((photonAnimSync & PhotonAnim.VATTACK) != 0) animTP.SetTrigger("VAttack");
            if ((photonAnimSync & PhotonAnim.FSKILL) != 0) animTP.SetTrigger("FSkill");
            if ((photonAnimSync & PhotonAnim.ESKILL) != 0) animTP.SetTrigger("ESkill");
            if ((photonAnimSync & PhotonAnim.SHIFTSKILL) != 0) animTP.SetTrigger("ShiftSkill");
            if ((photonAnimSync & PhotonAnim.QSKILL) != 0) animTP.SetTrigger("QSkill");

            if (photonAnimSync > 0) photonAnimSync = 0;
        }
    }

    public override void CheckDead()
    {
        if(hp <= 0)
            stateMachine.SetState(CaseyState.Die);
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
        if (stateMachine == null) return false;
        return (stateMachine.GetState() == CaseyState.Die);
    }

    public override bool IsMine()
    {
        return pv.IsMine;
    }

    public override void ShowDeadUI()
    {
        if(!pv.IsMine)  return;
        HUD.transform.GetChild(1).gameObject.SetActive(true);
    }

    public override void ShowResultUI(bool bIsVictory)
    {
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

    //=========== Slow ================//
    bool bIsStopped = false;
    [Header("슬로우 시간")]
    public float CaseySlowTime = 3f;
    [HideInInspector]
    public float CurTimeValue;

    public void StartSlow(float slowValue)
    {
        if (!bIsStopped)
        {
            Debug.Log("슬로우 상태");
            bIsStopped = true;
            Time.timeScale = slowValue;
            StartCoroutine(Slowing());
        }
    }

    IEnumerator Slowing()
    {
        yield return new WaitForSecondsRealtime(CaseySlowTime);
        Time.timeScale = 1;
        bIsStopped = false;
    }

    public override void Dragged(GameObject dragger)
    {
        pv.RPC("RPC_Dragged", RpcTarget.AllBuffered, dragger.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    private void RPC_Dragged(int viewID)
    {
        dragger = PhotonNetwork.GetPhotonView(viewID).gameObject;
        stateMachine.SetState(CaseyState.Dragged);
    }
}

public class CaseyStateMachine
{
    // 멤버 변수
    PlayableState state;
    Casey casey;

    // 생성자
    public CaseyStateMachine(Casey _casey)
    {
        // 생성될때 기본 상태는 Idle
        casey = _casey;
        state = new CaseyIdle(_casey);
        state.Enter();
    }

    // 멤버 함수
    public void SetState(CaseyState newState)
    {
        // 중복으로 상태를 설정하는지 검사
        if (GetState() == newState)
            return;

        // 이전 상태의 Exit() 함수를 호출한다.
        state.Exit();

        // newState에 따라 상태를 설정한다.
        switch (newState)
        {
            case CaseyState.Idle: state = new CaseyIdle(casey); break;
            case CaseyState.Move: state = new CaseyMove(casey); break;
            case CaseyState.Jump: state = new CaseyJump(casey); break;
            case CaseyState.Reload: state = new CaseyReload(casey); break;
            case CaseyState.Attack: state = new CaseyAttack(casey); break;
            case CaseyState.RAttack: state = new CaseyRAttack(casey); break;
            case CaseyState.MeleeAttack: state = new CaseyMeleeAttack(casey); break;
            case CaseyState.ESkill: state = new CaseyESkill(casey); break;
            case CaseyState.ShiftSkill: state = new CaseyShiftSkill(casey); break;
            case CaseyState.QSkill: state = new CaseyQSkill(casey); break;
            case CaseyState.Die: state = new CaseyDead(casey); break;
            case CaseyState.Dragged: state = new CaseyDragged(casey); break;
        }

        state.Enter();
    }

    public void UpdateState()
    {
        state.Update();
    }

    public CaseyState GetState()
    {
        CaseyState curState = CaseyState.Error;

        string typeStr = null;
        typeStr = state.Type;
        switch (typeStr)
        {
            case "CaseyIdle": curState = CaseyState.Idle; break;
            case "CaseyMove": curState = CaseyState.Move; break;
            case "CaseyJump": curState = CaseyState.Jump; break;
            case "CaseyReload": curState = CaseyState.Reload; break;
            case "CaseyAttack": curState = CaseyState.Attack; break;
            case "CaseyRAttack": curState = CaseyState.RAttack; break;
            case "CaseyMeleeAttack": curState = CaseyState.MeleeAttack; break;
            case "CaseyESkill": curState = CaseyState.ESkill; break;
            case "CaseyShiftSkill": curState = CaseyState.ShiftSkill; break;
            case "CaseyQSkill": curState = CaseyState.QSkill; break;
            case "CaseyDead": curState = CaseyState.Die; break;
            case "CaseyDragged": curState = CaseyState.Dragged; break;
        }

        return curState;
    }
}

public abstract class CaseyBase : PlayableState
{
    protected Casey casey;
    protected Animator animTP;
    protected Animator animFP;

    public CaseyBase(Casey _casey)
    { 
        casey = _casey; animTP = casey.animTP; animFP = casey.animFP; 
    }
}

public class CaseyIdle : CaseyBase
{
    public override string Type { get { return "CaseyIdle"; } }

    public CaseyIdle(Casey casey) : base(casey) { }

    public override void Enter()
    {
        Debug.Log("Enter Idle");
        animFP.SetTrigger("Idle");
        animTP.SetTrigger("Idle");
        casey.photonAnimSync |= Casey.PhotonAnim.IDLE;
    }

    public override void Exit()
    {
        //Debug.Log("Operate Idle");
    }

    public override void Update()
    {
        casey.TryMove();
    }
}

public class CaseyMove : CaseyBase
{
    public override string Type { get { return "CaseyMove"; } }

    public CaseyMove(Casey casey) : base(casey) { }
    int h;
    int v;

    public override void Enter()
    {
        //Debug.Log("Enter Move");
        if (!animFP.GetCurrentAnimatorStateInfo(0).IsName("CaseyFP_Idle"))
            animFP.SetTrigger("Idle");
        animTP.SetTrigger("Idle");
        animTP.SetTrigger("Move");
        casey.photonAnimSync |= Casey.PhotonAnim.IDLE;
        casey.photonAnimSync |= Casey.PhotonAnim.MOVE;
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        casey.TryJump();
        casey.TryMove();
    }
}

public class CaseyJump : CaseyBase
{
    public override string Type { get { return "CaseyJump"; } }

    public CaseyJump(Casey casey) : base(casey) { }

    public override void Enter()
    {
        Debug.Log("Enter Jump");

        // 땅위에서 스페이스바를 눌러 Jump상태에 진입했는지, 공중에 띄워져서 Jump상태에 진입했는지에 따라 다르게 진입지점을 설정한다.
        if (!casey.bIsGrounded)
        {
            animTP.SetTrigger("Fall");
            casey.photonAnimSync |= Casey.PhotonAnim.FALL;
        }
    }

    public override void Exit()
    {
        // empty
    }

    public override void Update()
    {
        casey.TryJump();
        casey.TryMove();
    }
}

public class CaseyReload : CaseyBase
{
    public override string Type { get { return "CaseyReload"; } }

    public CaseyReload(Casey casey) : base(casey) { }

    public override void Enter()
    {
        animFP.SetTrigger("Reload");
        animTP.SetTrigger("Reload");
        casey.photonAnimSync |= Casey.PhotonAnim.RELOAD;

        // 수동 재장전인 경우 0.1배 더 빠르게 재장전을 한다.
        if (casey.bEndReload)
        {
            casey.bEndReload = false;
            animFP.speed = casey.manualReloadSpeed;
            animTP.speed = casey.manualReloadSpeed;
        }
    }

    public override void Exit()
    {
        if (!casey.bEndReload) casey.bEndReload = true;
    }

    public override void Update()
    {
        casey.TryJump();
        casey.TryMove();

        // 재장전 애니메이션이 끝났다면 탄창을 꽉 채운다.
        if (
            animFP.GetCurrentAnimatorStateInfo(0).IsName("CaseyFP_Reload") &&
            animFP.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.98f)
        {
            casey.remainBullet = casey.maxMagazine;
            casey.bEndReload = true;

            animFP.speed = casey.autoReloadSpeed;
            animTP.speed = casey.autoReloadSpeed;
        }
    }
}

public class CaseyAttack : CaseyBase
{
    public override string Type { get { return "CaseyAttack"; } }

    public CaseyAttack(Casey casey) : base(casey) { }

    public override void Enter()
    {
        casey.bEndMainAttack = false;
        if(casey.fireTime >= casey.MLautoFireRate)
        {
            animFP.SetTrigger("Attack");
        }
    }

    public override void Exit()
    {
        if (!casey.bEndMainAttack) casey.bEndMainAttack = true;
    }

    public override void Update()
    {
        casey.UpdateLAttack();
    }
}

public class CaseyRAttack : CaseyBase
{
    public override string Type { get { return "CaseyRAttack"; } }

    public CaseyRAttack(Casey casey) : base(casey) { }

    public override void Enter()
    {
        casey.bEndSubAttack = false;
        animFP.SetTrigger("Idle");
    }

    public override void Exit()
    {
        if (!casey.bEndSubAttack) casey.bEndSubAttack = true;
    }

    public override void Update()
    {
        casey.UpdateRAttack();
    }
}

public class CaseyMeleeAttack : CaseyBase
{
    public override string Type { get { return "CaseyMeleeAttack"; } }

    public CaseyMeleeAttack(Casey casey) : base(casey) { }

    public override void Enter()
    {
        casey.bEndMeleeAttack = false;
        casey.StartVattack();
    }

    public override void Exit()
    {
        if (!casey.bEndMeleeAttack) casey.bEndMeleeAttack = true;
        casey.pv.RPC("RPC_SetIK", RpcTarget.AllBuffered, true);
        casey.pv.RPC("RPC_VHitBox_SetActive", RpcTarget.AllBuffered, false);
    }

    public override void Update()
    {
        casey.TryJump();
        casey.TryMove();

        //if (
        //    animFP.GetCurrentAnimatorStateInfo(0).IsName("CaseyFP_MeleeAttack") &&
        //    animFP.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.98f)
        //{
        //    casey.bEndMeleeAttack = true;
        //    casey.EndVAttack();
        //}
    }
}

public class CaseyESkill : CaseyBase
{
    public override string Type { get { return "CaseyESkill"; } }

    public CaseyESkill(Casey casey) : base(casey) { }

    public override void Enter()
    {
        casey.bEndESkill = false;
        casey.StartESkill();
    }

    public override void Exit()
    {
        if (!casey.bEndESkill) casey.bEndESkill = true;
        casey.pv.RPC("RPC_Cone_SetActive", RpcTarget.AllBuffered, false);
    }

    public override void Update()
    {
        if (
            animFP.GetCurrentAnimatorStateInfo(0).IsName("CaseyFP_ESkill") &&
            animFP.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.98f)
        {
            casey.bEndESkill = true;
        }
    }
}

public class CaseyShiftSkill : CaseyBase
{
    public override string Type { get { return "CaseyShiftSkill"; } }

    public CaseyShiftSkill(Casey casey) : base(casey) { }

    public override void Enter()
    {
        casey.bEndShiftSkill = false;
        casey.StartDash();
    }

    public override void Exit()
    {
        if (!casey.bEndShiftSkill) casey.bEndShiftSkill = true;
        casey.bDashAudioPlay = false;
        casey.pv.RPC("RPC_SetIK", RpcTarget.AllBuffered, true);
        casey.pv.RPC("RPC_Dash_SetActive", RpcTarget.AllBuffered, false);
    }

    public override void Update()
    {
        casey.UpdateDash();
    }
}

public class CaseyQSkill : CaseyBase
{
    public override string Type { get { return "CaseyQSkill"; } }

    public CaseyQSkill(Casey casey) : base(casey) { }

    public override void Enter()
    {
        casey.bEndQSkill = false;
        casey.StartUlt();
    }

    public override void Exit()
    {
        if (!casey.bEndQSkill) casey.bEndQSkill = true;
        casey.pv.RPC("RPC_SetIK", RpcTarget.AllBuffered, true);
    }

    public override void Update()
    {
        casey.UpdateQSkill();

        //if (
        //    animFP.GetCurrentAnimatorStateInfo(0).IsName("CaseyFP_QSkill") &&
        //    animFP.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.98f)
        //{
        //    casey.bEndQSkill = true;
        //}
    }
}

public class CaseyDead : CaseyBase
{
    public override string Type { get { return "CaseyDead"; } }

    public CaseyDead(Casey casey) : base(casey) { }

    HUDManager hudManager;

    public override void Enter()
    {
        casey.bEndDead = false;
        casey.bIsTurnEnable = false;
        casey.EnterDead();
        casey.audioManager.PlayVoiceSound(0); // 죽음 사운드 추가

        hudManager = casey.HUD.GetComponent<HUDManager>();
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
            casey.audioManager.PlayVoiceSound(1); // 부활 사운드 추가
            casey.SendRespawn();
        }

        if (!hudManager.GameFinishHUD.isActiveAndEnabled)
            return;

        timer = hudManager.GetGameFinishHUD_Timer();
        if (timer <= 0.1)
        {
            PhotonNetwork.Destroy(casey.gameObject);
        }
    }
}

public class CaseyDragged : CaseyBase
{
    public override string Type { get { return "CaseyDragged"; } }

    public CaseyDragged(Casey casey) : base(casey) { }

    const float LIFE_TIME = 0.3f;
    float t;
    Vector3 startPos;

    public override void Enter()
    {
        t = 0f;
        startPos = casey.transform.position;
        casey.bEndDragged = false;
        casey.bIsTurnEnable = false;

        animFP.SetTrigger("Idle");
        animTP.SetTrigger("Idle");
        casey.photonAnimSync |= Casey.PhotonAnim.IDLE;
    }

    public override void Exit()
    {
        if(!casey.bEndDragged) casey.bEndDragged = true;
        casey.bIsTurnEnable = true;
    }

    public override void Update()
    {
        if(t < LIFE_TIME)
        {
            t += Time.deltaTime;
            casey.characterController.Move(
                (casey.dragger.transform.position - startPos) * (Time.deltaTime / (LIFE_TIME + 0.1f)));
        }
        else
        {
            casey.bEndDragged = true;
        }
    }
}