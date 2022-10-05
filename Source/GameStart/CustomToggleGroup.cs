using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomToggleGroup : MonoBehaviour
{
    [SerializeField]
    CustomToggle[] fixedToggles = null;

    int i_currentToggle;

    public int Index
    {
        get
        {
            return i_currentToggle;
        }
    }

    // UIȰ��ȭ��, ��� �׷쿡 ���� ��۵��� ��� �׷��� �������ش�.
    void OnEnable()
    {
        for (int i = 0; i < fixedToggles.Length; i++)
            fixedToggles[i].toggleGroup = this;

        // ó�� �����ɶ� �ƹ� ��۵� ���õ��� ���� ���·� �����Ѵ�.
        i_currentToggle = -1;
    }

    // ���� ���õ� ����� �����Ѵ�.
    public void SetCurrentToggle(CustomToggle newToggle)
    {
        // �ƹ� ��۵� ���õ��� �ʾҴ��� �˻��ѵ�, ������ ����� disable ���ش�.
        if(i_currentToggle != -1)
            fixedToggles[i_currentToggle].OnToggleDisable();

        // ���� ���õ� ��ۿ� �ش�Ǵ� �ε����� ã���� ���� ��� �ε����� �������ش�.
        for(int i = 0; i < fixedToggles.Length; i++)
        {
            if(newToggle.Equals(fixedToggles[i]))
            {
                i_currentToggle = i;
                break;
            }
        }
    }
}
