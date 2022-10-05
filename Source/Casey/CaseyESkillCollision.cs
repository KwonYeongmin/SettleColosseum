using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CaseyESkillCollision : MonoBehaviour
{
    //���Ծȿ� ���� ������Ʈ�� �������� �Ǵ�
    [Header("������ ��ų")]
    Vector3 dir;
    [Tooltip("��� ������� �ӵ�")] public float speed = 12.0f;
    [Tooltip("������ �Ÿ�")] public float keepDistance = 10.0f;
    [Tooltip("(Look)���� ����� �Ÿ�")] public float Distance;
    [Tooltip("E������")] public int damage = 10;
    [Tooltip("������ ������ ���ɿ���->�ݶ��̴��� �������� �� �ѹ� �浹�� �ѹ��� ������ �ֱ�����")] public bool damageable;
    
    public PlayerAudio audioManager;
    
    GameObject casey;
    private GameObject hitObj;

    void Start()
    {

        casey = transform.root.gameObject;
        damageable = true;
    }

    private void OnTriggerEnter(Collider collider)
    {
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
            if (damageable)
            {
                Damage(collider);
                damageable = false;
            }
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        //if (collider.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
        //    collider.gameObject.layer == LayerMask.NameToLayer("Dummy"))
        //{
        //    Distance = Vector3.Distance(collider.transform.position, casey.transform.position);
        //    if (Vector3.Distance(collider.transform.position, casey.transform.position) < keepDistance) return;
        //    dir = GameObject.Find("CaseyController").transform.forward;

        //    MoveEnemy(collider);
        //}
    }

    private void OnTriggerEnd(Collider collider)
    {
        damageable = true;
    }


    void Damage(Collider collider)//������ �ο�
    {
        // ������ ��������, ������ �Ѿ����� ������ �������� �ش�.
        GameObject owner = transform.root.gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        if (hitPlayer != null)//ĳ����
        {
            //����� ���
            if (hitPlayer.GetComponent<AudioSource>() != null)
            {
                hitPlayer.GetComponent<AudioSource>().clip = audioManager.InstanceClip[0];
                hitPlayer.gameObject.GetComponent<AudioSource>().Play();
            }
            //

            if (hitObj.GetComponent<Casey>() != null)//���̽�
            {
                Casey player = hitObj.GetComponent<Casey>();

                // �ǵ带 ���� �ִ� ���̶�� ������ �ʰ� ó���Ѵ�.
                if(player.barrierHp <= float.Epsilon)   player.Dragged(owner);
                player.TakeDamage_Sync((int)damage);
            }
            else//�ζ�
            {
                Rora player = hitObj.GetComponent<Rora>();

                Debug.Log("Dragger: " + owner);
                player.Dragged(owner);
                player.TakeDamage_Sync((int)damage);
            }
        }
        else if (collider.gameObject.layer == LayerMask.NameToLayer("Dummy"))//����
        {
            collider.gameObject.transform.root.GetComponent<DummyMonster>().TakeDummyDamage((int)damage);
        }

        // �ñر� �������� ä���
        owner.GetComponent<Casey>().UpdateUltgauge((int)damage);
    }

    void MoveEnemy(Collider collider)
    {
        collider.transform.root.position -= dir * speed * Time.deltaTime;
    }
    

    void PlayEnemySound(Collision collision)
    {
        //  << ����� 
        if ((collision.GetContact(0).otherCollider.tag == "Head") || (collision.GetContact(0).otherCollider.tag == "Body"))
        {

        }
        // >> 
    }
}