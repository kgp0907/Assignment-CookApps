using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePopup : MonoBehaviour
{
    public UpgradeCell[] UpgradeCells;

    public void Init()
    {
        gameObject.SetActive(false);
        CurrencyManager.Instance.CurrencyInfUpdateAction -= UpdateCells;
        CurrencyManager.Instance.CurrencyInfUpdateAction += UpdateCells;
    }

    public void UpdateCells()
    {
        for (int i = 0; i < UpgradeCells.Length; i++)
        {
            UpgradeCells[i].UpdateUI();
        }
    }

    public void OpenPopup()
    {
        gameObject.SetActive(true);
        UpdateCells();
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}
