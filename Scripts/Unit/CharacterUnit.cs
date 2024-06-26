using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUnit : BaseUnit
{
    public Skill Skill;
    public bool isSkillReady = true;
    public bool isSkillIng = false;
    public Action LevelChangeAction = null;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        UNIT_STATE = UNIT_STATE.MOVE;
        thisHpBar.Init(this);
        attackTarget = null;
        HealTarget = null;
        HpChangeAction = null;
        Animator.Rebind();
        MovePosArrivaled = false;

        moveCurrentTime = 0;

        lerpMaxTime = UnityEngine.Random.Range(0.8f, 1);
        UnitFlip(this);
        ResetMaterialColor();
        float MaxHp = Data.MaxHealth;
        CurrentMaxHealth = Data.MaxHealth;
        CurrentHealth = CurrentMaxHealth;
        CurrentAttackPower = Data.AttackPower;
        thisHpBar.Init(this);
        isSkillReady = true;
        isSkillIng = false;

        if (Skill != null)
        {
            Skill.DeActiveEffect();
        }
       
    }

    public override void AttackState()
    {
        if (UNIT_STATE != UNIT_STATE.ATTACK)
            return;

        // 공격 대상이 유효한지 검사 (null이 아니고, 살아있어야 함)
        if (attackTarget != null && attackTarget.UNIT_STATE != UNIT_STATE.DEAD)
        {
            Animator.SetTrigger("Attack");
        }
        else
        {
            // 공격 대상이 유효하지 않거나 사망한 경우 상태를 변경
            Animator.ResetTrigger("Attack"); // 현재 진행 중인 공격 애니메이션 트리거를 리셋
            UNIT_STATE = UNIT_STATE.IDLE;
            SetMove(false); // 이동 및 공격 애니메이션 비활성화
        }
    }

    public override void Update()
    {
        // 상태 체크 로직을 수행
        CheckState();

        // 각 상태에 따른 행동 실행
        switch (UNIT_STATE)
        {
            case UNIT_STATE.IDLE:
                IdleState();
                break;
            case UNIT_STATE.MOVE:
                MoveState();
                break;
            case UNIT_STATE.PATROL:
                break;
            case UNIT_STATE.ATTACK:

                if (isSkillReady == true)
                {
                    UseSkill();
                }
                else
                {
                    AttackState();
                }
                break;
            default:
                break;
        }
    }

    public override void IdleState()
    {
        // 유닛 주변에 몬스터가 없는지 확인
        if (UnitManager.Instance.SummonMonsterList.Count == 0)
        {
            // 몬스터가 없으므로 Idle 애니메이션 실행
            Animator.SetBool("Move", false);
            Animator.SetTrigger("Idle");

        }
        else
        {
            Animator.SetBool("Move", true);
        }
    }

    public override void MoveState()
    {
        // 상태가 MOVE가 아니면 반환
        if (UNIT_STATE != UNIT_STATE.MOVE)
            return;

        // 공격 대상이 없거나 사망했을 경우
        if (attackTarget == null || attackTarget.UNIT_STATE == UNIT_STATE.DEAD)
        {
            // 가장 가까운 몬스터를 새 타겟으로 설정
            attackTarget = FindAttackTarget();
            if (attackTarget == null)
            {
                // 가까운 몬스터가 없다면 Idle 상태로 전환
                IdleState();
                return;
            }
            // 새 타겟에 따라 방향 조정
            UnitFlip(this);
        }

        // 공격 대상과의 거리를 체크
        float distanceToTarget = Vector3.Distance(transform.position, attackTarget.transform.position);

        // 사거리 안에 들어왔는지 확인
        if (distanceToTarget <= Data.AttackRange)
        {
            // 이동을 멈추고 공격 상태로 전환
            Animator.SetTrigger("Attack");
        }
        else
        {
            // 이동 애니메이션 활성화 및 타겟을 향해 이동 명령을 스테이트 머신으로 전달
            Animator.SetBool("Move", true);

        }
    }

    public override void Dead()
    {
        if (UNIT_STATE == UNIT_STATE.DEAD)
            return;

        // 공격 대상에서 제거
        foreach (var unit in UnitManager.Instance.SummonCharacterList)
        {
            if (unit && unit.AttackTarget == this)
                unit.AttackTarget = null;
        }

        IdleState();
        Animator.SetTrigger("Dead");
        UNIT_STATE = UNIT_STATE.DEAD;
        Animator.speed = 1;

        ResetMaterialColor();

        if (!gameObject.activeSelf)
            return;

        // 몬스터 리스트에서 제거
        UnitManager.Instance.SummonCharacterList.Remove(this);
        UnitManager.Instance.RemovePlayer(this); // 안전하게 제거
        UnitDeath(this);
        OnDeathEventInvoke();
        DeadCoroutine = StartCoroutine(CoDead());
    }

    public void UnitDeath(BaseUnit unit)
    {
        Dead(); // 유닛 사망 로직 처리, 사망 애니메이션 등

        StartCoroutine(ReturnToPoolAfterDelay(unit.gameObject, unit.Data.Name, 1.0f)); // 2초 후에 풀로 반환
    }

    private IEnumerator ReturnToPoolAfterDelay(GameObject unit, string tag, float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPool.Instance.ReturnObject(unit, tag);
    }

    public override BaseUnit FindAttackTarget()
    {
        BaseUnit closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        if (UnitManager.Instance.SummonMonsterList.Count > 0)
        {
            foreach (BaseUnit player in UnitManager.Instance.SummonMonsterList)
            {
                if (player.UNIT_STATE != UNIT_STATE.DEAD)
                {
                    float distance = Vector3.Distance(player.transform.position, currentPosition);
                    if (distance < minDistance)
                    {
                        closest = player;
                        minDistance = distance;
                    }
                }
            }
        }

        return closest;
    }

    public void FireArrow()
    {
        GameObject arrow = ObjectPool.Instance.GetObject("Arrow"); // 오브젝트 풀에서 화살 가져오기

        if (arrow != null)
        {
            if(AttackTarget != null)
            {
                Vector3 targetPosition = AttackTarget.transform.position; // 타겟의 위치
                Vector3 displacement = targetPosition - firePoint.position; // 발사 지점과 타겟 사이의 변위
                float initialVelocity = CalculateLaunchSpeed(displacement, Physics.gravity.y, 5); // 초기 속도 계산

                Arrow arrowScript = arrow.GetComponent<Arrow>();
                arrow.transform.position = firePoint.position; // 발사 위치 설정
                arrow.transform.rotation = Quaternion.LookRotation(displacement); // 타겟 방향으로 회전
                arrowScript.LaunchArrow(this);
                //arrowScript.SetTarget(AttackTarget.transform.position, initialVelocity); // 타겟 설정 및 발사
                arrow.SetActive(true);
            }
      
        }
    }

    public void FireMagicArrow()
    {
        if (AttackTarget != null)
        {
            GameObject arrow = ObjectPool.Instance.GetObject("MagicArrow"); // 오브젝트 풀에서 화살 가져오기
            if (arrow != null)
            {
                Vector3 targetPosition = AttackTarget.transform.position; // 타겟의 위치
                Vector3 displacement = targetPosition - firePoint.position; // 발사 지점과 타겟 사이의 변위
                float initialVelocity = CalculateLaunchSpeed(displacement, Physics.gravity.y, 5); // 초기 속도 계산

                MagicArrow arrowScript = arrow.GetComponent<MagicArrow>();
                arrow.transform.position = firePoint.position; // 발사 위치 설정
                arrow.transform.rotation = Quaternion.LookRotation(displacement); // 타겟 방향으로 회전
                arrowScript.LaunchArrow(this);
                //arrowScript.SetTarget(AttackTarget.transform.position, initialVelocity); // 타겟 설정 및 발사
                arrow.SetActive(true);

                UNIT_STATE = UNIT_STATE.MOVE;
            }

        }
    }

    public void FireMissile()
    {
        if (AttackTarget != null)
        {
            GameObject arrow = ObjectPool.Instance.GetObject("Fire"); // 오브젝트 풀에서 화살 가져오기

            if (arrow != null)
            {
                Vector3 targetPosition = AttackTarget.transform.position; // 타겟의 위치
                Vector3 displacement = targetPosition - firePoint.position; // 발사 지점과 타겟 사이의 변위
                float initialVelocity = CalculateLaunchSpeed(displacement, Physics.gravity.y, 5); // 초기 속도 계산

                FireMagic arrowScript = arrow.GetComponent<FireMagic>();
                arrow.transform.position = firePoint.position; // 발사 위치 설정
                arrow.transform.rotation = Quaternion.LookRotation(displacement); // 타겟 방향으로 회전
                arrowScript.LaunchArrow(this);
                //arrowScript.SetTarget(AttackTarget.transform.position, initialVelocity); // 타겟 설정 및 발사
                arrow.SetActive(true);
            }

        }
    }

    private float CalculateLaunchSpeed(Vector3 displacement, float gravity, float angle)
    {
        angle = Mathf.Deg2Rad * angle; // 각도를 라디안으로 변환
        float h = displacement.y; // 수직 변위
        displacement.y = 0; // 수평 변위만을 고려하기 위해 y는 0으로 설정
        float d = displacement.magnitude; // 수평 거리
        float a = angle; // 발사 각도

        float speed = Mathf.Sqrt(d * Mathf.Abs(gravity) / Mathf.Sin(2 * a)); // 포물선 운동의 속도 계산식
        return speed;
    }

    public virtual void UseSkill()
    {
        if (attackTarget != null && attackTarget.UNIT_STATE != UNIT_STATE.DEAD)
        {
            UNIT_STATE = UNIT_STATE.SKILL;
            // Implement skill logic
            StartCoroutine(Cooldown());

            if (Data.ClassType == ClassType.Archer)
            {
                Animator.ResetTrigger("Attack");
                Animator.SetTrigger("Skill");
            }
            else if (Data.ClassType == ClassType.Priest)
            {
                Animator.ResetTrigger("Attack");
                Animator.SetTrigger("Skill");
                //Skill.Activate(this, null);
            }
            //else if(Data.ClassType == ClassType.Thief)
            //{
            //    Animator.ResetTrigger("Attack");
            //    Animator.SetTrigger("Skill");  // 스킬 애니메이션 실행
            //    Skill.Activate(this, attackTarget);
            //}
            else
            {
                Animator.ResetTrigger("Attack");
                Animator.SetTrigger("Skill");  // 스킬 애니메이션 실행
            }
        }
        else
        {
            // 공격 대상이 유효하지 않거나 사망한 경우 상태를 변경
            Animator.ResetTrigger("Skill");
            Animator.ResetTrigger("Attack"); // 현재 진행 중인 공격 애니메이션 트리거를 리셋
            UNIT_STATE = UNIT_STATE.IDLE;
            SetMove(false); // 이동 및 공격 애니메이션 비활성화
        }
    }

    public void ActiveSkill()
    {
        if (attackTarget != null && attackTarget.UNIT_STATE != UNIT_STATE.DEAD)
        {
            Skill.Activate(this, attackTarget);
        }    
    }

    public void ActiveHealSkill()
    {
        if (UNIT_STATE != UNIT_STATE.DEAD)
        {
            Skill.Activate(this, null);
        }
    }

    private IEnumerator Cooldown()
    {
        isSkillReady = false;
        CharacterData characterData = Data as CharacterData;
        yield return new WaitForSeconds(characterData.SkillCooldown);
        isSkillReady = true;
    }
}
