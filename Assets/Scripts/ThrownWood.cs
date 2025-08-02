// ThrownWood.cs
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class ThrownWood : MonoBehaviour
{
    [Header("Configuração da Animação")]
    public float arcHeight = 2f;
    public float spinSpeed = 720f;

    // Flag para controlar se a lenha pode ser coletada
    public bool isCollectible = false;

    private void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // Torna a lenha coletável após um curto período para evitar
        // que o jogador a pegue de volta instantaneamente.
        // DOVirtual.DelayedCall é uma forma limpa do DOTween de fazer um timer.
        DOVirtual.DelayedCall(0.2f, () => {
            isCollectible = true;
        });
    }

    public void Launch(Vector3 targetPosition, float duration)
    {
        // ... seu código de Launch continua exatamente o mesmo ...
        Sequence launchSequence = DOTween.Sequence();
        Vector3 startPoint = transform.position;
        Vector3 midPoint = Vector3.Lerp(startPoint, targetPosition, 0.5f) + (Vector3.up * arcHeight);
        Vector3[] path = { midPoint, targetPosition };
        launchSequence.Append(transform.DOPath(path, duration, PathType.CatmullRom).SetEase(Ease.OutQuad));
        launchSequence.Join(transform.DORotate(new Vector3(0, 0, spinSpeed * duration), duration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
    }
}