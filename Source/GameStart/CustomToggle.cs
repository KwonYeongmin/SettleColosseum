using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// ����� �� ���¸� ��Ÿ���� ������
public enum CustomToggleState
{
    Normal = 0,
    Highlighted,
    Selected,
    Disabled,
    Inactive
};

// ��� �׷� ���� �ٸ� UI�� Ŭ���ص� ���� ���°� �������� �ʴ� ��� UI
public class CustomToggle : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    CustomToggleStateMachine stateMachine;   // ����� ���¸� �����ϴ� ���� ���
    Animator animator;

    public CustomToggleGroup toggleGroup = null;    // ����� ���� ��� �׷�(��� �׷� ��ũ��Ʈ���� ���� �ʱ�ȭ)
    public UnityEvent onSelect;    // ��� ���ýÿ� �߻��� �̺�Ʈ

    // UI Ȱ��ȭ��, �ִϸ����� ������Ʈ�� ���µ� ���� ��踦 �ʱ�ȭ���ش�.
    public void OnEnable()
    {
        animator = GetComponent<Animator>();
        stateMachine = new CustomToggleStateMachine(animator);   // �ִϸ����ʹ� ���������� ���� ��迡�� ���ǹǷ� �Ѱ��ش�.
    }

    // ���� ���°� Disabled ���¶�� Disable �ִϸ��̼��� �������� üũ��, �����ٸ� ��� ���·� �ǵ��ư���.
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

    // Ŭ�� �̺�Ʈ �߻���, ���� ���, ���̶���Ʈ ���¶�� ���� �Լ� ����
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

    // ���콺�� ������ ������, ���� ��� ���¶�� ���̶���Ʈ ���·� ��ȯ�Ѵ�.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (stateMachine.GetState() == CustomToggleState.Normal)
        {
            stateMachine.SetState(CustomToggleState.Highlighted);
        }
    }

    // ���콺�� ������ ������������, ���� ���̶���Ʈ ���¶�� ��� ���·� ��ȯ�Ѵ�.
    public void OnPointerExit(PointerEventData eventData)
    {
        if (stateMachine.GetState() == CustomToggleState.Highlighted)
        {
            stateMachine.SetState(CustomToggleState.Normal);
        }
    }

    // ���� ��� �׷� ���� �ٸ� ����� ���õǾ�����, ����� Disabled ���·� ��ȯ�Ѵ�.
    public void OnToggleDisable()
    {
        stateMachine.SetState(CustomToggleState.Disabled);
    }

    // ���� ���¸� ���� ���·� ��ȯ�ϰ� ���� �̺�Ʈ�� �߻���Ų�� �̸� ��� �׷쿡 �˸���.
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

// ����� ���¸� �����ϴ� ���� ���
public class CustomToggleStateMachine
{
    CustomToggleState state;    // ����� ����
    Animator animator;

    // �� ���� ��� ������ �⺻ ���¸� ��ַ� �������ش�.
    public CustomToggleStateMachine(Animator animator)
    {
        this.animator = animator;
        state = CustomToggleState.Normal;
    }

    // �� ���¸� �������ְ� ���� �Լ��� ȣ�����ִ� �Լ�.
    public void SetState(CustomToggleState newState)
    {
        // �� ���·� ���¸� �缳���ϰ�, �� ���¿� �˸��� �ִϸ��̼��� ����Ѵ�.
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

    // ���� ���¸� ��ȯ�ϴ� �Լ�
    public CustomToggleState GetState()
    {
        return state;
    }
}