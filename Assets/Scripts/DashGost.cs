using UnityEngine;

public class DashGhost : MonoBehaviour
{
    public float duration = 0.3f;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("DashGhost: SpriteRenderer não encontrado!");
            Destroy(gameObject); // Se não tem sprite, destrói
            return;
        }

        sr.sortingOrder -= 1;
        Color color = sr.color;
        color.a = 0.8f; // começa com transparência visível
        sr.color = color;

        Destroy(gameObject, duration);
    }

    void Update()
    {
        if (sr == null) return;

        Color color = sr.color;
        color.a -= Time.deltaTime / duration;
        sr.color = color;
    }
}
