// Assets/Scripts/PlayerController.cs
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

    [Header("Inventário")]
    public int lenhaNoInventario = 0;

    private Rigidbody2D rb;
    private Vector2 input;
    private bool isDashing = false;
    private float lastDashTime = -Mathf.Infinity;

    [Header("Ataque Melee Automático")]
    public float attackRadius = 2.5f;
    public float attackCooldown = 1f;
    public float attackDamage = 1;
    public LayerMask enemyLayer;
    public GameObject slashEffectPrefab;
    private float lastAttackTime = -Mathf.Infinity;

    [Header("Lançar Lenha")]
    public GameObject woodPrefab;
    public float throwDuration = 0.7f;
    private Transform bonfireTransform;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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

    void HandleMeleeAttack()
    {
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

                        if (slashEffectPrefab != null)
                        {
                            Vector2 directionToEnemy = (closestEnemy.position - transform.position).normalized;
                            float angle = Mathf.Atan2(directionToEnemy.y, directionToEnemy.x) * Mathf.Rad2Deg;
                            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
                            Instantiate(slashEffectPrefab, closestEnemy.position, targetRotation);
                        }
                    }
                }
            }
        }
    }

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
            AudioManager.Instance.PlaySoundEffect(2);
            StartCoroutine(Dash());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Lenha"))
        {
            ThrownWood woodScript = collision.GetComponent<ThrownWood>();
            if (woodScript != null && !woodScript.isCollectible) return;

            lenhaNoInventario++;
            Destroy(collision.gameObject);
            ThrowWood();
        }
        if (collision.CompareTag("Bonfire"))
        {
            ResetDashCooldown();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Lenha"))
        {
            ThrownWood woodScript = collision.GetComponent<ThrownWood>();
            if (woodScript != null && !woodScript.isCollectible) return;

            lenhaNoInventario++;
            Destroy(collision.gameObject);
            ThrowWood();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    public void ThrowWood()
    {
        lenhaNoInventario--;
        GameObject wood = Instantiate(woodPrefab, transform.position, Quaternion.identity);
        ThrownWood thrownWoodScript = wood.GetComponent<ThrownWood>();

        if (thrownWoodScript != null && bonfireTransform != null)
        {
            thrownWoodScript.Launch(bonfireTransform.position, throwDuration);
        }
    }

    public void ResetDashCooldown()
    {
        lastDashTime = -Mathf.Infinity;
    }

    /// <summary>
    /// Reduz o cooldown atual do dash em uma certa quantidade. Chamado por upgrades.
    /// </summary>
    public void ReduceDashCooldown(float amount)
    {
        if (!isDashing) // Só reduz se não estiver no meio de um dash
        {
            lastDashTime -= amount;
        }
    }
}