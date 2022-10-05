using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyManager : MonoBehaviour
{
    // �̱��� ����
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

    // ==Ű ��============================
    // �̵�
    public KeyCode moveLeft =       KeyCode.A;
    public KeyCode moveRight =      KeyCode.D;
    public KeyCode moveUp =         KeyCode.W;
    public KeyCode moveDown =       KeyCode.S;

    // ����
    public KeyCode Jump =           KeyCode.Space;

    //������
    public KeyCode Reload =         KeyCode.R;


    // ����
    public KeyCode Attack =         KeyCode.Mouse0;     //�ְ���
    public KeyCode RAttack =        KeyCode.Mouse1;     //�������
    public KeyCode MeleeAttack =    KeyCode.V;          //��������
    public KeyCode ESkill =         KeyCode.E;          //�������
    public KeyCode FSkill =         KeyCode.F;          //����
    public KeyCode ShiftSkill =     KeyCode.LeftShift;  //�뽬
    public KeyCode QSkill =         KeyCode.Q;          //�ñر�

    // �ɼ�
    public KeyCode Menu = KeyCode.Escape;          // �ɼ�â


    // ==���� �Լ�======================
    public int GetAxisRawHorizontal()
    {
        // Input.GetAxisRaw("Horizontal") �Լ��� ��ü�ϴ� �Լ�
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
