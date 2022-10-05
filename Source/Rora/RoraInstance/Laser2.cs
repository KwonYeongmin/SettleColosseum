using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class Laser2 : ObjectWithHP
{
    //==========================Effect==========================
    public GameObject HitEffect;
    [HideInInspector] private ParticleSystem[] Effects;
    [HideInInspector] private ParticleSystem[] Hit;
    [HideInInspector] public float HitOffset = 0;
    [HideInInspector] public bool useLaserRotation = false;

    //[Header("����Ʈ")]
    [HideInInspector] public GameObject[] PointEffect;

    //[Header("������")]
    private LineRenderer laser;
    [HideInInspector] public float laserTransparentValue = 1f;

    [HideInInspector] public float MainTextureLength = 1f;
    [HideInInspector] public float NoiseTextureLength = 1f;
    private Vector4 Length = new Vector4(1, 1, 1, 1);
    private bool LaserSaver = false;
    private bool UpdateSaver = false;

    public float MaxLength;

    //========================== ��Ÿ ���� ==========================
    private Transform transform;
    private Collision Hitbox;
    private GameObject HitObj;
    private bool bIsDamaged = false;

    //========================== RAttack ����ü �� ==========================
    [Header("����ü HP ���� ����")]
    private int DefaultHP =1;
    public float HPDamage = 1f;
    
    [Header("����ü ������, ��� ��� ����")]
    public int DefaultDamage;
    [HideInInspector] public int damage;
    [HideInInspector] public float head_coef;

    [HideInInspector] public GameObject camObj;
    public bool bIsFP = false;      // 1��Ī ����
    private bool bLaserFired = true;

    private GameObject owner;
    private Vector3 vecToTarget;    // �߻� ��ġ���� ��ǥ ���������� ����
    public float xSpread;          // x�� ź ���� ����
    public float ySpread;          // y�� ź ���� ����

    private float LIFE_TIME = 0.2f;
    private float lifeTimer = 0f;

    //========================== ���溯�� ==========================
    [HideInInspector] public PhotonView pv;

    public void Shot()
    {
        Initialize();
        ShotBullet();
    }

    private void Initialize()
    {
        pv = GetComponent<PhotonView>();

        // HP �ʱ�ȭ
        hp = DefaultHP;

        // ���� �ʱ�ȭ
        transform = this.GetComponent<Transform>();
        bIsDamaged = false;

        // ������ �ʱ�ȭ
        damage = DefaultDamage;

        // ������ �� ����Ʈ �ʱ�ȭ
        laser = GetComponent<LineRenderer>();
        Effects = GetComponentsInChildren<ParticleSystem>();
        Hit = HitEffect.GetComponentsInChildren<ParticleSystem>();

        // 3��Ī�� ��� ������ ���� �Ѿ��� �� �÷��̾ ã�� ī�޶� ��ġ�� ���´�.
        if (camObj == null)
        {
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

            // 3��Ī ������ �߻� ��ġ�� �θ� ������Ʈ transform�� �����Ѵ�.
            camObj = owner.transform.GetChild(1).gameObject;
            transform.parent = camObj.transform;
            transform.position = camObj.transform.position + (camObj.transform.forward * 1.8f);
        }
        else
        {
            // 1��Ī�� ��� 1��Ī �÷��׸� true�� �ٲٰ� Owner�� �������ش�.
            bIsFP = true;
            owner = camObj.transform.root.gameObject;
        }

        // ���� �������� �����Ѵ�.
        damage = owner.GetComponent<SkillControl>().RAttack_Damage;
        head_coef = owner.GetComponent<SkillControl>().RAttack_HeadCoef;
    }

    private void ShotBullet()
    {
        if (laser != null && UpdateSaver == false)
        {
            laser.SetPosition(0, transform.position);

            vecToTarget = camObj.transform.position + (camObj.transform.forward * MaxLength);
            vecToTarget += camObj.transform.right * xSpread;
            vecToTarget += camObj.transform.up * ySpread;
            vecToTarget -= camObj.transform.position;

            // �ڱ� �ڽ��� ���� ���� �����Ѵ�.
            RaycastHit hit;
            int layerMask = (1 << LayerMask.NameToLayer("Player"));
            if (Physics.Raycast(transform.position, vecToTarget, out hit, MaxLength, ~layerMask))
            {
                Damage(hit);
                SetEndPositionWithCollider(hit);
            }
            else
            {
                SetEndPositionWithoutCollider();
            }

            if (laser.enabled == false && LaserSaver == false)
            {
                LaserSaver = true;
                laser.enabled = true;
            }
        }
    }


    void Update()
    {
        // �߻� ����Ʈ�� ���� ��ø� �����ش�
        if (lifeTimer > 0.1f && bLaserFired)
            transform.GetChild(0).gameObject.SetActive(false);

        // Ȱ��ȭ �ð��� �����ϰų�, �ð��� �����ٸ� ������Ʈ�� �ı��Ѵ�.
        if (lifeTimer < LIFE_TIME)
            lifeTimer += Time.deltaTime;
        else
            Destroy(gameObject);

        laserTransparentValue = 1f - (lifeTimer / LIFE_TIME);
        Color StartColor = laser.startColor;
        Color EndColor = laser.endColor;
        StartColor.a = laserTransparentValue;
        EndColor.a = laserTransparentValue / 2.0f;
        laser.startColor = StartColor;
        laser.endColor = EndColor;
    }

    private void Damage(RaycastHit hit)
    {
        // 1��Ī �������� ��� ���� ������ ���� �ʴ´�.
        if (bIsFP) return;

        // ������ ���� ��� ������Ʈ�� ���� �������� �ش�.
        GameObject hitObj = hit.collider.transform.root.gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        if (hitPlayer != null)//ĳ����
        {
            // ���� ������ ���� �������� �����Ѵ�.
            float damageResult = damage;
            if (hit.collider.gameObject.CompareTag("Head"))     //��� �Ǻ�
            {
                damageResult *= head_coef;

                // Head ���� �� audio ���
                /*
                if (hit.collider.gameObject.transform.root.GetComponent<PlayerAudio>() != null)
                {
                    hit.collider.gameObject.transform.root.GetComponent<PlayerAudio>().PlayheadShot();
                }*/
            }
            else if (hit.collider.gameObject.CompareTag("BlackHole"))    // ����� ��Ȧ�� ���
            {
                hit.collider.gameObject.GetComponent<BlackHole>().Absorb(damageResult);
                return;
            }

            if (hitObj.GetComponent<Casey>() != null)//���̽�
            {
                hitObj.GetComponent<Casey>().TakeDamage_Sync((int)damageResult);
            }
            else//�ζ�
            {
                hitObj.GetComponent<Rora>().TakeDamage_Sync((int)damageResult);
            }

            // Ÿ�� ȿ������ ����Ѵ�.
            /*
            PlayAudio AudioManager = hitObj.GetComponent<PlayAudio>();
            if (!hit.collider.gameObject.GetComponent<AudioSource>())
            {
                hit.collider.gameObject.AddComponent<AudioSource>().clip = AudioManager.InstanceClip[0];
            }
            else
            {
                hit.collider.gameObject.GetComponent<AudioSource>().clip = AudioManager.InstanceClip[0];
            }
            hit.collider.gameObject.GetComponent<AudioSource>().Play();*/
        }
        else//����ü
        {
            if (hit.collider.gameObject.transform.root.GetComponent<ObjectWithHP>())
                hit.collider.gameObject.transform.root.GetComponent<ObjectWithHP>().TakeDamage((int)damage);
        }
    }

    private void SetEndPositionWithCollider(RaycastHit hit)
    {
        laser.SetPosition(1, hit.point);

        HitEffect.transform.position = hit.point + hit.normal * HitOffset;
        if (useLaserRotation)
            HitEffect.transform.rotation = transform.rotation;
        else
            HitEffect.transform.LookAt(hit.point + hit.normal);

        foreach (var AllPs in Effects)
        {
            if (!AllPs.isPlaying) AllPs.Play();
        }
        //Texture tiling
        Length[0] = MainTextureLength * (Vector3.Distance(transform.position, hit.point));
        Length[2] = NoiseTextureLength * (Vector3.Distance(transform.position, hit.point));
    }

    private void SetEndPositionWithoutCollider()
    {
        var EndPos = transform.position + vecToTarget;

        laser.SetPosition(1, EndPos);
        HitEffect.transform.position = EndPos;

        foreach (var AllPs in Hit)
        {
            if (AllPs.isPlaying) AllPs.Stop();
        }
        //Texture tiling
        Length[0] = MainTextureLength * (Vector3.Distance(transform.position, EndPos));
        Length[2] = NoiseTextureLength * (Vector3.Distance(transform.position, EndPos));
    }


    public void DisablePrepare()
    {
        if (laser != null)
        {
            laser.enabled = false;
        }
        UpdateSaver = true;
        if (Effects != null)
        {
            foreach (var AllPs in Effects)
            {
                if (AllPs.isPlaying) AllPs.Stop();
            }
        }
    }


   
    
}
