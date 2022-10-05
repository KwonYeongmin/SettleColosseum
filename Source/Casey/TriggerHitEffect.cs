using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TriggerHitEffect : MonoBehaviour
{
    [Tooltip("Ÿ�� ����Ʈ")] public GameObject hitEffect;
    [Tooltip("(Look)������")] public float damage;
    [Tooltip("������ ������ ���ɿ���->�ݶ��̴��� �������� �� �ѹ� �浹�� �ѹ��� ������ �ֱ�����")] public bool damageable;

    Casey casey;
    [HideInInspector] PlayerAudio AudioManager;

    private GameObject hitObj;
    private Collider other;

    void Start()
    {
        damageable = true;
    }

    private void OnTriggerEnter(Collider collider)
    {
        other = collider;

        // �����ϴ� �ڱ� �ڽ��� ���Ѵ�.
        PhotonView pv_mine = transform.root.GetComponent<PhotonView>();

        // ���� ������Ʈ�� ���Ѵ�.
        hitObj = collider.transform.root.gameObject;
        PhotonView pv_other = hitObj.GetComponent<PhotonView>();

        // ���� ������Ʈ�� ���� ������Ʈ��� �׳� ������.
        if (pv_other == null)
        {
            return;
        }

        if (pv_mine != pv_other)
        {
            if(!damageable) return;
            damageable = false;
            InstanceEffect(collider);
            Damage(hitObj);
        }
    }

    private void OnTriggerEnd(Collider collider)
    {
        damageable = true;
    }

    void InstanceEffect(Collider collider)//����Ʈ ��� & ������ ����
    {
        if (collider.CompareTag("Body") || collider.CompareTag("Dummy"))
        {
            Instantiate(hitEffect, collider.transform.position, Quaternion.identity);
            {
                if(transform.gameObject.name== "VAttackHitBox")
                    damage = transform.root.gameObject.GetComponent<Casey>().Vdamage;
                else
                    damage = transform.root.gameObject.GetComponent<Casey>().dash_damage;
            }
        }
    }

    void Damage(GameObject hitObj)//������ �ο�
    {
        // ������ ��������, ������ �Ѿ����� ������ �������� �ش�.
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        if (hitPlayer != null)//ĳ����
        {
            Debug.Log("Damage: " + damage);
            if (hitObj.GetComponent<Casey>() != null)//���̽�
            {
                hitObj.GetComponent<Casey>().TakeDamage_Sync((int)damage);
            }
            else//�ζ�
            {
                hitObj.GetComponent<Rora>().TakeDamage_Sync((int)damage);
            }
        }

        transform.root.GetComponent<Casey>().UpdateUltgauge((int)damage);
    }

}