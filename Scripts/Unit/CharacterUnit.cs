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

        // ���� ����� ��ȿ���� �˻� (null�� �ƴϰ�, ����־�� ��)
        if (attackTarget != null && attackTarget.UNIT_STATE != UNIT_STATE.DEAD)
        {
            Animator.SetTrigger("Attack");
        }
        else
        {
            // ���� ����� ��ȿ���� �ʰų� ����� ��� ���¸� ����
            Animator.ResetTrigger("Attack"); // ���� ���� ���� ���� �ִϸ��̼� Ʈ���Ÿ� ����
            UNIT_STATE = UNIT_STATE.IDLE;
            SetMove(false); // �̵� �� ���� �ִϸ��̼� ��Ȱ��ȭ
        }
    }

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
        // ���� �ֺ��� ���Ͱ� ������ Ȯ��
        if (UnitManager.Instance.SummonMonsterList.Count == 0)
        {
            // ���Ͱ� �����Ƿ� Idle �ִϸ��̼� ����
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
            Animator.SetTrigger("Attack");
        }
        else
        {
            // �̵� �ִϸ��̼� Ȱ��ȭ �� Ÿ���� ���� �̵� ����� ������Ʈ �ӽ����� ����
            Animator.SetBool("Move", true);

        }
    }

    public override void Dead()
    {
        if (UNIT_STATE == UNIT_STATE.DEAD)
            return;

        // ���� ��󿡼� ����
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

        // ���� ����Ʈ���� ����
        UnitManager.Instance.SummonCharacterList.Remove(this);
        UnitManager.Instance.RemovePlayer(this); // �����ϰ� ����
        UnitDeath(this);
        OnDeathEventInvoke();
        DeadCoroutine = StartCoroutine(CoDead());
    }

    public void UnitDeath(BaseUnit unit)
    {
        Dead(); // ���� ��� ���� ó��, ��� �ִϸ��̼� ��

        StartCoroutine(ReturnToPoolAfterDelay(unit.gameObject, unit.Data.Name, 1.0f)); // 2�� �Ŀ� Ǯ�� ��ȯ
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
        GameObject arrow = ObjectPool.Instance.GetObject("Arrow"); // ������Ʈ Ǯ���� ȭ�� ��������

        if (arrow != null)
        {
            if(AttackTarget != null)
            {
                Vector3 targetPosition = AttackTarget.transform.position; // Ÿ���� ��ġ
                Vector3 displacement = targetPosition - firePoint.position; // �߻� ������ Ÿ�� ������ ����
                float initialVelocity = CalculateLaunchSpeed(displacement, Physics.gravity.y, 5); // �ʱ� �ӵ� ���

                Arrow arrowScript = arrow.GetComponent<Arrow>();
                arrow.transform.position = firePoint.position; // �߻� ��ġ ����
                arrow.transform.rotation = Quaternion.LookRotation(displacement); // Ÿ�� �������� ȸ��
                arrowScript.LaunchArrow(this);
                //arrowScript.SetTarget(AttackTarget.transform.position, initialVelocity); // Ÿ�� ���� �� �߻�
                arrow.SetActive(true);
            }
      
        }
    }

    public void FireMagicArrow()
    {
        if (AttackTarget != null)
        {
            GameObject arrow = ObjectPool.Instance.GetObject("MagicArrow"); // ������Ʈ Ǯ���� ȭ�� ��������
            if (arrow != null)
            {
                Vector3 targetPosition = AttackTarget.transform.position; // Ÿ���� ��ġ
                Vector3 displacement = targetPosition - firePoint.position; // �߻� ������ Ÿ�� ������ ����
                float initialVelocity = CalculateLaunchSpeed(displacement, Physics.gravity.y, 5); // �ʱ� �ӵ� ���

                MagicArrow arrowScript = arrow.GetComponent<MagicArrow>();
                arrow.transform.position = firePoint.position; // �߻� ��ġ ����
                arrow.transform.rotation = Quaternion.LookRotation(displacement); // Ÿ�� �������� ȸ��
                arrowScript.LaunchArrow(this);
                //arrowScript.SetTarget(AttackTarget.transform.position, initialVelocity); // Ÿ�� ���� �� �߻�
                arrow.SetActive(true);

                UNIT_STATE = UNIT_STATE.MOVE;
            }

        }
    }

    public void FireMissile()
    {
        if (AttackTarget != null)
        {
            GameObject arrow = ObjectPool.Instance.GetObject("Fire"); // ������Ʈ Ǯ���� ȭ�� ��������

            if (arrow != null)
            {
                Vector3 targetPosition = AttackTarget.transform.position; // Ÿ���� ��ġ
                Vector3 displacement = targetPosition - firePoint.position; // �߻� ������ Ÿ�� ������ ����
                float initialVelocity = CalculateLaunchSpeed(displacement, Physics.gravity.y, 5); // �ʱ� �ӵ� ���

                FireMagic arrowScript = arrow.GetComponent<FireMagic>();
                arrow.transform.position = firePoint.position; // �߻� ��ġ ����
                arrow.transform.rotation = Quaternion.LookRotation(displacement); // Ÿ�� �������� ȸ��
                arrowScript.LaunchArrow(this);
                //arrowScript.SetTarget(AttackTarget.transform.position, initialVelocity); // Ÿ�� ���� �� �߻�
                arrow.SetActive(true);
            }

        }
    }

    private float CalculateLaunchSpeed(Vector3 displacement, float gravity, float angle)
    {
        angle = Mathf.Deg2Rad * angle; // ������ �������� ��ȯ
        float h = displacement.y; // ���� ����
        displacement.y = 0; // ���� �������� ����ϱ� ���� y�� 0���� ����
        float d = displacement.magnitude; // ���� �Ÿ�
        float a = angle; // �߻� ����

        float speed = Mathf.Sqrt(d * Mathf.Abs(gravity) / Mathf.Sin(2 * a)); // ������ ��� �ӵ� ����
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
            //    Animator.SetTrigger("Skill");  // ��ų �ִϸ��̼� ����
            //    Skill.Activate(this, attackTarget);
            //}
            else
            {
                Animator.ResetTrigger("Attack");
                Animator.SetTrigger("Skill");  // ��ų �ִϸ��̼� ����
            }
        }
        else
        {
            // ���� ����� ��ȿ���� �ʰų� ����� ��� ���¸� ����
            Animator.ResetTrigger("Skill");
            Animator.ResetTrigger("Attack"); // ���� ���� ���� ���� �ִϸ��̼� Ʈ���Ÿ� ����
            UNIT_STATE = UNIT_STATE.IDLE;
            SetMove(false); // �̵� �� ���� �ִϸ��̼� ��Ȱ��ȭ
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
