using UnityEngine;

public class Bonfire : MonoBehaviour
{
    public int energia = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Lenha"))
        {
            energia++; // ou algum sistema mais complexo
            Destroy(collision.gameObject);
        }
    }
}
