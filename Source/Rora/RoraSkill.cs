using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.IO;

//======================================== RoraSkill ========================================
abstract class RoraSkill : MonoBehaviour
{
    // == 변수
    protected float damage;
    protected float head_coef;

    // == 쿨타임
    protected float coolTime = 0;
    protected float curCoolTime = 0;

    // == 탄창
    protected float bulletPerShot;

    // == 컴포넌트
    protected SkillControl SkillCtrSC;
    protected PlayerAudio AudioManager;

    // == 포톤 변수
    protected PhotonView pv;

    // ==함수
    public virtual void Init(SkillControl skillCtrSC_,
                                    float cooltime_,
                                    float damage_, float head_coef_)
    {
        damage = damage_;
        head_coef = head_coef_;
        coolTime = cooltime_;
        curCoolTime = coolTime;
        SkillCtrSC = skillCtrSC_;


        this.pv = GetComponent<PhotonView>();
        this.AudioManager = SkillCtrSC.audioManager;
    }

    public abstract void OnSkillStart();

    public abstract void OnSkillUpdate();

    public abstract void OnSkillEnd();

    protected void VacateCooltime() { curCoolTime = 0; }

    public bool FillCooltime()
    {
        if (curCoolTime < coolTime)
        {
            curCoolTime += (Time.deltaTime*0.5f);
            return false;
        }
        else { return true; }
    }

    protected virtual void Shoot() { }

    public float GetCurCooltime() { return curCoolTime; }

    public void IncreaseCurCooltime(float per)
    {
        int gauge = (int)(coolTime * per / 100);
        curCoolTime = ((curCoolTime + gauge) >= coolTime) ? coolTime : curCoolTime + gauge;
    }
}



//======================================== Attack ========================================

class Attack : RoraSkill  //주공격(레이저빔쏘기)
{
    // == 변수
    private Vector3 direction;
    private Quaternion rotation;

    //==컴포넌트 관련
    Camera mCamera;
    Transform skillPointFP;
    Transform skillPointTP; //camera

    // == 레이저 관련 변수
    private GameObject LaserPrefab;
    private GameObject InstanceFP;
    private GameObject InstanceTP;
    private Laser1 LaserSC;
    private float maxLength;

    // ==공격 상태 관련 변수
    private int attackState = 0;
    private bool bIsLAttackStarted = false;
    private bool bIsShot = false;


    // == 함수
    public Attack() { }

    private float defaultDamage;

    public void Init(SkillControl skillctr)
    {
        base.Init(skillctr, 0, skillctr.Attack_damage, skillctr.Attack_headCoef);
        LaserPrefab = skillctr.Lasers[0];
        mCamera = skillctr.mCamera;
        skillPointTP = skillctr.SkillPointTP;
        skillPointFP = skillctr.SkillPointFP;
        bulletPerShot = skillctr.Attack_bulletsPerShots;

        //==초기화
        defaultDamage = damage;
        attackState = 0;
        bIsLAttackStarted = false;
        bIsShot = false;
    }

    public override void OnSkillStart()
    {
        attackState = 1;
        VacateCooltime();

        damage = defaultDamage; //<< damage 초기화
    }

    public override void OnSkillUpdate()
    {
        Debug.Log("attack state: " + attackState);
        if (attackState == 1)
        {
            Destroy(InstanceFP);

            { // 1인칭 궤적
                InstanceFP = Instantiate(LaserPrefab, skillPointFP.position, skillPointFP.rotation);

                if (InstanceFP != null)
                {
                    InstanceFP.transform.parent = skillPointFP.transform;
                    LaserSC = InstanceFP.GetComponent<Laser1>();
                    LaserSC.camObj = mCamera.gameObject;
                    LaserSC.bIsFP = true;
                    maxLength = LaserSC.MaxLength;
                }
            }

            { // 3인칭 궤적
                InstanceTP = PhotonNetwork.Instantiate(
                    Path.Combine("PhotonPrefabs/RoraInstance", "LAttackLaser"), 
                    skillPointTP.transform.position,
                    skillPointTP.transform.rotation);
                InstanceTP.GetComponent<Laser1>().damage = damage;
                InstanceTP.GetComponent<Laser1>().head_coef = head_coef;
                // 자기가 플레이하는 캐릭터라면 3인칭 궤적이 안보이게 처리한다.
                if (pv.IsMine)
                {
                    InstanceTP.GetComponent<LineRenderer>().enabled = false;
                    InstanceTP.transform.GetChild(0).gameObject.SetActive(false);
                    InstanceTP.transform.GetChild(1).gameObject.SetActive(false);
                }
            }

            attackState = 2;
        }
        else if (attackState == 2) //Laser update
        {
            SkillCtrSC.ReduceMagazineContinuous(bulletPerShot);
        }
        else if (attackState == 0)
        {
            if (LaserSC) LaserSC.DisablePrepare();
            Destroy(InstanceFP);
            if(InstanceTP != null)
                PhotonNetwork.Destroy(InstanceTP);
        }
    }

