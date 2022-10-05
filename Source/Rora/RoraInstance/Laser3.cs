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

    [Header("레이저")]
    private LineRenderer laser;
    public float laserTransparentValue = 0.5f;
    public int laserTransparentSpeed = 10;

    [HideInInspector] public float MainTextureLength = 1f;
    [HideInInspector] public float NoiseTextureLength = 1f;
    private Vector4 Length = new Vector4(1, 1, 1, 1);
    private bool LaserSaver = false;
    private bool UpdateSaver = false;


    [Header("값 설정")]
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
    public bool bIsFP = false;      // 1인칭 여부
    private Vector3 vecToTarget;    // 발사 위치에서 목표 지점까지의 벡터

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

        //레이저 초기화
        laser = GetComponent<LineRenderer>();

        Effects = GetComponentsInChildren<ParticleSystem>();
        Hit = HitEffect.GetComponentsInChildren<ParticleSystem>();

        // 3인칭인 경우 판정을 위해 총알을 쏜 플레이어를 찾고 카메라 위치를 얻어온다.
        if (camObj == null)
        {
            // 총알을 쏜 플레이어를 찾는다.
            Playable[] players = FindObjectsOfType<Playable>();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].GetComponent<PhotonView>().Controller == pv.Owner)
                {
                    owner = players[i].gameObject;
                    break;
                }
            }

            // 오브젝트 풀에서 임시로 생성한 경우 그냥 return한다.
            if (owner == null) return;

            // 3인칭 레이저 발사 위치와 부모 오브젝트 transform을 설정한다.
            camObj = owner.transform.GetChild(1).gameObject;
            transform.parent = camObj.transform;
            transform.position = camObj.transform.position + (camObj.transform.forward * 1.8f);
        }
        else
        {
            // 1인칭인 경우 1인칭 플래그를 true로 바꾸고 Owner를 설정해준다.
            bIsFP = true;
            owner = camObj.transform.root.gameObject;
        }

        // 공격 데미지를 설정한다.
        head_coef = owner.GetComponent<SkillControl>().reflection_headCoef;

        // 길이를 설정한다.
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

            // 자기 자신이 맞은 경우는 제외한다.
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
        // 1인칭 레이저인 경우 공격 판정을 하지 않는다.
        if (bIsFP) return;

        // 공격을 맞은 상대 오브젝트를 구해 데미지를 준다.
        GameObject hitObj = hit.collider.transform.root.gameObject;
        Playable hitPlayer = hitObj.GetComponent<Playable>();
        // Debug.Log("Hit Object: " + hitObj);
        // Debug.Log("Hit Player: " + hitPlayer);
        if (hitPlayer != null)//캐릭터
        {
            // 맞은 부위에 따라 데미지를 갱신한다.
            float damageResult = damage;
            if (hit.collider.gameObject.CompareTag("Head"))     //헤드 판별
            {
                damageResult *= head_coef;

                // Head 판정 시 audio 재생
                /*
                if (hit.collider.gameObject.transform.root.GetComponent<PlayerAudio>() != null)
                {
                    hit.collider.gameObject.transform.root.GetComponent<PlayerAudio>().PlayheadShot();
                }*/
            }
            else if (hit.collider.gameObject.CompareTag("BlackHole"))    // 맞춘게 블랙홀일 경우
            {
                hit.collider.gameObject.GetComponent<BlackHole>().Absorb(damageResult);
                return;
            }

            Debug.Log("Damage Result: " + damageResult);
            if (hitObj.GetComponent<Casey>() != null)//케이시
            {
                hitObj.GetComponent<Casey>().TakeDamage_Sync((int)damageResult);
            }
            else//로라
            {
                hitObj.GetComponent<Rora>().TakeDamage_Sync((int)damageResult);
            }

            // 타격 효과음을 재생한다.
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
        else//투사체
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
