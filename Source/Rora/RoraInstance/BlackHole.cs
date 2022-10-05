using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BlackHole : ObjectWithHP, IPunPrefabPool
{

    //========================== Blackhole ���� ���� ==========================
    public float maxSize = 3.0f;
    [Range(2, 10)] public float GrowingSpeed = 10.0f;
    [Range(2, 20)] public float DisappearSpeed = 10.0f;



    //========================== ��Ÿ ���� ==========================
    [HideInInspector] public Animator animator;
    private int State = 0;


    //========================== E��ų ���� ���� ==========================
    public float absorbedDamage = 0f;
    public int GetAbsorbedDamage() { return Mathf.RoundToInt(absorbedDamage * damage_coef); }
    float PercentValue = 0.2f;
    [HideInInspector] public bool bIsCC = false;

    [HideInInspector]
    public PhotonView pv;

    public AudioClip[] BlackholeAudio;
    AudioSource audioSource;

    public float damage_coef = 0.5f;

    //========================== ��Ȧ ���� ���� �Լ�==========================
    public void UpdateState(int value)   {State = value;}
    public int GetBlackMode()  {return State; }

    //========================== �浹 üũ ���� �Լ� ==========================
    private Collider Other;
    private GameObject HitObj;

    //========================== Start �� �ʱ�ȭ ==========================
    void OnEnable()
    {
        pv = GetComponent<PhotonView>(); //���� �ʱ�ȭ
        absorbedDamage = 0;
        Initialized();
    }

    private void Initialized()
    {
        // �Ѿ��� �� �÷��̾ ã�´�.
        GameObject owner = null;
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
        if (owner == null) return;

        // 3��Ī ������ �߻� ��ġ�� �θ� ������Ʈ transform�� �����Ѵ�.
        transform.parent = owner.transform.GetChild(1);
        audioSource = GetComponent< AudioSource > ();
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        // ������ ����� ����
        damage_coef = owner.GetComponent<SkillControl>().absorbedCoef;

        State = 0;
    }

    void Update()
    {
        switch (State)
        {
            case 0: { Instantiated(); } break; //��Ȧ ����

            case 1: { Grow(); } break; // ��Ȧ ����

            case 2: { UsingSkill(); } break; //��Ȧ ��ų ���
            case 3: { Disappear(); } break; // ��Ȧ ����

            case 4: { PhotonNetwork.Destroy(gameObject); } break; // ��Ȧ �ı�
        }
    }

    //��Ȧ ����
    private void Instantiated()
    {
        hp = 0; // ����� ������ �ʱ�ȭ
        audioSource.clip = BlackholeAudio[0];
        audioSource.loop = false;
        audioSource.Play();
        
    }


    //��Ȧ ����
    private void Grow()
    {
        //����� ��� 
        audioSource.clip = BlackholeAudio[1];
        audioSource.loop = true;
        audioSource.Play();

        float cur_size = transform.localScale.x;

        if (cur_size < maxSize)
        {
            cur_size += Time.deltaTime * GrowingSpeed;
            transform.localScale = new Vector3(cur_size, cur_size, cur_size);
        }
    }

    // E��ų ���
    private void UsingSkill()
    {
        //  ����� ��� 
        {
           // audioSource.clip = blackHoleSound[1]; 
          //  audioSource.loop = true;
          //  audioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        Other = collision;

        // �����ϴ� �ڱ� �ڽ��� ���Ѵ�
        PhotonView pv_mine = transform.root.GetComponent<PhotonView>();

        // ���� ������Ʈ�� ���Ѵ�.
        HitObj = collision.transform.root.gameObject;
        PhotonView pv_other = HitObj.GetComponent<PhotonView>();

        //���� ������Ʈ�� ���� ������Ʈ��� �׳� ������.
        if (pv_other == null) return;

        if (pv_mine != pv_other)
        {
            pv.RPC("RPC_AbsordDamage", RpcTarget.OthersBuffered, pv_other.ViewID); //���� ����ϱ�
        }
    }
    
    [PunRPC]
    private void RPC_AbsordDamage(int viewID)
    {
        GameObject hitObj = PhotonNetwork.GetPhotonView(viewID).gameObject;

        if (!pv.IsMine)
        {
            // ��� �����
            //audioSource.clip = BlackholeAudio[1]; //����� ��� 
            //audioSource.Play();

            // �ı� ���ѵ� �Ǵ��� ����
            bool bCanDestroy = true;

            // ���̽� 
            {
                if (hitObj.GetComponent<MLEffect>() != null) //������ �ְ���
                {
                    absorbedDamage += (int)hitObj.GetComponent<MLEffect>().damage;
                }
                else if (hitObj.GetComponent<MREffect>() != null) // ������ ��������
                {
                    absorbedDamage += (int)hitObj.GetComponent<MREffect>().damage;
                }
                else if (hitObj.GetComponent<QBullet>() != null) // ���̽� QSkill
                {
                    absorbedDamage += (int)hitObj.GetComponent<QBullet>().damage;
                }
                else
                {
                    bCanDestroy = false;
                }
            }


            // Rora
            {
                if (hitObj.GetComponent<ManaEffect>() != null) // �ζ� F��ų
                {
                    absorbedDamage += hitObj.GetComponent<ManaEffect>().damage;
                }
                else
                {
                    bCanDestroy = false;
                }
            }

            // �ε�ģ ������Ʈ�� �ı��ϰų� ��Ȱ���Ѵ�.
            if (bCanDestroy)    PhotonNetwork.Destroy(hitObj);
        }
    }

    public void Absorb(float damage)
    {
        pv.RPC("RPC_Absorb", RpcTarget.AllBuffered, damage);
    }

    [PunRPC]
    private void RPC_Absorb(float damage)
    {
        absorbedDamage += damage;
    }

    // ��Ȧ ����
    private void Disappear()
    {
        float cur_size = transform.localScale.x;
        if (cur_size >= 0.0f)
        {
            cur_size -= Time.deltaTime * DisappearSpeed;
            transform.localScale = new Vector3(cur_size, cur_size, cur_size);
        }
    }
}
