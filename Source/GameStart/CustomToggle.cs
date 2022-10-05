using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// 토글의 각 상태를 나타내는 열거형
public enum CustomToggleState
{
    Normal = 0,
    Highlighted,
    Selected,
    Disabled,
    Inactive
};

// 토글 그룹 밖의 다른 UI를 클릭해도 선택 상태가 해제되지 않는 토글 UI
public class CustomToggle : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    CustomToggleStateMachine stateMachine;   // 토글의 상태를 관리하는 상태 기계
    Animator animator;

    public CustomToggleGroup toggleGroup = null;    // 토글이 속한 토글 그룹(토글 그룹 스크립트에서 따로 초기화)
    public UnityEvent onSelect;    // 토글 선택시에 발생할 이벤트

    // UI 활성화시, 애니메이터 컴포넌트를 얻어온뒤 상태 기계를 초기화해준다.
    public void OnEnable()
    {
        animator = GetComponent<Animator>();
        stateMachine = new CustomToggleStateMachine(animator);   // 애니메이터는 최종적으로 상태 기계에서 사용되므로 넘겨준다.
    }

    // 현재 상태가 Disabled 상태라면 Disable 애니메이션이 끝났는지 체크후, 끝났다면 노멀 상태로 되돌아간다.
    public void Update()
    {
        if (stateMachine.GetState() == CustomToggleState.Disabled)
        {
            if (
                animator.GetCurrentAnimatorStateInfo(0).IsName("Disabled") &&
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                stateMachine.SetState(CustomToggleState.Normal);
            }
        }
    }

    // 클릭 이벤트 발생시, 만약 노멀, 하이라이트 상태라면 선택 함수 실행
    public void OnPointerClick(PointerEventData eventData)
    {
        if (
            stateMachine.GetState() == CustomToggleState.Normal ||
            stateMachine.GetState() == CustomToggleState.Highlighted)
        {
            toggleGroup.SetCurrentToggle(this);
            stateMachine.SetState(CustomToggleState.Selected);
        }
        else if(
            stateMachine.GetState() == CustomToggleState.Selected)
        {
            Select();
        }
    }

    // 마우스를 가져다 댔을시, 만약 노멀 상태라면 하이라이트 상태로 전환한다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (stateMachine.GetState() == CustomToggleState.Normal)
        {
            stateMachine.SetState(CustomToggleState.Highlighted);
        }
    }

    // 마우스가 밖으로 빠져나갔을때, 만약 하이라이트 상태라면 노멀 상태로 전환한다.
    public void OnPointerExit(PointerEventData eventData)
    {
        if (stateMachine.GetState() == CustomToggleState.Highlighted)
        {
            stateMachine.SetState(CustomToggleState.Normal);
        }
    }

    // 속한 토글 그룹 내의 다른 토글이 선택되었을때, 토글을 Disabled 상태로 전환한다.
    public void OnToggleDisable()
    {
        stateMachine.SetState(CustomToggleState.Disabled);
    }

    // 현재 상태를 선택 상태로 전환하고 선택 이벤트를 발생시킨뒤 이를 토글 그룹에 알린다.
    public void Select()
    {
        onSelect.Invoke();
    }

    public void SetInteractable(bool bInteractable)
    {
        if(bInteractable)   stateMachine.SetState(CustomToggleState.Normal);
        else                stateMachine.SetState(CustomToggleState.Inactive);
    }
}

// 토글의 상태를 관리하는 상태 기계
public class CustomToggleStateMachine
{
    CustomToggleState state;    // 토글의 상태
    Animator animator;

    // 새 상태 기계 생성시 기본 상태를 노멀로 설정해준다.
    public CustomToggleStateMachine(Animator animator)
    {
        this.animator = animator;
        state = CustomToggleState.Normal;
    }

    // 새 상태를 설정해주고 진입 함수를 호출해주는 함수.
    public void SetState(CustomToggleState newState)
    {
        // 새 상태로 상태를 재설정하고, 각 상태에 알맞은 애니메이션을 출력한다.
        switch (newState)
        {
            case CustomToggleState.Normal:
                state = CustomToggleState.Normal;
                animator.SetTrigger("Normal");
                break;
            case CustomToggleState.Highlighted:
                state = CustomToggleState.Highlighted;
                animator.SetTrigger("Highlighted");
                break;
            case CustomToggleState.Selected:
                state = CustomToggleState.Selected;
                animator.SetTrigger("Selected");
                break;
            case CustomToggleState.Disabled:
                state = CustomToggleState.Disabled;
                animator.SetTrigger("Disabled");
                break;
            case CustomToggleState.Inactive:
                state = CustomToggleState.Inactive;
                animator.SetTrigger("Inactive");
                break;
        }
    }

    // 현재 상태를 반환하는 함수
    public CustomToggleState GetState()
    {
        return state;
    }
}