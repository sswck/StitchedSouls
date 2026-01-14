using UnityEngine;
using DG.Tweening;
using Spine.Unity; // [필수] Spine 기능을 사용하기 위해 추가

public class Unit : MonoBehaviour
{
    #region Variables

    [Header("Stats")]
    public string unitName;
    public int maxHP = 50;
    public int currentHP;
    public int maxMovePoints = 2;
    public int currentMovePoints;

    [Header("Position")]
    public int gridX;
    public int gridY;

    [Header("Visual & Animation")]
    // [중요] 스파인 애니메이션 제어 컴포넌트
    [SerializeField] private SkeletonAnimation skeletonAnimation; 
    
    [SpineAnimation] public string idleAnimName = "Idle";
    [SpineAnimation] public string attackAnimName = "attack_standing";

    [Header("UI")]
    public GameObject hpBarPrefab;
    private UnitHPBar hpBar;

    #endregion

    // 초기화
    public void Init(int startX, int startY)
    {
        currentHP = maxHP;
        currentMovePoints = maxMovePoints;
        gridX = startX;
        gridY = startY;

        // [수정] GridManager가 있다면 사용하고, 없으면 간단한 계산식 사용
        // (현재 프로젝트 상황에 맞춰 1칸=1유닛 크기로 배치)
        transform.position = new Vector3(gridX, 0.5f, gridY);

        InitializeHPBar();
        
        // [추가] 시작 시 대기 모션 재생
        PlayAnim(idleAnimName, true);
    }

    // ----------------------------------------------------------------
    // [1] 애니메이션 헬퍼 함수들
    // ----------------------------------------------------------------
    
    // 애니메이션 재생
    private void PlayAnim(string animName, bool loop)
    {
        if (skeletonAnimation == null || string.IsNullOrEmpty(animName)) return;
        skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
    }

    // 좌우 반전 (왼쪽으로 갈 때는 뒤집기)
    private void SetFlip(bool isLeft)
    {
        if (skeletonAnimation != null)
        {
            // 스파인은 ScaleX에 -1을 곱해 좌우를 뒤집습니다.
            skeletonAnimation.Skeleton.ScaleX = isLeft ? -1 : 1;
        }
    }

    private void InitializeHPBar()
    {
        if (hpBarPrefab == null) return;
        GameObject go = Instantiate(hpBarPrefab, transform);
        // 스파인 캐릭터 높이에 맞춰 HP바 위치 조정 (필요시 숫자 변경)
        go.transform.localPosition = Vector3.up * 2.5f; 
        hpBar = go.GetComponent<UnitHPBar>();
        UpdateHPBar();
    }

    // ----------------------------------------------------------------
    // [2] 이동 로직 (오직 키보드/AI로만 작동)
    // ----------------------------------------------------------------

    public void Move(int dirX, int dirY)
    {
        // 1. 방향 전환 (Spine 좌우 반전)
        if (dirX != 0) SetFlip(dirX < 0);

        int targetX = gridX + dirX;
        int targetY = gridY + dirY;

        // 2. 맵 밖인지 확인 (GridManager가 없어도 동작하도록 하드코딩 범위 추가 가능)
        // 일단 기존 GridManager 로직 유지하되, null 체크 추가
        if (GridManager.Instance != null)
        {
            if (targetX < 0 || targetX >= GridManager.Instance.width ||
                targetY < 0 || targetY >= GridManager.Instance.height)
            {
                transform.DOShakePosition(0.2f, 0.1f);
                return;
            }
        }
        else 
        {
            // GridManager가 없는 경우 임시 범위 체크 (0~9)
            if (targetX < 0 || targetX > 9 || targetY < 0 || targetY > 9) return;
        }

        // 3. 장애물 확인
        if (BattleManager.Instance.GetUnitAt(targetX, targetY) != null)
        {
            transform.DOShakePosition(0.2f, 0.1f);
            return;
        }

        // 4. 이동 실행
        gridX = targetX;
        gridY = targetY;

        currentMovePoints--;
        
        // 실제 월드 좌표로 이동
        Vector3 targetPos = new Vector3(gridX, 0.5f, gridY);
        transform.DOJump(targetPos, 0.5f, 1, 0.3f);
    }

