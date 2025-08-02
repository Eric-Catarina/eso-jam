// Assets/Scripts/Enemy.cs
using UnityEngine;
using DG.Tweening; // Importe o DOTween para o feedback visual

public class Enemy : MonoBehaviour
{
    [Header("Atributos")]
    public float speed = 3f;
    public int maxHealth = 2;
    private int currentHealth;

    [Header("Alvo")]
    private Transform target; // O alvo do inimigo (a fogueira)

    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();

        // Encontra a fogueira pela tag "Bonfire"
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
        // Movimenta o inimigo em direção ao alvo
        if (target != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
    }

    // Método público para que outros objetos possam causar dano
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Feedback visual de dano: pisca em branco rapidamente
        if (sr != null)
        {
            sr.DOColor(Color.white, 0.1f).SetLoops(2, LoopType.Yoyo);
        }
        
        // Toca som de dano
        // AudioManager.Instance.PlaySoundEffect(SEU_INDICE_DE_DANO_AQUI);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Toca som de morte
        // AudioManager.Instance.PlaySoundEffect(SEU_INDICE_DE_MORTE_AQUI);
        
        // Aqui você pode adicionar um efeito de partículas de morte antes de destruir
        
        Destroy(gameObject);
    }
    
    // Opcional: dano ao colidir com a fogueira
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bonfire"))
        {
            // Adicione a lógica de dano à fogueira se quiser
            // collision.GetComponent<Bonfire>().ReceberDano(1);
            Destroy(gameObject);
        }
    }
}