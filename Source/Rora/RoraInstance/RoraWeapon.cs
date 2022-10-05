using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoraWeapon : MonoBehaviour
{
    //================================ ����Ʈ ============================
    [Header("����Ʈ")]
    public GameObject[] PointEffects;



    //================================ �������� ���� ============================
    [Header("�������� ������")]
    public int DefaultDamage;
    [HideInInspector] public int Damage;
    [Range(0.0f, 1.0f)]
    public float Head_coef = 0.5f;

    //================================ ��Ÿ����============================

    [HideInInspector] public bool bOnVAttack = false;
    [HideInInspector] public bool bIsDamaged = false;
    private GameObject HitObj;
    private Collision HitBox;



    //================================ ���� ============================
    [HideInInspector] public PhotonView pv;



    //================================ audio ============================
    [HideInInspector] public PlayerAudio AudioManager;


    void Start()
    {
        Damage = DefaultDamage;
        bOnVAttack = false;
        bIsDamaged = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // collision �ʱ�ȭ
        HitBox = collision;

        // �����ϴ� �ڽ� ���ϱ�
        HitObj = collision.transform.gameObject;
        PhotonView pv_other = HitObj.GetComponent<PhotonView>();

        if (pv_other == null) return;

        if (pv != pv_other)
        {
            // �̹� �¾����� ������
            if (bIsDamaged) return;
            bIsDamaged = true;

            InstaniateEffect(collision); //����Ʈ ����, ������ ���
            GiveDamage(HitObj);
        }
    }


    // ������ ������
    void GiveDamage(GameObject hitObj)
    {
        Playable hitplayer = hitObj.GetComponent<Playable>();

        if (hitplayer.gameObject.transform.root.GetComponent<Casey>() != null)
        {
            hitplayer.gameObject.transform.root.GetComponent<Casey>().TakeDamage_Sync((int)Damage);
        }
        else if (hitplayer.gameObject.transform.root.GetComponent<Rora>() != null)
        {
            hitplayer.gameObject.transform.root.GetComponent<Rora>().TakeDamage_Sync((int)Damage);
        }
    }

    // ����Ʈ ����, damage ���
    void InstaniateEffect(Collision collision)
    {
        GameObject effect = null;
        if (collision.GetContact(0).thisCollider.transform.tag == "Head")
        {
             effect = Instantiate(PointEffects[0], collision.GetContact(0).point, Quaternion.identity);
            Damage = DefaultDamage + Mathf.RoundToInt(DefaultDamage * Head_coef);

            // Head ���� �� audio ���
            /*
            if (collision.collider.gameObject.transform.root.GetComponent<PlayerAudio>() != null)
            {
                collision.collider.gameObject.transform.root.GetComponent<PlayerAudio>().PlayheadShot();
            }*/

        }
        else if (collision.GetContact(0).thisCollider.transform.tag == "Body")
        {
             effect = Instantiate(PointEffects[1], collision.GetContact(0).point, Quaternion.identity);
            Damage = DefaultDamage;
        }
        
        Destroy(effect, 1f);
    }

    // ����� �÷���
    /*
    void PlayEnemyAudio(Collision other)
    {
        if (!other.collider.gameObject.GetComponent<AudioSource>())
        {
            other.collider.gameObject.AddComponent<AudioSource>().clip = AudioManager.InstanceClip[1];
            other.collider.gameObject.GetComponent<AudioSource>().Play(); //���
        }
        else
        {
            other.collider.gameObject.GetComponent<AudioSource>().clip = AudioManager.InstanceClip[1];
            other.collider.gameObject.GetComponent<AudioSource>().Play(); //���
        }
    }*/
}


