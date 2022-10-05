using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DummyMonster : ObjectWithHP
{
    // ü��
    [Header("ü��")]
    public Image hpBar;
    [Tooltip("HP�⺻ ��")] public int DEFAULT_HP = 200;
    public int curHp;

    void OnEnable()
    {
        hp = DEFAULT_HP;
        initHpbarSize();
    }
    
    void Update()
    {
        hpBar.rectTransform.localScale=new Vector3((float)hp / (float)DEFAULT_HP, 1.0f,1.0f);
    }

    void initHpbarSize()
    {
        hpBar.rectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }
    
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.layer==LayerMask.NameToLayer("BulletTest")  ||
    //       collision.gameObject.layer == LayerMask.NameToLayer("Projectiles")
    //      )
    //    {
    //        // curHp = (curHp <= 0) ? 0 : curHp - 10;
    //        //Debug.Log("������");
    //        //DecreseHP(10);
    //    }

    //}

    //private void OnTriggerEnter(Collider collision)
    //{

    //    if (collision.gameObject.layer == LayerMask.NameToLayer("BulletTest") ||
    //       collision.gameObject.layer == LayerMask.NameToLayer("Projectiles") 
    //       )
    //    {
    //        //  curHp = (curHp <= 0) ? 0 : curHp - 10;
    //        //  Debug.Log("������");
    //        //DecreseHP(10);
    //    }
    //}
}
