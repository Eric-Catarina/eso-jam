// Dentro de public class Bonfire : MonoBehaviour
using System;
using UnityEngine;
public class Bonfire : MonoBehaviour
{
    // Substitua "energia" por um sistema de vida atual/máxima
    public float maxHealth = 100f;
    public float currentHealth;
    public float taxaDePerdaDeEnergia = 0.5f; // Agora é um stat público
    public LightFlicker luzDaFogueira; // Arraste o objeto da luz da fogueira aqui no Inspector

    void Start()
    {
        currentHealth = maxHealth;
        // Adicione uma verificação para a luz, se ela for essencial
        if (luzDaFogueira == null)
        {
            Debug.LogError("A referência para LightFlicker não foi definida no Bonfire!");
        }
    }

    void Update()
    {
        currentHealth -= taxaDePerdaDeEnergia * Time.deltaTime;

        if (luzDaFogueira != null)
        {
            // Mapeia a vida (0 a maxHealth) para a intensidade da luz
            luzDaFogueira.baseIntensity = Mathf.Lerp(0.3f, 1.5f, currentHealth / maxHealth);
            luzDaFogueira.baseOuterRadius = Mathf.Lerp(2f, 7f, currentHealth / maxHealth);
        }


        // Condição de derrota (Game Over)
        if (currentHealth <= 0)
        {
            Debug.Log("A FOGUEIRA APAGOU! GAME OVER.");
            // Aqui você pode adicionar a lógica de fim de jogo, como:
            // Time.timeScale = 0; // Pausa o jogo
            // Mostrar uma tela de "Game Over"
        }
    }

    // O OnTriggerEnter2D para lenha agora acontece no Player, então este pode ser simplificado ou removido
    // Se outros objetos além da lenha podem interagir, mantenha-o. Senão, pode apagar.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Lenha"))
        {
            // Aqui você pode adicionar lógica para coletar a lenha, como:
            GameManager.Instance.AddXp(1);
            Destroy(collision.gameObject); // Remove a lenha do jogo
            currentHealth += 10; // Por exemplo, adiciona 10 de vida à fogueira
            currentHealth = Mathf.Min(currentHealth, maxHealth); // Garante que não ultrapasse o máximo
            Debug.Log("Lenha coletada! Vida da fogueira aumentada.");
            GameManager.Instance.SpawnOrangeExplosion(transform.position); // Efeito visual de coleta
        }
          }

    public void ReceberDano(int dano)
    {
        currentHealth -= dano;
    }

    // --- NOVOS MÉTODOS PARA UPGRADES ---
    public void IncreaseMaxHealth(float multiplier)
    {
        float oldMaxHealth = maxHealth;
        maxHealth *= multiplier;
        // Cura a fogueira pela quantidade que a vida máxima aumentou
        currentHealth += maxHealth - oldMaxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Garante que não ultrapasse o novo máximo
    }

    public void HealToFull()
    {
        currentHealth = maxHealth;
    }
}