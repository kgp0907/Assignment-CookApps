using System.Collections;
using UnityEngine;

public class MagicArrow : MonoBehaviour
{
    public float SkillConfficent;
    public float speed = 15f; // 화살의 속도
    public Transform target;  // 타겟의 Transform
    private BaseUnit player;  // 화살을 발사한 플레이어
    private bool isFlying = false; // 화살이 날아가는 상태인지

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
            // 타겟 방향 계산
            Vector3 targetDirection = target.position - transform.position;

            // 화살의 방향을 타겟 방향으로 설정
            transform.rotation = Quaternion.LookRotation(targetDirection);

            // 화살 이동
            transform.position += targetDirection.normalized * speed * Time.deltaTime;

            // 타겟에 도달했는지 확인
            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                ReachTarget();
            }
        }
    }

    // 타겟에 도달했을 때 실행
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
            // 타겟에 닿으면 실행될 로직
            ObjectPool.Instance.ReturnObject(gameObject, "MagicArrow");
            player.AttackBySkillConfficnt(SkillConfficent);
            gameObject.SetActive(false); // 화살 비활성화
        }
    }

    // 일정 시간 후에 화살을 자동으로 반환
    IEnumerator ReturnArrow()
    {
        yield return new WaitForSeconds(1.5f);

        if (isFlying)
        {
            ReachTarget();  // 아직 타겟에 도달하지 않았다면 화살을 반환
        }      
    }
}
