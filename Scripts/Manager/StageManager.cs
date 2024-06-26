using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class StageManager : MonoSingleton<StageManager>
{
    [NonSerialized]
    public Transform[] spawnPoints; // 몬스터 생성 위치
    public TextMeshProUGUI StageNumText;
    [NonSerialized]
    public int currentMonsterCount = 0; // 현재 몬스터 수
    public int maxMonsters = 5; // 최대 몬스터 수
    public int KillMonsterRequiredForBoss;
    public int respawnLimitPerStage = 10; // 스테이지당 최대 몬스터 재생성 횟수
    public int StageNum = 1;
    public float spawnDelay = 5.0f; // 몬스터 생성 주기 (5초)
    [NonSerialized]
    public bool IsBossSpawn = false;
    private int respawnCount = 0; // 현재 재생성된 몬스터 수
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
        // 다음 스테이지 로딩 로직
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
        respawnCount = 0; // 재생성 횟수 초기화
        currentMonsterCount = 0; // 현재 몬스터 수 초기화
        StageNum++;
        InitStage();
    }
}