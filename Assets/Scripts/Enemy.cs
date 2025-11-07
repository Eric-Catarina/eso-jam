// Assets/Scripts/Enemy.cs
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public abstract class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyData enemyData;
    public float dano { get; private set; }
    protected float currentHealth;
    
    protected float finalSpeed;
    public Transform target { get; private set; }
    protected SpriteRenderer sr;
    protected bool fireDeath = false;

    private IEnemyState currentState;
    private float randomOffset;

    public void Initialize(EnemyData data)
    {
        enemyData = data;
        dano = enemyData.dano;

        ApplyDifficultyModifiers();
        currentHealth = enemyData.baseMaxHealth;
        
        sr = GetComponentInChildren<SpriteRenderer>();
        randomOffset = Random.Range(0f, 100f);

        GameObject bonfireObject = GameObject.FindGameObjectWithTag("Bonfire");
        if (bonfireObject != null)
        {
            target = bonfireObject.transform;
        }
        
        ChangeState(new ChaseState());
    }

    protected virtual void ApplyDifficultyModifiers()
    {
        finalSpeed = enemyData.baseSpeed * DifficultyManager.Instance.EnemySpeedMultiplier;
        // Vida máxima já é definida com base no EnemyData, que será modificado pela dificuldade
        enemyData.baseMaxHealth = Mathf.RoundToInt(enemyData.baseMaxHealth * DifficultyManager.Instance.EnemyHealthMultiplier);
    }

    private void Update()
    {
        currentState?.Execute(this);
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState = newState;
        currentState.OnEnter(this);
    }

    public abstract void PerformMovement();
    
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
        GameEvents.RaiseEnemyKilled(this);
        GameManager.Instance.SpawnBlueExplosion(transform.position);

        if (fireDeath)
        {
            AudioManager.Instance.PlaySoundEffect(6);
            GameManager.Instance.SpawnOrangeExplosion(transform.position);
        }
        else
        {
            HandleDrops();
        }
        Destroy(gameObject);
    }
    
    public void SetFireDeath() => fireDeath = true;
    public void ForceDie() => Die();

    protected virtual void HandleDrops()
    {
        if (GameManager.Instance.woodDropPrefab == null) return;

        float expectedDrops = enemyData.baseWoodToDrop * GameManager.Instance.woodDropMultiplier;
        int wholeDrops = Mathf.FloorToInt(expectedDrops);
        float fractionalChance = expectedDrops - wholeDrops;

        for (int i = 0; i < wholeDrops; i++) SpawnWood();
        if (Random.value < fractionalChance) SpawnWood();
    }

    private void SpawnWood()
    {
        AudioManager.Instance.PlaySoundEffect(0);
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
        Instantiate(GameManager.Instance.woodDropPrefab, spawnPos, Quaternion.identity);
    }
    
    public EnemySaveData GetSaveData()
    {
        return new EnemySaveData
        {
            type = enemyData.enemyType,
            position = transform.position,
            currentHealth = this.currentHealth
        };
    }

    public void LoadFromSaveData(EnemySaveData data)
    {
        this.currentHealth = data.currentHealth;
    }
}