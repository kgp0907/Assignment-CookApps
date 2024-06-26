using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterUnit : BaseUnit
{
    public MONSTER_TYPE MONSTER_TYPE = MONSTER_TYPE.NORMAL;
    private EnemyData EnemyData;
    // Start is called before the first frame update
    void Start()
    {
        Init();   
       
    }

    public override void Init()
    {
        EnemyData = Data as EnemyData;
        UNIT_STATE = UNIT_STATE.IDLE;
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

        // 스테이지 수준에 따른 체력과 공격력 계산
        float stageMultiplier = 1 + (StageManager.Instance.StageNum * 0.1f);
        CurrentMaxHealth = Data.MaxHealth * stageMultiplier;
        CurrentAttackPower = Data.AttackPower * stageMultiplier;

        CurrentHealth = CurrentMaxHealth;  // 최대 체력으로 현재 체력 설정

        // UI나 다른 초기화 로직
        thisHpBar.Init(this);

        if (MONSTER_TYPE == MONSTER_TYPE.BOSS)
        {
            OnDeath -= UnitManager.Instance.CheckAllMonstersDefeated;
            OnDeath += UnitManager.Instance.CheckAllMonstersDefeated;
        }
    }

    // Update is called once per frame
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
                //MoveTowardsDestination();
                //CheckForEnemies();
                break;
            case UNIT_STATE.ATTACK:
                AttackState();
                break;
            default:
                break;
        }
    }

    public override void AttackState()
    {
        base.AttackState();
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
            UNIT_STATE = UNIT_STATE.ATTACK;
            Animator.SetTrigger("Attack");
        }
        else
        {
            // 이동 애니메이션 활성화 및 타겟을 향해 이동 명령을 스테이트 머신으로 전달
            UNIT_STATE = UNIT_STATE.MOVE;
            Animator.SetBool("Move", true);
        }
    }

    public override void Dead()
    {
        if (UNIT_STATE == UNIT_STATE.DEAD)
            return;

        IdleState();
        Animator.SetTrigger("Dead");
        UNIT_STATE = UNIT_STATE.DEAD;
        Animator.speed = 1;

        ResetMaterialColor();

        if (LastHitBy != null && LastHitBy.Data.UNIT_TYPE ==  UNIT_TYPE.CHARACTER)
        {
            CharacterUnit characterUnit = LastHitBy as CharacterUnit;
            CharacterData characterData = LastHitBy.Data as CharacterData;
            // 마지막 타격을 가한 캐릭터에게 경험치를 준다.
            characterData.GainExp(Const.MonsterExp *StageManager.Instance.StageNum);
            characterUnit.LevelChangeAction.Invoke();
        }

        // 모든 유닛의 attackTarget에서 이 몬스터를 제거
        if (Data.UNIT_TYPE == UNIT_TYPE.ENEMY)
        {
            if (UnitManager.Instance.SummonCharacterList.Count <= 0)
                return;

            foreach (var unit in UnitManager.Instance.SummonCharacterList)
            {
                if (unit == null)
                    return;

                if (unit.AttackTarget != null)
                {
                    if (unit.AttackTarget == this)
                    {
                        unit.AttackTarget = null; // 공격 대상 몬스터가 죽으면 대상 해제
                    }
                }

            }
        }

        // 사망 처리 완료 후, 몬스터 리스트에서 제거
        if (MONSTER_TYPE == MONSTER_TYPE.BOSS)
        {
            StageManager.Instance.HandleAllMonstersDefeated();
        }

        UnitManager.Instance.RemoveMonster(this);
        StageManager.Instance.MonsterReSpwan();

        UnitDeath(this);

        CurrencyManager.Instance.AddGold(50 + (StageManager.Instance.StageNum*10));

        if (!gameObject.activeSelf)
        {
            return;
        }

        DeadCoroutine = StartCoroutine(CoDead());
    }

    public void UnitDeath(BaseUnit unit)
    {
        Dead(); // 유닛 사망 로직 처리, 사망 애니메이션 등

        if (unit != null)
        {
            StartCoroutine(ReturnToPoolAfterDelay(unit.gameObject, unit.Data.Name, 1.0f)); // 1초 후에 풀로 반환
        }      
    }

    private IEnumerator ReturnToPoolAfterDelay(GameObject unit, string tag, float delay)
    {
        // 대기 시간 후
        yield return new WaitForSeconds(delay);

        // 풀로 반환 전에 객체를 비활성화
        //unit.SetActive(false);

        // 오브젝트 풀에 반환
        ObjectPool.Instance.ReturnObject(unit, tag);
    }

    public void CheckForEnemies()
    {
        if (FindAttackTarget() != null)
        {
            Animator.SetBool("Patrol", false);
            Animator.SetBool("Move", true);
            UNIT_STATE = UNIT_STATE.MOVE; // 상태를 'Move'로 변경
        }
    }

    public override BaseUnit FindAttackTarget()
    {
        BaseUnit closest = null;
        float minDistance = EnemyData.ChaseRange;
        Vector3 currentPosition = transform.position;

        if (UnitManager.Instance.SummonCharacterList.Count > 0)
        {
            foreach (BaseUnit monster in UnitManager.Instance.SummonCharacterList)
            {
                float distance = Vector3.Distance(monster.transform.position, currentPosition);
                if (distance < minDistance)
                {
                    closest = monster;
                    minDistance = distance;
                }
            }
        }

        return closest;
    }

    private void UpdatePatrolDestination()
    {
        PatrolDestination = GetRandomPatrolPoint();
    }

    private Vector3 GetRandomPatrolPoint()
    {
        float range = 10.0f; // 순찰 범위 지정
        Vector3 randomPoint = new Vector3(
            UnityEngine.Random.Range(-range, range),
            transform.position.y,
            UnityEngine.Random.Range(-range, range)
        );
        return transform.position + randomPoint;
    }

    public void MoveTowardsDestination()
    {
        float step = 1 * Time.deltaTime; // 이동 속도 조정
        Vector3 currentPosition = transform.position;
        Vector3 moveDirection = (PatrolDestination - currentPosition).normalized; // 이동 방향 계산

        transform.position = Vector3.MoveTowards(currentPosition, PatrolDestination, step);

        // 목적지 도달 여부 확인
        if (Vector3.Distance(currentPosition, PatrolDestination) < 1f)
        {
            UpdatePatrolDestination(); // 새로운 목적지 설정
        }
        else
        {
            if (moveDirection.x > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (moveDirection.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    private Coroutine stunCoroutine;

    public void Stun(float duration)
    {
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        stunCoroutine = StartCoroutine(DoStun(duration));
    }

    private IEnumerator DoStun(float duration)
    {
        UNIT_STATE = UNIT_STATE.STUNNED;  // 상태를 스턴 상태로 변경
        Animator.SetTrigger("Stun");
        yield return new WaitForSeconds(duration);
        Animator.SetBool("Move",true);
        UNIT_STATE = UNIT_STATE.MOVE;  // 스턴 해제 후 기본 상태로 복귀
    }
}
