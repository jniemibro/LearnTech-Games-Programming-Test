namespace LearnTechGamesTest
{
    using UnityEngine;
    using UnityEngine.UI;
    using SceneManager = UnityEngine.SceneManagement.SceneManager;

    public class TitleScene : MonoBehaviour
    {
        [SerializeField] Button loadButton;
        [SerializeField] Button quitButton;

        const int GAMEPLAY_SCENE_INDEX = Constants.GAMEPLAY_SCENE_INDEX;

        void Awake()
        {
            // disable load button if there's no save
            if (!PlayerPrefs.HasKey(Constants.QUESTION_INDEX_SAVE_KEY))
                loadButton.interactable = false;

            // deactivate quit button for certain platforms
            if (!ShouldDisplayQuitButton())
                quitButton.gameObject.SetActive(false);
        }

        public void NewGame()
        {
            Game.ResetSaveState();
            SceneManager.LoadScene(GAMEPLAY_SCENE_INDEX);
        }

        public void LoadGame()
        {
            Game.LoadSaveState();
            SceneManager.LoadScene(GAMEPLAY_SCENE_INDEX);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
        }

        bool ShouldDisplayQuitButton()
        {
            return !Application.isMobilePlatform;
        }
    }
}