using UnityEngine; // Importa o namespace fundamental da Unity, contendo classes como MonoBehaviour, GameObject, Transform, etc.

public class PlayerBullet : MonoBehaviour
{
    // Variáveis públicas que podem ser ajustadas no Inspector da Unity.
    public float velocidade = 10.0f; // Controla a velocidade de movimento do projétil do jogador.
    public float tempoDeVida = 2.0f; // Define por quanto tempo o projétil existirá antes de ser automaticamente destruído.

    // === Nova variável para Rotação do Tiro ===
    public float velocidadeRotacao = 720.0f; // Controla a velocidade de rotação do projétil em torno de seu próprio eixo (para um efeito de parafuso).

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
        // Este método é chamado uma vez por frame para atualizar a lógica do projétil.

        // Move o projétil para frente (no eixo Z local), na direção para a qual ele está apontando.
        // 'transform.forward' garante que o tiro se move na direção 'para frente' do seu próprio objeto.
        transform.Translate(transform.forward * velocidade * Time.deltaTime, Space.World);

        // === Lógica de Rotação do Tiro ===
        // Rotaciona o projétil em torno de seu próprio eixo de movimento (transform.forward).
        // Isso cria o efeito de "parafuso" enquanto ele viaja.
        transform.Rotate(transform.forward, velocidadeRotacao * Time.deltaTime, Space.Self);
    }

    // Método chamado automaticamente pela Unity quando um "Trigger Collider" deste objeto colide com outro collider.
    /// <param name="other">O Collider do outro GameObject que entrou em contato com este.</param>
    void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto com o qual colidiu tem a tag "Alien".
        if (other.CompareTag("Alien"))
        {
            // Tenta obter o componente AlienController do GameObject atingido.
            AlienController alien = other.GetComponent<AlienController>();
            if (alien != null)
            {
                // Informa ao alien que ele foi atingido, chamando seu método TakeDamage().
                alien.TakeDamage();
            }
            SpawnExplosion(); // Instancia o efeito de explosão na posição do tiro do jogador.
            Destroy(gameObject); // Destrói o projétil do jogador.
            Debug.Log("Alien atingido!"); // Loga uma mensagem para depuração.
        }
        // Se o objeto que colidiu tem a tag "AlienBoss".
        else if (other.CompareTag("AlienBoss"))
        {
            // Tenta obter o componente AlienBossController do GameObject atingido.
            AlienBossController boss = other.GetComponent<AlienBossController>();
            if (boss != null)
            {
                // Informa ao boss que ele foi atingido, chamando seu método TakeDamage().
                boss.TakeDamage();
            }
            SpawnExplosion(); // Instancia o efeito de explosão na posição do tiro do jogador.
            Destroy(gameObject); // Destrói o projétil do jogador.
            Debug.Log("Boss atingido!"); // Loga uma mensagem para depuração.
        }
        // Se o objeto que colidiu tem a tag "EnemyBullet" (colisão entre tiros).
        else if (other.CompareTag("EnemyBullet"))
        {
            // Aciona uma explosão no local da colisão. Ambos os tiros (jogador e inimigo) serão destruídos.
            // O tiro inimigo será destruído e explodirá em seu próprio script EnemyBullet.cs.
            SpawnExplosion(); // Instancia o efeito de explosão do tiro do jogador.
            
            // Destrói o projétil do jogador.
            Destroy(gameObject); 
        }
        // Para qualquer outra coisa que o tiro do jogador não deve passar por (ex: ambiente, parede, objetos não interativos).
        // A condição verifica que a colisão NÃO é com o jogador, nem com outro tiro inimigo, nem com um asteroide de fundo.
        else if (!other.CompareTag("Player") && !other.CompareTag("EnemyBullet") && !other.CompareTag("BackgroundAsteroid"))
        {
            SpawnExplosion(); // Instancia o efeito de explosão do tiro do jogador ao colidir.
            Destroy(gameObject); // Destrói o projétil do jogador.
        }
    }

    void SpawnExplosion()
    {
        // Verifica se o prefab da explosão foi atribuído no Inspector.
        if (prefabExplosao != null)
        {
            Instantiate(prefabExplosao, transform.position, Quaternion.identity); // Cria a explosão na posição do tiro.
        }
        else // Se o prefab da explosão não foi atribuído.
        {
            Debug.LogWarning("Prefab de explosão não atribuído no PlayerBullet!"); // Loga um aviso no console.
        }
    }
}