    public override void OnSkillEnd()
    {
        attackState = 0;
    }
}




//======================================== RAttack ========================================

class RAttack : RoraSkill //보조공격
{
    //==산발탄
    public int pelletCount = 10;
    private float spreadRange;

    //==컴포넌트 관련
    private Camera mCamera;
    private Transform skillPointFP;
    private Transform skillPointTP;

    // == 투사체(레이저) 관련 변수
    private GameObject LaserPrefab;
    private Laser2 LaserSC;

    // ==공격 상태 관련 변수
    bool bIsRAttackStart = false;

    //== 함수

    public RAttack() { }

    public void Init(SkillControl skillCtrSC)
    {
        base.Init(skillCtrSC, skillCtrSC.RAttack_cooltime, skillCtrSC.RAttack_Damage, skillCtrSC.RAttack_HeadCoef);

        mCamera = SkillCtrSC.mCamera;
        LaserPrefab = SkillCtrSC.Lasers[1];

        skillPointTP = SkillCtrSC.SkillPointTP;
        skillPointFP = SkillCtrSC.SkillPointFP;

        spreadRange = SkillCtrSC.RAttack_spreadRange;
        pelletCount = SkillCtrSC.RAttack_bulletsPerShots;

        bIsRAttackStart = false;
    }

    public override void OnSkillStart()
    {
        if(!pv.IsMine)  return;

        bIsRAttackStart = true;
        VacateCooltime();

        ShootSpread();
        SkillCtrSC.ReduceMagazine(pelletCount);
    }

    public override void OnSkillUpdate()
    {
        if (!pv.IsMine) return;
        if (!bIsRAttackStart) FillCooltime();
    }

    public override void OnSkillEnd()
    {
        if (!pv.IsMine) return;
        bIsRAttackStart = false;
    }

    public void ShootSpread()
    {
        if (!pv.IsMine) return;

        for (int i = 0; i < pelletCount; i++)
        {
            // 각 총알의 랜덤 퍼짐 범위를 구한다
            float xSpread = Random.Range(-spreadRange, spreadRange);
            float ySpread = Random.Range(-spreadRange, spreadRange);


            { // 3인칭 생성
                GameObject InstanceTP = PhotonNetwork.Instantiate(
                    Path.Combine("PhotonPrefabs/RoraInstance", "RAttackLaser"), 
                    skillPointTP.transform.position,
                    skillPointTP.transform.rotation);
                pv.RPC("RPC_ShootTP", RpcTarget.AllBuffered, 
                    InstanceTP.GetComponent<PhotonView>().ViewID, xSpread, ySpread);

                InstanceTP.GetComponent<Laser2>().damage = (int)damage;
                InstanceTP.GetComponent<Laser2>().head_coef = head_coef;

                // 자기가 플레이하는 캐릭터라면 3인칭 궤적이 안보이게 처리한다.
                if (pv.IsMine)
                {
                    InstanceTP.GetComponent<LineRenderer>().enabled = false;
                    InstanceTP.transform.GetChild(0).gameObject.SetActive(false);
                    InstanceTP.transform.GetChild(1).gameObject.SetActive(false);
                }
            }

            { // 1인칭 생성
                GameObject InstanceFP = Instantiate(
                    LaserPrefab, skillPointFP.transform.position, skillPointFP.transform.rotation);
                LaserSC = InstanceFP.GetComponent<Laser2>();
                LaserSC.xSpread = xSpread;
                LaserSC.ySpread = ySpread;
                LaserSC.camObj = mCamera.gameObject;
                LaserSC.Shot();
            }
        }
    }
    
