using UnityEngine; // Fornece funcionalidades básicas da Unity, como classes para componentes de jogo e manipulação de objetos.
using System.Collections.Generic; // Permite o uso de coleções como List<T> para gerenciar grupos de objetos.
using UnityEngine.SceneManagement; // Oferece ferramentas para carregar e gerenciar cenas do jogo.
using TMPro; // Essencial para utilizar componentes de texto avançados (TextMeshProUGUI) na interface do usuário.
using UnityEngine.InputSystem; // Habilita a detecção de inputs do teclado e outros dispositivos de entrada de forma moderna.
using UnityEngine.UI; // Contém classes para elementos da interface do usuário, como Sliders e Buttons.


public class GameFlowManager : MonoBehaviour
{
    // === Referências para Prefabs ===
    public GameObject prefabAlien; // Prefab do alien comum.
    public GameObject prefabAlienBoss; // Prefab do chefe alienígena.

    // === Configurações de Gameplay ===
    public int totalVidasIniciais = 3; // Vidas do jogador ao iniciar.
    public float velocidadeAlienInicial = 1.0f; // Velocidade lateral da formação alien.
    public float distanciaDescidaAlien = 0.5f; // Distância que aliens descem ao virar.
    public float espacamentoAlienX = 1.5f; // Espaçamento horizontal entre aliens.
    public float espacamentoAlienZ = 1.5f; // Espaçamento em profundidade entre linhas alien.
    public Vector3 posicaoInicialGrid = new Vector3(-6.5f, 0.5f, 14.0f); // Posição de spawn da grade alien.
    public int linhasAliens = 2; // Número de linhas de aliens na formação.
    public int colunasAliens = 2; // Número de colunas de aliens na formação.
    public float limiteXAliens = 6.5f; // Limite X para a formação alien virar.

    // === Variáveis de Estado do Jogo ===
    private int nivelAtual; // Nível de jogo atual.
    private float direcaoAlien = 1.0f; // Direção lateral da formação alien (1.0 = direita, -1.0 = esquerda).
    private List<GameObject> aliensNaCena; // Lista de aliens ativos na cena.
    private GameObject alienBossInstance; // Instância do chefe quando ativo.

    // === Referências para Elementos da UI (arrastar no Inspector!) ===
    public TMPro.TextMeshProUGUI livesText; // Texto que exibe as vidas do jogador.
    public TMPro.TextMeshProUGUI levelText; // Texto que exibe o nível atual.
    public Slider bossHealthSlider; // Barra de vida do chefe.
    
    // === Eventos para UI ===
    public static event System.Action<string> OnGameOver; // Disparado ao fim do jogo (derrota).
    public static event System.Action<string> OnGameVictory; // Disparado ao fim do jogo (vitória).
    // Elementos específicos das telas de Game Over e Vitória.
    public GameObject gameOverScreen; // Objeto da tela de Game Over.
    public TMPro.TextMeshProUGUI gameOverTitleText; // Título da tela de Game Over.
    public Button restartButton; // Botão de reiniciar na tela de Game Over.
    public GameObject victoryScreen; // Objeto da tela de Vitória.
    public TMPro.TextMeshProUGUI victoryTitleText; // Título da tela de Vitória.
    public Button restartButtonVictory; // Botão de reiniciar na tela de Vitória.


    void Awake()
    {
        // Garante que só haja uma instância do GameFlowManager na cena.
        // Isso impede que múltiplas cópias do gerenciador de jogo existam.
        if (FindObjectsByType<GameFlowManager>(FindObjectsSortMode.None).Length > 1) // Verifica se já existe outra instância do GameFlowManager.
        {
            Destroy(gameObject); // Se sim, destrói este novo GameObject para evitar duplicidade.
        }

        aliensNaCena = new List<GameObject>(); // Inicializa a lista que rastreará os aliens ativos na cena.
    }

    void Start()    
    {
        InitializeGame(); // Chama o método para configurar o jogo no seu estado inicial.

        // Garante que os elementos de UI do jogo em tempo real (HUD) estejam ativos no início da partida.
        if (livesText != null) livesText.gameObject.SetActive(true); // Ativa o texto que exibe as vidas.
        if (levelText != null) levelText.gameObject.SetActive(true); // Ativa o texto que exibe o nível.

        // Garante que a barra de vida do boss e as telas de fim de jogo comecem desativadas.
        if (bossHealthSlider != null) bossHealthSlider.gameObject.SetActive(false); // Desativa a barra de vida do boss.
        if (gameOverScreen != null) gameOverScreen.gameObject.SetActive(false); // Desativa a tela de Game Over.
        if (victoryScreen != null) victoryScreen.gameObject.SetActive(false); // Desativa a tela de Vitória.
    }

