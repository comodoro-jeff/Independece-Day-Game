using UnityEngine; // Importa o namespace fundamental da Unity, contendo classes como MonoBehaviour, GameObject, Transform, etc.

public class AlienBossController : MonoBehaviour
{
    // === Configurações do Boss ===
    // Variáveis públicas que podem ser ajustadas no Inspector para controlar o comportamento do chefe.
    public int vidaMaxima = 33; // Define o total de pontos de vida do chefe.
    public GameObject prefabTiroInimigo; // Referência ao prefab do projétil que o chefe irá disparar.
    public float tempoEntreTirosBoss = 1.0f; // Define a frequência (em segundos) com que o chefe dispara.
    public float velocidadeMovimentoBoss = 5.0f; // Controla a velocidade do movimento senoidal do chefe.

    // Variáveis privadas que controlam o estado interno do chefe.
    private int vidaAtual; // Armazena a contagem atual de vida do chefe.
    private float proximoTiroTempo; // Contador para controlar o cooldown do disparo do chefe.
    private float movimentoSenoidalTimer = 0f; // Timer usado para controlar o padrão de movimento senoidal do chefe.

    // Evento estático para notificar outros scripts (como a UI) sobre mudanças na vida do chefe.
    public static event System.Action<int, int> OnBossHealthChanged;

    void Start()
    {
        // Este método é chamado uma vez quando o script é ativado.
        vidaAtual = vidaMaxima; // Inicializa a vida atual do chefe com o valor máximo.
        proximoTiroTempo = Time.time + tempoEntreTirosBoss; // Define o tempo para o primeiro disparo do chefe.
        OnBossHealthChanged?.Invoke(vidaAtual, vidaMaxima); // Dispara o evento de mudança de vida para atualizar a UI inicialmente.
    }

    void Update()
    {
        // === Lógica de Movimento Senoidal do Boss ===
        movimentoSenoidalTimer += Time.deltaTime; // Incrementa o timer de movimento senoidal.
        // Calcula a nova posição X do chefe usando uma função seno para criar um movimento oscilatório.
        float newX = Mathf.Sin(movimentoSenoidalTimer * velocidadeMovimentoBoss) * 7.0f; 
        // Calcula a nova posição Z do chefe, também usando uma função seno para uma leve oscilação em profundidade.
        float newZ = 12.0f + Mathf.Sin(movimentoSenoidalTimer * velocidadeMovimentoBoss * 0.5f) * 2.0f; 

        // Aplica a nova posição calculada ao Transform do GameObject do chefe, mantendo sua altura (Y) atual.
        transform.position = new Vector3(newX, transform.position.y, newZ);

        // === Lógica de Disparo ===
        // Verifica se o tempo para o próximo disparo já passou.
        if (Time.time >= proximoTiroTempo)
        {
            Atirar(); // Chama o método para disparar um projétil.
            proximoTiroTempo = Time.time + tempoEntreTirosBoss; // Define o tempo para o próximo disparo.
        }
    }

    // Método chamado automaticamente pela Unity quando um "Trigger Collider" deste objeto colide com outro collider.
    void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto com o qual colidiu tem a tag "PlayerBullet".
        if (other.CompareTag("PlayerBullet"))
        {
            TakeDamage(); // Chama o método para aplicar dano ao chefe.
            Destroy(other.gameObject); // Destrói o projétil do jogador após a colisão.
        }
    }

    // Aplica dano ao chefe, decrementando sua vida e notificando a UI.
    public void TakeDamage()
    {
        vidaAtual--; // Decrementa a vida atual do chefe.
        OnBossHealthChanged?.Invoke(vidaAtual, vidaMaxima); // Dispara o evento de mudança de vida para atualizar a UI.

        if (vidaAtual <= 0) // Se a vida do chefe chegou a zero.
        {
            Debug.Log("Alien Boss Destruído!"); // Loga uma mensagem de destruição.
            Destroy(gameObject); // Destrói o GameObject do chefe da cena.
        }
    }

    // Instancia um projétil inimigo a partir do chefe.
    void Atirar()
    {
        // Verifica se o prefab do tiro inimigo foi atribuído no Inspector.
        if (prefabTiroInimigo != null)
        {
            // Calcula a posição de spawn do tiro (ligeiramente abaixo e à frente do chefe).
            Vector3 spawnPosition = transform.position + new Vector3(0, -2.0f, -2.0f); 
            // Instancia o prefab do tiro inimigo na posição calculada e sem rotação inicial.
            Instantiate(prefabTiroInimigo, spawnPosition, Quaternion.identity); 
        }
        else // Se o prefab do tiro não foi atribuído.
        {
            // Registra um aviso no console da Unity.
            Debug.LogWarning("Prefab do tiro inimigo não atribuído no AlienBossController do Prefab Alien Boss!"); 
        }
    }
}
