using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RAttackLaser : MonoBehaviour
{
    [Header("����ü ��")]
    public float Speed = 10.0f;
    public float Size = 1;
    public float LifeTime;

    [Header("�������� ������")]
    public GameObject LaserTP;
    public GameObject LaserFP;

    // ���� ���� ����
   private Vector3 Origin_TP;
   private Vector3 Origin_FP;
   private Vector3 Dir_TP;
   private Vector3 Dir_FP;

    RaycastHit rayHit;
    [HideInInspector] public PhotonView pv;
    [HideInInspector] public GameObject owner;

    void Awake()
    {
        //pv�ʱ�ȭ
        pv = this.GetComponent<PhotonView>(); 

        // ũ�� ����
        float defaultRadius = this.gameObject.GetComponent<Transform>().localScale.x;
        this.gameObject.GetComponent<Transform>().localScale = new Vector3(defaultRadius * Size, defaultRadius * Size, defaultRadius * Size);

        //�ν��Ͻ� ������ ������Ʈ ã���ֱ�
        Playable[] players = FindObjectsOfType<Playable>();

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<PhotonView>().Controller == pv.Owner)
            {
                owner = players[i].gameObject;
                break;
            }
        }

       
        {
            // �����Ǵ� ������
            Origin_TP = owner.GetComponent<SkillControl>().SkillPointTP.transform.position;
            Origin_FP = owner.GetComponent<SkillControl>().SkillPointFP.transform.position;


            // 3��Ī ��� ���� : ī�޶� ���� �� ����
            Dir_TP = owner.transform.GetChild(1).GetComponent<Camera>().transform.forward; 

            int layermask = (1 << 11) + (1 << 12) + (1 << 14);
            layermask = ~layermask;

            // ���� ���
            Physics.Raycast(Origin_TP, Dir_TP, out rayHit, 300, layermask);

            // 1��Ī ��� ����(3��Ī ��θ� ���� ��������) 
            Dir_FP = (rayHit.point - Origin_FP).normalized; //���̿� �ε��� ������ ���� �����ֱ�
            if (rayHit.collider == null) Dir_FP = Camera.main.transform.forward; // �ε����� �ݶ��̴��� ������ 3��Ī�� ���� ����
        }
    }

    void Start()
    {
        // if (!pv.IsMine) { } //3��Ī�� ��� 1��Ī �������� �Ⱥ��̵��� ���ش�.

        LaserTP.SetActive(true);

        LaserTP.transform.position = Origin_TP;
        LaserFP.transform.position = Origin_FP;
    }

    void Update()
    {
        if (!LaserTP.activeSelf) Destroy(this.gameObject); //3��Ī �������� �������� ������ �ı�
        Destroy(this.gameObject,LifeTime);

        AimFP();
        AimTP();
    }

    void AimFP()
    {
        LaserFP.transform.position += Dir_FP * Speed * Time.deltaTime;
    }
    void AimTP()
    {
        LaserTP.transform.position += Dir_TP * Speed * Time.deltaTime;
    }
}
