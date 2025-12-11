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
    public int maxMovePoints = 2;
    public int currentMovePoints;

    [Header("Position")]
    public int gridX;
    public int gridY;

    [Header("Direction")]
    public Vector2Int lookDir = new Vector2Int(0, 1);

    public void Init(int startX, int startY)
    {
        currentHP = maxHP;
        currentMovePoints = maxMovePoints;
        gridX = startX;
        gridY = startY;

        // 시작 위치로 즉시 이동
        transform.position = GridManager.Instance.GetWorldPosition(startX, startY);
    }

    public void Move(int dirX, int dirY)
    {
        if (dirX != 0 || dirY != 0)
        {
            lookDir = new Vector2Int(dirX, dirY);
            RotateModel();
        }

        int targetX = gridX + dirX;
        int targetY = gridY + dirY;

        if (targetX < 0 || targetX >= GridManager.Instance.width ||
            targetY < 0 || targetY >= GridManager.Instance.height)
        {
            Debug.Log("더 이상 갈 수 없습니다!");
            // 연출: 벽에 막힌 느낌 (살짝 흔들기)
            transform.DOShakePosition(0.2f, 0.1f);
            return;
        }

        if (BattleManager.Instance.GetUnitAt(targetX, targetY) != null)
        {
            Debug.Log("다른 유닛이 길을 막고 있습니다!");
            transform.DOShakePosition(0.2f, 0.1f);
            return;
        }

        gridX = targetX;
        gridY = targetY;

        currentMovePoints--;
        Debug.Log($"이동 완료! 남은 이동력: {currentMovePoints}");

        Vector3 targetPos = GridManager.Instance.GetWorldPosition(gridX, gridY);
        transform.DOJump(targetPos, 0.5f, 1, 0.3f);
    }

    public bool CanMove()
    {
        return currentMovePoints > 0;
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

    public void Attack(int pushPower)
    {
        int targetX = gridX + lookDir.x;
        int targetY = gridY + lookDir.y;

        // 1. 공격 연출 (앞으로 살짝 찌르기)
        Vector3 punchDir = new Vector3(lookDir.x, 0, lookDir.y) * 0.5f;
        transform.DOMove(transform.position + punchDir, 0.1f).SetLoops(2, LoopType.Yoyo);

        Unit target = BattleManager.Instance.GetUnitAt(targetX, targetY);

        if (target != null)
        {
            Debug.Log($"[타격!] {target.name}을 공격했습니다!");

            if (pushPower > 0)
            {
                target.GetKnockedBack(lookDir.x, lookDir.y);
                // pushPower(강도) 개념을 적용하려면, GetKnockedBack을 조금 손봐야 할 수도 있습니다. 
                // 일단 지금은 '1칸 밀기'로 가정하고 위 코드로 진행합니다.
            }
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

    public void ConsumeMovePoint()
    {
        currentMovePoints--;
        Debug.Log($"남은 이동력: {currentMovePoints}");
    }

    void RotateModel()
    {
        Vector3 dirVector = new Vector3(lookDir.x, 0, lookDir.y);

        if (dirVector != Vector3.zero)
        {
            transform.DORotateQuaternion(Quaternion.LookRotation(dirVector), 0.2f);
        }
    }

    public void OnTurnStart()
    {
        currentMovePoints = maxMovePoints;
        Debug.Log($"{unitName}: 턴 시작! 이동력 회복됨.");
        
        // (나중에 PP 회복 로직도 여기에 추가 가능)
        // currentPP += 2; 
    }

    public void AI_TakeAction(Unit target)
    {
        if (target == null) return;

        // 1. 거리 계산 (Manhattan Distance: 격자 거리)
        int dist = Mathf.Abs(target.gridX - gridX) + Mathf.Abs(target.gridY - gridY);

        // 2. 공격 범위(1칸) 안에 있는가?
        if (dist <= 1)
        {
            // 공격! (방향을 타겟 쪽으로 돌리고 공격)
            int dirX = target.gridX - gridX;
            int dirY = target.gridY - gridY;
            
            // 시선 갱신
            lookDir = new Vector2Int(dirX, dirY);
            RotateModel();

            // 공격 (적은 1의 힘으로 넉백 공격한다고 가정)
            Debug.Log($"🤖 AI {unitName}: 공격 시도!");
            Attack(1); 
        }
        else
        {
            // 3. 거리가 멀다면 이동 (추격)
            // X축 차이가 더 크면 X축 이동, 아니면 Y축 이동 (간단한 길찾기)
            int moveDirX = 0;
            int moveDirY = 0;

            if (Mathf.Abs(target.gridX - gridX) > Mathf.Abs(target.gridY - gridY))
            {
                // X축 이동 (타겟이 내 오른쪽에 있으면 +1, 왼쪽이면 -1)
                moveDirX = (target.gridX > gridX) ? 1 : -1;
            }
            else
            {
                // Y축 이동
                moveDirY = (target.gridY > gridY) ? 1 : -1;
            }

            Debug.Log($"🤖 AI {unitName}: 플레이어 추격 이동 ({moveDirX}, {moveDirY})");
            Move(moveDirX, moveDirY);
        }
    }
}
