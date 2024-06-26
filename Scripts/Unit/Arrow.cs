using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 15f; // ȭ���� �ӵ��� ���� �� ������ ����
    public Transform target;  // Ÿ���� ��ġ

    private Vector3 direction;
    private BaseUnit player;
    private bool isFlying = false; // ȭ���� ���ư��� �ִ����� ����

    // Ÿ�� ���� �� ȭ�� �߻�
    public void LaunchArrow(BaseUnit playerUnit)
    {
        player = playerUnit;
        target = player.AttackTarget.transform;
        isFlying = true;
        StartCoroutine(ReturnArrow());
    }

    void Update()
    {
        if (isFlying && target != null)
        {
            // ��ǥ ��ġ�� ���� ��ġ ������ ���� ���� ���
            Vector3 targetDirection = target.position - transform.position;

            // ȭ���� Ÿ���� ��Ȯ�ϰ� �ٶ󺸵��� ����
            transform.rotation = Quaternion.LookRotation(targetDirection);

            // ���� ���͸� ����ȭ�ϰ� �ӵ��� ���Ͽ� ȭ�� �̵�
            transform.position += targetDirection.normalized * speed * Time.deltaTime;

            // Ÿ�ٿ� �����ߴ��� �˻�
            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                ReachTarget();
            }
        }
    }

    // Ÿ�ٿ� �������� �� ȣ��Ǵ� �޼ҵ�
    private void ReachTarget()
    {
        isFlying = false; // ���ư��� ���¸� ����
        ObjectPool.Instance.ReturnObject(gameObject, "Arrow"); // ȭ���� ������Ʈ Ǯ�� ��ȯ
        gameObject.SetActive(false); // Ȱ�� ���� ��Ȱ��ȭ
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == target)
        {
            // Ÿ�ٿ� ������ ����� ����
            ObjectPool.Instance.ReturnObject(gameObject, "Arrow"); // ������Ʈ Ǯ�� ȭ�� ��ȯ
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