using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Turret : CachedObject
{
    // ������Ʈ
    [HideInInspector] public PhotonView pv;

    // Transform
    public float Size = 1;

    // ���� ����
    private float LifeTime = 5;
    private float damage = 5f;
    private float DamageInterval = 0.25f;
    private float SlowValue = 0.8f;
    private float KeepSlowTime = 3.0f;

    // ���� ����
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

        // ��ó�� �� ���� ��ž�� ������ ��ġ�� �������ش�.
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
        // ���ÿ����� Update�� �����Ѵ�.
        if (!pv.IsMine)
            return;

        // ���͹� ���� ������ �ֱ�
        CheckDamageToEnemy();

        // Ȱ��ȭ Ÿ�̸� ���� �� üũ
        lifeTimer += Time.deltaTime;
        if(lifeTimer > LifeTime)
            PhotonNetwork.Destroy(gameObject);
    }

    void CheckDamageToEnemy()
    {
        // Ÿ�̸� ����
        timer += Time.deltaTime;

        // Interval���� ���ظ� �ش�.
        if (timer > DamageInterval)
        {
            timer = 0f;

            // �ֺ� �������� �΋Hģ �ݶ��̴��� ���� ��� ����� ���ܽ�Ų��.
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
