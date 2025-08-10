// Assets/Scripts/Enemy.cs
using UnityEngine;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;

public abstract class Enemy : MonoBehaviour
{
    [Header("Atributos Base (antes da dificuldade)")]
    public float baseSpeed = 3f;
    public float baseMaxHealth = 2;
    public float dano = 4;
    protected float currentHealth;

    [Header("Alvo e Drops")]
    public GameObject woodDropPrefab;
    [Range(0f, 1f)]
    public float woodDropChance = 0.5f;

    // Variáveis internas
    protected float finalSpeed;
    protected Transform target;
    protected SpriteRenderer sr;
    protected bool fireDeath = false;
    public static Action OnEnemyKilled;

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
        // << NOVO: Notifica o GameManager que um inimigo morreu >>
        GameManager.Instance.OnEnemyKilled();
        
        GameManager.Instance.SpawnBlueExplosion(transform.position);
        OnEnemyKilled?.Invoke();

        if (fireDeath)
        {
            AudioManager.Instance.PlaySoundEffect(6);
            GameManager.Instance.SpawnOrangeExplosion(transform.position);
            Destroy(gameObject);
            return;
        }

        if (woodDropPrefab != null)
        {
            float totalDropChance = woodDropChance * GameManager.Instance.woodDropRate;
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