using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class StageManager : MonoSingleton<StageManager>
{
    [NonSerialized]
    public Transform[] spawnPoints; // ���� ���� ��ġ
    public TextMeshProUGUI StageNumText;
    [NonSerialized]
    public int currentMonsterCount = 0; // ���� ���� ��
    public int maxMonsters = 5; // �ִ� ���� ��
    public int KillMonsterRequiredForBoss;
    public int respawnLimitPerStage = 10; // ���������� �ִ� ���� ����� Ƚ��
    public int StageNum = 1;
    public float spawnDelay = 5.0f; // ���� ���� �ֱ� (5��)
    [NonSerialized]
    public bool IsBossSpawn = false;
    private int respawnCount = 0; // ���� ������� ���� ��
    private bool isStageClear = false;

    private void Start()
    {
        InitStage();
    }

    public void InitStage()
    {
        currentMonsterCount = 0;
        respawnCount = 0;
        IsBossSpawn = false;
        isStageClear = false;
        UnitManager.Instance.InitializeUnits();

        for (int i = 0; i < maxMonsters; i++)
        {
            MonsterReSpwan();
        }

        
        StageNumText.text = StageNum.ToString();
    }

    public void HandleAllMonstersDefeated()
    {
        isStageClear = false;
        UnitManager.Instance.CheckAllMonstersDefeated();
        StartCoroutine(CoNextStage());
        // ���� �������� �ε� ����
    }

    IEnumerator CoNextStage()
    {
        isStageClear = true;
        UnitManager.Instance.KillAllMonster();
        yield return new WaitForSeconds(2.5f);
        OnStageClear();
    }

    public void MonsterReSpwan()
    {
        if (isStageClear == true)
        {
            return;
        }

        if (respawnCount > KillMonsterRequiredForBoss && IsBossSpawn == false)
        {
            IsBossSpawn = true;
            SpawnMonster("Boss");
        }
        else if (UnitManager.Instance.SummonMonsterList.Count < maxMonsters && respawnCount < respawnLimitPerStage)
        {
            SpawnMonster("Monster");
        }
       
    }

    private void SpawnMonster(string tag)
    {
        if (UnitManager.Instance.SummonMonsterList.Count < maxMonsters)
        {
            currentMonsterCount++;
            respawnCount++;
            UnitManager.Instance.SummonMonster(tag);
        }
    }

    public void OnMonsterKilled()
    {
        currentMonsterCount--;
    }

    public void OnStageClear()
    {
        isStageClear = false;
        respawnCount = 0; // ����� Ƚ�� �ʱ�ȭ
        currentMonsterCount = 0; // ���� ���� �� �ʱ�ȭ
        StageNum++;
        InitStage();
    }
}