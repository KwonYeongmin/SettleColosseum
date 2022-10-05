using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// 플레이어블 캐릭터들의 기반 클래스
public abstract class Playable : ObjectWithHP
{
    // pv
    [HideInInspector] public PhotonView pv;

    // 카메라 & 캐릭터 회전
    [Header("회전")]
    [Tooltip("마우스 감도")]
  //  [Range(0.1f, 2.0f)]
    public float mouseSensitivity = 2f;
    public GameObject camera;
    [Tooltip("회전 속도")]
    public float cameraRotSpeed = 200f;
    protected float cameraRotX = 0f;
    protected float cameraRotY;

    // 상태이상 관련
    public bool bCanInteract = true;
    private float slowTimer = float.MaxValue;
    private float keepSlowTime = 3f;
    protected float slowValue = 1f;

    // 기타 변수들
    protected RagdollApplier ragdollApplier;

    protected virtual void OnEnable()
    {
        cameraRotY = transform.rotation.eulerAngles.y;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        InputProc();
        CheckTimers();
    }

    // 플레이어의 입력을 감지해 처리하는 함수.
    private void InputProc()
    {
        // 아무키도 안눌렀을때
        if (!Input.anyKey)
        {
            Idle();
        }
        else
        {
            // 이동키를 누른 경우
            if (
                KeyManager.Inst.GetAxisRawHorizontal() != 0 ||
                KeyManager.Inst.GetAxisRawVertical() != 0)
            {
                Move();
            }

            // 점프키를 누른 경우
            if (Input.GetKeyDown(KeyManager.Inst.Jump))
            {
                Jump();
            }

            // 일반 공격키를 누른 경우
            if (Input.GetKeyDown(KeyManager.Inst.Attack))
            {
                Attack();
            }

            // 특수 공격키(RMOUSE)를 누른 경우
            if (Input.GetKey(KeyManager.Inst.RAttack))
            {
                RAttack();
            }

            // 근접 공격키를 누른 경우
            if (Input.GetKeyDown(KeyManager.Inst.MeleeAttack))
            {
                MeleeAttack();
            }

            // 스킬(E 스킬)을 쓴 경우
            if (Input.GetKeyDown(KeyManager.Inst.ESkill))
            {
                ESkill();
            }

            // 스킬(F 스킬)을 쓴 경우
            if (Input.GetKeyDown(KeyManager.Inst.FSkill))
            {
                FSkill();
            }

            // 스킬(Shift 스킬)을 쓴 경우
            if (Input.GetKeyDown(KeyManager.Inst.ShiftSkill))
            {
                ShiftSkill();
            }

            // 궁극기(Q 스킬)를 쓴 경우
            if (Input.GetKeyDown(KeyManager.Inst.QSkill))
            {
                QSkill();
            }

            // 재장전 키를 누른 경우
            if (Input.GetKeyDown(KeyManager.Inst.Reload))
            {
                Reload();
            }
        }
    }

    private void CheckTimers()
    {
        if(slowTimer < keepSlowTime)
            keepSlowTime += Time.deltaTime;
        else
            slowValue = 1f;
    }

    // 하위 플레이어블에서 구현해야할 목록
    public abstract void Idle();
    public abstract void Move();
    public abstract void Jump();
    public abstract void Attack();
    public abstract void RAttack();
    public abstract void MeleeAttack();
    public abstract void ESkill();
    public abstract void FSkill();
    public abstract void ShiftSkill();
    public abstract void QSkill();
    public abstract void Reload();
    public abstract void CheckDead();
    public abstract void Dragged(GameObject dragger);
    public abstract bool IsDead();
    public abstract bool IsMine();
    public abstract void ShowDeadUI();
    public abstract void ShowResultUI(bool bIsVictory);
    public abstract void TakeDamage_Sync(int damage);


    // 두 캐릭터에서 공통적으로 쓰이는 함수들
    // 카메라 회전
    public virtual void RotateCamera()
    {
        // 카메라 회전
        float limit_rotX_min = -60.0f;  //X축 회전 각도 최솟값
        float limit_rotX_max = 80.0f;   //X축 회전 각도 최댓값

        // 마우스 Y 이동 값
        float mouseY = mouseSensitivity * Input.GetAxis("Mouse Y");

        cameraRotX += mouseY * cameraRotSpeed * Time.deltaTime;

        cameraRotX = Mathf.Clamp(cameraRotX, limit_rotX_min, limit_rotX_max);
        camera.transform.localEulerAngles = new Vector3(-cameraRotX, 0, 0);
    }

    // 캐릭터 회전
    public virtual void RotateCharacter()
    {
        // 캐릭터 회전
        // 마우스 X 이동 값
        float mouseX = mouseSensitivity * Input.GetAxis("Mouse X");

        cameraRotY += mouseX * cameraRotSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, cameraRotY, 0);
    }

    // 플레이어 속한 팀을 구하는 함수
    public int GetPlayerTeamNumber()
    {
        var players = PhotonNetwork.CurrentRoom.Players;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i + 1].IsLocal)
                return ((i % 2 == 0) ? 0 : 1);
        }

        return -1;
    }

    // 게임 매니저에게 리스폰 요청을 보낸다.
    public virtual void SendRespawn()
    {
        GameStateManager.Inst.Respawn(gameObject);
    }

    public void SetRotationY(float rotY)
    {
        cameraRotY = rotY;
    }

    public void SetSlowValue(float value, float keepTime)
    {
        pv.RPC("RPC_SetSlowValue", RpcTarget.AllBuffered, value, keepTime);
    }

    [PunRPC]
    protected void RPC_SetSlowValue(float value, float keepTime)
    {
        slowTimer = 0f;
        slowValue = value;
        keepSlowTime = keepTime;
    }
}

// 상태들의 기반 클래스
public abstract class PlayableState
{
    // 프로퍼티
    public abstract string Type { get; }

    // 멤버 함수
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}