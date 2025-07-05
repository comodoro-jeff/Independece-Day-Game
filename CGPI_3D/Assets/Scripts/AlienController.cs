using UnityEngine; // Importa o namespace fundamental da Unity, contendo classes como MonoBehaviour, GameObject, Transform, etc.

public class AlienController : MonoBehaviour
{

    public GameObject prefabTiroInimigo;     // Referência ao prefab do projétil que este alienígena irá disparar.
    public float tempoMinimoEntreTiros = 2.0f; // Tempo mínimo em segundos que este alienígena espera antes de poder atirar novamente.
    public float tempoMaximoEntreTiros = 5.0f; // Tempo máximo em segundos que este alienígena espera antes de poder atirar novamente.
    public GameObject prefabExplosao; // Referência ao prefab do efeito de explosão a ser instanciado quando o alienígena é destruído.
    public int vidaAlien = 1; // Define a vida inicial deste alienígena.
    private int vidaAtualAlien; // Variável privada que armazena a contagem de vida atual do alienígena.

    private float proximoTiroTempo;         // Contador para determinar quando este alienígena pode disparar o próximo tiro.
    private bool podeAtirar = true;         // Flag booleana que controla o cooldown de disparo do alienígena.

    // === Variável para Rotação ===
    public float velocidadeRotacao = 50.0f; // Velocidade em graus por segundo com que o alienígena rotaciona em seu próprio eixo.

    void Start()
    {
        proximoTiroTempo = Time.time + Random.Range(tempoMinimoEntreTiros, tempoMaximoEntreTiros); // Define um tempo inicial aleatório para o primeiro tiro.
        vidaAtualAlien = vidaAlien; // Inicializa a vida atual do alienígena com o valor da vida inicial configurada.
    }

    void Update()
    {
        // Lógica de Disparo: Verifica se o tempo para o próximo tiro já passou e se o alienígena pode atirar (não está em cooldown).
        if (Time.time >= proximoTiroTempo && podeAtirar)
        {
            Atirar(); // Chama o método para disparar um tiro.
            podeAtirar = false; // Define a flag para indicar que o alienígena entrou em cooldown de disparo.
            proximoTiroTempo = Time.time + Random.Range(tempoMinimoEntreTiros, tempoMaximoEntreTiros); // Agenda o tempo para o próximo tiro.
            Invoke("ResetPodeAtirar", 0.5f); // Agenda a reativação da flag 'podeAtirar' após um curto período.
        }

        // Lógica de Rotação Constante: Rotaciona o alienígena em torno do seu eixo Y local (vertical) continuamente.
        transform.Rotate(Vector3.up, velocidadeRotacao * Time.deltaTime); 
    }

    // Este método é chamado para aplicar dano ao alienígena.
    // É tipicamente invocado pelo projétil do jogador quando há uma colisão.
    public void TakeDamage() 
    {
        vidaAtualAlien--; // Decrementa a vida atual do alienígena.
        Debug.Log("Vida do Alien: " + vidaAtualAlien); // Loga a vida restante para depuração.

        if (vidaAtualAlien <= 0) // Verifica se a vida do alienígena chegou a zero.
        {
            // Se a vida acabou, instancia o efeito de explosão na posição do alienígena.
            if (prefabExplosao != null) 
            {
                Instantiate(prefabExplosao, transform.position, Quaternion.identity); 
            }
            Destroy(gameObject); // Destrói o GameObject do alienígena.
            Debug.Log("Alien destruído!"); // Loga uma mensagem de destruição.
            // O comentário abaixo sugere uma funcionalidade de notificação para o GameFlowManager.
            // GameFlowManager.OnAlienDestroyed?.Invoke(); 
        }
    }

    // Método chamado automaticamente pela Unity quando um "Trigger Collider" deste objeto colide com outro collider.
    void OnTriggerEnter(Collider other)
    {        
        // Verifica se o objeto que colidiu tem a tag "PlayerBullet".
        // Esta parte do código está lidando com a colisão do projétil do jogador.
        // No entanto, a lógica de "TakeDamage" para o alien já é chamada pelo PlayerBullet.cs.
        // Este bloco aqui pode causar dupla destruição se o PlayerBullet também destrói o alvo.
        if (other.CompareTag("PlayerBullet")) 
        {
            Destroy(other.gameObject); // Destrói o projétil do jogador após a colisão.
            Destroy(gameObject);      // Destrói este alienígena. ->
            Debug.Log("Alien atingido e destruído!"); // Loga a informação de que o alienígena foi atingido e destruído.
        }
    }

    // Método responsável por criar e disparar um projétil inimigo.
    void Atirar()
    {
        // Verifica se o prefab do tiro inimigo foi atribuído no Inspector.
        if (prefabTiroInimigo != null) 
        {
            // Calcula a posição de spawn do tiro (ligeiramente abaixo do alienígena).
            Vector3 spawnPosition = transform.position + new Vector3(0, -0.5f, 0); 
            // Instancia o prefab do tiro inimigo na posição calculada e sem rotação inicial.
            Instantiate(prefabTiroInimigo, spawnPosition, Quaternion.identity); 
        }
        else // Se o prefab do tiro não foi atribuído.
        {
            Debug.LogWarning("Prefab do tiro inimigo não atribuído no AlienController do Prefab Alien!"); // Loga um aviso no console.
        }
    }

    // Método privado usado para reativar a flag 'podeAtirar' após o cooldown de disparo.
    private void ResetPodeAtirar()
    {
        podeAtirar = true; // Define a flag para permitir que o alienígena atire novamente.
    }
}