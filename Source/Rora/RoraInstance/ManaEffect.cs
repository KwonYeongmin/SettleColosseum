using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ManaEffect : ObjectWithHP
{
    //==========================Effect==========================
    public GameObject impactParticle;
    public GameObject[] PointEffect;

    //========================== ��Ÿ ���� ==========================
    [HideInInspector] public GameObject owner;
    private bool bIsDamaged = false;
    private Collider other;
    private GameObject hitObj;

    //========================== Mana ����ü �� ==========================
    [Header("����ü HP ���� ����")]
    public int DefaultHP = 1;
    public float HPDamage = 1f;

    [Header("����ü ������, ��� ��� ����")]
    public int defaultDamage;
    [Range(0.0f, 2.0f)]
    public float Head_coef;
    [HideInInspector] public int damage;

    //========================== ���� ���� ============================
    Vector3 origin;
    Vector3 dir;
    public float Speed;

    //========================== ���� �ð� ============================
    float lifeTime = 0f;
    public float defaultLifetime = 10f;

    //========================== ���� ���� ============================
    [HideInInspector] public PhotonView pv;

    //========================== ����� ���� ==========================
    [HideInInspector] public PlayerAudio audioManager;
    private AudioSource audioSource;

    void OnEnable()
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

        // ��ġ�� �ʱ�ȭ�Ѵ�.
        transform.position = owner.transform.GetChild(1).position + (owner.transform.GetChild(1).forward * 3f);

        // �Ѿ��� ���ư� ������ ���Ѵ�.
        dir = owner.transform.GetChild(1).GetComponent<Camera>().transform.forward;

        // �Ϻ� ���̾ ����ĳ��Ʈ���� �����Ѵ�.
        int layerMask = (1 << 11) + (1 << 12) + (1 << 14);
        layerMask = ~layerMask;

        audioSource = this.GetComponent<AudioSource>();
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        hp = DefaultHP;
        damage = defaultDamage;
        bIsDamaged = false;
    }

    void Update()
    {
        // ���� �ð��� ���� �����ٸ� �ı��Ѵ�.
        lifeTime += Time.deltaTime;
        if (lifeTime >= defaultLifetime)
            Destroy(gameObject);

        // �Ѿ��� ���ư��� �Ѵ�.
        Trajectory_TP();
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("FSkill Collision With: " + collider.name);

        // �ݸ��� �ʱ�ȭ
        other = collider;

        //damage �ʱ�ȭ
        damage = defaultDamage;

        //������ �ڱ� �ڽ��̶�� �����Ѵ�.
        if(collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            return;

        //���� ������Ʈ ���ϱ�
        hitObj = collider.transform.gameObject;
        PhotonView pv_other = hitObj.GetComponent<PhotonView>();

        //���� ������Ʈ�� ���� ������Ʈ(������ ������ ������Ʈ�� �ƴ�)��� �Ѿ��� �ı��ϰ� ������.
        if (pv_other == null) { Destroy(this.gameObject); return; }

        if (bIsDamaged) return; //�̹� ���� ������Ʈ��� ����

        bIsDamaged = true;

        audioSource.Play();

        InstantiateEffect(collider); //����Ʈ ���� �� ������ ���
        pv.RPC("RPC_GiveDamage", RpcTarget.AllBuffered, pv_other.ViewID); //HP ����ȭ
        CheckProjectileHP();
        //PlayEnemyAudio(collision);
    }


    private void CheckProjectileHP()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);

            ParticleSystem[] trails = GetComponentsInChildren<ParticleSystem>();
            //Component at [0] is that of the parent i.e. this object (if there is any)
            for (int i = 1; i < trails.Length; i++)
            {
                ParticleSystem trail = trails[i];
                if (!trail.gameObject.name.Contains("Trail"))
                    continue;

                trail.transform.SetParent(null);
                Destroy(trail.gameObject, 2);
            }
        }
        
    }

    // ������ ������
    [PunRPC]
    void RPC_GiveDamage(int ViewID)
    {
        GameObject hitObj = PhotonNetwork.GetPhotonView(ViewID).gameObject;
        Playable hitplayer = this.hitObj.GetComponent<Playable>();

        if (hitplayer != null)// ĳ���Ͷ��
        {
            if (hitObj.CompareTag("BlackHole"))    // ����� ��Ȧ�� ���
            {
                hitObj.GetComponent<BlackHole>().Absorb(damage);
                Debug.Log("Hit Absorbed Damage: " + hitObj.GetComponent<BlackHole>().absorbedDamage);
                return;
            }

            if (hitObj.GetComponent<Casey>() != null) hitObj.GetComponent<Casey>().TakeDamage((int)damage); //���̽ö��
            if (hitObj.GetComponent<Rora>() != null) hitObj.GetComponent<Rora>().TakeDamage((int)damage); //�ζ���
            Destroy(gameObject);
        }
        else // ����ü���
        {
            if (other.gameObject.transform.root.GetComponent<ObjectWithHP>() != null)
                other.gameObject.transform.root.GetComponent<ObjectWithHP>().TakeDamage((int)damage);
        }
    }

    // ����Ʈ ���� �� ������ ���� �� ����� ����
    void InstantiateEffect(Collider collider)
    {
        GameObject effect = null;

        effect = Instantiate(
            impactParticle, 
            collider.transform.position, 
            Quaternion.identity);

        if (collider.gameObject.CompareTag("Head"))
        {
            damage = defaultDamage + Mathf.RoundToInt(defaultDamage * Head_coef);
        }
       else if (collider.gameObject.CompareTag("Body"))
        {
            damage = defaultDamage;
        }
        Destroy(effect, 1f);
    }

    // ����� �÷���
    /*
    void PlayEnemyAudio(Collision collision)
    {
        if (!collision.collider.gameObject.GetComponent<AudioSource>())
        {
        collision.collider.gameObject.AddComponent<AudioSource>().clip = audioManager.InstanceClip[1];
        collision.collider.gameObject.GetComponent<AudioSource>().Play(); //���
        }
        else
        {
        collision.collider.gameObject.GetComponent<AudioSource>().clip = audioManager.InstanceClip[1];
        collision.collider.gameObject.GetComponent<AudioSource>().Play(); //���
        }
    }*/

    void Trajectory_TP()
    {
        transform.position += dir * Speed * Time.deltaTime;
    }
}


