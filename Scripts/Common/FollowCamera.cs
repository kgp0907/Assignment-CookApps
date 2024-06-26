using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform Target; // ī�޶� ���� ���
    public Vector3 Offset = new Vector3(0f, 2f, -10f); // ī�޶�� ��� ������ ������
    public float SmoothSpeed = 1f; // ī�޶� �������� �ε巯�� ����

    private UnitManager unitManager = null;

    private void Start()
    {
        unitManager = UnitManager.Instance;
    }

    private void Update()
    {
        UpdateTarget(); // Ÿ�� ������Ʈ
        if (Target != null)
        {
            FollowTarget(); // Ÿ�� ���󰡱�
        }
    }

    void UpdateTarget()
    {
        if (unitManager.SummonCharacterList.Count == 0)
        {
            Target = null; // ����Ʈ�� ����ִٸ� Ÿ���� ����
            return;
        }

        foreach (BaseUnit unit in unitManager.SummonCharacterList)
        {
            if (unit != null && unit.gameObject.activeInHierarchy && unit.UNIT_STATE != UNIT_STATE.DEAD && unit.Data.ClassType == ClassType.Knight)
            {
                Target = unit.transform;
                return; // ù ��° ����ִ� ������ Ÿ������ ����
            }
        }

        // ��Ŀ�� �׾��� ���, �ٸ� ����ִ� ĳ���͸� Ÿ������ ����
        foreach (BaseUnit unit in unitManager.SummonCharacterList)
        {
            if (unit != null && unit.gameObject.activeInHierarchy && unit.UNIT_STATE != UNIT_STATE.DEAD)
            {
                Target = unit.transform;
                return; // ù ��° ����ִ� �ٸ� ������ Ÿ������ ����
            }
        }

        // ��� ������ �׾��ٸ� Ÿ�� ����
        Target = null;
    }

    void FollowTarget()
    {
        Vector3 desiredPosition = Target.position + Offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed);
        transform.position = smoothedPosition;

        // ī�޶� �׻� ����� �ٶ󺸵��� ����
        transform.LookAt(Target);
    }
}
