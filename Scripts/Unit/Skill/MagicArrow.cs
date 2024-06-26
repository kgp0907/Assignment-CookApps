using System.Collections;
using UnityEngine;

public class MagicArrow : MonoBehaviour
{
    public float SkillConfficent;
    public float speed = 15f; // ȭ���� �ӵ�
    public Transform target;  // Ÿ���� Transform
    private BaseUnit player;  // ȭ���� �߻��� �÷��̾�
    private bool isFlying = false; // ȭ���� ���ư��� ��������

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
            // Ÿ�� ���� ���
            Vector3 targetDirection = target.position - transform.position;

            // ȭ���� ������ Ÿ�� �������� ����
            transform.rotation = Quaternion.LookRotation(targetDirection);

            // ȭ�� �̵�
            transform.position += targetDirection.normalized * speed * Time.deltaTime;

            // Ÿ�ٿ� �����ߴ��� Ȯ��
            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                ReachTarget();
            }
        }
    }

    // Ÿ�ٿ� �������� �� ����
    private void ReachTarget()
    {
        isFlying = false;
        player.Attack();
        ObjectPool.Instance.ReturnObject(gameObject, "MagicArrow");
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == target)
        {
            // Ÿ�ٿ� ������ ����� ����
            ObjectPool.Instance.ReturnObject(gameObject, "MagicArrow");
            player.AttackBySkillConfficnt(SkillConfficent);
            gameObject.SetActive(false); // ȭ�� ��Ȱ��ȭ
        }
    }

    // ���� �ð� �Ŀ� ȭ���� �ڵ����� ��ȯ
    IEnumerator ReturnArrow()
    {
        yield return new WaitForSeconds(1.5f);

        if (isFlying)
        {
            ReachTarget();  // ���� Ÿ�ٿ� �������� �ʾҴٸ� ȭ���� ��ȯ
        }      
    }
}
