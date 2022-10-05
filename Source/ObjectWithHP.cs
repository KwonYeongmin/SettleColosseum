using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

    // HP�� ���� ������Ʈ���� ��� Ŭ����
public class ObjectWithHP : CachedObject
{
    protected int hp;

    // HP�� ���� ������Ʈ���� ���������� ���̴� �Լ���
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

