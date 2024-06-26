using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSkill : Skill
{
    public CharacterUnit CharacterUnit;

    public override void Activate(BaseUnit user, BaseUnit target = null)
    {
        // �� ������ ���� ü���� ���� ���� ã��
        BaseUnit lowestHealthUnit = FindLowestHealthUnit();

        if (lowestHealthUnit != null)
        {
            // ġ�� ����� ������ ���, �� ����� ġ��
            ApplyEffect(lowestHealthUnit);
            ShowHealingEffect(lowestHealthUnit);
            float healValue = CharacterUnit.Data.AttackPower * skillCoefficient;
            ObjectPool.Instance.GetDamageFont(healValue.ToString(), lowestHealthUnit, FONT_TYPE.HEAL);
        }

        CharacterUnit.UNIT_STATE = UNIT_STATE.MOVE;
    }

    public override void ApplyEffect(BaseUnit target)
    {
        // ��� ���� ġ��
        target.Heal(CharacterUnit.CurrentAttackPower * skillCoefficient);
    }

    private BaseUnit FindLowestHealthUnit()
    {
        // user�� ���� ���� ��� ���ֵ��� �ݺ��Ͽ� ü���� ���� ���� ���� ã��
        float minHealth = float.MaxValue;
        BaseUnit lowestHealthUnit = null;

        foreach (var unit in UnitManager.Instance.SummonCharacterList)
        {
            if (unit.CurrentHealth < minHealth && unit.UNIT_STATE != UNIT_STATE.DEAD)
            {
                minHealth = unit.CurrentHealth;
                lowestHealthUnit = unit;
            }
        }

        return lowestHealthUnit;
    }

    private void ShowHealingEffect(BaseUnit unit)
    {
        if (unit.Collider != null)
        {
            Vector3 effectPosition = unit.Collider.bounds.center;  // �ݶ��̴��� �߽� ��ġ�� ���� ��ǥ�� ����

            GameObject healingEffect = ObjectPool.Instance.GetObject("HealingEffect");
          
            healingEffect.transform.SetParent(unit.transform,false);
            healingEffect.transform.localPosition = new Vector3(0, effectPosition.y, 0);  // ����Ʈ�� ��ġ ����
            healingEffect.SetActive(true);

            ObjectPool.Instance.ReturnObjectByDelay(healingEffect, "HealingEffect", 1f);  // ���� �ð� �� ��ȯ
        }
    }

}