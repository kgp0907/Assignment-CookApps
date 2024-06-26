using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UnitHpBar : MonoBehaviour
{
    public SpriteRenderer HpBarEdge;
    public SpriteRenderer HpBarBackGround; // 가장 뒤의 하얀색 -Jun 23-08-21
    public SpriteRenderer HpBarMiddle; // 중간의 붉은색 -Jun 23-08-21
    public SpriteRenderer HpBarFront; // 가장 앞의 초록색 -Jun 23-08-21

    private BaseUnit thisUnit; // 이 hpbar가 담당하는 유닛 -Jun 23-08-21

    public void Init(BaseUnit _unit)
    {
        transform.SetParent(_unit.transform);
        gameObject.SetActive(true);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        thisUnit = _unit;

        _unit.HpChangeAction += WhenDamage;

        HpBarFront.transform.localScale = HpBarBackGround.transform.localScale;
        HpBarMiddle.transform.localScale = HpBarBackGround.transform.localScale;

        if (_unit.Data.UNIT_TYPE == UNIT_TYPE.ENEMY)
        {
            ColorUtility.TryParseHtmlString("#FFC979", out Color middleColor);
            HpBarMiddle.color = middleColor;
            ColorUtility.TryParseHtmlString("#EE1515", out Color color);
            HpBarFront.color = color;

        }
        else
        {
            ColorUtility.TryParseHtmlString("#E23410", out Color middleColor);
            HpBarMiddle.color = middleColor;
            ColorUtility.TryParseHtmlString("#0FFF00", out Color color);
            HpBarFront.color = color;
        }
    }

    public void InActive()
    {
        thisUnit.HpChangeAction -= WhenDamage;
        thisUnit = null;
        gameObject.SetActive(false);
    }

    public void FadeOutAnimPlay()
    {
        GetComponent<Animator>().SetTrigger("FadeOut");
    }

    public void SetHpbar()
    {
        double ratio = thisUnit.CurrentHealth / (double)thisUnit.CurrentMaxHealth;  // 최대 체력 대비 현재 체력의 비율 계산

        ratio = System.Math.Clamp(ratio, 0f, 1f);  // 비율을 0과 1 사이로 제한

        DOTween.Kill(this);

        HpBarFront.transform.localScale = new Vector2((float)ratio * HpBarBackGround.transform.localScale.x, HpBarBackGround.transform.localScale.y);
        HpBarMiddle.transform.localScale = new Vector2((float)ratio * HpBarBackGround.transform.localScale.x, HpBarBackGround.transform.localScale.y);
    }

    private void WhenDamage()
    {
        double ratio = thisUnit.CurrentHealth / thisUnit.CurrentMaxHealth;

        ratio = System.Math.Clamp(ratio, 0f, 1f);

        HpBarFront.transform.localScale = new Vector2((float)ratio * HpBarBackGround.transform.localScale.x, HpBarBackGround.transform.localScale.y);
        HpBarMiddle.transform.DOScaleX(HpBarFront.transform.localScale.x, 0.5f).OnComplete(() =>
        {
            if (thisUnit is not null && thisUnit.UNIT_STATE != UNIT_STATE.DEAD)
            {
                HpBarMiddle.transform.localScale = new Vector2(HpBarFront.transform.localScale.x, HpBarBackGround.transform.localScale.y);
            }
        });
    }

    private void SetPosition()
    {
        transform.position = new Vector3(thisUnit.transform.position.x, thisUnit.transform.position.y - 0.1f, thisUnit.transform.position.z);
        transform.rotation = Quaternion.Euler(45, 0, 0);
    }
}