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
