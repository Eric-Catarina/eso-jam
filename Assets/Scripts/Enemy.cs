// Enemy.cs
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;
    public int danoAFogueira = 1;
    private Transform target;

    void Start()
    {
        // Encontra a fogueira pela tag "Bonfire"
        GameObject bonfire = GameObject.FindGameObjectWithTag("Bonfire");
        if (bonfire != null)
        {
            target = bonfire.transform;
        }
    }

    void Update()
    {
        if (target != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
    }

    // Se o inimigo colidir com a fogueira
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bonfire"))
        {
            // Causa dano à fogueira (você precisará criar um método para isso em Bonfire.cs)
            collision.GetComponent<Bonfire>().ReceberDano(danoAFogueira);
            Destroy(gameObject); // O inimigo se sacrifica
        }
    }
}