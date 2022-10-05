using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MREffect : ObjectWithHP
{
    [Tooltip("ü��")] public int defaultHp = 3;
    [Tooltip("������")] public float defaultDamage = 60f;
    [Tooltip("(Look)������")] public float damage;
    [Tooltip("�⺻ Ÿ�� ����Ʈ")] public GameObject hiteffect;
    [Tooltip("�� Ÿ�� ����Ʈ")] public GameObject hitbodyeffect;
    [Tooltip("������ ������ ���ɿ���->�ݶ��̴��� �������� �� �ѹ� �浹�� �ѹ��� ������ �ֱ�����")] public bool damageable;
    public PhotonView pv;
    float speed;
    RaycastHit rayHit;
    GameObject hitObj;
    Collision other;

    void OnEnable()
    {
        pv = GetComponent<PhotonView>();
        if (pv.IsMine)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        hp = defaultHp;
        damage = defaultDamage;
        damageable = true;
        speed = transform.root.gameObject.GetComponent<MRBullet>().speed;
    }

    void Update()
    {
        SetInActive_TP();
    }

    private void OnCollisionEnter(Collision collision)
    {
        other = collision;

        // �Ѿ��� pv�� �ʱ�ȭ���� �ʾҴٸ� �ʱ�ȭ��Ų��.
        if(pv == null)
            pv = GetComponent<PhotonView>();
        damage = defaultDamage;

        // ���ÿ����� �ǰ� ������ �����Ѵ�.
        if(!pv.IsMine)
            return;

        // ���� ������Ʈ�� ���Ѵ�.
        hitObj = collision.transform.root.gameObject;
        PhotonView pv_other = hitObj.GetComponent<PhotonView>();

        // ���� ������Ʈ�� ���� ������Ʈ��� �Ѿ��� �ı��ϰ� ������.
        if (pv_other == null)
        {
            Destroy(gameObject);
            return;
        }

        // pv �������� ���� ������ ������̶�� �������� ������.
        if (pv_other.Controller != pv.Owner)
        {
            if (!damageable) return;
            damageable = false;
            InstanceEffect(collision);
            pv.RPC("RPC_Damage", RpcTarget.AllBuffered, pv_other.ViewID); 
            TakeDamage(defaultHp);
        }
    }

    void InstanceEffect(Collision collision)//����Ʈ ��� & ������ ����
    {
        if (collision.GetContact(0).otherCollider.gameObject.CompareTag("Head") ||
            collision.GetContact(0).otherCollider.gameObject.CompareTag("Body") ||
            collision.GetContact(0).otherCollider.gameObject.CompareTag("Dummy"))
        {
            Instantiate(hitbodyeffect, collision.GetContact(0).point, Quaternion.identity);
        }
        Instantiate(hiteffect, collision.GetContact(0).point, Quaternion.identity);

       // Debug.Log("RAttack Hit : " + collision.transform.root.name + " -> " + collision.transform.name);
    }

    [PunRPC]
    void RPC_Damage(int viewID)//������ �ο�
    {
        // ������ ��������, ������ �Ѿ����� ������ �������� �ش�.
        GameObject hitObj = PhotonNetwork.GetPhotonView(viewID).gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        if (hitPlayer != null)//ĳ����
        {
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

        // �ñر� �������� ä���
        Casey owner = transform.parent.GetComponent<MRBullet>().Owner.GetComponent<Casey>();
        if (owner.pv.IsMine)
        {
            owner.UpdateUltgauge((int)damage);
        }
    }

    void SetInActive_TP()
    {
        if (hp <= 0)
            Destroy(gameObject);
    }

}