using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK : MonoBehaviour
{
    [Header("IK")]
    [Tooltip("")]
    [Range(0, 1)] public float posWeight = 1;
    [Range(0, 1)] public float rotWeight = 1;
    //[Range(0, 1)] public float rotWeight = 1;
    //[Range(0, 359)] public float xRot = 0.0f;
    //[Range(0, 359)] public float yRot = 0.0f;
    //[Range(0, 359)] public float zRot = 0.0f;
    [Tooltip("�� IK")]
    public bool CaseyArmIKEnable = true;
    [Tooltip("�� IK")]
    public bool CaseyHeadIKEnable = true;
    [Tooltip("�ٶ� Ÿ��")]
    public Transform target;
    protected Animator animator; // �ִϸ�����
    private int selecteWeight = 1;

    public GameObject m_camera;

    void Start()
    {
        animator = GetComponent<Animator>();

    }

    private void OnAnimatorIK(int layerIndex)
    {
        Debug.Log("OnAnimatorIK ����");

        if(CaseyArmIKEnable)
            SetPositionWeightArm();

        if(CaseyHeadIKEnable)
            SetPositionWeightHead();
    }

    private void SetPositionWeightArm()//position weight��ŭ �� �̵�
    {
        Debug.Log("IK����");
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, posWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rotWeight);

        animator.SetIKPosition(AvatarIKGoal.RightHand, target.position);
        Quaternion handRotation = Quaternion.LookRotation(target.position - transform.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, handRotation);

    }

        private void SetPositionWeightHead()//position weight��ŭ �� �̵�
    {
        animator.SetLookAtWeight(1);
        animator.SetLookAtPosition(target.position);
        
    }
}

