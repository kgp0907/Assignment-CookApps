using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMoveStateMachine : BaseMoveStateMachine
{
    CharacterUnit CharacterUnit;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (unit == null)
        {
            unit = animator.gameObject.GetComponent<BaseUnit>();
            animator.speed = 1;
            unit.UNIT_STATE = UNIT_STATE.MOVE;
        }

        // Ÿ�� ã�� �Ǵ� ������Ʈ
        if (unit.AttackTarget == null || unit.AttackTarget.UNIT_STATE == UNIT_STATE.DEAD)
        {
            unit.AttackTarget = unit.FindAttackTarget();  // ���� ����� ���͸� �� Ÿ������ ����
            if (unit.AttackTarget == null)
            {
                // ����� ���Ͱ� ���ٸ� Idle ���·� ��ȯ
                animator.SetTrigger("Idle");
                return;
            }
            unit.UnitFlip(unit.AttackTarget);  // �� Ÿ�ٿ� ���� ���� ����
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }
}
