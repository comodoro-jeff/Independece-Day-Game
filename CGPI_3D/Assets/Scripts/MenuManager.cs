using UnityEngine; // Importa o namespace fundamental da Unity, contendo classes como MonoBehaviour, GameObject, etc.
using UnityEngine.SceneManagement; // Importa o namespace para gerenciamento de cenas, permitindo carregar cenas.
using UnityEngine.UI; // Importa o namespace para componentes de UI, como Button (embora não usado diretamente no script, é comum para botões).

public class MenuManager : MonoBehaviour
{
    public void LoadGameplayScene()
    {
        Debug.Log("Carregando cena de jogo..."); // Loga uma mensagem no console.
        // Carrega a cena com o nome "Gameplay".
        // Certifique-se de que a cena "Gameplay" está adicionada em File > Build Settings.
        SceneManager.LoadScene("Gameplay"); 
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo..."); // Loga uma mensagem no console.
        Application.Quit(); // Fecha a aplicação (funciona apenas em builds, não no editor da Unity).

        // Diretiva de pré-processador: o código abaixo só será compilado e executado no editor da Unity.
        #if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false; // Para o modo de Play no editor da Unity.
        #endif
    }
}