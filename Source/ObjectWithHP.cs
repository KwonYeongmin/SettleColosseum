using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

    // HP를 가진 오브젝트들의 기반 클래스
public class ObjectWithHP : CachedObject
{
    protected int hp;

    // HP를 가진 오브젝트에서 공통적으로 쓰이는 함수들
    public virtual void IncreaseHP(int value)
    {
        hp += value;
    }

    public virtual void TakeDamage(int value)
    {
        hp = (hp <= value) ? 0 : hp-value;
    }

    public virtual void TakeDummyDamage(int value)
    {
        hp = (hp <= value) ? 1 : hp-value;
    }
}

