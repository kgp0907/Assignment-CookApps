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

    protected float stageClearMoveDelayTime = 0; // 적 다잡고 나서 다음 스테이지 이동할때 이동 애니메이션 넘어갈때까지 잠시 대기하는 시간 체크 용도 -Jun 23-09-13
    public float StageClearMoveDelayTime { get => stageClearMoveDelayTime; set => stageClearMoveDelayTime = value; }
    public float lerpMaxTime = 1f;
    public System.Action HpChangeAction = null;
    public Coroutine DeadCoroutine = null; // 사망 로직 코루틴 -Jun 23-08-21
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
            if (value) // true면 더하고
            {
                if (!UnitManager.Instance.ArrivaledUnitList.Contains(this))
                    UnitManager.Instance.ArrivaledUnitList.Add(this);
            }
            else if (!value) // false 빼고
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
        // 공격 대상이 사라지거나 사거리 밖으로 벗어났는지 확인
        if (attackTarget == null || !IsTargetInRange(attackTarget) || attackTarget.UNIT_STATE == UNIT_STATE.DEAD)
        {
            attackTarget = null;
            UNIT_STATE = UNIT_STATE.MOVE; // 새로운 목표를 찾도록 상태 변경
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


    #region 유닛 상태별 함수
    // 피격
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
        HpChangeAction?.Invoke();  // HP 변경 후 즉시 호출
        
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
        //Animator.Play("idle");  // 즉시 Idle 애니메이션으로 전환
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

    // 색을 바꾼뒤 일정시간 후 원래 색으로 되돌린다.
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
        // 적이 존재하는지 체크하고, 해당 적의 위치에 따라 유닛의 방향을 조정
        if (_unit.AttackTarget != null && _unit.UNIT_STATE!=UNIT_STATE.DEAD)
        {
            // 적이 유닛의 오른쪽에 위치할 경우
            if (_unit.AttackTarget.transform.position.x > _unit.transform.position.x)
            {
                if(_unit.Data.UNIT_TYPE == UNIT_TYPE.ENEMY)
                {
                    _unit.transform.localScale = new Vector3(150, 150, 1);  // 기본 방향으로 설정 (오른쪽을 바라봄)
                    ThisHpBar.transform.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    _unit.transform.localScale = new Vector3(-150, 150, 1);  // 기본 방향으로 설정 (오른쪽을 바라봄)
                    ThisHpBar.transform.localScale = new Vector3(-1f, 1f, 1f);
                }
            
            }
            // 적이 유닛의 왼쪽에 위치할 경우
            else
            {
                if (_unit.Data.UNIT_TYPE == UNIT_TYPE.ENEMY)
                {
                    _unit.transform.localScale = new Vector3(-150, 150, 1);  // 기본 방향으로 설정 (오른쪽을 바라봄)
                    ThisHpBar.transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                else
                {
                    _unit.transform.localScale = new Vector3(150, 150, 1); // 수평으로 뒤집어서 왼쪽을 바라봄
                    ThisHpBar.transform.localScale = new Vector3(1f, 1f, 1f);
                }
               
            }
        }
        // 힐 대상이 있을 경우, 힐 대상의 위치에 따라 방향 조정
        else if (_unit.HealTarget != null)
        {
            // 힐 대상이 오른쪽에 위치할 경우
            if (_unit.HealTarget.transform.position.x > _unit.transform.position.x)
            {
                _unit.transform.localScale = new Vector3(-150, 150, 1);  // 기본 방향으로 설정
                ThisHpBar.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            // 힐 대상이 왼쪽에 위치할 경우
            else
            {
                _unit.transform.localScale = new Vector3(150, 150, 1); // 수평으로 뒤집어서 왼쪽을 바라봄
                ThisHpBar.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
        else
        {
            // 기본 방향 설정: 클래스 타입에 따라 기본적으로 바라보는 방향을 설정할 수 있음
            // 예: 몬스터는 기본적으로 왼쪽을 바라보게, 플레이어는 오른쪽을 바라보게 설정
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
        CurrentHealth = CurrentMaxHealth;  // 체력을 최대로 회복
        Debug.Log(CurrentHealth);
        HpChangeAction.Invoke();
        UNIT_STATE = UNIT_STATE.IDLE;    // 상태를 대기 상태로 설정
        Animator.SetTrigger("Idle");     // 대기 애니메이션 재생
        gameObject.SetActive(true);      // 게임 오브젝트 활성화
        UnitManager.Instance.RegisterPlayer(this);
    }

    public void OnDeathEventInvoke()
    {
        OnDeath.Invoke();
    }
}
