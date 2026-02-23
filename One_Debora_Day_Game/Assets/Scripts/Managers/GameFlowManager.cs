using UnityEngine;
using UnityEngine.SceneManagement;
public class GameFlowManager : MonoBehaviour
{
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
static void AutoBoot()
{
if (SceneManager.GetActiveScene().name == "Boot")
SceneManager.LoadScene("MainMenu");
}
public void LoadScene(string name) => SceneManager.LoadScene(name);
public void QuitGame() => Application.Quit();
}
