using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class QExplosion : MonoBehaviour
{
    [Tooltip("���ߵ�����")] public float explosionDamage = 40.0f;
    [HideInInspector] public PhotonView pv;
    private GameObject hitObj;
    Collision other;

    void Start()
    {
        // pv �ʱ�ȭ
        pv = GetComponent<PhotonView>();
    }
    
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        other = collision;

        // �Ѿ��� pv�� �ʱ�ȭ���� �ʾҴٸ� �ʱ�ȭ��Ų��.
        if (pv == null)
            pv = GetComponent<PhotonView>();

        // ���ÿ����� �ǰ� ������ �����Ѵ�.
        if (!pv.IsMine)
            return;

        // ���� ������Ʈ�� ���Ѵ�.
        hitObj = collision.transform.root.gameObject;
        PhotonView pv_hit = hitObj.GetComponent<PhotonView>();

        Debug.Log("QExplosion Hit Obj: " + hitObj.name);
        Debug.Log("QExplosion Hit PV: " + pv_hit);

        pv.RPC("RPC_Damage", RpcTarget.AllBuffered, pv_hit.ViewID);
    }

    [PunRPC]
    void RPC_Damage(int viewID)//������ �ο�
    {
        // ������ ��������, ������ �Ѿ����� ������ �������� �ش�.
        GameObject hitObj = PhotonNetwork.GetPhotonView(viewID).gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        if (hitPlayer != null)//ĳ����
        {
            Debug.Log("Damage: " + explosionDamage);
            if (hitObj.GetComponent<Casey>() != null)//���̽�
            {
                hitObj.GetComponent<Casey>().TakeDamage((int)explosionDamage);
            }
            else//�ζ�
            {
                hitObj.GetComponent<Rora>().TakeDamage((int)explosionDamage);
            }
        }
        else//����ü
        {
            if (other.gameObject.transform.root.GetComponent<ObjectWithHP>())
                other.gameObject.transform.root.GetComponent<ObjectWithHP>().TakeDamage((int)explosionDamage);
        }
    }
}
