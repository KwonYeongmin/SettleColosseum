using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MLBullet : MonoBehaviour
{
    [Header("����ü")]
    [Tooltip("FP�Ѿ�")] public GameObject MLBullet_FP;
    [Tooltip("TP�Ѿ�")] public GameObject MLBullet_TP;
    [Tooltip("�ӵ�")] public float speed = 100.0f;
    [Tooltip("������Ÿ��")] public float defaultLifetime = 10.0f;
    [Tooltip("(Look)������Ÿ��")] public float lifeTime = 0f;

    // public AudioClip shotFX;

    Vector3 origin_TP;
    Vector3 origin_FP;
    Vector3 dir_TP;
    Vector3 dir_FP;

    RaycastHit rayHit;
    [HideInInspector] public PhotonView pv;
    [HideInInspector] public GameObject owner;

    private void Awake()
    {
        // pv �ʱ�ȭ
        pv = GetComponent<PhotonView>();

        // �Ѿ��� �� �÷��̾ ã�´�.
        Playable[] players = FindObjectsOfType<Playable>();
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<PhotonView>().Controller == pv.Owner)
            {
                owner = players[i].gameObject;
                break;
            }
        }

        // ������Ʈ Ǯ���� �ӽ÷� ������ ��� �׳� return�Ѵ�.
        if(owner == null)   return;

        origin_FP = owner.GetComponent<Casey>().FP_RMuzzle.transform.position;
        origin_TP = owner.GetComponent<Casey>().TP_Muzzle.transform.position;

        dir_TP = owner.transform.GetChild(3).GetComponent<Camera>().transform.forward;
        int layerMask = (1 << 11) + (1 << 12) + (1 << 14);
        layerMask = ~layerMask;

        Physics.Raycast(origin_TP, dir_TP, out rayHit, 300, layerMask);
        dir_FP = (rayHit.point - origin_FP).normalized;

        if (rayHit.collider == null)
        {
            dir_FP = Camera.main.transform.forward;
        }
    }

    void Start()
    {
        pv = GetComponent<PhotonView>();
        if (!pv.IsMine)
        {
            MLBullet_FP.transform.GetChild(0).gameObject.SetActive(false);
        }
        MLBullet_TP.SetActive(true);

        MLBullet_FP.transform.position = origin_FP;
        MLBullet_TP.transform.position = origin_TP;
        /*
        if (shotFX != null && GetComponent<AudioSource>())
        {
            GetComponent<AudioSource>().PlayOneShot(shotFX);
        }
        */
    }

    void Update()
    {
        lifeTime += Time.deltaTime;
        if (MLBullet_TP.activeSelf == false || lifeTime >= defaultLifetime)
            Destroy(gameObject);

        Trajectory_FP();
        Trajectory_TP();
    }

    void Trajectory_FP() //1��Ī ����
    {
        MLBullet_FP.transform.position += dir_FP * speed * Time.deltaTime;
    }

    public void Trajectory_TP() //3��Ī ����
    {
        if (MLBullet_TP == null) return;
        MLBullet_TP.transform.position += dir_TP * speed * Time.deltaTime;
    }
}
