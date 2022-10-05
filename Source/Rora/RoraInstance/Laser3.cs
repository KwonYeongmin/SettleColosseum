using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;
using Photon.Pun;


public class Laser3 : ObjectWithHP
{
    private ParticleSystem[] Effects;
    private ParticleSystem[] Hit;
    //[HideInInspector] 
    public GameObject HitEffect;

    [HideInInspector] public float HitOffset = 0;
    public bool useLaserRotation = false;

    [Header("������")]
    private LineRenderer laser;
    public float laserTransparentValue = 0.5f;
    public int laserTransparentSpeed = 10;

    [HideInInspector] public float MainTextureLength = 1f;
    [HideInInspector] public float NoiseTextureLength = 1f;
    private Vector4 Length = new Vector4(1, 1, 1, 1);
    private bool LaserSaver = false;
    private bool UpdateSaver = false;


    [Header("�� ����")]
    public int HP;

    public int defaultDamage;
    [HideInInspector] public int damage;
    [HideInInspector] public float head_coef;

    public float MaxLength;
    public float Radius;
    public float LifeTime;
    public float Speed;

    private GameObject owner;
    [HideInInspector] public GameObject camObj;
    public bool bIsFP = false;      // 1��Ī ����
    private Vector3 vecToTarget;    // �߻� ��ġ���� ��ǥ ���������� ����

    [HideInInspector]public  PhotonView pv;

    public void Shot()
    {
        Initialize();
        ShotBullet();
    }

    private void Initialize()
    {
        pv = this.GetComponent<PhotonView>();

        hp = HP;

        float defaultRadius = this.gameObject.GetComponent<Transform>().localScale.x;
        this.gameObject.GetComponent<Transform>().localScale
          = new Vector3(defaultRadius * Radius, defaultRadius * Radius, defaultRadius * Radius);

        //������ �ʱ�ȭ
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
        head_coef = owner.GetComponent<SkillControl>().reflection_headCoef;

        // ���̸� �����Ѵ�.
        MaxLength = owner.GetComponent<SkillControl>().reflection_maxLength;

        Destroy(this.gameObject, LifeTime);
    }

    private void ShotBullet()
    {
        if (laser != null && UpdateSaver == false)
        {
            laser.SetPosition(0, transform.position);

            vecToTarget = 
                (camObj.transform.position + (camObj.transform.forward * MaxLength)) - transform.position;

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
        HP = hp;

        laser.material.SetTextureScale("_MainTex", new Vector2(Length[0], Length[1]));
        laser.material.SetTextureScale("_Noise", new Vector2(Length[2], Length[3]));

        // >> Laser Fade Out
        laserTransparentValue -= Time.deltaTime * laserTransparentSpeed / 5;
        Color EndColor = laser.startColor;
        Color StartColor = laser.startColor;
        StartColor.a = 0;
        EndColor.a = laserTransparentValue;
        laser.SetColors(EndColor, StartColor);
        // <<
    }

    private void Damage(RaycastHit hit)
    {
        // 1��Ī �������� ��� ���� ������ ���� �ʴ´�.
        if (bIsFP) return;

        // ������ ���� ��� ������Ʈ�� ���� �������� �ش�.
        GameObject hitObj = hit.collider.transform.root.gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        // Debug.Log("Hit Object: " + hitObj);
        // Debug.Log("Hit Player: " + hitPlayer);
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

            Debug.Log("Damage Result: " + damageResult);
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
        //Effects can = null in multiply shooting
        if (Effects != null)
        {
            foreach (var AllPs in Effects)
            {
                if (AllPs.isPlaying) AllPs.Stop();
            }
        }

        Destroy(this.gameObject, LifeTime);
    }
}
