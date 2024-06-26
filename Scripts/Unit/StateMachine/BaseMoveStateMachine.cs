using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMoveStateMachine : StateMachineBehaviour
{
    protected BaseUnit unit = null;

    // 애니메이션 상태 진입 시 초기 설정
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // 애니메이션 상태 업데이트 중 계속 체크
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
        // 공격 대상과의 거리를 체크
        float distanceToTarget = Vector3.Distance(unit.transform.position, unit.AttackTarget.transform.position);

        // 사거리 안에 들어왔는지 확인
        if (distanceToTarget <= unit.Data.AttackRange)
        {
            
            animator.SetBool("Move", false);
            unit.UNIT_STATE = UNIT_STATE.ATTACK;
            animator.SetTrigger("Attack"); // 공격 상태로 전환
        }
        else
        {
            // 타겟을 향해 이동
            Vector3 moveDirection = (unit.AttackTarget.transform.position - unit.transform.position).normalized;
            unit.Rigidbody.MovePosition(unit.transform.position + moveDirection * unit.devideCurrentMoveAndLerpMaxTime);
        }
    }

    // 애니메이션 상태 종료 시 클린업
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        unit.MoveCurrentTime = 0;
    }
}