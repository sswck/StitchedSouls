using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using System.Collections.Generic;

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
    [SerializeField] private SkeletonAnimation skeletonAnimation; 
    
    [SpineAnimation] public string idleAnimName = "Idle";
    [SpineAnimation] public string attackAnimName = "attack_standing";

    [Header("UI")]
    public GameObject hpBarPrefab;
    private UnitHPBar hpBar;

    #endregion

    public void Init(int startX, int startY)
    {
        currentHP = maxHP;
        currentMovePoints = maxMovePoints;
        gridX = startX;
        gridY = startY;

        if (AnchorGridManager.Instance != null)
            transform.position = AnchorGridManager.Instance.GetWorldPosition(gridX, gridY);
        else
            transform.position = new Vector3(gridX * 1.1f, 0.5f, gridY * 1.1f);

        InitializeHPBar();
        PlayAnim(idleAnimName, true);
    }

    // ----------------------------------------------------------------
    // [1] 애니메이션 & 비주얼
    // ----------------------------------------------------------------
    private void PlayAnim(string animName, bool loop)
    {
        if (skeletonAnimation == null || string.IsNullOrEmpty(animName)) return;
        skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
    }

    private void SetFlip(bool isLeft)
    {
        if (skeletonAnimation != null)
        {
            skeletonAnimation.Skeleton.ScaleX = isLeft ? -1 : 1;
        }
    }

    private void InitializeHPBar()
    {
        if (hpBarPrefab == null) return;
        GameObject go = Instantiate(hpBarPrefab, transform);
        go.transform.localPosition = Vector3.up * 2.5f; // 필요시 숫자변경으로 체력바와 유닛과의 높이조절
        hpBar = go.GetComponent<UnitHPBar>();
        UpdateHPBar();
    }

    public void UpdateHPBar()
    {
        if (hpBar != null) hpBar.SetHP(currentHP, maxHP);
    }

    // ----------------------------------------------------------------
    // [2] 이동 로직 (키보드/AI 전용)
    // ----------------------------------------------------------------
    public bool CanMove() => currentMovePoints > 0;

    public void Move(int dirX, int dirY)
    {
        if (dirX != 0) SetFlip(dirX < 0);

        int targetX = gridX + dirX;
        int targetY = gridY + dirY;

        if (AnchorGridManager.Instance != null)
        {
            if (targetX < 0 || targetX >= AnchorGridManager.Instance.width ||
                targetY < 0 || targetY >= AnchorGridManager.Instance.height)
            {
                transform.DOShakePosition(0.2f, 0.1f);
                return;
            }
        }

        if (BattleManager.Instance.GetUnitAt(targetX, targetY) != null)
        {
            transform.DOShakePosition(0.2f, 0.1f);
            return;
        }

        gridX = targetX;
        gridY = targetY;
        currentMovePoints--;

        Vector3 targetPos = AnchorGridManager.Instance != null 
            ? AnchorGridManager.Instance.GetWorldPosition(gridX, gridY) 
            : new Vector3(gridX * 1.1f, 0.5f, gridY * 1.1f);

        transform.DOJump(targetPos, 0.5f, 1, 0.3f);
    }

    public void OnTurnStart()
    {
        currentMovePoints = maxMovePoints;
    }

    // ----------------------------------------------------------------
    // [3] 카드 액션 로직
    // ----------------------------------------------------------------
    public void PerformAction(CardData card, Sequence seq)
    {
        Debug.Log($"[{unitName}] 카드 실행: {card.cardName} (Type: {card.targetType})");

        switch (card.targetType)
        {
            case TargetType.Pattern:
                AttackPattern(card, seq);
                break;

            case TargetType.Self:
                if (seq != null)
                {
                    // 버프 쓰는 시늉 (공격 모션 재활용하거나 별도 모션 사용)
                    seq.AppendCallback(() => ApplyEffect(this, card));
                    seq.AppendInterval(0.2f);
                }
                else ApplyEffect(this, card);
                break;

            case TargetType.AllEnemies:
                AttackAllEnemies(card, seq);
                break;

            case TargetType.AllAllies:
                // 구현 예정
                break;
        }
    }

    public void AttackPattern(CardData card, Sequence seq)
    {
        // 현재 바라보는 방향 확인 (왼쪽을 보고 있으면 패턴도 좌우 반전)
        int direction = (skeletonAnimation != null && skeletonAnimation.Skeleton.ScaleX < 0) ? -1 : 1;

        List<Unit> validTargets = new List<Unit>();

        if (card.targetPattern != null)
        {
            foreach (Vector2Int offset in card.targetPattern)
            {
                int checkX = gridX + (offset.x * direction);
                int checkY = gridY + offset.y; 

                Unit target = BattleManager.Instance.GetUnitAt(checkX, checkY);
                if (target != null && target != this)
                {
                    validTargets.Add(target);
                }
            }
        }

        if (seq != null)
        {
            seq.AppendCallback(() => PlayAnim(attackAnimName, false));
            seq.AppendInterval(0.3f);

            seq.AppendCallback(() => 
            {
                if (validTargets.Count > 0)
                {
                    foreach (var target in validTargets)
                    {
                        ApplyEffect(target, card);
                        
                        // 넉백 (선택 사항: 바로 앞 1칸 공격일 때만 넉백)
                        if (card.targetPattern.Count == 1 && card.targetPattern[0].x == 1)
                        {
                            target.GetKnockedBack(direction, 0);
                        }
                    }
                }
                else
                {
                    Debug.Log("허공을 공격했습니다.");
                }
            });

            // 복귀
            seq.AppendInterval(0.5f);
            seq.AppendCallback(() => PlayAnim(idleAnimName, true));
        }
    }

    public void AttackAllEnemies(CardData card, Sequence seq)
    {
        if (seq != null)
        {
            seq.AppendCallback(() => PlayAnim(attackAnimName, false));
            seq.AppendInterval(0.3f);
            seq.AppendCallback(() => 
            {
                foreach (var unit in BattleManager.Instance.allUnits)
                {
                    // 플레이어가 아니면 모두 적이라고 가정 (나중에 태그나 팀 구분 필요)
                    if (unit != this && unit.gameObject.activeInHierarchy)
                    {
                        ApplyEffect(unit, card);
                    }
                }
                Debug.Log("⚡ 적 전체 공격!");
            });
            seq.AppendInterval(0.5f);
            seq.AppendCallback(() => PlayAnim(idleAnimName, true));
        }
    }

    private void ApplyEffect(Unit target, CardData card)
    {
        if (target == null) return;

        if (card.cardType == CardType.Attack)
        {
            target.TakeDamage(card.value);
        }
        else if (card.cardType == CardType.Defense)
        {
            // 일단은 체력 회복으로 대체 (나중에 방어도 추가)
            Debug.Log($"🛡️ {target.unitName} 방어/회복 (+{card.value})");
            // target.Heal(card.value); 
        }
        else if (card.cardType == CardType.Skill)
        {
            Debug.Log($"✨ {target.unitName}에게 스킬 효과!");
        }
    }
    
    // ----------------------------------------------------------------
    // [4] 피격 및 기타
    // ----------------------------------------------------------------
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{unitName} 피격! 남은 체력: {currentHP}");
        UpdateHPBar();

        if (skeletonAnimation != null)
        {
            Color originalColor = skeletonAnimation.skeleton.GetColor();
            
            // 빨간색으로 변경 후 원래대로 복구 (DOTween.To 사용)
            // Spine은 Material이 아니라 Skeleton 자체의 색을 바꿔야 합니다.
            DOTween.To(() => skeletonAnimation.skeleton.GetColor(), 
                       x => skeletonAnimation.skeleton.SetColor(x), 
                       Color.red, 0.1f)
                   .SetLoops(2, LoopType.Yoyo)
                   .OnComplete(() => skeletonAnimation.skeleton.SetColor(Color.white));
        }

        transform.DOShakePosition(0.3f, 0.2f);

        if (currentHP <= 0) Die();
    }

    public void GetKnockedBack(int pushX, int pushY)
    {
        int nextX = gridX + pushX;
        int nextY = gridY + pushY;
        bool isWallHit = false;
        bool isUnitHit = false;

        if (AnchorGridManager.Instance != null)
        {
            if (nextX < 0 || nextX >= AnchorGridManager.Instance.width ||
                nextY < 0 || nextY >= AnchorGridManager.Instance.height)
            {
                isWallHit = true;
            }
        }

        Unit obstacle = BattleManager.Instance.GetUnitAt(nextX, nextY);
        if (obstacle != null)
        {
            isUnitHit = true;
        }

        if (isWallHit || isUnitHit)
        {
            Debug.Log($"💥 {unitName} 넉백 충돌! (벽/유닛)");

            transform.DOShakePosition(0.5f, 0.5f, 20, 90);

            TakeDamage(10); 
            return;
        }

        gridX = nextX;
        gridY = nextY;

        Vector3 targetPos = AnchorGridManager.Instance != null 
            ? AnchorGridManager.Instance.GetWorldPosition(gridX, gridY)
            : new Vector3(gridX * 1.1f, 0.5f, gridY * 1.1f);

        transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutBack);
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

    public void AI_TakeAction(Unit target)
    {
        if (target == null) return;
        int dist = Mathf.Abs(target.gridX - gridX) + Mathf.Abs(target.gridY - gridY);

        if (dist <= 1)
        {
            // 방향 전환 후 공격
            int dirX = target.gridX - gridX;
            if (dirX != 0) SetFlip(dirX < 0);
            
            // AI는 단순하게 기본 공격(1칸 앞)을 한다고 가정하여 가짜 카드 데이터 생성 후 실행
            // (실제로는 AI도 CardData를 가지고 있어야 함)
            // 여기선 간단히 AttackPattern 메서드를 모방하여 직접 처리
            Debug.Log($"🤖 AI {unitName} 공격!");
            
            // 임시 공격 연출
            PlayAnim(attackAnimName, false);
            // 0.3초 뒤 데미지
            DOVirtual.DelayedCall(0.3f, () => {
                if(target != null) target.TakeDamage(7);
            });
            // 0.8초 뒤 복귀
            DOVirtual.DelayedCall(0.8f, () => PlayAnim(idleAnimName, true));
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