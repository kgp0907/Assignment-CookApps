using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;
using static DG.Tweening.DOTweenModuleUI;
using static UnityEngine.UI.CanvasScaler;

public class BaseUnit : MonoBehaviour
{
    public Animator Animator;
    public Rigidbody Rigidbody;
    public Collider Collider;
    public SphereCollider SphereCollider;
    public UNIT_STATE UNIT_STATE = UNIT_STATE.MOVE;
    public UnitData Data = null;
    public float devideCurrentMoveAndLerpMaxTime = 0;
    public float CurrentHealth;
    public float CurrentAttackPower;
    public float CurrentMaxHealth;
    protected BaseUnit attackTarget = null; 
    public BaseUnit AttackTarget { get=>attackTarget; set=>attackTarget = value; }

    protected BaseUnit healTarget = null;
    public BaseUnit HealTarget { get => healTarget; set => healTarget = value; }
    public event Action OnDeath;
    public bool Left = false;
    public bool Right = false;

    public BaseUnit LastHitBy { get; private set; }
    protected float moveCurrentTime = 0;
    public float MoveCurrentTime { get => moveCurrentTime; set => moveCurrentTime = value; }

    protected float stageClearMoveDelayTime = 0; // �� ����� ���� ���� �������� �̵��Ҷ� �̵� �ִϸ��̼� �Ѿ������ ��� ����ϴ� �ð� üũ �뵵 -Jun 23-09-13
    public float StageClearMoveDelayTime { get => stageClearMoveDelayTime; set => stageClearMoveDelayTime = value; }
    public float lerpMaxTime = 1f;
    public System.Action HpChangeAction = null;
    public Coroutine DeadCoroutine = null; // ��� ���� �ڷ�ƾ -Jun 23-08-21
    public Vector3 PatrolDestination;
    public Transform firePoint;
    [SerializeField] protected UnitHpBar thisHpBar = null;
    public UnitHpBar ThisHpBar { get => thisHpBar; }

    [SerializeField] private bool movePosArrivaled = false;
    public bool MovePosArrivaled
    {
        get => movePosArrivaled;
        set
        {
            if (value) // true�� ���ϰ�
            {
                if (!UnitManager.Instance.ArrivaledUnitList.Contains(this))
                    UnitManager.Instance.ArrivaledUnitList.Add(this);
            }
            else if (!value) // false ����
            {
                if (UnitManager.Instance.ArrivaledUnitList.Contains(this))
                    UnitManager.Instance.ArrivaledUnitList.Remove(this);
            }

            movePosArrivaled = value;
        }
    }

    private void Start()
    {
        Init();
    }

    public virtual void Init()
    {
        
    }


    public virtual void Update()
    {
       
    }

    public void CheckState()
    {
        // ���� ����� ������ų� ��Ÿ� ������ ������� Ȯ��
        if (attackTarget == null || !IsTargetInRange(attackTarget) || attackTarget.UNIT_STATE == UNIT_STATE.DEAD)
        {
            attackTarget = null;
            UNIT_STATE = UNIT_STATE.MOVE; // ���ο� ��ǥ�� ã���� ���� ����
        }
    }

    public bool IsTargetInRange(BaseUnit target)
    {
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            return distance <= Data.AttackRange;
        }

