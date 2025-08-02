using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class SwordSlashEffect : MonoBehaviour
{
    [Header("Configurações da Animação")]
    [Tooltip("Duração total da animação do corte, em segundos.")]
    public float animationDuration = 0.3f;

    [Tooltip("Escala final que o sprite irá atingir.")]
    public float targetScale = 1.5f;

    [Tooltip("Rotação final do sprite em graus. Use valores como 90, -90, etc., para mudar a direção do corte.")]
    public float targetRotation = 45f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // Garante que temos a referência do SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SwordSlashEffect precisa de um componente SpriteRenderer neste GameObject!");
            Destroy(gameObject); // Destrói se não houver sprite para animar
        }
    }

    void Start()
    {
        // Garante que o alfa inicial do sprite seja 1 (totalmente visível)
        Color startColor = spriteRenderer.color;
        startColor.a = 1f;
        spriteRenderer.color = startColor;

        // Inicia o sprite com escala zero para que ele "apareça" do nada
        transform.localScale = Vector3.zero;

        // Cria e executa a sequência de animação com DOTween
        AnimateSlash();
    }

    private void AnimateSlash()
    {
        // Cria uma sequência para rodar várias animações ao mesmo tempo
        Sequence slashSequence = DOTween.Sequence();

        // 1. Animação de Escala: cresce do zero até a escala alvo.
        slashSequence.Join(transform.DOScale(targetScale, animationDuration)
            .SetEase(Ease.OutQuad)); // Ease.OutQuad dá uma sensação de "explosão" inicial

        // 2. Animação de Fade: desaparece ao longo da duração.
        slashSequence.Join(spriteRenderer.DOFade(0f, animationDuration)
            .SetEase(Ease.InQuad)); // Ease.InQuad faz o fade acelerar no final

        // // 3. Animação de Rotação: gira para dar um efeito de arco.
        // slashSequence.Join(transform.DORotate(new Vector3(0, 0, targetRotation), animationDuration, RotateMode.FastBeyond360)
        //     .SetEase(Ease.OutSine));

        // Define uma ação para ser executada QUANDO a sequência terminar
        slashSequence.OnComplete(() =>
        {
            // Destrói o GameObject ao final de toda a animação
            Destroy(gameObject);
        });
    }
}