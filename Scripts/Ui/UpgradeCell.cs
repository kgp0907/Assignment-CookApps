using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCell : MonoBehaviour
{
    public Button UpgradeBtn;
    public Image Portrait;
    public TextMeshProUGUI HpValue;
    public TextMeshProUGUI AtkValue; 
    public TextMeshProUGUI RequireGoldCnt;
    private CharacterData characterData;

    public void SetData(CharacterData data)
    {
        if (data != null)
        {
            characterData = data;
            UpdateUI();
            
        }
    }

    public void UpdateUI()
    {
        if (characterData != null)
        {
            double requireUpgradeGold = Const.RequireUpgradeGoldPerCnt * (characterData.UpgradeCnt + 1);

            UpgradeBtn.interactable = CurrencyManager.Instance.IsEnoughCurrency(requireUpgradeGold);
            Portrait.sprite = characterData.Portrait;
            HpValue.text = characterData.MaxHealth.ToString();
            AtkValue.text = characterData.AttackPower.ToString();
            RequireGoldCnt.text = requireUpgradeGold.ToString();
        }
    }

    public void OnClickLevelUpBtn()
    {
        double requireUpgradeGold = Const.RequireUpgradeGoldPerCnt * (characterData.UpgradeCnt + 1);

        if (CurrencyManager.Instance.IsEnoughCurrency(requireUpgradeGold))
        {         
            CurrencyManager.Instance.UseGold(requireUpgradeGold);
            characterData.Upgrade();
            UpdateUI();
        }
    }
}
