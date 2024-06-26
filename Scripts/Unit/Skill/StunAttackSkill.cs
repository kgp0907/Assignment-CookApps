using UnityEngine;

public class StunAttackSkill : Skill
{
    public float stunDuration = 1.0f;  // ���� ���� �ð�

    public override void Activate(BaseUnit user, BaseUnit target)
    {
        ActiveEffect();  // Ȱ��ȭ ȿ�� ����

        Collider[] hitEnemies = Physics.OverlapSphere(user.transform.position, attackRange);
        Collider closestEnemy = null;
        float closestDistance = float.MaxValue;
       
        // ��Ÿ� ������ ���� ����� �� ã��
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

        // ���� ����� ������ �������� ���� ����
        if (closestEnemy != null)
        {
            MonsterUnit enemy = closestEnemy.GetComponent<MonsterUnit>();        
            ApplyEffect(enemy);  // ���� ȿ�� ����
            user.UnitFlip(enemy);
            enemy.TakeDamage(user.Data.AttackPower * skillCoefficient);  // ������ ����
        }

        user.UNIT_STATE = UNIT_STATE.MOVE;
    }

    public override void ApplyEffect(BaseUnit target)
    {
        if (target is MonsterUnit monster)
        {
            monster.Stun(stunDuration);  // ���͸� ���� ��Ŵ
            ShowStunEffect(monster);
        }
    }

    private void ShowStunEffect(BaseUnit unit)
    {
        if (unit.Collider != null)
        {
            Vector3 effectPosition = unit.Collider.bounds.center;  // �ݶ��̴��� �߽� ��ġ�� ���� ��ǥ�� ����

            GameObject healingEffect = ObjectPool.Instance.GetObject("StunEffect");

            healingEffect.transform.SetParent(unit.transform,false);
            healingEffect.transform.localPosition = new Vector3(0, effectPosition.y+1.2f, 0);  // ����Ʈ�� ��ġ ����
            healingEffect.SetActive(true);

            ObjectPool.Instance.ReturnObjectByDelay(healingEffect, "StunEffect", stunDuration);  // ���� �ð� �� ��ȯ
        }
    }

    public override void ActiveEffect()
    {
       
    }

    public override void DeActiveEffect()
    {
       
    }
}