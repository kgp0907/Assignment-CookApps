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

        // �������� ���ؿ� ���� ü�°� ���ݷ� ���
        float stageMultiplier = 1 + (StageManager.Instance.StageNum * 0.1f);
        CurrentMaxHealth = Data.MaxHealth * stageMultiplier;
        CurrentAttackPower = Data.AttackPower * stageMultiplier;

        CurrentHealth = CurrentMaxHealth;  // �ִ� ü������ ���� ü�� ����

        // UI�� �ٸ� �ʱ�ȭ ����
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
        // ���� üũ ������ ����
        CheckState();

        // �� ���¿� ���� �ൿ ����
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
        // ���°� MOVE�� �ƴϸ� ��ȯ
        if (UNIT_STATE != UNIT_STATE.MOVE)
            return;

        // ���� ����� ���ų� ������� ���
        if (attackTarget == null || attackTarget.UNIT_STATE == UNIT_STATE.DEAD)
        {
            // ���� ����� ���͸� �� Ÿ������ ����
            attackTarget = FindAttackTarget();
            if (attackTarget == null)
            {
                // ����� ���Ͱ� ���ٸ� Idle ���·� ��ȯ
                IdleState();
                return;
            }
            // �� Ÿ�ٿ� ���� ���� ����
            UnitFlip(this);
        }

        // ���� ������ �Ÿ��� üũ
        float distanceToTarget = Vector3.Distance(transform.position, attackTarget.transform.position);

        // ��Ÿ� �ȿ� ���Դ��� Ȯ��
        if (distanceToTarget <= Data.AttackRange)
        {
            // �̵��� ���߰� ���� ���·� ��ȯ
            UNIT_STATE = UNIT_STATE.ATTACK;
            Animator.SetTrigger("Attack");
        }
        else
        {
            // �̵� �ִϸ��̼� Ȱ��ȭ �� Ÿ���� ���� �̵� ����� ������Ʈ �ӽ����� ����
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
            // ������ Ÿ���� ���� ĳ���Ϳ��� ����ġ�� �ش�.
            characterData.GainExp(Const.MonsterExp *StageManager.Instance.StageNum);
            characterUnit.LevelChangeAction.Invoke();
        }

        // ��� ������ attackTarget���� �� ���͸� ����
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
                        unit.AttackTarget = null; // ���� ��� ���Ͱ� ������ ��� ����
                    }
                }

            }
        }

        // ��� ó�� �Ϸ� ��, ���� ����Ʈ���� ����
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
        Dead(); // ���� ��� ���� ó��, ��� �ִϸ��̼� ��

        if (unit != null)
        {
            StartCoroutine(ReturnToPoolAfterDelay(unit.gameObject, unit.Data.Name, 1.0f)); // 1�� �Ŀ� Ǯ�� ��ȯ
        }      
    }

    private IEnumerator ReturnToPoolAfterDelay(GameObject unit, string tag, float delay)
    {
        // ��� �ð� ��
        yield return new WaitForSeconds(delay);

        // Ǯ�� ��ȯ ���� ��ü�� ��Ȱ��ȭ
        //unit.SetActive(false);

        // ������Ʈ Ǯ�� ��ȯ
        ObjectPool.Instance.ReturnObject(unit, tag);
    }

    public void CheckForEnemies()
    {
        if (FindAttackTarget() != null)
        {
            Animator.SetBool("Patrol", false);
            Animator.SetBool("Move", true);
            UNIT_STATE = UNIT_STATE.MOVE; // ���¸� 'Move'�� ����
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
        float range = 10.0f; // ���� ���� ����
        Vector3 randomPoint = new Vector3(
            UnityEngine.Random.Range(-range, range),
            transform.position.y,
            UnityEngine.Random.Range(-range, range)
        );
        return transform.position + randomPoint;
    }

    public void MoveTowardsDestination()
    {
        float step = 1 * Time.deltaTime; // �̵� �ӵ� ����
        Vector3 currentPosition = transform.position;
        Vector3 moveDirection = (PatrolDestination - currentPosition).normalized; // �̵� ���� ���

        transform.position = Vector3.MoveTowards(currentPosition, PatrolDestination, step);

        // ������ ���� ���� Ȯ��
        if (Vector3.Distance(currentPosition, PatrolDestination) < 1f)
        {
            UpdatePatrolDestination(); // ���ο� ������ ����
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
        UNIT_STATE = UNIT_STATE.STUNNED;  // ���¸� ���� ���·� ����
        Animator.SetTrigger("Stun");
        yield return new WaitForSeconds(duration);
        Animator.SetBool("Move",true);
        UNIT_STATE = UNIT_STATE.MOVE;  // ���� ���� �� �⺻ ���·� ����
    }
}
