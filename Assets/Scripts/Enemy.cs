// Assets/Scripts/Enemy.cs
using UnityEngine;
using DG.Tweening;

public abstract class Enemy : MonoBehaviour
{
    [Header("Atributos Base")]
    public float speed = 3f;
    public int maxHealth = 2;
    public int dano = 4;
    protected int currentHealth;

    [Header("Alvo e Drops")]
    public GameObject woodDropPrefab;
    [Range(0f, 1f)]
    public float woodDropChance = 0.5f;
    protected Transform target;
    protected SpriteRenderer sr;
    protected bool fireDeath = false;

    // Usamos 'protected virtual' para que as classes filhas possam adicionar sua própria lógica
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();

        GameObject bonfireObject = GameObject.FindGameObjectWithTag("Bonfire");
        if (bonfireObject != null)
        {
            target = bonfireObject.transform;
        }
    }

    private void Update()
    {
        if (target != null)
        {
            Move();
        }
    }

    // O método de movimento agora é abstrato, forçando cada filho a ter seu próprio jeito de se mover
    protected abstract void Move();

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        sr.DOColor(Color.white, 0.1f).SetLoops(2, LoopType.Yoyo);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // O método Die() agora é virtual para que o inimigo Tank possa sobrescrevê-lo
    protected virtual void Die()
    {
        GameManager.Instance.SpawnBlueExplosion(transform.position);

        if (fireDeath)
        {
            GameManager.Instance.SpawnOrangeExplosion(transform.position);
            Destroy(gameObject);
            return;
        }

        if (woodDropPrefab != null)
        {
            float totalDropChance = woodDropChance + GameManager.Instance.bonusWoodDropChance;
            if (Random.value < totalDropChance)
            {
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