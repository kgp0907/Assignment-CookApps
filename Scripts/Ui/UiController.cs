using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiController : MonoSingleton<UiController>
{
    public UpgradePopup UpgradePopup;
    public TextMeshProUGUI GoldCnt;

    public GameObject ClearTextObj;
    public GameObject GameOverTextObj;
    public OnFieldCharacterUi[] onFieldCharacterUis;

    private void Start()
    {
        CurrencyManager.Instance.CurrencyInfUpdateAction -= GoldCntUpdate;
        CurrencyManager.Instance.CurrencyInfUpdateAction += GoldCntUpdate;
        GoldCntUpdate();
        UpgradePopup.Init();
    }

    public void GoldCntUpdate()
    {
        GoldCnt.text = CurrencyManager.Instance.GetGoldCnt().ToString();
    }

    public void InputDataOnFieldCharacterUi(CharacterUnit unit,int index)
    {
        if (unit != null)
        {
            if (onFieldCharacterUis.Length > index)
            {
                onFieldCharacterUis[index].LinkCharacter(unit);
            }
            if (UpgradePopup.UpgradeCells.Length > index)
            {
                UpgradePopup.UpgradeCells[index].SetData(unit.Data as CharacterData);
            }
        }     
    }

    public void ActiveClearTextObj()
    {
        StartCoroutine(CoDelayActiveClearTextObj());
    }

    IEnumerator CoDelayActiveClearTextObj()
    {
        ClearTextObj.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        ClearTextObj.SetActive(false);
    }

    public void ActiveGameOverTextObj()
    {
        StartCoroutine(CoDelayActiveGameOverTextObj());
    }

    IEnumerator CoDelayActiveGameOverTextObj()
    {
        GameOverTextObj.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        GameOverTextObj.SetActive(false);
    }

    public void OpenUpgradePopup()
    {
        UpgradePopup.OpenPopup();
    }
}
