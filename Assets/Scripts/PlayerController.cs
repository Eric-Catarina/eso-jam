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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleInput();
        HandleDashInput();
        HandleThrowInput();
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            Vector2 targetVelocity = input * moveSpeed;
            rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
    }

    void HandleInput()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    void HandleDashInput()
    {
        if (Input.GetKeyDown(dashKey) && Time.time >= lastDashTime + dashCooldown && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        Vector2 dashDir = input != Vector2.zero ? input : Vector2.up;
        rb.velocity = dashDir.normalized * dashForce;

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    void HandleThrowInput()
    {
        if (Input.GetMouseButtonDown(0) && woodPrefab != null && lenhaNoInventario > 0)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            Vector2 throwDirection = (mouseWorldPos - transform.position).normalized;
            Transform spawnPoint = woodSpawnPoint != null ? woodSpawnPoint : transform;

            GameObject wood = Instantiate(woodPrefab, spawnPoint.position, Quaternion.identity);
            Rigidbody2D woodRb = wood.GetComponent<Rigidbody2D>();

            if (woodRb != null)
            {
                woodRb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
            }

            lenhaNoInventario--;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Lenha"))
        {
            lenhaNoInventario++;
            Destroy(collision.gameObject);
        }
    }
}
