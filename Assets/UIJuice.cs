using UnityEngine;
using UnityEngine.Events;
using DG.Tweening; // Certifique-se de que esta linha está presente

[RequireComponent(typeof(CanvasGroup))]
public class UIJuice : MonoBehaviour
{
    [Header("Configurações de Animação")]
    [SerializeField] protected float duration = 0.5f;
    [SerializeField] protected float delay = 0f;
    [SerializeField] protected Ease easeType = Ease.OutBack;
    [SerializeField] private Vector3 startScale = new Vector3(0.8f, 0.8f, 0.8f);

    [Header("Comportamento")]
    [SerializeField] protected bool playOnEnable = false;

    [Header("Eventos de Animação")]
    [Tooltip("Disparado quando a animação de entrada começa.")]
    public UnityEvent OnPlayStart;

    [Tooltip("Disparado quando a animação de entrada é concluída.")]
    public UnityEvent OnPlayComplete;

    [Tooltip("Disparado quando a animação de saída começa.")]
    public UnityEvent OnReverseStart;

    [Tooltip("Disparado quando a animação de saída é concluída.")]
    public UnityEvent OnReverseComplete;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Sequence sequence;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    protected virtual void OnEnable()
    {
        if (playOnEnable)
        {
            PlayAnimation();
        }
    }

    /// <summary>
    /// Deixa o painel ativo e inicia a animação de entrada.
    /// Útil para chamar a partir de botões ou outros scripts.
    /// </summary>
    public void SetActiveAndPlay()
    {
        gameObject.SetActive(true);
        PlayAnimation();
    }

    /// <summary>
    /// Inicia a animação de "entrada" (por exemplo, abrir uma tela).
    /// </summary>
    public virtual void PlayAnimation()
    {
        KillExistingSequence();
        CreateForwardSequence();

        if (sequence != null)
        {
            sequence.OnStart(() => OnPlayStart?.Invoke());
            sequence.OnComplete(() => OnPlayComplete?.Invoke());
            sequence.Play();
        }
    }

    /// <summary>
    /// Inicia a animação de "saída" (por exemplo, fechar uma tela).
    /// </summary>
    public virtual void PlayReverseAnimation()
    {
        KillExistingSequence();
        CreateReverseSequence();

        if (sequence != null)
        {
            sequence.OnStart(() => OnReverseStart?.Invoke());
            sequence.OnComplete(() => OnReverseComplete?.Invoke());
            sequence.Play();
        }
    }

    /// <summary>
    /// Cria a sequência de animação de entrada (aparecer/abrir).
    /// </summary>
    private void CreateForwardSequence()
    {
        gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;

        // Define o estado inicial antes da animação
        canvasGroup.alpha = 0f;
        rectTransform.localScale = startScale;

        // Adicione .SetUpdate(UpdateType.Realtime, true) para que a sequência ignore o timeScale
        sequence = DOTween.Sequence(true).SetUpdate(UpdateType.Normal, true);

        if (delay > 0)
        {
            sequence.AppendInterval(delay);
        }

        // Adicione .SetUpdate(UpdateType.Realtime, true) para cada tween individualmente
        sequence.Append(canvasGroup.DOFade(1f, duration).SetEase(easeType).SetUpdate(UpdateType.Normal, true));
        sequence.Join(rectTransform.DOScale(Vector3.one, duration).SetEase(easeType).SetUpdate(UpdateType.Normal, true));

        // Pausa a sequência para que o método Play() possa controlá-la
        sequence.Pause();
    }

    /// <summary>
    /// Cria a sequência de animação de saída (desaparecer/fechar).
    /// </summary>
    private void CreateReverseSequence()
    {
        // Impede cliques durante a animação de saída
        canvasGroup.blocksRaycasts = false;

        // Adicione .SetUpdate(UpdateType.Realtime, true) para que a sequência ignore o timeScale
        sequence = DOTween.Sequence(true).SetUpdate(UpdateType.Normal, true);

        // O delay não é aplicado na animação reversa por padrão, mas pode ser adicionado se necessário
        // Adicione .SetUpdate(UpdateType.Realtime, true) para cada tween individualmente
        sequence.Append(canvasGroup.DOFade(0f, duration).SetEase(Ease.InBack).SetUpdate(UpdateType.Normal, true));
        sequence.Join(rectTransform.DOScale(startScale, duration).SetEase(Ease.InBack).SetUpdate(UpdateType.Normal, true));
        sequence.OnComplete(() => gameObject.SetActive(false)); // Desativa o objeto ao final

        // Pausa a sequência para que o método Play() possa controlá-la
        sequence.Pause();
    }

    /// <summary>
    /// Para e destrói qualquer sequência de animação ativa para evitar sobreposições.
    /// </summary>
    private void KillExistingSequence()
    {
        if (sequence != null && sequence.IsActive())
        {
            sequence.Kill();
        }
    }

    protected virtual void OnDestroy()
    {
        KillExistingSequence();
    }
}