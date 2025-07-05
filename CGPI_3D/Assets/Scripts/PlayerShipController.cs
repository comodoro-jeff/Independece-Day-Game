using UnityEngine; // Importa o namespace fundamental da Unity, contendo classes como MonoBehaviour, GameObject, Transform, etc.
using UnityEngine.InputSystem; // Importa o namespace do novo sistema de input da Unity, para detectar entradas do teclado como 'Keyboard.current'.


public class PlayerShipController : MonoBehaviour
{
    // Variáveis públicas que podem ser ajustadas no Inspector da Unity
    public float velocidadeMovimento = 8.0f; // Controla a velocidade com que a nave se move pelo cenário.
    public GameObject prefabTiroJogador;     // Referência ao prefab do tiro que a nave irá disparar.
    public float tempoEntreTiros = 0.5f;      // Define o intervalo mínimo em segundos entre um tiro e outro.
    public int vidasIniciais = 3;             // Define o número de vidas com as quais o jogador começa a partida.

    // Limites de movimento da nave na tela
    public float limiteX = 6.5f; // A coordenada X máxima (positiva e negativa) que a nave pode alcançar lateralmente.
    public float limiteZInferior = 0f; // A coordenada Z mínima (mais próxima da câmera) que a nave pode alcançar.
    public float limiteZSuperior = 14.0f; // A coordenada Z máxima (mais distante da câmera) que a nave pode alcançar.

    // === Novas variáveis para Rotação Suave ===
    public float rollAmount = 20.0f; // Quantidade máxima de inclinação lateral (guinada) da nave em graus, ao mover para os lados.
    public float pitchAmount = 15.0f; // Quantidade máxima de inclinação para cima/baixo (arremesso) da nave em graus, ao mover para frente/trás.
    public float rotationSpeed = 5.0f; // Velocidade com que a nave retorna à sua rotação neutra ou atinge a rotação de inclinação desejada.

    private Quaternion targetRotation; // Armazena a rotação para a qual a nave está se movendo suavemente.

    // Animação de hit
    public float duracaoAnimacaoHit = 0.5f; // Duração em segundos da animação de rotação que ocorre quando a nave é atingida.
    public float velocidadeRotacaoHit = 720.0f; // Velocidade em graus por segundo da rotação da nave durante a animação de hit.

    private float proximoTiroTempo;           // Um contador para controlar quando a nave pode disparar o próximo tiro.
    private int vidasAtuais;                  // A contagem atual de vidas do jogador na partida.
    private bool estaEmAnimacaoHit;           // Uma flag booleana que indica se a nave está atualmente no meio da animação de hit.
    private float timerAnimacaoHit;           // Um timer que controla a duração restante da animação de hit.
    private Quaternion rotacaoOriginal;       // Armazena a rotação inicial da nave para que ela possa ser restaurada após a animação de hit ou desinclinação.

    // Evento que pode ser usado por um script de UI ou Game Manager para atualizar as vidas
    public static event System.Action<int> OnLivesChanged; // Evento estático que notifica outros scripts sobre mudanças na contagem de vidas do jogador.
    public static event System.Action OnPlayerDied; // Evento estático que notifica outros scripts quando o jogador perde a última vida.

    void Start()
    {
        vidasAtuais = vidasIniciais; // Define a contagem de vidas atuais para o valor inicial configurado.
        proximoTiroTempo = 0f; // Inicializa o contador para permitir que a nave atire imediatamente no início.
        estaEmAnimacaoHit = false; // Garante que a nave não está em animação de hit no começo.
        rotacaoOriginal = transform.rotation; // Armazena a rotação inicial do GameObject da nave.
        targetRotation = rotacaoOriginal; // Define a rotação alvo inicial como a rotação original da nave.
        OnLivesChanged?.Invoke(vidasAtuais); // Dispara o evento OnLivesChanged para notificar outros scripts (como a UI) sobre a contagem de vidas inicial.
    }

