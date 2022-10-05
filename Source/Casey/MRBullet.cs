using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MRBullet : MonoBehaviour
{
    [Header("����ü")]
    [Tooltip("FP�Ѿ�")] public GameObject MRBullet_FP;
    [Tooltip("TP�Ѿ�")] public GameObject MRBullet_TP;
    [Tooltip("�ӵ�")] public float speed = 50.0f;
    [Tooltip("������Ÿ��")] public float defaultLifetime = 3.0f;
    [Tooltip("(Look)������Ÿ��")] public float lifeTime = 0f;

    Vector3 origin_TP;
    Vector3 origin_FP;
    Vector3 dir_TP;
    Vector3 dir_FP;
    RaycastHit rayHit;
    [HideInInspector] public PhotonView pv;

    GameObject owner;
    public GameObject Owner { get { return owner; } }


    private void Awake()
    {
        // pv �ʱ�ȭ
        pv = GetComponent<PhotonView>();

        // �Ѿ��� �� �÷��̾ ã�´�.
        Playable[] players = FindObjectsOfType<Playable>();
        for(int i = 0; i < players.Length; i++)
        {
            if(players[i].GetComponent<PhotonView>().Controller == pv.Owner)
            {
                owner = players[i].gameObject;
                break;
            }
        }

        // ������Ʈ Ǯ���� �ӽ÷� ������ ��� �׳� return�Ѵ�.
        if (owner == null) return;

        origin_FP = owner.GetComponent<Casey>().FP_RMuzzle.transform.position;
        origin_TP = owner.GetComponent<Casey>().TP_Muzzle.transform.position;

        dir_TP = owner.transform.GetChild(3).GetComponent<Camera>().transform.forward;
        int layerMask = (1 << 11) + (1 << 12) + (1 << 14);//������ ���̾�
        layerMask = ~layerMask;

        Physics.Raycast(origin_TP, dir_TP, out rayHit, 300, layerMask);
        dir_FP = (rayHit.point - origin_FP).normalized;
        //dir_TP = (rayHit.point - origin_TP).normalized;

        if (rayHit.collider == null)
        {
            dir_FP = Camera.main.transform.forward;
            Debug.Log("MR Ray : null");
            return;
        }
        else
            Debug.Log("MR Ray : " + rayHit.collider.gameObject.layer + " : " + rayHit.collider.transform.name);
    }

    void Start()
    {
        if (!pv.IsMine)
        {
            MRBullet_FP.transform.GetChild(0).gameObject.SetActive(false);
        }
        MRBullet_TP.SetActive(true);

        MRBullet_FP.transform.position = origin_FP;
        MRBullet_TP.transform.position = origin_TP;
    }

    void Update()
    {
        lifeTime += Time.deltaTime;
        if (MRBullet_TP == null || lifeTime >= defaultLifetime)
            Destroy(gameObject);

        Trajectory_FP();
        Trajectory_TP();
    }

    void Trajectory_FP() //1��Ī ����
    {
        MRBullet_FP.transform.position += dir_FP * speed * Time.deltaTime;
    }

    public void Trajectory_TP() //3��Ī ����
    {
        if (MRBullet_TP == null) return;
        MRBullet_TP.transform.position += dir_TP * speed * Time.deltaTime;
    }

    public void SetOwner(GameObject _owner)
    {
        owner = _owner;
    }
}