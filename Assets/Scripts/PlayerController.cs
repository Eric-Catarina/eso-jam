using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveSpeed = 5f;

    [Header("Jiggle")]
    public Transform spriteTransform; // Referência ao objeto visual do jogador
    public float jiggleScale = 0.9f;
    public float jiggleDuration = 0.3f;

    [Header("Lenha")]
    public GameObject woodPrefab;
    public float throwRange = 5f;
    public float throwDuration = 0.3f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartJiggle();
    }

    void Update()
    {
        HandleMovementInput();
        HandleThrowInput();
    }

    void HandleMovementInput()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (moveInput.magnitude > 0.01f)
        {
            Vector3 moveDelta = (Vector3)moveInput * moveSpeed * Time.deltaTime;
            transform.position += moveDelta;
        }
    }

    void HandleThrowInput()
    {
        if (Input.GetMouseButtonDown(0) && woodPrefab != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            Vector3 direction = (mouseWorldPos - transform.position).normalized;
            Vector3 targetPos = transform.position + direction * throwRange;

            GameObject thrownWood = Instantiate(woodPrefab, transform.position, Quaternion.identity);
            
            // DOTween animation
            thrownWood.transform.DOMove(targetPos, throwDuration).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // Aqui você pode fazer a lenha "cair no chão" ou desaparecer
                    // Destroy(thrownWood, 3f);
                });
        }
    }

    void StartJiggle()
    {
        if (spriteTransform != null)
        {
            // Loop infinito de pulinhos
            spriteTransform
                .DOScale(new Vector3(jiggleScale, 1.1f / jiggleScale, 1f), jiggleDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
}
