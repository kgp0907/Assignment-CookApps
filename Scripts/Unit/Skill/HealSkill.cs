using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSkill : Skill
{
    public CharacterUnit CharacterUnit;

    public override void Activate(BaseUnit user, BaseUnit target = null)
    {
        // 팀 내에서 가장 체력이 낮은 유닛 찾기
        BaseUnit lowestHealthUnit = FindLowestHealthUnit();

        if (lowestHealthUnit != null)
        {
            // 치유 대상이 정해진 경우, 그 대상을 치유
            ApplyEffect(lowestHealthUnit);
            ShowHealingEffect(lowestHealthUnit);
            float healValue = CharacterUnit.Data.AttackPower * skillCoefficient;
            ObjectPool.Instance.GetDamageFont(healValue.ToString(), lowestHealthUnit, FONT_TYPE.HEAL);
        }

        CharacterUnit.UNIT_STATE = UNIT_STATE.MOVE;
    }

    public override void ApplyEffect(BaseUnit target)
    {
        // 대상 유닛 치유
        target.Heal(CharacterUnit.CurrentAttackPower * skillCoefficient);
    }

    private BaseUnit FindLowestHealthUnit()
    {
        // user와 같은 팀의 모든 유닛들을 반복하여 체력이 가장 낮은 유닛 찾기
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
            Vector3 effectPosition = unit.Collider.bounds.center;  // 콜라이더의 중심 위치를 월드 좌표로 얻음

            GameObject healingEffect = ObjectPool.Instance.GetObject("HealingEffect");
          
            healingEffect.transform.SetParent(unit.transform,false);
            healingEffect.transform.localPosition = new Vector3(0, effectPosition.y, 0);  // 이펙트의 위치 설정
            healingEffect.SetActive(true);

            ObjectPool.Instance.ReturnObjectByDelay(healingEffect, "HealingEffect", 1f);  // 일정 시간 후 반환
        }
    }

}