using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMoveStateMachine : StateMachineBehaviour
{
    protected BaseUnit unit = null;

    // �ִϸ��̼� ���� ���� �� �ʱ� ����
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // �ִϸ��̼� ���� ������Ʈ �� ��� üũ
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (unit.AttackTarget == null || unit.AttackTarget.UNIT_STATE == UNIT_STATE.DEAD)
        {
            animator.SetTrigger("Idle");
            return;
        }

        unit.MoveCurrentTime += Time.fixedDeltaTime;

        if (unit.MoveCurrentTime >= 1)
            unit.MoveCurrentTime = 1;

        unit.devideCurrentMoveAndLerpMaxTime = Mathf.Sin(unit.MoveCurrentTime / 1 * Mathf.PI * Random.Range(0.015f, 0.018f));
        // ���� ������ �Ÿ��� üũ
        float distanceToTarget = Vector3.Distance(unit.transform.position, unit.AttackTarget.transform.position);

        // ��Ÿ� �ȿ� ���Դ��� Ȯ��
        if (distanceToTarget <= unit.Data.AttackRange)
        {
            
            animator.SetBool("Move", false);
            unit.UNIT_STATE = UNIT_STATE.ATTACK;
            animator.SetTrigger("Attack"); // ���� ���·� ��ȯ
        }
        else
        {
            // Ÿ���� ���� �̵�
            Vector3 moveDirection = (unit.AttackTarget.transform.position - unit.transform.position).normalized;
            unit.Rigidbody.MovePosition(unit.transform.position + moveDirection * unit.devideCurrentMoveAndLerpMaxTime);
        }
    }

    // �ִϸ��̼� ���� ���� �� Ŭ����
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        unit.MoveCurrentTime = 0;
    }
}