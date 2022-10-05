using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Itempack : MonoBehaviour
{
    public int healPoint = 50;//�÷��̾� ü�� 50�ø�
    public int shieldPoint = 15;//�� ��ġ 15�ø�
    public float ultPoint = 20;//�ñر� ������ 20%�ø�
    
    [Tooltip("����Ʈ")] public GameObject aura;

    [Tooltip("�ѹ��� ȿ�� �ֱ����� ����")] public bool effectable;

    [HideInInspector] public PhotonView pv;
    //������ �Դ� ȿ��
    //����Ʈ1������

    GameObject effect;
    GameObject hitObj;

    void Start()
    {
        effectable = true;
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
    }

    private void OnTriggerEnter(Collider collider)
    {
        hitObj = collider.transform.root.gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();

        if (hitPlayer == null) return;

        if (!effectable) return;

        InstanceEffect(hitObj.transform.position);
        effectable = false;
    }

    private void OnTriggerExit(Collider collider)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        effectable = true;
        hitObj = null;
    }
    
    void InstanceEffect(Vector3 pos)//����Ʈ ��� & ������ ����
    {
        Debug.Log(gameObject.name);
        effect = Instantiate(aura, pos, Quaternion.identity);
        effect.transform.SetParent(hitObj.transform);
        if (gameObject.name == "HealPack(Clone)")
            HealPack();
        else if (gameObject.name == "ShieldPack(Clone)")
            ShieldPack();
        else if (gameObject.name == "UltPack(Clone)")
            UltPack();
        effectable = true;
        hitObj = null;
        Destroy(gameObject);
    }

    [PunRPC]
    void RPC_ItemInactive()//������ �ο�
    {
        effectable = true;
        hitObj = null;
        this.gameObject.SetActive(false);
    }

    void HealPack()
    {
        if (hitObj.GetComponent<Casey>() != null)//���̽�
            hitObj.GetComponent<Casey>().IncreaseHP_Sync(healPoint);
        else//�ζ�
            hitObj.GetComponent<Rora>().IncreaseHP_Sync(healPoint);
    }

    void ShieldPack()
    {
        if (hitObj.GetComponent<Casey>() != null)//���̽�
            hitObj.GetComponent<Casey>().RPC_IncreaseShield(shieldPoint);
        else
            hitObj.GetComponent<Rora>().RPC_IncreaseShield(shieldPoint);
    }

    void UltPack()
    {
        if (hitObj.GetComponent<Casey>() != null)//���̽�
            hitObj.GetComponent<Casey>().UpdatePerUltgauge(ultPoint);
        else//�ζ�
            hitObj.GetComponent<SkillControl>().GetQskill().IncreaseCurCooltime(ultPoint);
    }
}
