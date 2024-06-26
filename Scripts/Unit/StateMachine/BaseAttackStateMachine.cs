using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAttackStateMachine : StateMachineBehaviour
{
    protected BaseUnit unit = null;
    protected bool attackExit = false;

    //public int AttackIndex = -1;

    public override void OnStateEnter(Animator Animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (unit is null)
        {
            unit = Animator.gameObject.GetComponentInParent<BaseUnit>();
        }

        attackExit = false;

        unit.UnitFlip(unit);

        if (unit.CurrentHealth <= 0 && unit.UNIT_STATE != UNIT_STATE.DEAD)
        {
            unit.Dead();
            return;
        }

        Animator.speed = 1;

        unit.UNIT_STATE = UNIT_STATE.ATTACK;
    }

    public override void OnStateUpdate(Animator Animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (unit.AttackTarget != null)
        {
            if (unit.AttackTarget.UNIT_STATE == UNIT_STATE.DEAD)
            {
                // 타겟이 사망하거나 없는 경우 즉시 공격 중단
                unit.AttackExit();

                if (unit.Data.UNIT_TYPE == UNIT_TYPE.ENEMY)
                {
                    Animator.SetBool("Patrol", true);
                }

                return;
            }
        }

        if ((Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f))
        {
            unit.AttackExit();
            attackExit = true;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.speed = 1;
    }
}
