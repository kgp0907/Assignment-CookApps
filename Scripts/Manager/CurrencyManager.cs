using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 재화를 관리하는 스크립트
/// </summary>
public class CurrencyManager : MonoSingleton<CurrencyManager>
{
    public double Gold = 0;
    public event Action CurrencyInfUpdateAction;

    // 골드를 획득 및 이벤트로 ui업데이트
    public void AddGold(double goldCnt)
    {
        Gold+= goldCnt;

        CurrencyInfUpdateAction.Invoke();
    }

    // 골드를 사용 및 이벤트로 ui업데이트
    public void UseGold(double goldCnt)
    {
        if (Gold >= goldCnt)
        {
            Gold -= goldCnt;
        }

        CurrencyInfUpdateAction.Invoke();
    }

    // 현재 가지고 있는 골드량 확인
    public double GetGoldCnt()
    {
        return Gold;
    }

    // 소지 골드가 요구량만큼 있는지 확인
    public bool IsEnoughCurrency(double requireCnt)
    {
        if (requireCnt <= Gold)
        {
            return true;
        }

        return false;
    }
}
