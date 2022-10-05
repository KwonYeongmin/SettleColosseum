using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RAttackLaser : MonoBehaviour
{
    [Header("투사체 값")]
    public float Speed = 10.0f;
    public float Size = 1;
    public float LifeTime;

    [Header("보조공격 레이저")]
    public GameObject LaserTP;
    public GameObject LaserFP;

    // 레이 관련 변수
   private Vector3 Origin_TP;
   private Vector3 Origin_FP;
   private Vector3 Dir_TP;
   private Vector3 Dir_FP;

    RaycastHit rayHit;
    [HideInInspector] public PhotonView pv;
    [HideInInspector] public GameObject owner;

    void Awake()
    {
        //pv초기화
        pv = this.GetComponent<PhotonView>(); 

        // 크기 설정
        float defaultRadius = this.gameObject.GetComponent<Transform>().localScale.x;
        this.gameObject.GetComponent<Transform>().localScale = new Vector3(defaultRadius * Size, defaultRadius * Size, defaultRadius * Size);

        //인스턴스 생성한 오브젝트 찾아주기
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
            // 생성되는 시작점
            Origin_TP = owner.GetComponent<SkillControl>().SkillPointTP.transform.position;
            Origin_FP = owner.GetComponent<SkillControl>().SkillPointFP.transform.position;


            // 3인칭 경로 설정 : 카메라가 보는 앞 방향
            Dir_TP = owner.transform.GetChild(1).GetComponent<Camera>().transform.forward; 

            int layermask = (1 << 11) + (1 << 12) + (1 << 14);
            layermask = ~layermask;

            // 레이 쏘기
            Physics.Raycast(Origin_TP, Dir_TP, out rayHit, 300, layermask);

            // 1인칭 경로 설정(3인칭 경로를 통해 정해진다) 
            Dir_FP = (rayHit.point - Origin_FP).normalized; //레이와 부딪힌 곳으로 방향 맞춰주기
            if (rayHit.collider == null) Dir_FP = Camera.main.transform.forward; // 부딪히는 콜라이더가 없으면 3인칭과 같은 방향
        }
    }

    void Start()
    {
        // if (!pv.IsMine) { } //3인칭인 경우 1인칭 레이저를 안보이도록 해준다.

        LaserTP.SetActive(true);

        LaserTP.transform.position = Origin_TP;
        LaserFP.transform.position = Origin_FP;
    }

    void Update()
    {
        if (!LaserTP.activeSelf) Destroy(this.gameObject); //3인칭 레이저가 켜져있지 않으면 파괴
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
