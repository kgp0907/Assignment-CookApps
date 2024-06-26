using System;
using UnityEngine;

public abstract class UnitData : ScriptableObject
{
    public float MaxHealth;
    public float AttackPower;
    public float AttackRange;
    public float AttackCooldown;
    public ClassType ClassType;
    public UNIT_TYPE UNIT_TYPE;
    public string Name;
}