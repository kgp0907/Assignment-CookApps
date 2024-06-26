using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaAttackSkill : Skill
{
    public CharacterUnit characterUnit;
    public Animator animator;

    private void Start()
    {
        DeActiveEffect();
    }

    public override void Activate(BaseUnit user, BaseUnit target)
    {
        ActiveEffect();

        animator.SetTrigger("Skill");
    
    }

    public void AttackRangeEnemy()
    {
        // 범위 내의 적을 찾아 데미지를 적용
        Collider[] hitEnemies = Physics.OverlapSphere(characterUnit.transform.position, attackRange);
        foreach (var hit in hitEnemies)
        {
            MonsterUnit enemy = hit.GetComponent<MonsterUnit>();
            if (enemy != null)
            {
                characterUnit.UnitFlip(enemy);
                enemy.TakeDamage(characterUnit.Data.AttackPower * skillCoefficient);
            }
        }

        characterUnit.UNIT_STATE = UNIT_STATE.MOVE;
    }
    

    public override void ActiveEffect()
    {
        gameObject.SetActive(true);
    }

    public override void ApplyEffect(BaseUnit target)
    {
        throw new System.NotImplementedException();
    }

    public override void DeActiveEffect()
    {
        gameObject.SetActive(false);
    }

}
