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

    // UI활성화시, 토글 그룹에 속한 토글들의 토글 그룹을 설정해준다.
    void OnEnable()
    {
        for (int i = 0; i < fixedToggles.Length; i++)
            fixedToggles[i].toggleGroup = this;

        // 처음 생성될땐 아무 토글도 선택되지 않은 상태로 생성한다.
        i_currentToggle = -1;
    }

    // 현재 선택된 토글을 수정한다.
    public void SetCurrentToggle(CustomToggle newToggle)
    {
        // 아무 토글도 선택되지 않았는지 검사한뒤, 이전의 토글을 disable 해준다.
        if(i_currentToggle != -1)
            fixedToggles[i_currentToggle].OnToggleDisable();

        // 새로 선택된 토글에 해당되는 인덱스를 찾은후 현재 토글 인덱스를 수정해준다.
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
