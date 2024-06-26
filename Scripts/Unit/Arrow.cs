using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 15f; // 화살의 속도를 조금 더 빠르게 설정
    public Transform target;  // 타겟의 위치

    private Vector3 direction;
    private BaseUnit player;
    private bool isFlying = false; // 화살이 날아가고 있는지의 여부

    // 타겟 설정 및 화살 발사
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
            // 목표 위치와 현재 위치 사이의 방향 벡터 계산
            Vector3 targetDirection = target.position - transform.position;

            // 화살이 타겟을 정확하게 바라보도록 설정
            transform.rotation = Quaternion.LookRotation(targetDirection);

            // 방향 벡터를 정규화하고 속도를 곱하여 화살 이동
            transform.position += targetDirection.normalized * speed * Time.deltaTime;

            // 타겟에 도달했는지 검사
            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                ReachTarget();
            }
        }
    }

    // 타겟에 도달했을 때 호출되는 메소드
    private void ReachTarget()
    {
        isFlying = false; // 날아가는 상태를 중지
        ObjectPool.Instance.ReturnObject(gameObject, "Arrow"); // 화살을 오브젝트 풀로 반환
        gameObject.SetActive(false); // 활성 상태 비활성화
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == target)
        {
            // 타겟에 닿으면 실행될 로직
            ObjectPool.Instance.ReturnObject(gameObject, "Arrow"); // 오브젝트 풀로 화살 반환
            player.Attack();
            gameObject.SetActive(false); // 화살 비활성화
        }
    }

    IEnumerator ReturnArrow()
    {
        yield return new WaitForSeconds(1.5f);
        ReachTarget();
    }
}