// Assets/Scripts/Enemy.cs
using UnityEngine;
using DG.Tweening;

public abstract class Enemy : MonoBehaviour
{
    [Header("Atributos Base (antes da dificuldade)")]
    public float baseSpeed = 3f;
    public int baseMaxHealth = 2;
    public int dano = 4;
    protected int currentHealth;

    [Header("Alvo e Drops")]
    public GameObject woodDropPrefab;
    [Range(0f, 1f)]
    public float woodDropChance = 0.5f;

    // Variáveis internas
    protected float finalSpeed;
    protected Transform target;
    protected SpriteRenderer sr;
    protected bool fireDeath = false;

    protected virtual void Start()
    {
        // Aplica os multiplicadores de dificuldade no momento do spawn
        ApplyDifficultyModifiers();
        
        currentHealth = baseMaxHealth; // A vida atual começa no máximo
                                       // Pega o sprite renderer do filho
        sr = GetComponentInChildren<SpriteRenderer>();

        GameObject bonfireObject = GameObject.FindGameObjectWithTag("Bonfire");
        if (bonfireObject != null)
        {
            target = bonfireObject.transform;
        }
    }

    // NOVO: Método que aplica a dificuldade
    protected virtual void ApplyDifficultyModifiers()
    {
        // Pega os multiplicadores do DifficultyManager
        finalSpeed = baseSpeed * DifficultyManager.Instance.EnemySpeedMultiplier;
        
        // Multiplica a vida base pelo modificador e arredonda para o inteiro mais próximo
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

    public void TakeDamage(int damage)
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

        GameManager.Instance.SpawnBlueExplosion(transform.position);

        if (fireDeath)
        {
                            AudioManager.Instance.PlaySoundEffect(6);

            GameManager.Instance.SpawnOrangeExplosion(transform.position);
            Destroy(gameObject);
            return;
        }

        if (woodDropPrefab != null)
        {
            float totalDropChance = woodDropChance + GameManager.Instance.bonusWoodDropChance;
            if (Random.value < totalDropChance)
            {
                AudioManager.Instance.PlaySoundEffect(0);

                Instantiate(woodDropPrefab, transform.position, Quaternion.identity);
            }
        }
        Destroy(gameObject);
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