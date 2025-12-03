using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class Unit : MonoBehaviour
{
    [Header("Stats")]
    public string unitName;
    public int maxHP = 50;
    public int currentHP;
    public int agility = 10;
    public int currentPP = 2;

    [Header("Position")]
    public int gridX;
    public int gridY;

    public void Init(int startX, int startY)
    {
        currentHP = maxHP;
        gridX = startX;
        gridY = startY;

        // 시작 위치로 즉시 이동
        transform.position = GridManager.Instance.GetWorldPosition(startX, startY);
    }

    public void Move(int moveAmount)
    {
        // 간단한 테스트를 위해 '위쪽(Y+)'으로만 이동하게 해봅시다.
        // 나중에는 '바라보는 방향'으로 이동하게 수정할 겁니다.
        
        int targetY = gridY + moveAmount;

        // 맵 밖으로 나가는지 체크 (0 ~ 4 사이여야 함)
        if (targetY >= GridManager.Instance.height)
        {
            targetY = GridManager.Instance.height - 1; // 맵 끝에 걸림
            Debug.Log("더 이상 갈 수 없습니다!");
        }

        gridY = targetY; // 내부 좌표 업데이트

        // 실제 화면상 이동 (DoTween) - 0.5초 동안 점프하듯 이동
        Vector3 targetPos = GridManager.Instance.GetWorldPosition(gridX, gridY);
        transform.DOJump(targetPos, 0.5f, 1, 0.5f);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = maxHP;
    }

    // 이동 함수 (DoTween 사용)
    public void MoveTo(Vector3 targetPos)
    {
        // 0.5초 동안 targetPos로 이동
        transform.DOMove(targetPos, 0.5f).SetEase(Ease.OutQuad);
    }

    // 피격 및 넉백 테스트용 함수
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{unitName}이(가) {damage}의 피해를 입었습니다! 남은 체력: {currentHP}");
        
        // 피격 연출 (살짝 흔들리기)
        transform.DOShakePosition(0.2f, 0.5f);
    }
}