    void Update()
    {
        // Este método é chamado uma vez por frame para atualizar a lógica de jogo do PlayerShip.

        // === Lógica de Movimento ===
        // Garante que o teclado está disponível para leitura do input.
        if (Keyboard.current == null) return; // Se o teclado não está disponível, sai do método.

        // Leitura das entradas do teclado para os eixos X (horizontal) e Z (vertical/profundidade).
        // Retorna -1f (esquerda/trás), 1f (direita/frente) ou 0f (sem input).
        float xInput = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? -1f : // Input para mover para a esquerda.
                       Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f; // Input para mover para a direita.

        float zInput = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? -1f : // Input para mover para trás.
                       Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f; // Input para mover para frente.

        // Cria um vetor de movimento no plano XZ (Y é 0, pois a nave não se move verticalmente no gameplay).
        Vector3 movement = new Vector3(xInput, 0f, zInput);

        // Normaliza o vetor de movimento para que o movimento diagonal não seja mais rápido
        // do que o movimento horizontal ou vertical (mantém a velocidade constante em todas as direções).
        if (movement.magnitude > 1f)
        {
            movement.Normalize(); // Reduz a magnitude do vetor para 1, mantendo a direção.
        }

        // Calcula a nova posição desejada da nave, baseada no movimento, velocidade e no tempo decorrido desde o último frame.
        Vector3 newPosition = transform.position + movement * velocidadeMovimento * Time.deltaTime;

        // Aplica os limites de tela para manter a nave dentro da área de jogo definida.
        newPosition.x = Mathf.Clamp(newPosition.x, -limiteX, limiteX); // Limita a posição X da nave.
        newPosition.z = Mathf.Clamp(newPosition.z, limiteZInferior, limiteZSuperior); // Limita a posição Z da nave.

        // Aplica a nova posição calculada ao Transform do GameObject da nave.
        transform.position = newPosition;

        // === Lógica de Disparo ===
        // Verifica se a barra de espaço foi pressionada, se o tempo de cooldown já passou,
        // e se a nave não está atualmente em animação de hit.
        if (Keyboard.current.spaceKey.isPressed && Time.time >= proximoTiroTempo && !estaEmAnimacaoHit)
        {
            Atirar(); // Chama o método para disparar um tiro.
            proximoTiroTempo = Time.time + tempoEntreTiros; // Define o tempo para o próximo tiro possível.
        }

        // --- Lógica de Rotação Suave e Animação de Hit ---
        // Controla a orientação visual da nave, alternando entre animação de hit e rotação suave baseada no input.
        if (estaEmAnimacaoHit) // Se a nave está em animação de hit (foi atingida recentemente).
        {
            timerAnimacaoHit -= Time.deltaTime; // Decrementa o timer da animação de hit.
            transform.Rotate(Vector3.forward, velocidadeRotacaoHit * Time.deltaTime, Space.Self); // Rotaciona a nave rapidamente em torno de seu eixo Z local para o efeito de hit.

            if (timerAnimacaoHit <= 0f) // Se o timer da animação de hit terminou.
            {
                estaEmAnimacaoHit = false; // Desativa a flag da animação de hit.
                transform.rotation = rotacaoOriginal; // Reseta a rotação da nave para sua orientação original (neutra).
            }
        }
        else // Se não está em animação de hit, aplica a rotação suave baseada no input do jogador.
        {
            targetRotation = rotacaoOriginal; // Redefine a rotação alvo para a rotação original da nave (sem inclinação).

            // Aplica inclinação lateral (roll) baseada no input horizontal (xInput).
            // Multiplica por -xInput para inclinar a nave na direção visualmente correta.
            targetRotation *= Quaternion.Euler(0, 0, rollAmount * -xInput); 

            // Aplica inclinação para cima/baixo (pitch) baseada no input vertical (zInput).
            targetRotation *= Quaternion.Euler(pitchAmount * zInput, 0, 0); 

            // Interpola (suaviza a transição) a rotação atual da nave para a rotação alvo desejada.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void Atirar()
    {
        // Verifica se o prefab do tiro do jogador foi atribuído no Inspector da Unity.
        if (prefabTiroJogador != null) 
        {
            // Calcula a posição de spawn do tiro: na posição da nave mais um offset para a frente.
            // 'transform.forward' é o vetor que aponta para a frente da nave, e 1.5f é a distância do offset.
            // Este offset deve ser ajustado conforme o tamanho e o modelo 3D da sua nave.
            Vector3 spawnPosition = transform.position + transform.forward * 1.5f; 
            
            // Instancia o prefab do tiro na posição calculada e sem rotação inicial (Quaternion.identity).
            Instantiate(prefabTiroJogador, spawnPosition, Quaternion.identity); 
        }
        else // Se o prefab do tiro não foi atribuído no Inspector.
        {
            // Registra um aviso no console da Unity, alertando sobre a falta do prefab.
            Debug.LogWarning("Prefab do tiro do jogador não atribuído no Inspector!"); 
        }
    }

    // Método chamado automaticamente pela Unity quando um "Trigger Collider" deste objeto colide com outro collider.
    void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que colidiu tem a tag "EnemyBullet", "Alien" ou "AlienBoss".
        if (other.CompareTag("EnemyBullet") || other.CompareTag("Alien") || other.CompareTag("AlienBoss")) 
        {
            // Garante que a nave só leve dano se não estiver já na animação de "hit" (evita dano múltiplo por um único evento).
            if (!estaEmAnimacaoHit) 
            {
                TakeDamage(); // Chama o método para aplicar dano à nave do jogador.
                // Se o objeto que causou a colisão foi um projétil inimigo...
                if (other.CompareTag("EnemyBullet")) 
                {
                    Destroy(other.gameObject); // ...destrói o projétil inimigo após a colisão.
                }
            }
        }
    }

    void TakeDamage()
    {
        vidasAtuais--; // Decrementa a contagem de vidas atuais da nave.
        OnLivesChanged?.Invoke(vidasAtuais); // Dispara o evento OnLivesChanged para notificar (ex: a UI) sobre a mudança de vidas.

        if (vidasAtuais <= 0) // Verifica se as vidas da nave chegaram a zero.
        {
            // Lógica para Game Over quando a nave não tem mais vidas.
            Debug.Log("GAME OVER!"); // Loga uma mensagem de Game Over no console.
            OnPlayerDied?.Invoke(); // Dispara o evento OnPlayerDied para notificar (ex: o GameFlowManager) que o jogador morreu.
            gameObject.SetActive(false); // Desativa o GameObject da nave, tornando-a invisível e inoperante.
        }
        else // Se a nave ainda tem vidas restantes após o dano.
        {
            estaEmAnimacaoHit = true; // Ativa a flag para iniciar a animação de "hit".
            timerAnimacaoHit = duracaoAnimacaoHit; // Define o timer para a duração da animação de hit.
            // Reposiciona a nave para uma posição de "respawn" (similar ao respawnNave do seu código 2D original).
            transform.position = new Vector3(0, 0.5f, 0f); // Move a nave para a posição central (0, 0.5, 0).
            Debug.Log("Nave atingida! Vidas restantes: " + vidasAtuais); // Loga a contagem de vidas restantes.
        }
    }
}