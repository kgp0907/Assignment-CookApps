using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Entity Data/Enemy")]
public class EnemyData : UnitData
{
    public GameObject EnemyPrefab;

    // ���� ���� ����
    public int AttacksPerSecond;
    public int ChaseRange = 4;
    public float SpawnRate;
}