    public bool CanMove()
    {
        return currentMovePoints > 0;
    }

    public void OnTurnStart()
    {
        currentMovePoints = maxMovePoints;
        Debug.Log($"{unitName}: 턴 시작! 이동력 회복.");
    }

    // ----------------------------------------------------------------
    // [3] 액션 및 피격 로직 (Spine 연동)
    // ----------------------------------------------------------------

    public void Attack(int pushPower)
    {
        // Spine이 바라보는 방향(-1 or 1)을 기준으로 공격 방향 결정
        int dirX = (skeletonAnimation != null && skeletonAnimation.Skeleton.ScaleX < 0) ? -1 : 1;
        int targetX = gridX + dirX;
        int targetY = gridY; // 현재는 좌우 공격만 가정 (상하 공격 필요시 로직 추가)

        // 1. 공격 애니메이션 재생 (반복 X)
        PlayAnim(attackAnimName, false);

        // 2. 공격 타이밍 맞추기 (애니메이션과 데미지 싱크)
        // BattleManager의 Sequence 딜레이(0.6초) 안에 모든 걸 처리
        DOVirtual.DelayedCall(0.3f, () => 
        {
            // 실제 데미지 처리
            Unit target = BattleManager.Instance.GetUnitAt(targetX, targetY);
            if (target != null)
            {
                Debug.Log($"⚔️ {unitName} 공격 -> {target.name}");
                target.TakeDamage(3); // 기본 데미지 3
                if (pushPower > 0) target.GetKnockedBack(dirX, 0);
            }
        });

        // 3. 공격 후 다시 Idle로 복귀 (0.6초 뒤)
        DOVirtual.DelayedCall(0.6f, () => 
        {
            PlayAnim(idleAnimName, true);
        });
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        UpdateHPBar();

        // 피격 시 붉게 깜빡임 (Spine은 MeshRenderer를 사용)
        var meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
        }

        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
        {
            gameObject.SetActive(false);
            if (BattleManager.Instance != null) BattleManager.Instance.OnUnitDead(this);
        });
    }

    public void GetKnockedBack(int pushX, int pushY)
    {
        int nextX = gridX + pushX;
        int nextY = gridY + pushY;

        // 벽이나 유닛 체크 로직 (기존 유지)
        if (BattleManager.Instance.GetUnitAt(nextX, nextY) != null)
        {
            transform.DOShakePosition(0.5f, 0.3f);
            return;
        }

        gridX = nextX;
        gridY = nextY;
        
        Vector3 targetPos = new Vector3(gridX, 0.5f, gridY);
        transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutBack);
    }

    private void UpdateHPBar()
    {
        if (hpBar != null) hpBar.SetHP(currentHP, maxHP);
    }

    // AI 로직 (기존 유지)
    public void AI_TakeAction(Unit target)
    {
        if (target == null) return;
        int dist = Mathf.Abs(target.gridX - gridX) + Mathf.Abs(target.gridY - gridY);

        if (dist <= 1)
        {
            // 타겟 방향 바라보기 (공격 전 회전)
            int dirX = target.gridX - gridX;
            if (dirX != 0) SetFlip(dirX < 0);
            
            Attack(1);
        }
        else
        {
            // 추격
            int moveX = 0;
            int moveY = 0;
            if (Mathf.Abs(target.gridX - gridX) > Mathf.Abs(target.gridY - gridY))
                moveX = (target.gridX > gridX) ? 1 : -1;
            else
                moveY = (target.gridY > gridY) ? 1 : -1;

            Move(moveX, moveY);
        }
    }
}