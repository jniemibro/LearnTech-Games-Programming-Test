namespace LearnTechGamesTest
{
    using UnityEngine;
    using SceneManager = UnityEngine.SceneManagement.SceneManager;

    public class TitleScreen : MonoBehaviour
    {
        public void NewGame()
        {
            Game.ResetState();
            SceneManager.LoadScene(1);
        }

        public void LoadGame()
        {
            Game.LoadState();
            SceneManager.LoadScene(1);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
        }
    }
}