    [PunRPC]
    private void RPC_ShootTP(int viewID, float xSpread, float ySpread)
    {
        LaserSC = PhotonNetwork.GetPhotonView(viewID).gameObject.GetComponent<Laser2>();
        LaserSC.xSpread = xSpread;
        LaserSC.ySpread = ySpread;
        LaserSC.Shot();
    }
}




//========================================VSkill ========================================

class MeleeAttack : RoraSkill // 근접 공격 (타격)
{
    //==컴포넌트 관련 변수
    Collider WandCollider;
    GameObject roraWeaponTP;
    bool bIsMLAttackStart = false;
    RoraWeapon weaponSC;

    //== 함수
    public void Init(SkillControl skillCtrSC)
    {
        base.Init(skillCtrSC, 0, skillCtrSC.Melee_damage, skillCtrSC.Melee_headCoef);

        roraWeaponTP = SkillCtrSC.wandTP;
        WandCollider = roraWeaponTP.GetComponent<Collider>();
        weaponSC = roraWeaponTP.GetComponent<RoraWeapon>();

        // ==== 초기화
        WandCollider.enabled = false;
        weaponSC.pv = pv;
        weaponSC.bOnVAttack = false;
    }

    public override void OnSkillStart()
    {
        WandCollider.enabled = true;
        weaponSC.bOnVAttack = true;
        weaponSC.AudioManager = AudioManager;
        VacateCooltime();
    }

    public override void OnSkillUpdate()
    {

    }

    public override void OnSkillEnd()
    {
        WandCollider.enabled = false;
        weaponSC.bOnVAttack = false;
    }
}




//======================================== Teleport ========================================

class ShiftSkill : RoraSkill //텔레포트
{
    //== 변수
    private float cameraDefaultFOV;
    private float cameraFOVSpeed = 2.0f;

    // == 파티클 관련 변수
    private ParticleSystem Particle1;
    private ParticleSystem Particle2;

    //== 컴포넌트 관련 변수
    private Camera mCamera;
    ChangeShader CharacterShader;
    ChangeShader WandShader;
    GameObject[] CharacterModels;
    Rora RoraSC;
    Transform skillPoint;
    Image filterScreen;
    // == 공격 상태 관련 변수
    private bool bIsTeleportEnd = true;

    // == 함수
    public void Init(SkillControl skillCtrSC, Rora roraSC)
    {
        base.Init(skillCtrSC, skillCtrSC.Teleport_coolTime, 0, 0);

        Particle1 = skillCtrSC.teleportparticle1.GetComponent<ParticleSystem>();
        Particle2 = skillCtrSC.teleportparticle2.GetComponent< ParticleSystem > ();

        skillPoint = skillCtrSC.SkillPointTP;
        mCamera = skillCtrSC.mCamera;

        RoraSC = roraSC;

        CharacterShader = new ChangeShader(SkillCtrSC.characterModels[0]);
        WandShader = new ChangeShader(SkillCtrSC.characterModels[1]);

        filterScreen = SkillCtrSC.FilterScreen;
        filterScreen.enabled = false;

        cameraDefaultFOV = mCamera.fieldOfView;

        cameraFOVSpeed = skillCtrSC.cameraFOVSpeed;
    }

    public override void OnSkillStart()
    {
        {
           // Particle1.gameObject.SetActive(true);
           // Particle1.Play();
        }
        {
            pv.RPC("RPC_SetActiveOnOffParticle", RpcTarget.AllBuffered, 0, true);
            pv.RPC("RPC_PlayParticle", RpcTarget.AllBuffered, 0);
        }
        VacateCooltime();
    }

    public void OnSkillEvent()
    {
        mCamera.fieldOfView = cameraDefaultFOV * 1.2f;

        WandShader.ChangeTransparent(0.0f, 1);
        CharacterShader.ChangeTransparent(0.0f, 1);
        bIsTeleportEnd = false;
    }

