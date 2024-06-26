using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoSingleton<UnitManager>
{
    public List<CharacterData> OriginCharacterData = new List<CharacterData>();
    public List<EnemyData> OriginEnemyData = new List<EnemyData>();

    [NonSerialized]
    public List<BaseUnit> SummonCharacterList = new List<BaseUnit>();
    [NonSerialized]
    public List<BaseUnit> SummonMonsterList = new List<BaseUnit>();
    public List<BaseUnit> ArrivaledUnitList = new List<BaseUnit>();
    private string[] characterTypes = { "Knight", "Thief", "Archer", "Priest" };

    public void InitializeUnits()
    {
        StopAllCoroutines();
        ClearAllUnits();

        for (int i = 0; i < characterTypes.Length; i++)
        {
            SpawnCharacter(i);
        }
    }

    private void ClearAllUnits()
    {
        foreach (var player in SummonCharacterList)
        {
            ObjectPool.Instance.ReturnObject(player.gameObject, player.Data.Name);
        }
        foreach (var monster in SummonMonsterList)
        {
            ObjectPool.Instance.ReturnObject(monster.gameObject, monster.Data.Name);
        }
        SummonCharacterList.Clear();
        SummonMonsterList.Clear();
    }

    private void SpawnCharacter(int index)
    {
        Vector3 spawnPosition = GetPlayerSpawnPosition(index);
        CharacterUnit player = ObjectPool.Instance.SpawnUnit(characterTypes[index], spawnPosition,transform) as CharacterUnit;
        if(player != null)
        {
            player.transform.SetParent(transform,false);
            player.transform.position = spawnPosition;
            player.Init();
            RegisterPlayer(player);

            UiController.Instance.InputDataOnFieldCharacterUi(player, index);

            player.OnDeath += () => StartCoroutine(RespawnPlayer(player, 4f));
        }

    }

    public void AllReturnCharacterObject()
    {
        for(int i = 0;i < SummonCharacterList.Count;i++) { }
    }

    IEnumerator RespawnPlayer(CharacterUnit player, float delay)
    {     
        if (SummonCharacterList.Count > 0)
        {
            yield return new WaitForSeconds(delay);
            if (SummonCharacterList.Count > 0)
            {
                BaseUnit nearestLivingPlayer = SummonCharacterList[UnityEngine.Random.Range(0, SummonCharacterList.Count)];
                Vector3 spawnPosition = nearestLivingPlayer.transform.position + UnityEngine.Random.insideUnitSphere * 5; // 주변에 소환
                spawnPosition.y = nearestLivingPlayer.transform.position.y; // 높이 조정
                player.transform.SetParent(transform,false);
                player.transform.position = spawnPosition;
                player.gameObject.SetActive(true);
                player.Revive();
                player.isSkillReady = true;
            }     
        }
        else
        {
            // 모든 캐릭터가 사망 시 게임 재시작 로직
            UiController.Instance.ActiveGameOverTextObj();
            yield return new WaitForSeconds(1.5f);           
            StageManager.Instance.InitStage();
        }
    }

    public void KillAllMonster()
    {
        for (int i = 0; i < SummonMonsterList.Count; i++)
        {
            SummonMonsterList[i].CurrentHealth = 0;
            SummonMonsterList[i].HpChangeAction.Invoke();
            SummonMonsterList[i].UNIT_STATE = UNIT_STATE.DEAD;
            SummonMonsterList[i].Animator.SetTrigger("Dead");
        }
    }

    Vector3 GetPlayerSpawnPosition(int index)
    {
        // 각 캐릭터의 위치 조정
        return new Vector3(index * 2.0f, 0, 0);
    }

    public void SummonMonster(string tag)
    {
        if (SummonCharacterList.Count > 0)
        {
            BaseUnit nearestPlayer = SummonCharacterList[0];  // 가장 앞에 있는 플레이어 선택
            Vector3 spawnPosition = GetMonsterSpawnPosition(nearestPlayer, 0); // 해당 플레이어 근처에 몬스터 소환 위치 계산
            BaseUnit monster = ObjectPool.Instance.SpawnUnit(tag, spawnPosition,transform);
            //monster.transform.SetParent(transform);
            RegisterMonster(monster);
        }
    }

    Vector3 GetMonsterSpawnPosition(BaseUnit player, int index)
    {
        float spawnRadius = 5.0f; // 플레이어로부터 5 유닛 거리 내에서 몬스터 소환
        float randomAngle = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad; // 랜덤 각도
        float randomDistance = UnityEngine.Random.Range(0.5f, spawnRadius); // 랜덤 거리 (플레이어와 너무 가깝지 않게 설정)

        Vector3 spawnPosition = player.transform.position + new Vector3(
            Mathf.Cos(randomAngle) * randomDistance, // X 좌표
            0, // Y 좌표 (2D 게임의 경우 Y는 변경하지 않거나, 3D 게임의 경우 적절한 높이 설정)
            Mathf.Sin(randomAngle) * randomDistance  // Z 좌표
        );

        return spawnPosition;
    }

    public bool ArrivalCheck()
    {
        if (ArrivaledUnitList.Count == 0)
            return false;

        if (ArrivaledUnitList.Count >= SummonMonsterList.Count + SummonCharacterList.Count)
            return true;

        return false;
    }

    public float XMaxPos { get => 3.8f; }

    public bool ObjInUnitCameraCheck(Transform _tf)
    {
        if (_tf.position.x >= XMaxPos ||
            _tf.position.x <= -XMaxPos ||
            _tf.position.z >= XMaxPos ||
            _tf.position.z <= -XMaxPos)
        {
            return false;
        }
        return true;
    }

    // 플레이어와 몬스터를 리스트에 추가하는 메서드
    public void RegisterPlayer(BaseUnit player)
    {
        SummonCharacterList.Add(player);
    }

    public void RegisterMonster(BaseUnit monster)
    {
        SummonMonsterList.Add(monster);
    }

    // 플레이어와 몬스터를 리스트에서 제거하는 메서드
    public void RemovePlayer(BaseUnit player)
    {
        SummonCharacterList.Remove(player);
    }

    public void RemoveMonster(BaseUnit monster)
    {
        SummonMonsterList.Remove(monster);
    }

    public void CheckAllMonstersDefeated()
    {
        // 모든 몬스터가 사망했는지 확인
        if (StageManager.Instance.IsBossSpawn==true)
        {
            UiController.Instance.ActiveClearTextObj();

            VictoryMotion();          
        }
    }

   
    public void VictoryMotion()
    {
        for (int i = 0; i < SummonCharacterList.Count; i++)
        {
            SummonCharacterList[i].UNIT_STATE = UNIT_STATE.VICTORY;
            SummonCharacterList[i].Animator.SetTrigger("Victory");
        }
    }


    public void UnitPreDead(BaseUnit _unit)
    {
        if(_unit.Data.UNIT_TYPE == UNIT_TYPE.ENEMY)
        {
            RemoveMonster(_unit);
        }
        else
        {
            RemovePlayer(_unit);
        }
    }

    private UnitManager() { }
}
