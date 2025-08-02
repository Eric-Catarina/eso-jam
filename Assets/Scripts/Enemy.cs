// Assets/Scripts/Enemy.cs
using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    [Header("Atributos")]
    public float speed = 3f;
    public int maxHealth = 2;
    public int dano = 4;
    private int currentHealth;

    [Header("Alvo")]
    private Transform target;

    // --- NOVO ---
    [Header("Drops")]
    [Tooltip("O prefab da lenha que o inimigo pode dropar ao morrer.")]
    public GameObject woodDropPrefab;
    [Tooltip("A chance de dropar a lenha, de 0.0 a 1.0 (ex: 0.5 para 50%).")]
    [Range(0f, 1f)]
    public float woodDropChance = 0.5f;
    private bool fireDeath = false;
    // --- FIM DO NOVO ---

    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();

        GameObject bonfireObject = GameObject.FindGameObjectWithTag("Bonfire");
        if (bonfireObject != null)
        {
            target = bonfireObject.transform;
        }
        else
        {
            Debug.LogError("Inimigo não encontrou a fogueira! Verifique se o objeto Bonfire tem a tag 'Bonfire'.");
        }
    }

    void Update()
    {
        if (target != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (sr != null)
        {
            sr.DOColor(Color.white, 0.1f).SetLoops(2, LoopType.Yoyo);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // --- MÉTODO ATUALIZADO ---// Em Enemy.cs, método Die()

private void Die()
{
        GameManager.Instance.SpawnBlueExplosion(transform.position);

    if (woodDropPrefab != null)
        {
            // Lógica de drop atualizada para incluir o bônus do GameManager
            float totalDropChance = woodDropChance + GameManager.Instance.bonusWoodDropChance;
            if (Random.value < totalDropChance)
            {
                if (fireDeath)
                {
                    GameManager.Instance.SpawnOrangeExplosion(transform.position);
                    Destroy(gameObject);

                    return;
                }
                Instantiate(woodDropPrefab, transform.position, Quaternion.identity);
            }
        }
    Destroy(gameObject);
}
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bonfire"))
        {
            collision.GetComponent<Bonfire>().ReceberDano(dano); // Exemplo de dano
            fireDeath = true;
            Die(); // O inimigo morre ao tocar a fogueira também
        }
    }
}