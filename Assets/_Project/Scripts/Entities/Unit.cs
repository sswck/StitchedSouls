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
    public int attackSpeed = 10; // 추가
    public int maxMovePoints = 2;
    public int currentMovePoints;
    public int damageBuff = 0;

    [Header("Ultimate Skill")]
    public CardData ultimateSkillCard;

    [Header("Buff Durations")]
    public int damageBuffDuration = 0;
    public int moveBonus = 0;
    public int moveBuffDuration = 0;

    [Header("Position")]
    public int gridX;
    public int gridY;
    [Tooltip("타일 중심에서 유닛 위치를 보정합니다. (예: 발이 타일 위에 오도록 Y를 낮추려면 (0, -0.3, 0) 등)")]
    public Vector3 tilePositionOffset = Vector3.zero;
    [Tooltip("2D 레이어 정렬 기준값. gridY가 작을수록 이 값에 더해져 앞에 그려집니다.")]
    [SerializeField] private int sortingOrderBase = 5; // [수정] 10 -> 5 (UI 가림 방지)
    [Tooltip("3D 프로젝트: gridY가 작을수록(앞줄) Z를 보정해 카메라에 가깝게 그려지게 합니다. 0이면 미적용.")]
    //[SerializeField] private float layerDepthStep = 0.1f; // sswck: 참조하는데가 없어 일단 주석처리했습니다

    [Header("Visual & Animation")]
    [SerializeField] private SkeletonAnimation skeletonAnimation; 
    
    [SpineAnimation] public string idleAnimName = "Idle";
    [SpineAnimation] public string attackAnimName = "attack_standing";

    [Header("UI")]
    public Sprite activeIcon;   // 턴일 때 표시될 스프라이트
    public Sprite inactiveIcon; // 턴이 아닐 때 표시될 스프라이트
    public GameObject hpBarPrefab;
    public Vector3 hpBarOffset = new Vector3(0f, 2.5f, 0f);
    public Vector3 playerHpBarOffset = new Vector3(0f, 3.2f, 0f);
    private UnitHPBar hpBar;

    [Header("Defense")]
    public int currentShield = 0;

    [Header("VFX Prefabs")]
    public GameObject hitVFXPrefab;
    public GameObject playerHitVFXPrefab;
    public GameObject defenseVFXPrefab;
    public GameObject ppRecoveryVFXPrefab;
    public GameObject damageBuffVFXPrefab;
    
    private GameObject activeBuffVFX;

    [Header("Unit SFX")]
    // public AudioClip moveSFX;
    public AudioClip attackSFX;

    [Header("PendingHeal")]
    private int pendingHealHP = 0;
    private int pendingHealPP = 0;

    [Header("Transparency")]
    private float currentAlpha = 1f;
    private Tweener alphaTweener;

    public void SetTransparency(float targetAlpha)
    {
        if (Mathf.Approximately(currentAlpha, targetAlpha)) return;
        
        currentAlpha = targetAlpha;

        if (skeletonAnimation != null)
        {
            alphaTweener?.Kill();
            Color color = skeletonAnimation.skeleton.GetColor();
            alphaTweener = DOTween.To(() => color.a, x => 
            {
                color.a = x;
                skeletonAnimation.skeleton.SetColor(color);
            }, targetAlpha, 0.3f);
        }
    }

    #endregion

    public void Init(int startX, int startY)
    {
        if (string.IsNullOrEmpty(unitName))
        {
            // "(Clone)" 같은 지저분한 글자가 붙는 걸 떼고 깔끔하게 이름만 가져옵니다.
            unitName = gameObject.name.Replace("(Clone)", "").Trim();
        }

        currentHP = maxHP;
        currentMovePoints = maxMovePoints;
        gridX = startX;
        gridY = startY;

        Vector3 pos;
        if (AnchorGridManager.Instance != null)
            pos = AnchorGridManager.Instance.GetTileCenterPosition(gridX, gridY);
        else
            pos = new Vector3(gridX * 1.1f, 0.5f, gridY * 1.1f);
        pos += tilePositionOffset;
        transform.position = pos;

        UpdateSortingOrder();

        InitializeHPBar();
        UpdateHPBar();
        PlayAnim(idleAnimName, true);
        if (BattleManager.Instance != null) BattleManager.Instance.UpdateUnitTransparencies();
    }

    private void ApplyLayerDepth(ref Vector3 pos, int gridY)
    {
        // 3D 뎁스 보정 기능을 비활성화했습니다. 
        // 2.5D 카메라에서 타일 중앙 위치를 왜곡시킬 수 있기 때문에 Z축 이동을 막고 Sorting Order만 사용합니다.
        return;
    }

    private void UpdateSortingOrder()
    {
        int order = sortingOrderBase;
        if (AnchorGridManager.Instance != null)
            order += (AnchorGridManager.Instance.height - 1 - gridY);

        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            r.sortingOrder = order;
        }

        // [추가] 체력바(Canvas) 정렬 동기화
        // 기본적으로 유닛 본체보다 앞에 오도록 설정하되,
        // Elite나 Boss 노드일 경우 유닛 종류별로 sortingOrderBase가 다를 수 있으므로 
        // 체력바(Canvas)는 gridY에만 의존하는 절대적인 정렬 기준(예: 500)을 적용하여 앞뒤 관계를 보장합니다.
        int canvasOrder = order + 50;
        if (GameManager.Instance != null && (GameManager.Instance.currentNodeType == NodeType.Elite || GameManager.Instance.currentNodeType == NodeType.Boss))
        {
            canvasOrder = 500 + (AnchorGridManager.Instance != null ? (AnchorGridManager.Instance.height - 1 - gridY) : 0);
        }

        foreach (var c in GetComponentsInChildren<Canvas>(true))
        {
            c.sortingOrder = canvasOrder;
        }
    }

    private void PlayAnim(string animName, bool loop)
    {
        if (skeletonAnimation == null || string.IsNullOrEmpty(animName)) return;
        skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
    }

    private void SetFlip(bool isLeft)
    {
        if (skeletonAnimation != null)
        {
            // [수정] 적 유닛의 스프라이트 기본 방향 반전 보정
            bool isPlayer = BattleManager.Instance != null && BattleManager.Instance.playerUnit == this;
            if (isPlayer)
                skeletonAnimation.Skeleton.ScaleX = isLeft ? -1 : 1;
            else
                skeletonAnimation.Skeleton.ScaleX = isLeft ? 1 : -1;
        }
    }

    private void InitializeHPBar()
    {
        if (hpBarPrefab == null) return;
        GameObject go = Instantiate(hpBarPrefab, transform);

        bool isPlayer = BattleManager.Instance != null && BattleManager.Instance.playerUnit == this;
        go.transform.localPosition = isPlayer ? playerHpBarOffset : hpBarOffset;

        hpBar = go.GetComponent<UnitHPBar>();
        UpdateHPBar();
    }

    public void UpdateHPBar()
    {
        if (hpBar != null) hpBar.SetHP(currentHP, maxHP);
    }

    public void UpdatePPBar()
    {
        if (hpBar != null && GameManager.Instance != null)
        {
            bool isPlayer = BattleManager.Instance != null && BattleManager.Instance.playerUnit == this;
            
            hpBar.ShowPPBar(isPlayer); 
            if (isPlayer)
            {
                hpBar.SetPP(GameManager.Instance.currentPP, GameManager.Instance.maxPP);
            }
        }
    }

    public bool CanMove() => currentMovePoints > 0;

    public void Move(int dirX, int dirY)
    {
        int targetX = gridX + dirX;
        int targetY = gridY + dirY;

        if (dirX != 0) SetFlip(dirX < 0);

        if (AnchorGridManager.Instance != null)
        {
            // [수정] width=5일 때 4번 인덱스 허용 (targetX < 0 || targetX >= width)
            if (targetX < 0 || targetX >= AnchorGridManager.Instance.width ||
                targetY < 0 || targetY >= AnchorGridManager.Instance.height)
            {
                Debug.Log($"[{unitName}] 이동 불가: 경계 밖 ({targetX}, {targetY})");
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
            ? AnchorGridManager.Instance.GetTileCenterPosition(gridX, gridY) 
            : new Vector3(gridX * 1.1f, 0.5f, gridY * 1.1f);
        targetPos += tilePositionOffset;

        UpdateSortingOrder();
        if (BattleManager.Instance != null) BattleManager.Instance.UpdateUnitTransparencies();

        // [수정] 트윈 중첩 방지
        transform.DOKill();
        transform.DOJump(targetPos, 0.5f, 1, 0.3f);

        // 발소리 재생 (주인공일 때만)
        bool isPlayer = BattleManager.Instance != null && BattleManager.Instance.playerUnit == this;
        if (isPlayer && SoundManager.Instance != null && SoundManager.Instance.moveSFX != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.moveSFX);
        }

        if (isPlayer && BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateMovementUI();
        }
    }

    public void OnTurnStart()
    {
        if (currentShield > 0)
        {
            Debug.Log($"⏳ 턴 시작: [{unitName}]의 이전 턴 방어도가 0으로 초기화되었습니다.");
            currentShield = 0;
            if (hpBar != null) hpBar.UpdateShieldUI(currentShield);
        }

        if (moveBuffDuration > 0)
        {
            moveBuffDuration--;
            if (moveBuffDuration <= 0)
            {
                maxMovePoints -= moveBonus;
                moveBonus = 0;
            }
        }

        if (damageBuffDuration > 0)
        {
            damageBuffDuration--;
            if (damageBuffDuration <= 0)
            {
                damageBuff = 0;
                if (activeBuffVFX != null)
                {
                    Destroy(activeBuffVFX);
                    activeBuffVFX = null;
                }
            }
        }

        if (pendingHealHP > 0)
        {
            Heal(pendingHealHP);
            pendingHealHP = 0;
        }

        if (pendingHealPP > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.currentPP = Mathf.Min(GameManager.Instance.currentPP + pendingHealPP, GameManager.Instance.maxPP);
            pendingHealPP = 0;
            
            if (BattleUIManager.Instance != null) BattleUIManager.Instance.UpdatePPUI();
            UpdatePPBar();
        }

        currentMovePoints = maxMovePoints;

        // [추가] 턴 시작 시 이동력 UI 갱신
        if (BattleManager.Instance != null && this == BattleManager.Instance.playerUnit)
        {
            if (BattleUIManager.Instance != null) BattleUIManager.Instance.UpdateMovementUI();
        }
    }

    public void PerformAction(CardData card, Sequence seq)
    {
        switch (card.targetType)
        {
            case TargetType.Pattern:
                AttackPattern(card, seq);
                break;
            case TargetType.Self:
                if (seq != null)
                {
                    seq.AppendCallback(() => ApplyEffect(this, card));
                    seq.AppendInterval(0.2f);
                }
                else ApplyEffect(this, card);
                break;
            case TargetType.AllEnemies:
                AttackAllEnemies(card, seq);
                break;
        }
    }

    public void AttackPattern(CardData card, Sequence seq)
    {
        int direction = (skeletonAnimation != null && skeletonAnimation.Skeleton.ScaleX < 0) ? -1 : 1;
        // 적 유닛은 ScaleX가 반전되어 있으므로 direction 보정이 필요할 수 있음
        bool isPlayer = BattleManager.Instance != null && BattleManager.Instance.playerUnit == this;
        if (!isPlayer) direction = (skeletonAnimation.Skeleton.ScaleX > 0) ? -1 : 1;

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
            seq.AppendCallback(() =>
            {
                PlayAnim(attackAnimName, false);

                if (isPlayer && SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(attackSFX);
                }
            });
            seq.AppendInterval(0.3f);
            seq.AppendCallback(() => 
            {
                foreach (var target in validTargets)
                {
                    ApplyEffect(target, card);
                }
            });
            seq.AppendInterval(0.5f);
            seq.AppendCallback(() => PlayAnim(idleAnimName, true));
        }
    }

    public void AttackAllEnemies(CardData card, Sequence seq)
    {
        if (seq != null)
        {
            seq.AppendCallback(() =>
            {
                PlayAnim(attackAnimName, false);
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(attackSFX);
                }
            });
            seq.AppendInterval(0.3f);
            seq.AppendCallback(() => 
            {
                foreach (var unit in BattleManager.Instance.allUnits)
                {
                    if (unit != this && unit.gameObject.activeInHierarchy)
                    {
                        ApplyEffect(unit, card);
                    }
                }
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
            int strBonus = (BattleManager.Instance != null && this == BattleManager.Instance.playerUnit
                && (card.cardType == CardType.Attack || card.cardName == "강타") && GameManager.Instance != null)
                ? GameManager.Instance.str * 3 : 0;
            int totalBuff = strBonus + damageBuff;
            int effectiveValue = card.value + totalBuff;
            if (BattleManager.Instance != null && this == BattleManager.Instance.playerUnit && target != BattleManager.Instance.playerUnit)
            {
                BattleManager.Instance.RecordDamageDeal(effectiveValue);
            }
            target.TakeDamage(effectiveValue);
        }
        if (card.healHP > 0) target.Heal(card.healHP);
        if (card.healPP > 0 && GameManager.Instance != null && target == BattleManager.Instance.playerUnit)
        {
            GameManager.Instance.currentPP = Mathf.Min(GameManager.Instance.currentPP + card.healPP, GameManager.Instance.maxPP);
            if (BattleUIManager.Instance != null) BattleUIManager.Instance.UpdatePPUI();
            target.UpdatePPBar();
            Debug.Log($"🔋 PP {card.healPP} 회복! 현재 PP: {GameManager.Instance.currentPP}");

            if (SoundManager.Instance != null && SoundManager.Instance.ppRecoverySFX != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.ppRecoverySFX);
            }

            if (target.ppRecoveryVFXPrefab != null)
            {
                GameObject vfx = Instantiate(target.ppRecoveryVFXPrefab, target.transform.position, Quaternion.identity);
                // 2D 환경에서 파티클이 캐릭터 뒤에 가려지는 현상 방지
                foreach (var r in vfx.GetComponentsInChildren<Renderer>()) r.sortingOrder = 30;
                Destroy(vfx, 1.2f);
            }
        }

        if (card.nextTurnHealHP > 0 || card.nextTurnHealPP > 0)
        {
            target.pendingHealHP += card.nextTurnHealHP;
            target.pendingHealPP += card.nextTurnHealPP;
            Debug.Log($"⏳ 다음 턴에 HP {card.nextTurnHealHP}, PP {card.nextTurnHealPP} 회복 예약됨!");
        }

        int blockGain = card.block;

        if (card.isMissingHPBlock)
        {
            blockGain = target.maxHP - target.currentHP;
        }

        if (blockGain > 0)
        {
            target.AddShield(blockGain);
        }
    }
    
    public void AddShield(int amount)
    {
        if (amount <= 0) return;

        currentShield += amount;
        Debug.Log($"🛡️ [{unitName}] 방어도 {amount} 획득! (현재 총 방어도: {currentShield})");

        if (SoundManager.Instance != null && SoundManager.Instance.defenseCardSFX != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.defenseCardSFX);
        }

        if (defenseVFXPrefab != null)
        {
            GameObject vfx = Instantiate(defenseVFXPrefab, transform.position, Quaternion.identity);
            foreach (var r in vfx.GetComponentsInChildren<Renderer>()) r.sortingOrder = 30;
            Destroy(vfx, 1.2f);
        }

        // UI 애니메이션 업데이트 호출
        if (hpBar != null)
        {
            hpBar.UpdateShieldUI(currentShield);
        }
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = damage;
        bool isPlayer = BattleManager.Instance != null && this == BattleManager.Instance.playerUnit;

        if (isPlayer && GameManager.Instance != null)
        {
            finalDamage = Mathf.Max(0, damage - GameManager.Instance.def);
        }

        if (currentShield > 0)
        {
            if (currentShield >= finalDamage)
            {
                // 방어도로 피해를 완벽히 막아낸 경우
                currentShield -= finalDamage;
                Debug.Log($"🛡️ [{unitName}] 방어도로 공격 완벽 차단! (들어온 피해: {finalDamage} / 남은 방어도: {currentShield})");
                finalDamage = 0; 
            }
            else
            {
                // 방어도가 뚫리고 남은 데미지가 들어가는 경우
                finalDamage -= currentShield;
                Debug.Log($"💥 [{unitName}] 방어도가 뚫렸습니다! (방어도 {currentShield} 소모 / 실제 들어온 피해: {finalDamage})");
                currentShield = 0;
            }
            // 쉴드 변경 적용
            if (hpBar != null) hpBar.UpdateShieldUI(currentShield);
        }
        
        if (isPlayer)
        {
            BattleManager.Instance.RecordDamageTaken(finalDamage);
        }

        if (finalDamage > 0)
        {
            currentHP -= finalDamage;
            UpdateHPBar();
        }

        if (SoundManager.Instance != null)
        {
            if (isPlayer) SoundManager.Instance.PlaySFX(SoundManager.Instance.allyHitSFX);
            else SoundManager.Instance.PlaySFX(SoundManager.Instance.enemyHitSFX);
        }

        if (isPlayer && playerHitVFXPrefab != null)
        {
            GameObject vfx = Instantiate(playerHitVFXPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            foreach (var r in vfx.GetComponentsInChildren<Renderer>()) r.sortingOrder = 30;
            Destroy(vfx, 1.2f);
        }
        else if (!isPlayer && hitVFXPrefab != null)
        {
            GameObject vfx = Instantiate(hitVFXPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            foreach (var r in vfx.GetComponentsInChildren<Renderer>()) r.sortingOrder = 30;
            Destroy(vfx, 1.2f);
        }

        if (skeletonAnimation != null)
        {
            Color hitColor = Color.red;
            hitColor.a = currentAlpha;
            Color baseColor = Color.white;
            baseColor.a = currentAlpha;

            DOTween.To(() => skeletonAnimation.skeleton.GetColor(), 
                       x => skeletonAnimation.skeleton.SetColor(x), 
                       hitColor, 0.1f)
                   .SetLoops(2, LoopType.Yoyo)
                   .OnComplete(() => skeletonAnimation.skeleton.SetColor(baseColor));
        }

        transform.DOShakePosition(0.3f, 0.2f);
        if (currentHP <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        UpdateHPBar();
    }

    public void ApplyDamageBuff(int amount, int duration)
    {
        damageBuff = amount;
        damageBuffDuration = duration;
        transform.DOScale(transform.localScale * 1.1f, 0.2f).SetLoops(2, LoopType.Yoyo);

        if (damageBuffVFXPrefab != null && activeBuffVFX == null)
        {
            activeBuffVFX = Instantiate(damageBuffVFXPrefab, transform.position, Quaternion.identity, transform);
            foreach (var r in activeBuffVFX.GetComponentsInChildren<Renderer>()) r.sortingOrder = 30;
        }
    }

    public void ApplyMoveBuff(int amount, int duration)
    {
        moveBonus = amount;
        moveBuffDuration = duration;
        maxMovePoints += amount;
        currentMovePoints += amount;

        // [추가] 버프 적용 시 이동력 UI 갱신
        if (BattleManager.Instance != null && this == BattleManager.Instance.playerUnit)
        {
            if (BattleUIManager.Instance != null) BattleUIManager.Instance.UpdateMovementUI();
        }
    }

    public void GetKnockedBack(int pushX, int pushY)
    {
        int nextX = gridX + pushX;
        int nextY = gridY + pushY;
        bool isWallHit = (AnchorGridManager.Instance != null && (nextX < 0 || nextX >= AnchorGridManager.Instance.width || nextY < 0 || nextY >= AnchorGridManager.Instance.height));
        Unit obstacle = BattleManager.Instance.GetUnitAt(nextX, nextY);

        if (isWallHit || obstacle != null)
        {
            transform.DOShakePosition(0.5f, 0.5f, 20, 90);
            TakeDamage(10); 
            return;
        }

        gridX = nextX;
        gridY = nextY;
        Vector3 targetPos = AnchorGridManager.Instance != null 
            ? AnchorGridManager.Instance.GetTileCenterPosition(gridX, gridY)
            : new Vector3(gridX * 1.1f, 0.5f, gridY * 1.1f);
        targetPos += tilePositionOffset;
        UpdateSortingOrder();
        if (BattleManager.Instance != null) BattleManager.Instance.UpdateUnitTransparencies();
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

        // [핵심 수정] 타겟(플레이어)을 항상 바라보도록 설정
        SetFlip(target.gridX < gridX);

        int dist = Mathf.Abs(target.gridX - gridX) + Mathf.Abs(target.gridY - gridY);

        if (dist <= 1)
        {
            Debug.Log($"🤖 AI {unitName} 공격!");
            PlayAnim(attackAnimName, false);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(attackSFX);
            }

            DOVirtual.DelayedCall(0.3f, () => {
                if(target != null) target.TakeDamage(7);
            });
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

            // 이동 시에도 타겟을 바라보도록 보장 (Move 내에서도 SetFlip이 호출되지만, AI 시점에선 플레이어를 고정해서 보는 것이 안정적)
            Move(moveX, moveY);
            SetFlip(target.gridX < gridX);
        }
    }
}
