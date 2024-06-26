using UnityEngine;

public class StunAttackSkill : Skill
{
    public float stunDuration = 1.0f;  // 스턴 지속 시간

    public override void Activate(BaseUnit user, BaseUnit target)
    {
        ActiveEffect();  // 활성화 효과 적용

        Collider[] hitEnemies = Physics.OverlapSphere(user.transform.position, attackRange);
        Collider closestEnemy = null;
        float closestDistance = float.MaxValue;
       
        // 사거리 내에서 가장 가까운 적 찾기
        foreach (Collider hit in hitEnemies)
        {
            MonsterUnit enemy = hit.GetComponent<MonsterUnit>();
            if (enemy != null)
            {
                float distance = Vector3.Distance(user.transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = hit;
                }
            }
        }

        // 가장 가까운 적에게 데미지와 스턴 적용
        if (closestEnemy != null)
        {
            MonsterUnit enemy = closestEnemy.GetComponent<MonsterUnit>();        
            ApplyEffect(enemy);  // 스턴 효과 적용
            user.UnitFlip(enemy);
            enemy.TakeDamage(user.Data.AttackPower * skillCoefficient);  // 데미지 적용
        }

        user.UNIT_STATE = UNIT_STATE.MOVE;
    }

    public override void ApplyEffect(BaseUnit target)
    {
        if (target is MonsterUnit monster)
        {
            monster.Stun(stunDuration);  // 몬스터를 스턴 시킴
            ShowStunEffect(monster);
        }
    }

    private void ShowStunEffect(BaseUnit unit)
    {
        if (unit.Collider != null)
        {
            Vector3 effectPosition = unit.Collider.bounds.center;  // 콜라이더의 중심 위치를 월드 좌표로 얻음

            GameObject healingEffect = ObjectPool.Instance.GetObject("StunEffect");

            healingEffect.transform.SetParent(unit.transform,false);
            healingEffect.transform.localPosition = new Vector3(0, effectPosition.y+1.2f, 0);  // 이펙트의 위치 설정
            healingEffect.SetActive(true);

            ObjectPool.Instance.ReturnObjectByDelay(healingEffect, "StunEffect", stunDuration);  // 일정 시간 후 반환
        }
    }

    public override void ActiveEffect()
    {
       
    }

    public override void DeActiveEffect()
    {
       
    }
}