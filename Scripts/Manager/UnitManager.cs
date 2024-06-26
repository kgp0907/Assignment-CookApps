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
                Vector3 spawnPosition = nearestLivingPlayer.transform.position + UnityEngine.Random.insideUnitSphere * 5; // �ֺ��� ��ȯ
                spawnPosition.y = nearestLivingPlayer.transform.position.y; // ���� ����
                player.transform.SetParent(transform,false);
                player.transform.position = spawnPosition;
                player.gameObject.SetActive(true);
                player.Revive();
                player.isSkillReady = true;
            }     
        }
        else
        {
            // ��� ĳ���Ͱ� ��� �� ���� ����� ����
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
        // �� ĳ������ ��ġ ����
        return new Vector3(index * 2.0f, 0, 0);
    }

    public void SummonMonster(string tag)
    {
        if (SummonCharacterList.Count > 0)
        {
            BaseUnit nearestPlayer = SummonCharacterList[0];  // ���� �տ� �ִ� �÷��̾� ����
            Vector3 spawnPosition = GetMonsterSpawnPosition(nearestPlayer, 0); // �ش� �÷��̾� ��ó�� ���� ��ȯ ��ġ ���
            BaseUnit monster = ObjectPool.Instance.SpawnUnit(tag, spawnPosition,transform);
            //monster.transform.SetParent(transform);
            RegisterMonster(monster);
        }
    }

    Vector3 GetMonsterSpawnPosition(BaseUnit player, int index)
    {
        float spawnRadius = 5.0f; // �÷��̾�κ��� 5 ���� �Ÿ� ������ ���� ��ȯ
        float randomAngle = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad; // ���� ����
        float randomDistance = UnityEngine.Random.Range(0.5f, spawnRadius); // ���� �Ÿ� (�÷��̾�� �ʹ� ������ �ʰ� ����)

        Vector3 spawnPosition = player.transform.position + new Vector3(
            Mathf.Cos(randomAngle) * randomDistance, // X ��ǥ
            0, // Y ��ǥ (2D ������ ��� Y�� �������� �ʰų�, 3D ������ ��� ������ ���� ����)
            Mathf.Sin(randomAngle) * randomDistance  // Z ��ǥ
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

    // �÷��̾�� ���͸� ����Ʈ�� �߰��ϴ� �޼���
    public void RegisterPlayer(BaseUnit player)
    {
        SummonCharacterList.Add(player);
    }

    public void RegisterMonster(BaseUnit monster)
    {
        SummonMonsterList.Add(monster);
    }

    // �÷��̾�� ���͸� ����Ʈ���� �����ϴ� �޼���
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
        // ��� ���Ͱ� ����ߴ��� Ȯ��
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
