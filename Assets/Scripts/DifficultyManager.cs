// Assets/Scripts/DifficultyManager.cs
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [Header("Controle de Tempo")]
    [Tooltip("Duração total da partida em segundos (ex: 120 para 2 minutos).")]
    public float totalMatchTime = 300f;
    private float elapsedTime = 0f;
    private bool isGameActive = true;

    [Header("Curvas de Dificuldade (0 a 1)")]
    [Tooltip("Controla a taxa de spawn. X=0 (início), X=1 (fim). Y é o multiplicador.")]
    public AnimationCurve spawnRateCurve;

    [Tooltip("Controla a velocidade dos inimigos.")]
    public AnimationCurve enemySpeedCurve;

    [Tooltip("Controla a vida dos inimigos.")]

    public AnimationCurve enemyHealthCurve;

    // --- Propriedades Públicas para outros scripts consultarem ---
    public float SpawnRateMultiplier;
    public float EnemySpeedMultiplier;
    public float EnemyHealthMultiplier;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Garante que o jogo comece na dificuldade inicial
        UpdateDifficulty(0);
    }

    private void Update()
    {
        if (!isGameActive) return;

        elapsedTime += Time.deltaTime;

        // Calcula o progresso do tempo como uma porcentagem de 0 a 1
        float timeProgress = Mathf.Clamp01(elapsedTime / totalMatchTime);

        UpdateDifficulty(timeProgress);

        // Se o tempo acabar, você pode adicionar uma condição de vitória aqui
        if (elapsedTime >= totalMatchTime)
        {
            isGameActive = false;
            Debug.Log("SOBREVIVEU! VITÓRIA!");
            // GameManager.Instance.WinGame();
        }
    }

    private void UpdateDifficulty(float progress)
    {
        // Avalia cada curva para obter os multiplicadores atuais
        SpawnRateMultiplier = spawnRateCurve.Evaluate(progress);
        EnemySpeedMultiplier = enemySpeedCurve.Evaluate(progress);
        EnemyHealthMultiplier = enemyHealthCurve.Evaluate(progress);
    }

    public void StopTimer()
    {
        isGameActive = false;
    }
    public void ResetTimer()
    {
        isGameActive = true;
    }
}