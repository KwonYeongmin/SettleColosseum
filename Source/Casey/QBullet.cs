using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class QBullet : ObjectWithHP
{
    [Header("Q�̻���")]
    [Tooltip("�ӵ�")] public float speed = 5.0f;
    [Tooltip("������Ÿ��")] public float defaultLifetime = 10.0f;
    [Tooltip("(Look)������Ÿ��")] public float lifeTime = 0f;
    [Tooltip("ü��")] public int defaultHp = 10;
    [Tooltip("�浹������")] public float crashDamage = 50.0f;
    [Tooltip("(Look)������")] public float damage;

    //[Tooltip("�ѱ�����Ʈ")] public GameObject muzzleEffect;
    //[Tooltip("��������Ʈ")] public GameObject tailEffect;
    [Tooltip("��������Ʈ")] public GameObject hitEffect;
    [Tooltip("�� Ÿ�� ����Ʈ")] public GameObject hitbodyEffect;
    [HideInInspector] public PhotonView pv;

    [Header("audio")]
    public AudioClip[] audioClips;
    private AudioSource audioSource;

    public GameObject owner;
    private GameObject hitObj;
    Collision other;

    Vector3 dir;

    void OnEnable()
    {
        // pv �ʱ�ȭ
        pv = GetComponent<PhotonView>();

        // ����� �ҽ� �ʱ�ȭ
        audioSource = this.GetComponent<AudioSource>();
        // ����� ���
        PlayQSkillAudio(0);


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
        if (owner == null) return;

        GameObject target = owner.transform.GetChild(3).GetChild(1).gameObject;
        dir = target.transform.forward;
        transform.LookAt(target.transform);
        hp = defaultHp;
        damage = crashDamage;
    }

    void Update()
    {
        lifeTime += Time.deltaTime;
        GetComponent<Rigidbody>().AddForce(dir * speed);
        //transform.position += dir * speed * Time.deltaTime;
        if (lifeTime >= defaultLifetime || hp <= 0)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        other = collision;

        // �Ѿ��� pv�� �ʱ�ȭ���� �ʾҴٸ� �ʱ�ȭ��Ų��.
        if(pv == null)
            pv = GetComponent<PhotonView>();
        damage = crashDamage;

        // ���ÿ����� �ǰ� ������ �����Ѵ�.
        if(!pv.IsMine)
            return;

        // ���� ������Ʈ�� ���Ѵ�.
        hitObj = collision.transform.root.gameObject;
        PhotonView pv_other = hitObj.GetComponent<PhotonView>();

        Debug.Log("Hit Obj: " + hitObj.name);
        Debug.Log("Hit PV: " + pv_other);

        // ���� ������Ʈ�� ���� ������Ʈ��� ���� �����ո� ����ѵ� �Ѿ��� �ı��ϰ� ������.
        if (pv_other == null)
        {
            Debug.Log("Hello!");
            PhotonNetwork.Instantiate(
                "PhotonPrefabs/CaseyInstance/BigExplosion", collision.GetContact(0).point, Quaternion.identity);
            Destroy(gameObject);
            return;
        }

        // pv �������� ���� ������ ������̶�� �������� ������.
        if (pv_other.Controller != pv.Owner)
        {
            InstanceEffect(collision);
            pv.RPC("RPC_Damage", RpcTarget.AllBuffered, pv_other.ViewID);
            TakeDamage(defaultHp);
            PlayQSkillAudio(1);
        }
    }

    void InstanceEffect(Collision collision)//����Ʈ ��� & ������ ����
    {
        if (collision.GetContact(0).otherCollider.gameObject.CompareTag("Head") ||
            collision.GetContact(0).otherCollider.gameObject.CompareTag("Body") ||
            collision.GetContact(0).otherCollider.gameObject.CompareTag("Dummy"))
        {
            Instantiate(hitbodyEffect, collision.GetContact(0).point, Quaternion.identity);
        }
        Instantiate(hitEffect, collision.GetContact(0).point, Quaternion.identity);

        Debug.Log("QSkill Hit : " + collision.transform.root.name + " -> " + collision.transform.name);
    }

    [PunRPC]
    void RPC_Damage(int viewID)//������ �ο�
    {
        // ������ ��������, ������ �Ѿ����� ������ �������� �ش�.
        GameObject hitObj = PhotonNetwork.GetPhotonView(viewID).gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        if (hitPlayer != null)//ĳ����
        {
            //Debug.Log("Damage: " + damage);
            if (hitObj.GetComponent<Casey>() != null)//���̽�
            {
                hitObj.GetComponent<Casey>().TakeDamage((int)damage);
            }
            else//�ζ�
            {
                hitObj.GetComponent<Rora>().TakeDamage((int)damage);
            }
        }
        else//����ü
        {
            if (other.gameObject.transform.root.GetComponent<ObjectWithHP>())
                other.gameObject.transform.root.GetComponent<ObjectWithHP>().TakeDamage((int)damage);
        }
    }

    private void PlayQSkillAudio(int index)
    {
        audioSource.clip = audioClips[index];
        audioSource.Play();
    }
}