    void OnEnable()
    {
        // Assina o evento de morte do jogador. Quando o PlayerShipController dispara OnPlayerDied, HandlePlayerDied será chamado.
        PlayerShipController.OnPlayerDied += HandlePlayerDied; 

        // Assina o evento de mudança de vidas do jogador. UpdateLivesUI será chamado quando as vidas mudarem.
        PlayerShipController.OnLivesChanged += UpdateLivesUI; 

        // Assina o evento de mudança de vida do chefe. UpdateBossHealthUI será chamado quando a vida do boss for alterada.
        AlienBossController.OnBossHealthChanged += UpdateBossHealthUI; 
    }

    void OnDisable()
    {
        // Desassina o evento de morte do jogador.
        PlayerShipController.OnPlayerDied -= HandlePlayerDied; 

        // Desassina o evento de mudança de vidas do jogador.
        PlayerShipController.OnLivesChanged -= UpdateLivesUI; 

        // Desassina o evento de mudança de vida do chefe.
        AlienBossController.OnBossHealthChanged -= UpdateBossHealthUI; 
    }

    void Update()
    {
        // Verifica se o jogo está em um estado pausado (Game Over ou Vitória).
        if (Time.timeScale == 0f) // Se a escala de tempo está em zero, o jogo está pausado.
        {
            // Verifica se o teclado está disponível e se a barra de espaço foi pressionada neste frame.
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) 
            {
                RestartGame(); // Chama o método para reiniciar o jogo.
            }
            return; // Sai do Update para evitar que a lógica de jogo (movimento, spawn) continue sendo processada enquanto pausado.
        }

        // Lógica de movimento da formação de aliens.
        AtualizarMovimentoAliens();

