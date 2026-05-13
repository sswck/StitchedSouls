using UnityEngine;
using Spine.Unity;

/// <summary>
/// VFX 프리팹의 투명도를 에디터에서 설정하고,
/// 애니메이션(Spine, Animator 등)에 의해 투명도가 덮어씌워지는 것을 방지하여
/// 실제 플레이에서도 투명도가 유지되도록 강제하는 스크립트입니다.
/// </summary>
public class VFXTransparency : MonoBehaviour
{
    [Header("Transparency Override")]
    [Range(0f, 1f)]
    [Tooltip("VFX의 전체 투명도를 설정합니다. (0 = 완전 투명, 1 = 불투명)")]
    public float alpha = 1f;

    [Header("Rotation Override")]
    [Tooltip("체크하면 아래의 rotation 값으로 매 프레임 강제 고정합니다. (애니메이션 무시)")]
    public bool overrideRotation = false;
    [Tooltip("강제 고정할 로컬 회전값(Euler Angles)입니다.")]
    public Vector3 customRotation = Vector3.zero;

    private SkeletonAnimation spineAnim;
    private SpriteRenderer[] spriteRenderers;
    private ParticleSystem[] particleSystems;

    void Start()
    {
        spineAnim = GetComponentInChildren<SkeletonAnimation>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        particleSystems = GetComponentsInChildren<ParticleSystem>();

        ApplyAlphaToParticles();
    }

    void LateUpdate()
    {
        // 1. 강제 회전 덮어쓰기 (애니메이터나 Spine에 의해 회전이 덮어씌워지는 것 방지)
        if (overrideRotation)
        {
            transform.localEulerAngles = customRotation;
        }

        // 2. Spine 애니메이션 또는 Unity Animator가 색상(투명도)을 매 프레임 덮어씌울 수 있으므로
        // LateUpdate에서 우리가 설정한 alpha 값으로 다시 강제합니다.

        if (spineAnim != null && spineAnim.skeleton != null)
        {
            Color color = spineAnim.skeleton.GetColor();
            if (!Mathf.Approximately(color.a, alpha))
            {
                color.a = alpha;
                spineAnim.skeleton.SetColor(color);
            }
        }

        if (spriteRenderers != null)
        {
            foreach (var sr in spriteRenderers)
            {
                if (sr == null) continue;
                Color c = sr.color;
                if (!Mathf.Approximately(c.a, alpha))
                {
                    c.a = alpha;
                    sr.color = c;
                }
            }
        }
    }

    private void ApplyAlphaToParticles()
    {
        if (particleSystems == null) return;

        foreach (var ps in particleSystems)
        {
            if (ps == null) continue;
            var main = ps.main;
            Color c = main.startColor.color;
            c.a *= alpha; // 기존 파티클 색상에 비율로 곱하거나 덮어씌움
            main.startColor = c;
        }
    }
}
