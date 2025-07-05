using UnityEngine; // Importa o namespace fundamental da Unity, contendo classes como MonoBehaviour, GameObject, Transform, etc.

public class EnemyBullet : MonoBehaviour
{
    // Variáveis públicas que podem ser ajustadas no Inspector da Unity.
    public float velocidade = 10.0f; // Controla a velocidade de movimento do projétil inimigo.
    public float tempoDeVida = 3.0f; // Define por quanto tempo o projétil existirá antes de ser automaticamente destruído.
    public float velocidadeRotacao = 500.0f; // Controla a velocidade de rotação do projétil em torno de seu próprio eixo.
    public GameObject prefabExplosao; // Referência ao prefab do efeito de explosão a ser instanciado quando o tiro é destruído.

    void Start()
    {
        // Este método é chamado uma vez quando o script é ativado.
        // Agenda a destruição do GameObject do projétil após o 'tempoDeVida' especificado,
        // garantindo que projéteis que não colidem sejam removidos da cena.
        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        // Move o projétil para "baixo" (no eixo Z negativo local), simulando que está indo em direção ao jogador.
        // 'Space.Self' garante que o movimento é relativo aos eixos locais do projétil.
        transform.Translate(Vector3.forward * -velocidade * Time.deltaTime, Space.Self);

        // Rotaciona o projétil em torno de seu próprio eixo "para frente" (eixo Z local), criando um efeito de parafuso.
        // 'Space.Self' garante que a rotação é relativa aos eixos locais do projétil.
        transform.Rotate(transform.forward, velocidadeRotacao * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto com o qual colidiu tem a tag "Player".
        if (other.CompareTag("Player"))
        {
            // Se o tiro inimigo atinge o jogador, ele é destruído.
            Destroy(gameObject); 
        }
        //  Se colidir com um PlayerBullet
        else if (other.CompareTag("PlayerBullet"))
        {
            // Aciona a explosão para AMBOS os tiros no local da colisão.
            // O tiro do jogador será destruído e explodirá em seu próprio script PlayerBullet.cs
            SpawnExplosion(); // Explode o tiro inimigo
            // Destrói o tiro inimigo.
            Destroy(gameObject); 
        }
        // Para qualquer outra coisa que o tiro inimigo não deve passar por (ex: ambiente, asteroides, etc.)
        else if (
            !other.CompareTag("Alien") && // Não é um Alien
            !other.CompareTag("EnemyBullet") && // Não é outro tiro inimigo
            !other.CompareTag("AlienBoss") // Não é o boss
        )
        {
            SpawnExplosion(); // Explode o tiro inimigo
            Destroy(gameObject);
        }
    }

    // Instancia o prefab de explosão na posição do tiro.
    void SpawnExplosion()
    {
        if (prefabExplosao != null)
        {
            Instantiate(prefabExplosao, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Prefab de explosão não atribuído no EnemyBullet!");
        }
    }
}