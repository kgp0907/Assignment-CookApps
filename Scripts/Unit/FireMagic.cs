using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FireMagic : MonoBehaviour
{
    private BaseUnit player;
    public float speed = 15f; // 화살의 속도를 조금 더 빠르게 설정
    public Transform target;  // 타겟의 위치

    private bool isFlying = false; // 화살이 날아가고 있는지의 여부

    // 타겟 설정 및 화살 발사
    public void LaunchArrow(BaseUnit playerUnit)
    {
        player = playerUnit;
        target = player.AttackTarget.transform;
        isFlying = true;

        StartCoroutine(MoveTowardsTarget()); // 타겟을 향해 이동 시작
    }

    IEnumerator MoveTowardsTarget()
    {
        while (isFlying && target != null)
        {
            // 타겟 위치로 이동
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            // 타겟에 도달했는지 검사
            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                // 타겟에 도달 시 수행할 작업
                ReachTarget();
                break;
            }
            yield return null;
        }
    }

    // 타겟에 도달했을 때 호출되는 메소드
    private void ReachTarget()
    {
        isFlying = false; // 날아가는 상태를 중지
        // 타겟에게 데미지를 주는 로직을 여기에 추가하거나 외부 이벤트를 트리거할 수 있습니다.
        ObjectPool.Instance.ReturnObject(gameObject, "Fire"); // 화살을 오브젝트 풀로 반환
        gameObject.SetActive(false); // 활성 상태 비활성화
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == target)
        {
            // 타겟에 닿으면 실행될 로직
            ObjectPool.Instance.ReturnObject(gameObject, "Fire"); // 오브젝트 풀로 화살 반환
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
