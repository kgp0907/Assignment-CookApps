using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Data", menuName = "Entity Data/Character")]
public class CharacterData : UnitData
{
    public GameObject CharacterPrefab;

    // 캐릭터 전용 변수
    public float SkillRange;
    public float SkillCooldown;
    public float RespawnTime;
    public float Exp;
    public int UpgradeCnt = 0;
    public int Level = 1;

    public Sprite Portrait;
    private const int BaseExpRequirement = 10;


    public int CurrentExpRequirement => BaseExpRequirement * Level;

    public void GainExp(int amount)
    {
        Exp += amount;

        // 경험치가 현재 레벨의 요구치를 충족하는지 확인
        while (Exp >= CurrentExpRequirement)
        {
            // 경험치가 충분하다면 레벨 업
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Exp -= CurrentExpRequirement; // 현재 레벨의 경험치 요구량만큼 차감
        Level++;
        MaxHealth+=Const.AddMaxHealthByLevelUp;
    }

    public void Upgrade()
    {
        UpgradeCnt++;
        AttackPower++;
        MaxHealth+= Const.AddMaxHealthByUpgrade;
    }
}
