using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoSingleton<ObjectPool>
{
    [System.Serializable]
    public struct Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    public Queue<DamageFont> DamageFontQueue = new Queue<DamageFont>();
    public DamageFont DamgaeFontPrefab;

    private void Awake()
    {
        InitializePools();
    }

    void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.transform.SetParent(transform, false);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject GetObject(string tag)
    {
        if (!poolDictionary.ContainsKey(tag) || poolDictionary[tag].Count == 0)
        {
            return null; 
        }

        GameObject obj = poolDictionary[tag].Dequeue();

        obj.SetActive(true);
        return obj;
    }


    public void ReturnObject(GameObject obj, string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogError("Pool with tag " + tag + " doesn't exist.");
            return;
        }

        obj.transform.SetParent(transform, false);
        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }

    public void GetDamageFont(string _value, BaseUnit unit,FONT_TYPE type)
    {
        if(DamageFontQueue.Count > 0)
        {
            var damageFont = DamageFontQueue.Dequeue();

            damageFont.Init(_value, unit, type);

            return;
        }

        var newDamageFont = GameObject.Instantiate<DamageFont>(DamgaeFontPrefab);

        newDamageFont.Init(_value, unit, type);
    }

    public void ReStoreDamageFont(DamageFont damageFont)
    {
        if(damageFont == null)
        {
            return;
        }

        DamageFontQueue.Enqueue(damageFont);
        damageFont.transform.SetParent(transform);
        damageFont.gameObject.SetActive(false);
    }

    public BaseUnit SpawnUnit(string tag, Vector3 position, Transform parent)
    {
        GameObject obj = ObjectPool.Instance.GetObject(tag);
        if (obj == null)
        {
            Debug.Log("No objects available in pool for tag: " + tag);
            return null;
        }

        obj.transform.SetParent(parent,false);
        obj.transform.position = position;
        obj.SetActive(true);
        BaseUnit unit = obj.GetComponent<BaseUnit>();
        unit.Init(); // 초기화 로직, 필요에 따라 유닛의 상태를 리셋
        return unit;
    }

    public void ReturnObjectByDelay(GameObject obj, string tag, float timer)
    {
        StartCoroutine(CoDelayReturnObject(obj, tag,timer));
    }

    IEnumerator CoDelayReturnObject(GameObject obj, string tag,float timer)
    {
        yield return new WaitForSeconds(timer);
        ReturnObject(obj, tag);
    }

    public void DespawnUnit(GameObject unit, string tag)
    {
        unit.SetActive(false); // 유닛 비활성화
        ObjectPool.Instance.ReturnObject(unit, tag); // 오브젝트 풀로 반환
    }
}