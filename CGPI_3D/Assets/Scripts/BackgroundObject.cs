using UnityEngine; // Importa o namespace fundamental da Unity, contendo classes como MonoBehaviour, GameObject, Transform, etc.

public class BackgroundObject : MonoBehaviour
{
    // === Configurações de Movimento ===
    // Variáveis públicas que controlam o movimento e os limites de reciclagem do objeto de fundo.
    public float velocidadeMovimento = 2.0f; // Velocidade com que o objeto se move em direção ao jogador (no eixo Z).
    public float limiteZInferior = -15.0f;   // Posição Z onde o objeto é "reciclado" (sai da tela, mais próximo da câmera).
    public float limiteZSuperior = 20.0f;    // Posição Z onde o objeto reaparece (longe da câmera).
    public float limiteX = 20.0f;            // Limite lateral (X) para o respawn do objeto.
    
    // Essas variáveis controlam a altura dos asteroides, mantendo-os em um plano inferior ao da ação principal.
    public float minLimiteY = -3.0f; // Menor altura Y para um asteroide (mais fundo).
    public float maxLimiteY = -1.0f; // Maior altura Y para um asteroide (mais perto do "chão" do jogo).

    // === Configurações de Rotação ===
    // Variáveis públicas que controlam a velocidade de rotação aleatória do objeto.
    public float minVelocidadeRotacao = 10.0f;  // Velocidade mínima de rotação em graus por segundo para cada eixo.
    public float maxVelocidadeRotacao = 50.0f;  // Velocidade máxima de rotação em graus por segundo para cada eixo.

    // Variáveis privadas para o estado de movimento e rotação do objeto.
    private Vector3 velocidadeRotacaoAleatoria; // Armazena a velocidade de rotação gerada aleatoriamente para cada um dos três eixos (X, Y, Z).
    private Vector3 direcaoMovimento; // Define a direção de translação do objeto no espaço (neste caso, para trás no eixo Z do mundo).

    void Start()
    {
        // Gera uma velocidade de rotação aleatória para cada eixo (X, Y, Z) dentro do intervalo especificado.
        velocidadeRotacaoAleatoria = new Vector3(
            Random.Range(minVelocidadeRotacao, maxVelocidadeRotacao),
            Random.Range(minVelocidadeRotacao, maxVelocidadeRotacao),
            Random.Range(minVelocidadeRotacao, maxVelocidadeRotacao)
        );

        // Define a direção de movimento do objeto. Vector3.back representa o eixo -Z no World Space.
        direcaoMovimento = Vector3.back; 
    }

    void Update()
    {
        // === Rotação Aleatória ===
        // Aplica a rotação gerada aleatoriamente a cada eixo do objeto, fazendo-o girar em seu próprio espaço.
        transform.Rotate(velocidadeRotacaoAleatoria * Time.deltaTime, Space.Self);

        // === Movimento de Translação (do fundo para a frente) ===
        // Move o objeto na direção definida (para trás no eixo Z do mundo) com a velocidade especificada,
        // criando o efeito de que está vindo do fundo da cena em direção à câmera.
        transform.Translate(direcaoMovimento * velocidadeMovimento * Time.deltaTime, Space.World);

        // === Reciclagem do Objeto (se sair da tela) ===
        // Verifica se o objeto passou da posição Z limite inferior.
        if (transform.position.z < limiteZInferior)
        {
            RespawnObject(); // Se sim, chama o método para reposicionar o objeto no início do ciclo de movimento.
        }
    }

    void RespawnObject()
    {
        // Define a nova posição do objeto: X e Y aleatórios dentro dos limites, e Z no limite superior (distante).
        transform.position = new Vector3(
            Random.Range(-limiteX, limiteX), // Posição X aleatória dentro do limite lateral.
            Random.Range(minLimiteY, maxLimiteY), // Posição Y aleatória dentro do novo limite de altura (abaixo das naves).
            limiteZSuperior // Posição Z no limite superior, reaparecendo longe da câmera.
        );

        // Opcional: Gera uma nova velocidade de rotação para adicionar mais variedade ao comportamento do objeto reciclado.
        velocidadeRotacaoAleatoria = new Vector3(
            Random.Range(minVelocidadeRotacao, maxVelocidadeRotacao),
            Random.Range(minVelocidadeRotacao, maxVelocidadeRotacao),
            Random.Range(minVelocidadeRotacao, maxVelocidadeRotacao)
        );

        // Gera uma nova escala aleatória para variar o tamanho do objeto reciclado.
        float randomScale = Random.Range(0.1f, 1.0f); // Define um valor de escala aleatório entre 0.1 e 1.0.
        transform.localScale = new Vector3(randomScale, randomScale, randomScale); // Aplica a nova escala uniformemente nos três eixos.
    }
}