        // Checa as condições para a progressão do nível ou para a vitória do jogo.
        ChecarEstadoNivel();
    }

    void InitializeGame()
    {
        // Este método configura o jogo para o seu estado inicial no começo de uma nova partida.
        Debug.Log("Iniciando Jogo..."); // Mensagem de log para indicar o início da inicialização.
        nivelAtual = 1; // Define o nível inicial do jogo como 1.
        direcaoAlien = 1.0f; // Define a direção inicial da formação alienígena (geralmente para a direita).
        
        LimparCenaInimigos(); // Garante que todos os inimigos da partida anterior sejam destruídos.

        // Reativa a nave do jogador e reseta seu estado ao iniciar/reiniciar o jogo.
        GameObject playerShip = GameObject.FindWithTag("Player"); // Encontra a nave do jogador na cena usando sua tag "Player".
        if (playerShip != null) // Se a nave do jogador for encontrada...
        {
            playerShip.SetActive(true); // ...reativa o GameObject da nave para que ela esteja visível.
            PlayerShipController psc = playerShip.GetComponent<PlayerShipController>(); // Obtém o componente PlayerShipController da nave.
            if (psc != null) // Se o script da nave for encontrado...
            {
                // Reseta as vidas e o estado interno da nave para o início da partida.
                psc.vidasIniciais = totalVidasIniciais; // Garante que as vidas da nave sejam resetadas para o valor inicial configurado.
                psc.SendMessage("Start"); // Re-chama o método Start() do PlayerShipController para resetar suas variáveis internas (posição, cooldowns, etc.).
            }
        } else { // Se a nave do jogador não for encontrada...
             Debug.LogError("Erro: PlayerShip não encontrada na cena com a tag 'Player'!"); // ...registra um erro no console.
        }

        SpawnAliensGrid(); // Spawna a formação inicial de aliens do Nível 1.

        // Atualiza o texto da UI que exibe o nível.
        if (levelText != null) 
        {
            levelText.text = "Nível: " + nivelAtual; 
        }

        // Esconde a barra de vida do boss no início da partida.
        if (bossHealthSlider != null) 
        {
            bossHealthSlider.gameObject.SetActive(false); 
        }
    }

    void LimparCenaInimigos()
    {
        // Itera sobre cada GameObject na lista 'aliensNaCena'.
        foreach (var alien in aliensNaCena) 
        {
            if (alien != null) Destroy(alien); // Se o alien ainda existe (não foi destruído por outra colisão, por exemplo), destrói o GameObject.
        }
        aliensNaCena.Clear(); // Limpa a lista 'aliensNaCena', removendo todas as referências após os objetos serem destruídos.

        // Verifica se há uma instância do chefe (boss) ativa na cena.
        if (alienBossInstance != null) 
        {
            Destroy(alienBossInstance); // Se sim, destrói o GameObject do chefe.
            alienBossInstance = null; // Define a referência do chefe como nula para indicar que ele não existe mais.
        }
    }

    void SpawnAliensGrid()
    {

        aliensNaCena.Clear(); // Limpa a lista de aliens atualmente ativos antes de spawnar novos.

        // Loop para iterar através das linhas da grade de aliens.
        for (int linha = 0; linha < linhasAliens; linha++) 
        {
            // Loop para iterar através das colunas da grade de aliens.
            for (int coluna = 0; coluna < colunasAliens; coluna++) 
            {
                // Calcula a posição X do alien dentro da grade, considerando o espaçamento.
                float xPos = posicaoInicialGrid.x + coluna * espacamentoAlienX;
                // Calcula a posição Z (profundidade) do alien, diminuindo para cada nova linha (descendo).
                float zPos = posicaoInicialGrid.z - linha * espacamentoAlienZ; 
                
                // Cria um vetor de posição para o novo alien. A posição Y é mantida fixa.
                Vector3 spawnPos = new Vector3(xPos, posicaoInicialGrid.y, zPos); 
                
                // Instancia (cria) um novo GameObject de alien a partir do prefab, na posição calculada e sem rotação inicial.
                GameObject novoAlien = Instantiate(prefabAlien, spawnPos, Quaternion.identity); 
                
                // Adiciona o novo alien à lista de aliens ativos para rastreamento.
                aliensNaCena.Add(novoAlien); 
            }
        }
    }

    void AtualizarMovimentoAliens()
    {
        // Retorna imediatamente se não houver aliens ou o chefe na cena, otimizando o processamento.
        if (aliensNaCena.Count == 0 && alienBossInstance == null) return;

        bool mudarDirecao = false; // Flag para indicar se a formação deve mudar de direção e descer.

        // Itera sobre cada alienígena ativo na cena.
        foreach (var alienGO in aliensNaCena)
        {
            if (alienGO == null) continue; // Pula aliens que podem ter sido destruídos em um frame anterior.

            // Move o alien lateralmente (no eixo X do mundo) usando a velocidade inicial definida.
            alienGO.transform.Translate(Vector3.right * direcaoAlien * velocidadeAlienInicial * Time.deltaTime, Space.World);

            // Verifica se o alien atingiu os limites laterais configurados (limiteXAliens).
            if (alienGO.transform.position.x > limiteXAliens || alienGO.transform.position.x < -limiteXAliens)
            {
                mudarDirecao = true; // Se atingiu, a formação inteira deve mudar de direção.
            }
            
            // Verifica se o alien invadiu a área de jogo do jogador (passou da linha de Game Over).
            // O valor -8.0f é uma estimativa da posição Z da nave do jogador, ajuste conforme a cena.
            if (alienGO.transform.position.z < -8.0f) 
            {
                TriggerGameOver("Os aliens invadiram!"); // Dispara a condição de Game Over.
                return; // Encerra o método para evitar que mais lógica de movimento seja processada após o Game Over.
            }
        }

        // Se a flag 'mudarDirecao' for verdadeira (pelo menos um alien atingiu o limite)...
        if (mudarDirecao)
        {
            direcaoAlien *= -1.0f; // ...inverte a direção lateral da formação (de direita para esquerda ou vice-versa).
            // Em seguida, faz com que todos os aliens desçam uma certa distância no eixo Z (em direção ao jogador).
            foreach (var alienGO in aliensNaCena)
            {
                if (alienGO != null)
                {
                    alienGO.transform.Translate(Vector3.back * distanciaDescidaAlien, Space.World); // Move para baixo.
                }
            }
        }
    }

    void ChecarEstadoNivel()
    {
        // Remove da lista quaisquer GameObjects de alien que foram destruídos (agora são nulos).
        aliensNaCena.RemoveAll(alien => alien == null);

        // Verifica se todos os aliens normais do nível atual foram destruídos.
        if (aliensNaCena.Count == 0)
        {
            // Lógica para a transição do Nível 1 para o Nível 2 (Boss).
            if (nivelAtual == 1)
            {
                Debug.Log("Todos os aliens do Nível 1 destruídos. Iniciando Nível 2 (Boss)."); // Mensagem para o console.
                nivelAtual = 2; // Avança o jogo para o Nível 2.
                if (levelText != null) // Atualiza o texto da UI do nível.
                {
                    levelText.text = "Nível: " + nivelAtual;
                }
                SpawnAlienBoss(); // Chama o método para spawnar o chefe.
            }
            // Lógica para disparar a vitória final após a derrota do chefe no Nível 2.
            else if (nivelAtual == 2 && alienBossInstance == null)
            {
                TriggerGameVictory(); // Chama o método para ativar a tela de vitória.
            }
        }
    }

    void SpawnAlienBoss()
    {
        // Verifica se o chefe ainda não está presente na cena (alienBossInstance é nulo)
        // e se o prefab do chefe foi corretamente atribuído no Inspector.
        if (alienBossInstance == null && prefabAlienBoss != null)
        {
            // Define a posição onde o chefe será spawnado. O Y=0.5f mantém ele na mesma altura da sua nave.
            Vector3 bossSpawnPos = new Vector3(0.0f, 0.5f, 12.0f); 
            // Instancia o prefab do chefe na posição definida e sem rotação inicial.
            alienBossInstance = Instantiate(prefabAlienBoss, bossSpawnPos, Quaternion.identity); 
            Debug.Log("Alien Boss Spawnado!"); // Loga uma mensagem para o console.

            // ATIVA A BARRA DE VIDA DO BOSS NA UI
            // Verifica se a referência ao slider da barra de vida do boss está atribuída.
            if (bossHealthSlider != null) 
            {
                bossHealthSlider.gameObject.SetActive(true); // Ativa o GameObject do slider para torná-lo visível.
            }
        }
        // Caso o prefab do chefe não tenha sido atribuído no Inspector, um erro é logado.
        else if (prefabAlienBoss == null) 
        {
             Debug.LogError("Prefab do Alien Boss não atribuído no GameFlowManager!"); 
        }
    }

    void HandlePlayerDied()
    {
        // O GameFlowManager não é responsável por gerenciar a contagem de vidas da nave diretamente.
        // Ele apenas reage ao evento de que a nave do jogador foi destruída (perdeu a última vida).
        // Chama o método TriggerGameOver para iniciar a sequência de fim de jogo, passando a mensagem de derrota.
        TriggerGameOver("Sua nave foi destruída!"); 
    }

    void UpdateLivesUI(int currentLives)
    {
        // Verifica se a referência ao componente de texto de vidas na UI está atribuída no Inspector.
        if (livesText != null) 
        {
            // Atualiza o texto na UI para exibir a contagem de vidas atual.
            livesText.text = "Vidas: " + currentLives; 
        }
    }

    void UpdateBossHealthUI(int currentHealth, int maxHealth)
    {
        // Verifica se a referência ao componente Slider da barra de vida do boss na UI está atribuída.
        if (bossHealthSlider != null) 
        {
            bossHealthSlider.maxValue = maxHealth; // Define o valor máximo do slider para a vida máxima do chefe.
            bossHealthSlider.value = currentHealth; // Define o valor atual do slider para a vida atual do chefe.
        }
    }

    public void RestartGame()
    {
        Debug.Log("Reiniciando Jogo..."); // Mensagem de log para indicar o reinício.
        Time.timeScale = 1f; // Restaura a escala de tempo para 1, normalizando a velocidade do jogo.

        // Desativa os componentes da tela de Game Over.
        if (gameOverScreen != null) gameOverScreen.SetActive(false); 
        if (gameOverTitleText != null) gameOverTitleText.gameObject.SetActive(false); 
        if (restartButton != null) restartButton.gameObject.SetActive(false); 

        // Desativa os componentes da tela de Vitória.
        if (victoryScreen != null) victoryScreen.SetActive(false); 
        if (victoryTitleText != null) victoryTitleText.gameObject.SetActive(false); 
        if (restartButtonVictory != null) restartButtonVictory.gameObject.SetActive(false); 

        // Recarrega a cena atual para reiniciar completamente o jogo.
        // Isso redefine todos os GameObjects e scripts para seus estados iniciais.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    void TriggerGameOver(string message)
    {
        Debug.Log("TriggerGameOver foi chamado com a mensagem: " + message); // Loga a mensagem de Game Over para o console.

        // Garante que o jogo não está pausado antes de prosseguir com a lógica de Game Over.
        if (Time.timeScale != 0f) 
        {
            Time.timeScale = 0f; // Pausa o jogo, definindo a escala de tempo para zero.

            // Desativa os elementos da UI de jogo em tempo real (HUD) para que a tela de Game Over seja o foco.
            if (livesText != null) livesText.gameObject.SetActive(false);     // Desativa o texto de vidas.
            if (levelText != null) levelText.gameObject.SetActive(false);     // Desativa o texto de nível.
            if (bossHealthSlider != null) bossHealthSlider.gameObject.SetActive(false); // Desativa a barra de vida do chefe.

            // Verifica se a referência ao GameObject da tela de Game Over está atribuída.
            if (gameOverScreen != null) 
            {
                gameOverScreen.SetActive(true); // Ativa o painel principal da tela de Game Over, tornando-o visível.

                // Configura o texto principal da tela de Game Over.
                // Verifica se a referência ao TextMeshProUGUI do título de Game Over está atribuída.
                if (gameOverTitleText != null) 
                {
                    gameOverTitleText.gameObject.SetActive(true); // Garante que o GameObject do título esteja ativo.
                    gameOverTitleText.text = "GAME OVER\n" + message; // Define o texto, combinando "GAME OVER" com a mensagem recebida.
                }
                
                // Ativa o botão de reinício na tela de Game Over.
                // Verifica se a referência ao botão de reinício está atribuída.
                if (restartButton != null) 
                {
                    restartButton.gameObject.SetActive(true); 
                }
            }

            // Dispara o evento 'OnGameOver', notificando outros scripts que podem estar ouvindo.
            OnGameOver?.Invoke(message); 

            // Encontra a nave do jogador na cena usando sua tag "Player".
            GameObject playerShip = GameObject.FindWithTag("Player"); 
            if (playerShip != null) // Se a nave for encontrada...
            {
                playerShip.SetActive(false); // ...desativa o GameObject da nave para que ela não seja mais visível.
            }
        }
    }

    void TriggerGameVictory()
    {
       // Este método é responsável por ativar a tela de Vitória e pausar o jogo.

       Debug.Log("TriggerGameVictory foi chamado!"); // Loga uma mensagem para o console indicando que o método foi chamado.

       // Garante que o jogo não está pausado antes de prosseguir com a lógica de Vitória.
       if (Time.timeScale != 0f) 
        {
           Time.timeScale = 0f; // Pausa o jogo, definindo a escala de tempo para zero.

           // Desativa os elementos da UI de jogo em tempo real (HUD) e as telas de Game Over, para que a tela de Vitória seja o foco.
           if (livesText != null) livesText.gameObject.SetActive(false);     // Desativa o texto de vidas.
           if (levelText != null) levelText.gameObject.SetActive(false);     // Desativa o texto de nível.
           if (bossHealthSlider != null) bossHealthSlider.gameObject.SetActive(false); // Desativa a barra de vida do chefe.
           if (gameOverScreen != null) gameOverScreen.gameObject.SetActive(false); // Garante que a tela de Game Over está desativada.
           if (gameOverTitleText != null) gameOverTitleText.gameObject.SetActive(false); // Garante que o título do Game Over está desativado.
           if (restartButton != null) restartButton.gameObject.SetActive(false); // Garante que o botão de Game Over está desativado.

           // Verifica se a referência ao GameObject da tela de Vitória está atribuída.
           if (victoryScreen != null) 
            {
               victoryScreen.SetActive(true); // Ativa o painel principal da tela de Vitória, tornando-o visível.

               // Configura o texto de título da tela de Vitória.
               // Verifica se a referência ao TextMeshProUGUI do título de Vitória está atribuída.
               if (victoryTitleText != null) 
                {
                   victoryTitleText.gameObject.SetActive(true); // Garante que o GameObject do título esteja ativo.
                   victoryTitleText.text = "VITÓRIA!"; // Define o texto do título da Vitória.
                }
               // Ativa o botão de reinício na tela de Vitória.
               // Verifica se a referência ao botão de reinício da tela de Vitória está atribuída.
               if (restartButtonVictory != null) 
                {
                   restartButtonVictory.gameObject.SetActive(true); 
                }
            }

           // Dispara o evento 'OnGameVictory', notificando outros scripts que podem estar ouvindo, e passa uma mensagem.
           OnGameVictory?.Invoke("Parabéns, você venceu!"); 
        }
    }
}