using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyManager : MonoBehaviour
{
    // 싱글턴 정의
    public static KeyManager keyManager;
    public static KeyManager Inst { get { return keyManager; } }

    void Start()
    {
        if(keyManager)
        {
            Destroy(this);
            return;
        }
        keyManager = this;
        DontDestroyOnLoad(gameObject);
    }

    // ==키 값============================
    // 이동
    public KeyCode moveLeft =       KeyCode.A;
    public KeyCode moveRight =      KeyCode.D;
    public KeyCode moveUp =         KeyCode.W;
    public KeyCode moveDown =       KeyCode.S;

    // 점프
    public KeyCode Jump =           KeyCode.Space;

    //재장전
    public KeyCode Reload =         KeyCode.R;


    // 공격
    public KeyCode Attack =         KeyCode.Mouse0;     //주공격
    public KeyCode RAttack =        KeyCode.Mouse1;     //서브공격
    public KeyCode MeleeAttack =    KeyCode.V;          //근접공격
    public KeyCode ESkill =         KeyCode.E;          //끌어오기
    public KeyCode FSkill =         KeyCode.F;          //방어기
    public KeyCode ShiftSkill =     KeyCode.LeftShift;  //대쉬
    public KeyCode QSkill =         KeyCode.Q;          //궁극기

    // 옵션
    public KeyCode Menu = KeyCode.Escape;          // 옵션창


    // ==지원 함수======================
    public int GetAxisRawHorizontal()
    {
        // Input.GetAxisRaw("Horizontal") 함수를 대체하는 함수
        if(Input.GetKey(moveLeft))          return -1;
        else if(Input.GetKey(moveRight))    return 1;
        else                                return 0;
    }

    public int GetAxisRawVertical()
    {
        if (Input.GetKey(moveDown))         return -1;
        else if (Input.GetKey(moveUp))      return 1;
        else                                return 0;
    }




    public void ChangeKey()
    {
        if (MenuManager.Inst.bToggle%2==0)
        {
            FSkill = KeyCode.F;
            MeleeAttack = KeyCode.V;
        }
        else
        {
            FSkill = KeyCode.V;
            MeleeAttack = KeyCode.F;
        }
    }

}
