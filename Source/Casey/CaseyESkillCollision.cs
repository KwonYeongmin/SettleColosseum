using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CaseyESkillCollision : MonoBehaviour
{
    //원뿔안에 들어온 오브젝트가 무엇인지 판단
    [Header("끌어당김 스킬")]
    Vector3 dir;
    [Tooltip("상대 끌어당기는 속도")] public float speed = 12.0f;
    [Tooltip("유지할 거리")] public float keepDistance = 10.0f;
    [Tooltip("(Look)현재 상대방과 거리")] public float Distance;
    [Tooltip("E데미지")] public int damage = 10;
    [Tooltip("데미지 입히기 가능여부->콜라이더가 여러개일 때 한번 충돌시 한번만 데미지 주기위함")] public bool damageable;
    
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
        // 공격하는 자기 자신을 구한다.
        PhotonView pv_mine = transform.root.GetComponent<PhotonView>();

        // 맞은 오브젝트를 구한다.
        hitObj = collider.transform.root.gameObject;
        PhotonView pv_other = hitObj.GetComponent<PhotonView>();

        // 맞은 오브젝트가 정적 오브젝트라면 그냥 끝낸다.
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


    void Damage(Collider collider)//데미지 부여
    {
        // 맞은게 상대방인지, 상대방의 총알인지 구분해 데미지를 준다.
        GameObject owner = transform.root.gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        if (hitPlayer != null)//캐릭터
        {
            //오디오 재생
            if (hitPlayer.GetComponent<AudioSource>() != null)
            {
                hitPlayer.GetComponent<AudioSource>().clip = audioManager.InstanceClip[0];
                hitPlayer.gameObject.GetComponent<AudioSource>().Play();
            }
            //

            if (hitObj.GetComponent<Casey>() != null)//케이시
            {
                Casey player = hitObj.GetComponent<Casey>();

                // 실드를 쓰고 있는 중이라면 끌리지 않게 처리한다.
                if(player.barrierHp <= float.Epsilon)   player.Dragged(owner);
                player.TakeDamage_Sync((int)damage);
            }
            else//로라
            {
                Rora player = hitObj.GetComponent<Rora>();

                Debug.Log("Dragger: " + owner);
                player.Dragged(owner);
                player.TakeDamage_Sync((int)damage);
            }
        }
        else if (collider.gameObject.layer == LayerMask.NameToLayer("Dummy"))//더미
        {
            collider.gameObject.transform.root.GetComponent<DummyMonster>().TakeDummyDamage((int)damage);
        }

        // 궁극기 게이지를 채운다
        owner.GetComponent<Casey>().UpdateUltgauge((int)damage);
    }

    void MoveEnemy(Collider collider)
    {
        collider.transform.root.position -= dir * speed * Time.deltaTime;
    }
    

    void PlayEnemySound(Collision collision)
    {
        //  << 오디오 
        if ((collision.GetContact(0).otherCollider.tag == "Head") || (collision.GetContact(0).otherCollider.tag == "Body"))
        {

        }
        // >> 
    }
}