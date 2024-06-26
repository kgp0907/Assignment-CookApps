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

        // 타겟 찾기 또는 업데이트
        if (unit.AttackTarget == null || unit.AttackTarget.UNIT_STATE == UNIT_STATE.DEAD)
        {
            unit.AttackTarget = unit.FindAttackTarget();  // 가장 가까운 몬스터를 새 타겟으로 설정
            if (unit.AttackTarget == null)
            {
                // 가까운 몬스터가 없다면 Idle 상태로 전환
                animator.SetTrigger("Idle");
                return;
            }
            unit.UnitFlip(unit.AttackTarget);  // 새 타겟에 따라 방향 조정
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