    public override void OnSkillUpdate()
    {
        if (!bIsTeleportEnd)
        {
            RoraSC.TeleportMove();

            
            {
                //Particle2.gameObject.SetActive(true);
                //Particle2.Play();
            }
            {
                pv.RPC("RPC_SetActiveOnOffParticle", RpcTarget.AllBuffered, 1, true); 
                pv.RPC("RPC_PlayParticle", RpcTarget.AllBuffered, 1); 
            }
           
            filterScreen.enabled = true;
            filterScreen.color = new Color32(0, 0, 0, 10);
        }
        else
        {
            mCamera.fieldOfView = cameraDefaultFOV;
            FillCooltime();
        }
    }

    public override void OnSkillEnd()
    {
        {
          //  Particle2.gameObject.SetActive(false);
          //  Particle1.Play();
        }
        {
            pv.RPC("RPC_SetActiveOnOffParticle", RpcTarget.AllBuffered, 1, false);
           pv.RPC("RPC_PlayParticle", RpcTarget.AllBuffered, 0);
        }

        filterScreen.enabled = false;

        CharacterShader.ChangeTransparent(1.0f, 0);
        WandShader.ChangeTransparent(1.0f, 0);

        bIsTeleportEnd = true;
    }

    [PunRPC]
    void RPC_SetActiveOnOffParticle(int index,bool on)
    {
        if (index == 0) Particle1.gameObject.SetActive(on);
        else if (index == 1) Particle2.gameObject.SetActive(on);
    }

    [PunRPC]
    void RPC_PlayParticle(int index)
    {
        if (index == 0) Particle1.Play();
        else if (index == 1) Particle2.Play();
    }

    [PunRPC]
    void RPC_StopParticle(int index)
    {
        if (index == 0) Particle1.Stop();
        else if (index == 1) Particle2.Stop();
    }
}




//======================================== ESkill ========================================
class Reflection : RoraSkill
{
    //==변수
    float ReflectionDamage;
    GameObject BlackholePrefab;

    List<GameObject> blackholes = new List<GameObject>();
    GameObject blackHole;
    BlackHole blackHoleSC;
    Laser1 laserSC;
    float SnipperTime;
    private float timeSaver;
    float maxLength = 0;

    //==컴포넌트 관련 변수
    Transform skillPointTP;
    Camera mCamera;
    Animator animator;
    GameObject Character;
    Rora roraSC;

    //== 공격 상태 관련 변수
    bool bIsReflectionSkillStart = false;
    bool bIsShotStart = false;

    public Reflection() { }

    //== 함수
    public void Init(SkillControl skillCrlSC, Rora roraSC, Animator animator)
    {
        base.Init(skillCrlSC, skillCrlSC.reflection_coolTime, 0, 0);

        mCamera = SkillCtrSC.mCamera;
        BlackholePrefab = SkillCtrSC.blackhole;
        this.animator = animator;
        skillPointTP = SkillCtrSC.SkillPointTPModel;
        Character = SkillCtrSC.gameObject;
        this.roraSC = roraSC;
    }

    public override void OnSkillStart()//생성
    {
        VacateCooltime();

        blackHole = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs/RoraInstance", "BlackHoleEffect"), 
            skillPointTP.position, mCamera.transform.rotation);
        blackHoleSC = blackHole.GetComponent<BlackHole>();
        blackHoleSC.animator = animator;

        bIsReflectionSkillStart = true;
    }

    public void UpdateState(int value)
    {
        blackHoleSC.UpdateState(value);
    }

    public override void OnSkillUpdate()
    {
        if (!bIsReflectionSkillStart && !bIsShotStart)
        {
            FillCooltime();
        }
        else
        {
            if (blackHoleSC.GetBlackMode() == 3)
            {
                SkillCtrSC.AbsoredDamage = blackHoleSC.GetAbsorbedDamage();
            }
            if (blackHoleSC.GetBlackMode() == 4)
            {
                SkillCtrSC.AbsoredDamage = blackHoleSC.GetAbsorbedDamage();
            }
        }
    }

    public override void OnSkillEnd()
    {
        bIsReflectionSkillStart = false;
        SkillCtrSC.AbsoredDamage = blackHoleSC.GetAbsorbedDamage();
        Debug.Log("SkillCtrSC.AbsoredDamage: " + SkillCtrSC.AbsoredDamage);
        if (blackHole) PhotonNetwork.Destroy(blackHole);
    }

    public float GetDamage() { return blackHoleSC.GetAbsorbedDamage(); }

    public void SetBIsShotStart(bool b) { bIsShotStart = b; }
}



