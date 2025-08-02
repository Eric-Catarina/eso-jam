using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveSpeed = 8f;
    public float acceleration = 10f;

    [Header("Dash")]
    public float dashForce = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 2f;
    public KeyCode dashKey = KeyCode.Space;

    [Header("Lançar Lenha")]
    public GameObject woodPrefab;
    public float throwForce = 10f;
    public Transform woodSpawnPoint;

    [Header("Inventário")]
    public int lenhaNoInventario = 0;

    private Rigidbody2D rb;
    private Vector2 input;
    private bool isDashing = false;
    private float lastDashTime = -Mathf.Infinity;

    [Header("Ataque Melee Automático")]
    public float attackRadius = 2.5f;   // O alcance do ataque
    public float attackCooldown = 1f;   // Ataca a cada 1 segundo
    public float  attackDamage = 1;        // Dano do ataque
    public LayerMask enemyLayer;        // Defina no Inspector qual camada é a dos inimigos
    public GameObject slashEffectPrefab;        // Efeito visual opcional para o ataque
    private float lastAttackTime = -Mathf.Infinity; // Timer para o ataque

    [Header("Lançar Lenha")]
    public float throwDuration = 0.7f; // Duração do voo da lenha
    private Transform bonfireTransform;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Pega a referência do LineRenderer e o desativa


        // Encontra a fogueira na cena para saber o alvo do arremesso
        GameObject bonfireObj = GameObject.FindGameObjectWithTag("Bonfire");
        if (bonfireObj != null)
        {
            bonfireTransform = bonfireObj.transform;
        }
    }

    void Update()
    {
        HandleInput();
        HandleDashInput();
        // HandleAimingAndThrowing(); // Substitui o antigo HandleThrowInput
        HandleMeleeAttack();
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            Vector2 targetVelocity = input * moveSpeed;
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
    }

    // NOVO: Lógica do ataque automático
    void HandleMeleeAttack()
    {
        // A lógica para verificar o cooldown permanece a mesma
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);

            if (enemiesInRange.Length > 0)
            {
                Transform closestEnemy = enemiesInRange
                    .OrderBy(enemy => Vector2.Distance(transform.position, enemy.transform.position))
                    .FirstOrDefault()?.transform;

                if (closestEnemy != null)
                {
                    lastAttackTime = Time.time;
                    Enemy enemyScript = closestEnemy.GetComponent<Enemy>();

                    if (enemyScript != null)
                    {
                        enemyScript.TakeDamage(attackDamage);

                        // --- INÍCIO DA LÓGICA DE ROTAÇÃO ---

                        if (slashEffectPrefab != null)
                        {
                            // 1. Calcula o vetor de direção do jogador para o inimigo.
                            //    Isso nos dá a "seta" que aponta para o alvo.
                            Vector2 directionToEnemy = (closestEnemy.position - transform.position).normalized;

                            // 2. Converte essa direção em um ângulo em graus.
                            //    Mathf.Atan2 é perfeito para isso, e Rad2Deg converte de radianos para graus.
                            float angle = Mathf.Atan2(directionToEnemy.y, directionToEnemy.x) * Mathf.Rad2Deg;

                            // 3. Cria a rotação final (Quaternion) que será usada na instanciação.
                            //    A rotação acontece no eixo Z em um jogo 2D.
                            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

                            // 4. Instancia o prefab na posição do inimigo e com a rotação que acabamos de calcular.
                            //    Substituímos o Quaternion.identity pela nossa targetRotation.
                            Instantiate(slashEffectPrefab, closestEnemy.position, targetRotation);


                        }

                        // --- FIM DA LÓGICA DE ROTAÇÃO ---
                    }
                }
            }
        }
    }

    // ... Seus outros métodos (HandleInput, Dash, etc.) ...
    void HandleInput()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        Vector2 dashDir = input != Vector2.zero ? input : (transform.up);
        rb.velocity = dashDir.normalized * dashForce;
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }


    void HandleDashInput()
    {
        if (Input.GetKeyDown(dashKey) && Time.time >= lastDashTime + dashCooldown && !isDashing)
        {
            AudioManager.Instance.PlaySoundEffect(2); // Toca o som do dash
            StartCoroutine(Dash());
        }
    }

    // Dentro de PlayerController.cs

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Lenha"))
        {
            // Pega o script da lenha que colidiu
            ThrownWood woodScript = collision.GetComponent<ThrownWood>();

            // Se a lenha não existir ou não for coletável, ignora.
            // O "woodScript == null" é para lenhas que podem estar no chão sem esse script.
            if (woodScript != null && !woodScript.isCollectible)
            {
                return;
            }

            // Se passar, a lenha é coletável.
            lenhaNoInventario++;
            Destroy(collision.gameObject);
            ThrowWood(); // Lança a lenha automaticamente ao coletar
        }
        if (collision.CompareTag("Bonfire"))
        {
            ResetDashCooldown(); // Reseta o cooldown do dash ao entrar na fogueira
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Lenha"))
        {
            ThrownWood woodScript = collision.GetComponent<ThrownWood>();

            // Se a lenha não existir ou não for coletável, ignora.
            // O "woodScript == null" é para lenhas que podem estar no chão sem esse script.
            if (woodScript != null && !woodScript.isCollectible)
            {
                return;
            }

            // Se passar, a lenha é coletável.
            lenhaNoInventario++;
            Destroy(collision.gameObject);
            ThrowWood(); // Lança a lenha automaticamente ao coletar
        }
    }

    // NOVO: Visualizar o raio de ataque no Editor da Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
    public void ThrowWood()
    {

        lenhaNoInventario--;
        // AudioManager.Instance.PlaySoundEffect(SEU_INDICE_DE_ARREMESSO_AQUI);

        GameObject wood = Instantiate(woodPrefab, transform.position, Quaternion.identity);
        ThrownWood thrownWoodScript = wood.GetComponent<ThrownWood>();

        if (thrownWoodScript != null)
        {
            // Inicia a animação de arremesso em direção à fogueira
            thrownWoodScript.Launch(bonfireTransform.position, throwDuration);
        }
    }

    public void ResetDashCooldown()
    {
        lastDashTime = -Mathf.Infinity; // Reseta o cooldown do dash
    }

}