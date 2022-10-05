using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// �÷��̾�� ĳ���͵��� ��� Ŭ����
public abstract class Playable : ObjectWithHP
{
    // pv
    [HideInInspector] public PhotonView pv;

    // ī�޶� & ĳ���� ȸ��
    [Header("ȸ��")]
    [Tooltip("���콺 ����")]
  //  [Range(0.1f, 2.0f)]
    public float mouseSensitivity = 2f;
    public GameObject camera;
    [Tooltip("ȸ�� �ӵ�")]
    public float cameraRotSpeed = 200f;
    protected float cameraRotX = 0f;
    protected float cameraRotY;

    // �����̻� ����
    public bool bCanInteract = true;
    private float slowTimer = float.MaxValue;
    private float keepSlowTime = 3f;
    protected float slowValue = 1f;

    // ��Ÿ ������
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

    // �÷��̾��� �Է��� ������ ó���ϴ� �Լ�.
    private void InputProc()
    {
        // �ƹ�Ű�� �ȴ�������
        if (!Input.anyKey)
        {
            Idle();
        }
        else
        {
            // �̵�Ű�� ���� ���
            if (
                KeyManager.Inst.GetAxisRawHorizontal() != 0 ||
                KeyManager.Inst.GetAxisRawVertical() != 0)
            {
                Move();
            }

            // ����Ű�� ���� ���
            if (Input.GetKeyDown(KeyManager.Inst.Jump))
            {
                Jump();
            }

            // �Ϲ� ����Ű�� ���� ���
            if (Input.GetKeyDown(KeyManager.Inst.Attack))
            {
                Attack();
            }

            // Ư�� ����Ű(RMOUSE)�� ���� ���
            if (Input.GetKey(KeyManager.Inst.RAttack))
            {
                RAttack();
            }

            // ���� ����Ű�� ���� ���
            if (Input.GetKeyDown(KeyManager.Inst.MeleeAttack))
            {
                MeleeAttack();
            }

            // ��ų(E ��ų)�� �� ���
            if (Input.GetKeyDown(KeyManager.Inst.ESkill))
            {
                ESkill();
            }

            // ��ų(F ��ų)�� �� ���
            if (Input.GetKeyDown(KeyManager.Inst.FSkill))
            {
                FSkill();
            }

            // ��ų(Shift ��ų)�� �� ���
            if (Input.GetKeyDown(KeyManager.Inst.ShiftSkill))
            {
                ShiftSkill();
            }

            // �ñر�(Q ��ų)�� �� ���
            if (Input.GetKeyDown(KeyManager.Inst.QSkill))
            {
                QSkill();
            }

            // ������ Ű�� ���� ���
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

    // ���� �÷��̾���� �����ؾ��� ���
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


    // �� ĳ���Ϳ��� ���������� ���̴� �Լ���
    // ī�޶� ȸ��
    public virtual void RotateCamera()
    {
        // ī�޶� ȸ��
        float limit_rotX_min = -60.0f;  //X�� ȸ�� ���� �ּڰ�
        float limit_rotX_max = 80.0f;   //X�� ȸ�� ���� �ִ�

        // ���콺 Y �̵� ��
        float mouseY = mouseSensitivity * Input.GetAxis("Mouse Y");

        cameraRotX += mouseY * cameraRotSpeed * Time.deltaTime;

        cameraRotX = Mathf.Clamp(cameraRotX, limit_rotX_min, limit_rotX_max);
        camera.transform.localEulerAngles = new Vector3(-cameraRotX, 0, 0);
    }

    // ĳ���� ȸ��
    public virtual void RotateCharacter()
    {
        // ĳ���� ȸ��
        // ���콺 X �̵� ��
        float mouseX = mouseSensitivity * Input.GetAxis("Mouse X");

        cameraRotY += mouseX * cameraRotSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, cameraRotY, 0);
    }

    // �÷��̾� ���� ���� ���ϴ� �Լ�
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

    // ���� �Ŵ������� ������ ��û�� ������.
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

// ���µ��� ��� Ŭ����
public abstract class PlayableState
{
    // ������Ƽ
    public abstract string Type { get; }

    // ��� �Լ�
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}