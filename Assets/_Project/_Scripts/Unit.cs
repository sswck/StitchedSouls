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

    public void Attack()
    {
        // 1. 공격 연출 (앞으로 살짝 찌르기)
        Vector3 punchPos = transform.position + new Vector3(0, 0, 0.5f);
        transform.DOMove(punchPos, 0.1f).SetLoops(2, LoopType.Yoyo);

        // 2. 내 앞(gridY + 1)에 누가 있는지 확인
        int targetX = gridX;
        int targetY = gridY + 1;

        Unit target = BattleManager.Instance.GetUnitAt(targetX, targetY);

        if (target != null)
        {
            Debug.Log($"[타격!] {target.name}을 공격했습니다!");
            // 타겟을 뒤로 1칸 밀어버림 (넉백)
            target.GetKnockedBack(0, 1); 
        }
        else
        {
            Debug.Log("[허공] 공격이 빗나갔습니다.");
        }
    }

    public void GetKnockedBack(int pushX, int pushY)
    {
        int nextX = gridX + pushX;
        int nextY = gridY + pushY;

        // 1. 맵 밖으로 나가는지 확인 (벽 충돌 체크)
        if (nextX < 0 || nextX >= GridManager.Instance.width || 
            nextY < 0 || nextY >= GridManager.Instance.height)
        {
            // 벽 꽝! (Wall Smash)
            Debug.Log($"<color=red>쾅!! {unitName}이(가) 벽에 부딪혀 기절했습니다!</color>");
            
            // 연출: 밀려나려다가 벽에 막혀서 심하게 떨림
            transform.DOShakePosition(0.5f, 0.5f, 20, 90); 
            
            // 데미지 처리 (나중에 추가)
            TakeDamage(10); 
            return;
        }

        // 2. 밀려날 곳에 다른 유닛이 있는지 확인 (연쇄 충돌 체크)
        Unit obstacle = BattleManager.Instance.GetUnitAt(nextX, nextY);
        if (obstacle != null)
        {
            // 유닛 꽝! (Unit Crash)
            Debug.Log($"<color=red>쿠당탕! {unitName}이(가) {obstacle.name}와 부딪혔습니다!</color>");
            transform.DOShakePosition(0.5f, 0.3f, 10, 90);
            return;
        }

        // 3. 장애물이 없으면 실제로 밀려남
        gridX = nextX;
        gridY = nextY;
        
        Vector3 targetPos = GridManager.Instance.GetWorldPosition(gridX, gridY);
        // 밀려나는 연출 (빠르게 튕겨나감)
        transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutBack);
    }
}
