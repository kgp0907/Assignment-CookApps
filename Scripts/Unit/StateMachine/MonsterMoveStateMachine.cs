using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMoveStateMachine : BaseMoveStateMachine
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (unit == null)
        {
            Transform parent = animator.transform.parent;
            unit = parent.GetComponent<BaseUnit>();
            animator.speed = 1;
            unit.UNIT_STATE = UNIT_STATE.MOVE;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SwitchToPatrolIfNoEnemies();
        base.OnStateUpdate(animator, stateInfo, layerIndex);
       
    }

    private void SwitchToPatrolIfNoEnemies()
    {
        if (unit.AttackTarget == null || unit.AttackTarget.UNIT_STATE == UNIT_STATE.DEAD)
        {
            unit.AttackTarget = unit.FindAttackTarget(); // 가장 가까운 플레이어를 새 타겟으로 설정
            if (unit.AttackTarget == null)
            {
                // 가까운 플레이어가 없다면 순찰 상태로 전환
                unit.UNIT_STATE = UNIT_STATE.PATROL;
                return;
            }
            unit.UnitFlip(unit);  // 새 타겟에 따라 방향 조정
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }
}