//======================================== SniperShot ========================================

class SniperShot : RoraSkill
{
    //== 변수 관련
    private float SnipperTime;
    private float totalDamage;
    private float FinalDamage;
    public float absorbedDamage;
    public GameObject character;

    //==컴포넌트 관련
    private Transform skillPointTP;
    private Transform skillPointFP;
    private Camera mCamera;

    //== 이펙트 관련 
    private GameObject Aura;
    private Image filterScreen;

    //== 히트스캔 레이저 관련 변수
    private GameObject[] Prefabs = new GameObject[2];
    private GameObject InstanceTP;
    private Laser3 LaserSC;
    private float maxLength;

    // ==공격 상태 관련 변수
    private int ShotState = 0;
    private bool bIsShotStart = false;
    private bool bIsShot = false;
    // == 
    List<GameObject> auras = new List<GameObject>();
    List<GameObject> AudioInstances = new List<GameObject>();

    public SniperShot() { }

    public void Init(SkillControl skillCtrSC)
    {
        base.Init(skillCtrSC, 0, skillCtrSC.reflection_damage, skillCtrSC.reflection_headCoef);

        Prefabs[0] = SkillCtrSC.reflectionAura;
        Prefabs[1] = SkillCtrSC.Lasers[2];

        mCamera = SkillCtrSC.mCamera;

        skillPointTP = SkillCtrSC.SkillPointTP;
        skillPointFP = SkillCtrSC.SkillPointFP;

        filterScreen = SkillCtrSC.FilterScreen;
        filterScreen.enabled = false;

        maxLength = SkillCtrSC.reflection_maxLength;

        SnipperTime = skillCtrSC.snipperTime;

        character = SkillCtrSC.gameObject;

        damage = SkillCtrSC.reflection_damage;
        absorbedDamage = SkillCtrSC.AbsoredDamage;

        ShotState = 0;
    }

    public override void OnSkillStart()
    {
        // 총 데미지 계산
        totalDamage = (absorbedDamage + damage > 100) ? 100 : absorbedDamage + damage;
        FinalDamage = totalDamage;
        Debug.Log("Total Absorbed Damage: " + absorbedDamage);

        // 저격모드 필터 키기
        filterScreen.color = new Color32(211, 255, 54, 50);
        filterScreen.enabled = true;

        // 아우라 발산
        Aura = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs/RoraInstance", "reflectionAura"), 
            transform.position, transform.rotation);
        auras.Add(Aura);
        pv.RPC("RPC_SetAuraParent", RpcTarget.AllBuffered, Aura.GetComponent<PhotonView>().ViewID);
        if (auras.Count > 1)
        {
            for (int i = 1; i < auras.Count; i++)
            {
                if(auras[i])    
                    PhotonNetwork.Destroy(auras[i]);
            }
        }

        // bool값 설정
        bIsShotStart = true;
        bIsShot = false;
    }

    public override void OnSkillUpdate()
    {
        // 저격 모드 진입후 해줄 작업 처리
        if (bIsShotStart)
        {
            // 유지 시간을 체크후 시간이 지났다면 저격 모드를 종료한다.
            float timer = 0;
            timer += Time.deltaTime;
            if (timer > SnipperTime) OnSkillEnd();
        }
    }

    public override void OnSkillEnd()
    {
        if(auras.Count > 0)    PhotonNetwork.Destroy(auras[0]);
        auras.Clear();
        filterScreen.enabled = false;
    }

    public virtual void Shoot()
    {
        // >> ::  1인칭 생성
        {
            Debug.Log("Shoot");
            GameObject InstanceFP = Instantiate(
                Prefabs[1], skillPointFP.transform.position, skillPointFP.transform.rotation);
            LaserSC = InstanceFP.GetComponent<Laser3>();
            LaserSC.camObj = mCamera.gameObject;
            LaserSC.Shot();
        }

        // >> ::  3인칭 생성 
        {
            InstanceTP = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "ESkillLaser"),
                transform.position, transform.rotation);
            pv.RPC("RPC_ShootTP", RpcTarget.AllBuffered,
                    InstanceTP.GetComponent<PhotonView>().ViewID);

            // 자기가 플레이하는 캐릭터라면 3인칭 궤적이 안보이게 처리한다.
            if (pv.IsMine)
            {
                InstanceTP.GetComponent<LineRenderer>().enabled = false;
                InstanceTP.transform.GetChild(0).gameObject.SetActive(false);
                InstanceTP.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    // 아우라 위치 고정
    [PunRPC]
    private void RPC_SetAuraParent(int viewID)
    {
        GameObject aura = PhotonNetwork.GetPhotonView(viewID).gameObject;
        aura.transform.parent = transform;
    }

    // 3인칭 공격 발생
    [PunRPC]
    private void RPC_ShootTP(int viewID)
    {
        LaserSC = PhotonNetwork.GetPhotonView(viewID).gameObject.GetComponent<Laser3>();
        LaserSC.damage = (int)totalDamage;
        LaserSC.Shot();
    }
};




