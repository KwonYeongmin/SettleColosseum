using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Turret : CachedObject
{
    // 컴포넌트
    [HideInInspector] public PhotonView pv;

    // Transform
    public float Size = 1;

    // 관련 변수
    private float LifeTime = 5;
    private float damage = 5f;
    private float DamageInterval = 0.25f;
    private float SlowValue = 0.8f;
    private float KeepSlowTime = 3.0f;

    // 내부 변수
    private float timer;
    private float lifeTimer;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
    }

    void OnEnable()
    {
        pv = this.GetComponent<PhotonView>();
        GetComponent<Transform>().localScale = new Vector3(Size, Size, Size);
        timer = 0f;
        lifeTimer = 0f;

        // 근처의 땅 위에 포탑이 박히게 위치를 조정해준다.
        RaycastHit hit;
        LayerMask layerMask = ~(1 << LayerMask.NameToLayer("Player"));
        if (Physics.Raycast(
            transform.position, 
            ((transform.position - transform.up) - transform.position).normalized, 
            out hit, 50, layerMask))
        {
            transform.position = hit.point;
        }
        else if(Physics.Raycast(
            transform.position,
            ((transform.position + transform.up) - transform.position).normalized,
            out hit, 10, layerMask))
        {
            transform.position = hit.point;
        }
    }

    public void InitTurret(float lifeTime, float damage, float damageInterval, float slowValue, float keepSlowTime)
    {
        this.LifeTime = lifeTime;
        this.damage = damage;
        this.DamageInterval = damageInterval;
        this.SlowValue = slowValue;
        this.KeepSlowTime = keepSlowTime;
    }

    void Update()
    {
        // 로컬에서만 Update를 실행한다.
        if (!pv.IsMine)
            return;

        // 인터벌 마다 데미지 주기
        CheckDamageToEnemy();

        // 활성화 타이머 증가 및 체크
        lifeTimer += Time.deltaTime;
        if(lifeTimer > LifeTime)
            PhotonNetwork.Destroy(gameObject);
    }

    void CheckDamageToEnemy()
    {
        // 타이머 증가
        timer += Time.deltaTime;

        // Interval마다 피해를 준다.
        if (timer > DamageInterval)
        {
            timer = 0f;

            // 주변 원형으로 부딫친 콜라이더를 얻어와 몇몇 대상을 제외시킨다.
            List<Collider> colliders = Physics.OverlapSphere(transform.position, 4.0f).ToList();
            colliders.RemoveAll(col => col.gameObject.layer == LayerMask.NameToLayer("Player"));
            colliders.RemoveAll(col => col.transform.root.GetComponent<PhotonView>() == null);

            for(int i = 0; i < colliders.Count; i++)
            {
                Playable enemy = colliders[i].transform.root.GetComponent<Playable>();
                if(enemy == null)   continue;
                if (enemy.GetComponent<PhotonView>().IsMine) continue;

                enemy.TakeDamage_Sync((int)damage);
                enemy.SetSlowValue(SlowValue, KeepSlowTime);
                return;
            }
        }
    }
}
