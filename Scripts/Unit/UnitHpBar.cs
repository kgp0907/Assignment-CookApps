using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UnitHpBar : MonoBehaviour
{
    public SpriteRenderer HpBarEdge;
    public SpriteRenderer HpBarBackGround; // ���� ���� �Ͼ�� -Jun 23-08-21
    public SpriteRenderer HpBarMiddle; // �߰��� ������ -Jun 23-08-21
    public SpriteRenderer HpBarFront; // ���� ���� �ʷϻ� -Jun 23-08-21

    private BaseUnit thisUnit; // �� hpbar�� ����ϴ� ���� -Jun 23-08-21

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
        double ratio = thisUnit.CurrentHealth / (double)thisUnit.CurrentMaxHealth;  // �ִ� ü�� ��� ���� ü���� ���� ���

        ratio = System.Math.Clamp(ratio, 0f, 1f);  // ������ 0�� 1 ���̷� ����

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