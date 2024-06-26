using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform Target; // 카메라가 따라갈 대상
    public Vector3 Offset = new Vector3(0f, 2f, -10f); // 카메라와 대상 사이의 오프셋
    public float SmoothSpeed = 1f; // 카메라 움직임의 부드러움 정도

    private UnitManager unitManager = null;

    private void Start()
    {
        unitManager = UnitManager.Instance;
    }

    private void Update()
    {
        UpdateTarget(); // 타겟 업데이트
        if (Target != null)
        {
            FollowTarget(); // 타겟 따라가기
        }
    }

    void UpdateTarget()
    {
        if (unitManager.SummonCharacterList.Count == 0)
        {
            Target = null; // 리스트가 비어있다면 타겟을 해제
            return;
        }

        foreach (BaseUnit unit in unitManager.SummonCharacterList)
        {
            if (unit != null && unit.gameObject.activeInHierarchy && unit.UNIT_STATE != UNIT_STATE.DEAD && unit.Data.ClassType == ClassType.Knight)
            {
                Target = unit.transform;
                return; // 첫 번째 살아있는 유닛을 타겟으로 설정
            }
        }

        // 탱커가 죽었을 경우, 다른 살아있는 캐릭터를 타겟으로 설정
        foreach (BaseUnit unit in unitManager.SummonCharacterList)
        {
            if (unit != null && unit.gameObject.activeInHierarchy && unit.UNIT_STATE != UNIT_STATE.DEAD)
            {
                Target = unit.transform;
                return; // 첫 번째 살아있는 다른 유닛을 타겟으로 설정
            }
        }

        // 모든 유닛이 죽었다면 타겟 해제
        Target = null;
    }

    void FollowTarget()
    {
        Vector3 desiredPosition = Target.position + Offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed);
        transform.position = smoothedPosition;

        // 카메라가 항상 대상을 바라보도록 설정
        transform.LookAt(Target);
    }
}
