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
            unit.AttackTarget = unit.FindAttackTarget(); // ���� ����� �÷��̾ �� Ÿ������ ����
            if (unit.AttackTarget == null)
            {
                // ����� �÷��̾ ���ٸ� ���� ���·� ��ȯ
                unit.UNIT_STATE = UNIT_STATE.PATROL;
                return;
            }
            unit.UnitFlip(unit);  // �� Ÿ�ٿ� ���� ���� ����
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }
}
