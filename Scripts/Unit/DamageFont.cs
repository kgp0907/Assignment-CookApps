using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DamageFont : MonoBehaviour
{
    public TextMeshPro text;

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        transform.rotation = Quaternion.Euler(45, 0, 0);
    }

    public void Init(string _s, BaseUnit _unit, FONT_TYPE _Type)
    {
        text.text = _s;

        gameObject.transform.localScale = Vector3.one * 0.8f;
        gameObject.transform.SetParent(_unit.transform);

        gameObject.transform.localPosition = new Vector3(0, 0, 0.5f);
        gameObject.SetActive(true);

        Color topColor = Color.white;
        Color bottomColor = Color.white;

        switch (_Type)
        {
            case FONT_TYPE.HIT:
                if (_unit.Data.UNIT_TYPE == UNIT_TYPE.CHARACTER)
                {
                    topColor = new Color32(200, 36, 36, 255);
                    bottomColor = new Color32(200, 36, 36, 255);
                }
                else
                {
                    topColor = Color.white;
                    bottomColor = Color.white;
                }
                break;
            case FONT_TYPE.HEAL:
                topColor = new Color32(182, 255, 41, 255);
                bottomColor = new Color32(0, 245, 12, 255);
                break;
        }

        PlayNumberTweening(_unit.Collider);

        text.colorGradient = new VertexGradient(topColor, topColor, bottomColor, bottomColor);

    }

    private void PlayNumberTweening(Collider collider)
    {
        // 캐릭터의 렌더러 또는 콜라이더로부터 높이를 얻습니다.
        float characterHeight = collider.bounds.size.y;

        // 트윈이 시작할 위치를 캐릭터의 현재 위치 + 캐릭터의 높이로 설정합니다.
        float startHeight = transform.position.y + characterHeight;

        // 모든 데미지 폰트가 같은 높이만큼 올라가도록 설정합니다
        Vector3 endPosition = new Vector3(transform.position.x, startHeight+0.3f, transform.position.z);

        // DOTween을 사용하여 데미지 폰트를 원하는 위치로 이동시킵니다.
        transform.DOMoveY(endPosition.y, 0.5f).OnComplete(() =>
        {
            ObjectPool.Instance.ReStoreDamageFont(this);
        });
    }
}
