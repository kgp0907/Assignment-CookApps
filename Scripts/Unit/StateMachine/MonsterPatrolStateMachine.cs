using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPatrolStateMachine : StateMachineBehaviour
{
    protected MonsterUnit unit = null;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (unit == null)
        {
            Transform parent = animator.transform.parent;

            unit = parent.GetComponent<MonsterUnit>();
            animator.speed = 1;
            unit.UNIT_STATE = UNIT_STATE.PATROL;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(unit.FindAttackTarget() != null)
        {
            unit.SetMove(true);
        }
        unit.MoveTowardsDestination();
        unit.CheckForEnemies();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        unit.Animator.SetBool("Patrol", false);
    }
}