        return false;
    }

    public virtual void IdleState()
    {
       
    }
   
    public virtual void MoveState()
    {
        
    }

    public virtual BaseUnit FindAttackTarget()
    {
        return null;
    }

    public virtual void AttackState()
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

    public void SetMove(bool _isTrue)
    {
        Animator.SetBool("Move", _isTrue);
    }

    public void Attack()
    {
        if (attackTarget != null)
        {          
            attackTarget.LastHitBy = this;
            attackTarget.Hit(CurrentAttackPower);      
        }
    }

    public void AttackBySkillConfficnt(float SkilConfficent)
    {
        if (attackTarget != null)
        {
            float damage = CurrentAttackPower * SkilConfficent;
            attackTarget.CurrentHealth -= damage;
            attackTarget.Hit(damage);
        }
    }


    #region ���� ���º� �Լ�
    // �ǰ�
    public virtual void Hit(float damage)
    {
        SetMaterialColor(Color.red);
        ResetMaterialColor(true, 0.25f);
        CurrentHealth -= damage;
        HpChangeAction?.Invoke();
        ObjectPool.Instance.GetDamageFont(damage.ToString(), this, FONT_TYPE.HIT);

        if (CurrentHealth <= 0 && UNIT_STATE != UNIT_STATE.DEAD)
        {
            Dead();

            return;
        }
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        HpChangeAction?.Invoke();  // HP ���� �� ��� ȣ��
        
        if (UNIT_STATE != UNIT_STATE.DEAD)
        {
            Hit(damage);
        }
    }

    public void Heal(float heal)
    {
        CurrentHealth+= heal;
        if (CurrentHealth >= CurrentMaxHealth)
        {
            CurrentHealth = CurrentMaxHealth;
        }
        HpChangeAction?.Invoke();
    }

    public virtual void Dead()
    {

    }

    public void AttackExit()
    {
        //Animator.Play("idle");  // ��� Idle �ִϸ��̼����� ��ȯ
        Animator.ResetTrigger("Attack");
        Animator.SetTrigger("Idle");
    }

    public IEnumerator CoDead()
    {
        yield return null;

        if (thisHpBar is not null)
        {
            thisHpBar.FadeOutAnimPlay();
        }

        DeadCoroutine = null;
    }

    public void Victory()
    {
        Animator.SetTrigger("Victory");
    }

    #endregion
    public virtual void SetMaterialColor(Color _color) { }

    // ���� �ٲ۵� �����ð� �� ���� ������ �ǵ�����.
    public void ResetMaterialColor(bool _isDelayed = false, float _waitTime = 0.0f)
    {
        if (!gameObject.activeSelf)
            return;

        if (_isDelayed == true)
            StartCoroutine(CoResetMaterialColor(_waitTime));
        else
        {
            StopCoroutine(CoResetMaterialColor(_waitTime));

            SetMaterialColor(Color.white);
        }
    }

    private IEnumerator CoResetMaterialColor(float _waitTime)
    {
        yield return new WaitForSeconds(_waitTime);

        if (UNIT_STATE.Equals(UNIT_STATE.DEAD) || CurrentHealth <= 0)
        {
            yield break;
        }

        SetMaterialColor(new Color(1, 1, 1, 1));
    }

    public void UnitFlip(BaseUnit _unit)
    {
        // ���� �����ϴ��� üũ�ϰ�, �ش� ���� ��ġ�� ���� ������ ������ ����
        if (_unit.AttackTarget != null && _unit.UNIT_STATE!=UNIT_STATE.DEAD)
        {
            // ���� ������ �����ʿ� ��ġ�� ���
            if (_unit.AttackTarget.transform.position.x > _unit.transform.position.x)
            {
                if(_unit.Data.UNIT_TYPE == UNIT_TYPE.ENEMY)
                {
                    _unit.transform.localScale = new Vector3(150, 150, 1);  // �⺻ �������� ���� (�������� �ٶ�)
                    ThisHpBar.transform.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    _unit.transform.localScale = new Vector3(-150, 150, 1);  // �⺻ �������� ���� (�������� �ٶ�)
                    ThisHpBar.transform.localScale = new Vector3(-1f, 1f, 1f);
                }
            
            }
            // ���� ������ ���ʿ� ��ġ�� ���
            else
            {
                if (_unit.Data.UNIT_TYPE == UNIT_TYPE.ENEMY)
                {
                    _unit.transform.localScale = new Vector3(-150, 150, 1);  // �⺻ �������� ���� (�������� �ٶ�)
                    ThisHpBar.transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                else
                {
                    _unit.transform.localScale = new Vector3(150, 150, 1); // �������� ����� ������ �ٶ�
                    ThisHpBar.transform.localScale = new Vector3(1f, 1f, 1f);
                }
               
            }
        }
        // �� ����� ���� ���, �� ����� ��ġ�� ���� ���� ����
        else if (_unit.HealTarget != null)
        {
            // �� ����� �����ʿ� ��ġ�� ���
            if (_unit.HealTarget.transform.position.x > _unit.transform.position.x)
            {
                _unit.transform.localScale = new Vector3(-150, 150, 1);  // �⺻ �������� ����
                ThisHpBar.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            // �� ����� ���ʿ� ��ġ�� ���
            else
            {
                _unit.transform.localScale = new Vector3(150, 150, 1); // �������� ����� ������ �ٶ�
                ThisHpBar.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
        else
        {
            // �⺻ ���� ����: Ŭ���� Ÿ�Կ� ���� �⺻������ �ٶ󺸴� ������ ������ �� ����
            // ��: ���ʹ� �⺻������ ������ �ٶ󺸰�, �÷��̾�� �������� �ٶ󺸰� ����
            if (_unit.Data.UNIT_TYPE == UNIT_TYPE.ENEMY)
            {
                _unit.transform.localScale = new Vector3(-150, 150, 1);
                ThisHpBar.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                _unit.transform.localScale = new Vector3(150, 150, 1);
                ThisHpBar.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }

    public void Revive()
    {
        CurrentHealth = CurrentMaxHealth;  // ü���� �ִ�� ȸ��
        Debug.Log(CurrentHealth);
        HpChangeAction.Invoke();
        UNIT_STATE = UNIT_STATE.IDLE;    // ���¸� ��� ���·� ����
        Animator.SetTrigger("Idle");     // ��� �ִϸ��̼� ���
        gameObject.SetActive(true);      // ���� ������Ʈ Ȱ��ȭ
        UnitManager.Instance.RegisterPlayer(this);
    }

    public void OnDeathEventInvoke()
    {
        OnDeath.Invoke();
    }
}
