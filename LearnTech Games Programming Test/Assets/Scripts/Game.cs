namespace LearnTechGamesTest
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using SceneManager = UnityEngine.SceneManagement.SceneManager;
    using TMPro;

    public class Game : MonoBehaviour
    {
        /*[SerializeField] RuntimeAnimatorController firstAnim;
        [SerializeField] RuntimeAnimatorController secondAnim;
        [SerializeField] GameObject bgGameObject;*/
        [SerializeField] Animator bgAnimator;

        [Space()]
        [SerializeField] TMP_Text questionText;
        [SerializeField] TMP_Text[] answerTexts;

        public static int questionIndex = 1;

        bool isBusy = false; // temporary delay between card switches

        static readonly Color CORRECT_COLOR = Color.green;
        static readonly Color WRONG_COLOR = Color.red;
        static readonly Color DEFAULT_COLOR = Color.white;

        const string SLIDE_OUT_TRIGGER = "Slide Out Trigger";
        const string SLIDE_IN_TRIGGER = "Slide In Trigger";
        const string QUESTION_INDEX_SAVE_KEY = "QuestionIndex";
        const float TEXT_FADE_SPEED = 5.0f;

        void Start()
        {
            //questionIndex = 1;
            UpdateQuestion();
            SetupAnswerButtons();
        }

        void SetupAnswerButtons()
        {
            // set answer button's text displays to match their value
            for (int i = 0; i < answerTexts.Length; i++)
            {
                answerTexts[i].text = (i + 1).ToString();
            }
        }

        void UpdateQuestion()
        {
            questionText.text = questionIndex.ToString();
        }

        void Update()
        {
            if (isBusy)
                return;

            // fade question text back to white
            questionText.color = Color.Lerp(questionText.color, DEFAULT_COLOR, TEXT_FADE_SPEED * Time.deltaTime);
        }

        void NextQuestion()
        {
            //questionText.color = Color.yellow;
            questionIndex += 1;
            if (questionIndex >= 10)
                questionIndex = 1;
            UpdateQuestion();
            ResetAnswerTexts();
            SaveState();
        }

        void VerifyAnswer(int answerIndex)
        {
            if (answerIndex == questionIndex - 1)
            {
                Debug.Log("Correct!");
                answerTexts[answerIndex].color = CORRECT_COLOR;
                questionText.color = CORRECT_COLOR;
                // auto-advance with a correct answer
                AdvanceQuestion();
            }
            else
            {
                answerTexts[answerIndex].color = WRONG_COLOR;
                Debug.Log("Wrong!");
            }
        }

        public IEnumerator SlideInAnimation()
        {
            yield return new WaitForSeconds(0.25f);
            NextQuestion();
            yield return new WaitForSeconds(0.25f);
            //bgAnimator.SetTrigger(SLIDE_IN_TRIGGER);
            isBusy = false;
        }

        void ResetAnswerTexts()
        {
            for (int i = 0; i < answerTexts.Length; i++)
            {
                answerTexts[i].color = DEFAULT_COLOR;
            }
        }

        internal static void ResetState()
        {
            questionIndex = 1;
            SaveState();
        }

        internal static void LoadState()
        {
            questionIndex = PlayerPrefs.GetInt(QUESTION_INDEX_SAVE_KEY);
            questionIndex = Mathf.Clamp(questionIndex, 1, 10);
        }

        static void SaveState()
        {
            PlayerPrefs.SetInt(QUESTION_INDEX_SAVE_KEY, questionIndex);
        }

        #region UNITY_EVENT_TARGETS

        public void AdvanceQuestion()
        {
            if (isBusy)
                return;

            isBusy = true;
            bgAnimator.SetTrigger(SLIDE_OUT_TRIGGER);
            StartCoroutine(SlideInAnimation());
        }

        public void AnswerButton(Transform t)
        {
            if (isBusy)
                return;

            // order of children should directly correspond to the number they represent
            int answerIndex = t.GetSiblingIndex();

            Animator answerAnimator = t.GetComponent<Animator>();
            if (answerAnimator)
                answerAnimator.SetTrigger("Bounce Trigger");

            VerifyAnswer(answerIndex);
        }

        public void QuitGame()
        {
            SceneManager.LoadScene(0);
        }

        #endregion

#if UNITY_EDITOR

        // fast way to assign answers in editor

        [ContextMenu("Acquire Answer Texts")]
        public void GetAnswerTexts()
        {
            answerTexts = GameObject.Find("Answers").GetComponentsInChildren<TMP_Text>();
        }

#endif
    }

}