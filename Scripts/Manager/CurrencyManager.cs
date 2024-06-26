using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ȭ�� �����ϴ� ��ũ��Ʈ
/// </summary>
public class CurrencyManager : MonoSingleton<CurrencyManager>
{
    public double Gold = 0;
    public event Action CurrencyInfUpdateAction;

    // ��带 ȹ�� �� �̺�Ʈ�� ui������Ʈ
    public void AddGold(double goldCnt)
    {
        Gold+= goldCnt;

        CurrencyInfUpdateAction.Invoke();
    }

    // ��带 ��� �� �̺�Ʈ�� ui������Ʈ
    public void UseGold(double goldCnt)
    {
        if (Gold >= goldCnt)
        {
            Gold -= goldCnt;
        }

        CurrencyInfUpdateAction.Invoke();
    }

    // ���� ������ �ִ� ��差 Ȯ��
    public double GetGoldCnt()
    {
        return Gold;
    }

    // ���� ��尡 �䱸����ŭ �ִ��� Ȯ��
    public bool IsEnoughCurrency(double requireCnt)
    {
        if (requireCnt <= Gold)
        {
            return true;
        }

        return false;
    }
}
