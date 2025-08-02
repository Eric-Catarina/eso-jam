// Dentro de public class Bonfire : MonoBehaviour
using System;
using UnityEngine;
public class Bonfire : MonoBehaviour
{
    public int energia = 10; // Comece com alguma energia inicial
    public float taxaDePerdaDeEnergia = 0.5f; // Perde 0.5 de energia por segundo
    public LightFlicker luzDaFogueira; // Arraste o objeto da luz da fogueira aqui no Inspector

    private float energiaAtual;

    void Start()
    {
        energiaAtual = energia;
        // Adicione uma verificação para a luz, se ela for essencial
        if (luzDaFogueira == null)
        {
            Debug.LogError("A referência para LightFlicker não foi definida no Bonfire!");
        }
    }

    void Update()
    {
        // Perder energia com o tempo
        energiaAtual -= taxaDePerdaDeEnergia * Time.deltaTime;

        // Atualiza a intensidade da luz com base na energia
        if (luzDaFogueira != null)
        {
            // Mapeia a energia (ex: 0 a 10) para uma intensidade de luz (ex: 0.5 a 1.5)
            luzDaFogueira.baseIntensity = Mathf.Lerp(0.5f, 1.5f, energiaAtual / energia);
            luzDaFogueira.baseOuterRadius = Mathf.Lerp(3f, 7f, energiaAtual / energia);
        }


        // Condição de derrota (Game Over)
        if (energiaAtual <= 0)
        {
            Debug.Log("A FOGUEIRA APAGOU! GAME OVER.");
            // Aqui você pode adicionar a lógica de fim de jogo, como:
            // Time.timeScale = 0; // Pausa o jogo
            // Mostrar uma tela de "Game Over"
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Lenha"))
        {
            energiaAtual++;
            // Garante que a energia não passe do máximo
            energiaAtual = Mathf.Clamp(energiaAtual, 0, energia);
            Destroy(collision.gameObject);
            // Toca um som ao alimentar a fogueira!
            // AudioManager.Instance.PlaySoundEffect(SEU_INDICE_DE_SOM_AQUI);
        }
    }

// Em Bonfire.cs

// ... seu código existente ...

// Implemente o método que estava faltando
public void ReceberDano(int dano)
{
    energiaAtual -= dano;
    // Toca um som de dano na fogueira
    // Adiciona um efeito de partículas de fumaça, etc.
}

// O resto do seu código de Bonfire.cs continua igual...
}