//======================================== FSkill ========================================

class FSkill : RoraSkill //마나 응축탄
{
    //==컴포넌트 관련
    Camera mCamera;
    Transform skillPointTP;
    Transform skillPointFP;
    // == 투사체(마나) 관련 변수
    private GameObject Prefab;
    private ManaEffect manaSC;

    // ==공격 상태 관련 변수
    private bool bIsAttackStart = false;

    //== 함수
    public FSkill() { }

    public void Init(SkillControl skillCtrlSC)
    {
        base.Init(skillCtrlSC, skillCtrlSC.ManaBomb_coolTime, skillCtrlSC.ManaBomb_damage, skillCtrlSC.Attack_headCoef);

        skillPointTP = SkillCtrSC.SkillPointTP;
        skillPointFP = SkillCtrSC.SkillPointFP;

        mCamera = SkillCtrSC.mCamera;
        Prefab = SkillCtrSC.manaEffects;
        bulletPerShot = SkillCtrSC.ManaBomb_PerShots;

        //==초기화
        bIsAttackStart = false;
    }

    public override void OnSkillStart()
    {
        bIsAttackStart = true;
        VacateCooltime();

       //  SkillCtrSC.ReduceMagazine((int)bulletPerShot);
    }


    public void InstantiateMana()
    {
        GameObject Instance
            = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs/RoraInstance", "FSkill"), 
                skillPointFP.transform.position, 
                transform.rotation);
    }

    public override void OnSkillUpdate()
    {
        if (!bIsAttackStart) FillCooltime();
    }

    public override void OnSkillEnd()
    {
        bIsAttackStart = false;
    }
}


//======================================== QSkill ========================================

class QSkill : RoraSkill // 로라 궁극기
{
    // == 컴포넌트 관련 변수
    Camera mCamera;
    GameObject model;
    Animator animator;

    // == 포탑 관련 변수
    GameObject Prefab;
    GameObject turret;
    List<GameObject> turrets = new List<GameObject>();
    TurretEffect turretSC;

    //== 이펙트 관련
    float turret_LifeTime;

    //== 공격 상태 관련 변수
    bool bIsQskillStart = false;

    //== 함수
    public QSkill() { }

    public void Init(SkillControl skillCtrlSC, Animator animator)
    {
        base.Init(skillCtrlSC, skillCtrlSC.QSkill_Cooltime, skillCtrlSC.Attack_damage, skillCtrlSC.Attack_headCoef);

        mCamera = SkillCtrSC.mCamera;//.QSkillEffect;
        Prefab = SkillCtrSC.QSkillEffect;
        model = SkillCtrSC.gameObject;
        this.animator = animator;
        turret_LifeTime = SkillCtrSC.QSkill_LifeTime;
    }

    public override void OnSkillStart()
    {
        bIsQskillStart = true;
        InstantiateTurret();
        VacateCooltime();
    }

    public void InstantiateTurret()
    {
        turret = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs/RoraInstance", "QskillEffects"), 
            transform.position, transform.rotation);
    }

    public override void OnSkillUpdate()
    {
        if (!bIsQskillStart) FillCooltime();
    }

    public override void OnSkillEnd()
    {
        bIsQskillStart = false;
    }
}