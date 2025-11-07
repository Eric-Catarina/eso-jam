// Assets/Scripts/Enemy.cs
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public abstract class Enemy : MonoBehaviour
{
    [Header("Atributos Base (antes da dificuldade)")]
    public float baseSpeed = 3f;
    public float baseMaxHealth = 2;
    public float dano = 4;
    protected float currentHealth;

    [Header("Drops")]
    public GameObject woodDropPrefab;
    [Tooltip("Define a quantidade base de lenha que este inimigo dropa. Pode ser um valor fracionado (ex: 0.5 para 50% de chance de dropar 1).")]
    public float baseWoodToDrop = 0.5f;

    // Variáveis internas
    protected float finalSpeed;
    protected Transform target;
    protected SpriteRenderer sr;
    protected bool fireDeath = false;

    protected virtual void Start()
    {
        ApplyDifficultyModifiers();
        currentHealth = baseMaxHealth;
        sr = GetComponentInChildren<SpriteRenderer>();

        GameObject bonfireObject = GameObject.FindGameObjectWithTag("Bonfire");
        if (bonfireObject != null)
        {
            target = bonfireObject.transform;
        }
    }

    protected virtual void ApplyDifficultyModifiers()
    {
        finalSpeed = baseSpeed * DifficultyManager.Instance.EnemySpeedMultiplier;
        baseMaxHealth = Mathf.RoundToInt(baseMaxHealth * DifficultyManager.Instance.EnemyHealthMultiplier);
    }

    private void Update()
    {
        if (target != null)
        {
            Move();
        }
    }

    protected abstract void Move();

    public void TakeDamage(float damage)
    {
        AudioManager.Instance.PlaySoundEffect(2);
        currentHealth -= damage;
        sr.DOColor(Color.white, 0.1f).SetLoops(2, LoopType.Yoyo);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // Dispara o evento global, passando este componente do inimigo para qualquer script que esteja ouvindo.
        GameEvents.RaiseEnemyKilled(this);

        GameManager.Instance.SpawnBlueExplosion(transform.position);

        if (fireDeath)
        {
            AudioManager.Instance.PlaySoundEffect(6);
            GameManager.Instance.SpawnOrangeExplosion(transform.position);
        }
        else
        {
            // A lógica de drop só acontece se o inimigo não morreu na fogueira.
            HandleDrops();
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Calcula e instancia os drops de lenha com base na quantidade esperada.
    /// </summary>
    protected virtual void HandleDrops()
    {
        if (woodDropPrefab == null) return;

        // Calcula a quantidade de drops com base no valor do inimigo e no multiplicador global.
        float expectedDrops = baseWoodToDrop * GameManager.Instance.woodDropMultiplier;
        int wholeDrops = Mathf.FloorToInt(expectedDrops);
        float fractionalChance = expectedDrops - wholeDrops;

        // Dropa a quantidade garantida.
        for (int i = 0; i < wholeDrops; i++)
        {
            SpawnWood();
        }

        // Verifica a chance fracionária de dropar um extra.
        if (Random.value < fractionalChance)
        {
            SpawnWood();
        }
    }

    /// <summary>
    /// Instancia uma única peça de lenha na posição do inimigo.
    /// </summary>
    private void SpawnWood()
    {
        AudioManager.Instance.PlaySoundEffect(0);
        // Adiciona um pequeno offset aleatório para que as lenhas não fiquem empilhadas.
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
        Instantiate(woodDropPrefab, spawnPos, Quaternion.identity);
    }


    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bonfire"))
        {
            collision.GetComponent<Bonfire>().ReceberDano(dano);
            fireDeath = true;
            Die();
        }
    }
}