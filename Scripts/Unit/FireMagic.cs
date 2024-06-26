using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FireMagic : MonoBehaviour
{
    private BaseUnit player;
    public float speed = 15f; // ȭ���� �ӵ��� ���� �� ������ ����
    public Transform target;  // Ÿ���� ��ġ

    private bool isFlying = false; // ȭ���� ���ư��� �ִ����� ����

    // Ÿ�� ���� �� ȭ�� �߻�
    public void LaunchArrow(BaseUnit playerUnit)
    {
        player = playerUnit;
        target = player.AttackTarget.transform;
        isFlying = true;

        StartCoroutine(MoveTowardsTarget()); // Ÿ���� ���� �̵� ����
    }

    IEnumerator MoveTowardsTarget()
    {
        while (isFlying && target != null)
        {
            // Ÿ�� ��ġ�� �̵�
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            // Ÿ�ٿ� �����ߴ��� �˻�
            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                // Ÿ�ٿ� ���� �� ������ �۾�
                ReachTarget();
                break;
            }
            yield return null;
        }
    }

    // Ÿ�ٿ� �������� �� ȣ��Ǵ� �޼ҵ�
    private void ReachTarget()
    {
        isFlying = false; // ���ư��� ���¸� ����
        // Ÿ�ٿ��� �������� �ִ� ������ ���⿡ �߰��ϰų� �ܺ� �̺�Ʈ�� Ʈ������ �� �ֽ��ϴ�.
        ObjectPool.Instance.ReturnObject(gameObject, "Fire"); // ȭ���� ������Ʈ Ǯ�� ��ȯ
        gameObject.SetActive(false); // Ȱ�� ���� ��Ȱ��ȭ
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == target)
        {
            // Ÿ�ٿ� ������ ����� ����
            ObjectPool.Instance.ReturnObject(gameObject, "Fire"); // ������Ʈ Ǯ�� ȭ�� ��ȯ
            player.Attack();
            gameObject.SetActive(false); // ȭ�� ��Ȱ��ȭ
        }
    }

    IEnumerator ReturnArrow()
    {
        yield return new WaitForSeconds(1.5f);
        ReachTarget();
    }
}
