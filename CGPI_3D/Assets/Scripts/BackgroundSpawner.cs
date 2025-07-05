using UnityEngine; // Importa o namespace fundamental da Unity, contendo classes como MonoBehaviour, GameObject, Transform, etc.

public class BackgroundSpawner : MonoBehaviour
{
    public GameObject prefabBackgroundObject; // Referência ao prefab do objeto de fundo (ex: asteroide, destroço) a ser spawnado.
    public int numeroDeObjetosIniciais = 80; // Define quantos objetos de fundo serão criados quando o jogo iniciar.

    // Limites de coordenadas para a área de spawn inicial dos objetos de fundo.
    public float minSpawnX = -10.0f; // Coordenada X mínima para o spawn.
    public float maxSpawnX = 10.0f;  // Coordenada X máxima para o spawn.
    public float minSpawnY = -5.0f;  // Coordenada Y mínima para o spawn (altura).
    public float maxSpawnY = -1.8f;  // Coordenada Y máxima para o spawn (altura).
    public float minSpawnZ = 15.0f;  // Coordenada Z mínima para o spawn (profundidade).
    public float maxSpawnZ = 50.0f;  // Coordenada Z máxima para o spawn (profundidade).

    void Start()
    {
        // Este método é chamado uma vez quando o script é ativado.
        SpawnInitialObjects(); // Chama o método para criar os objetos de fundo iniciais.
    }

    void SpawnInitialObjects()
    {
        // Verifica se o prefab do objeto de fundo foi atribuído no Inspector.
        if (prefabBackgroundObject == null)
        {
            Debug.LogError("Prefab do Background Object não atribuído no BackgroundSpawner!"); // Registra um erro se o prefab estiver faltando.
            return; // Sai do método para evitar erros.
        }

        // Loop para instanciar o número desejado de objetos iniciais.
        for (int i = 0; i < numeroDeObjetosIniciais; i++)
        {
            // Gera uma posição aleatória dentro dos limites definidos para cada objeto.
            Vector3 spawnPos = new Vector3(
                Random.Range(minSpawnX, maxSpawnX), // Posição X aleatória.
                Random.Range(minSpawnY, maxSpawnY), // Posição Y aleatória.
                Random.Range(minSpawnZ, maxSpawnZ)  // Posição Z aleatória.
            );
            // Instancia o prefab do objeto de fundo na posição aleatória calculada e sem rotação inicial.
            Instantiate(prefabBackgroundObject, spawnPos, Quaternion.identity); 
        }
    }
}