using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Data", menuName = "Entity Data/Character")]
public class CharacterData : UnitData
{
    public GameObject CharacterPrefab;

    // ĳ���� ���� ����
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

        // ����ġ�� ���� ������ �䱸ġ�� �����ϴ��� Ȯ��
        while (Exp >= CurrentExpRequirement)
        {
            // ����ġ�� ����ϴٸ� ���� ��
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Exp -= CurrentExpRequirement; // ���� ������ ����ġ �䱸����ŭ ����
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
