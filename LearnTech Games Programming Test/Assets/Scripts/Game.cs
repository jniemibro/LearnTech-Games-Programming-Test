namespace LearnTechGamesTest
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using SceneManager = UnityEngine.SceneManagement.SceneManager;
    using TMPro;

    [DisallowMultipleComponent]
    public sealed class Game : MonoBehaviour
    {
        enum GameState
        {
            Correct,
            SlideOut,
            SlideIn,
            Play
        }

        #region FIELDS

        public static int questionNumber = 1;
        int lastAnswer = 0;

        GameState state = GameState.SlideIn;

        [SerializeField] Image bgImage; 
        [SerializeField] Animator bgAnimator;

        [Space()]
        [SerializeField] TMP_Text questionText;
        [SerializeField] TMP_Text[] answerTexts;

        Color defaultBgImageColor;
        float delayTimer = SLIDE_DURATION;
        int counter = -1;

        static readonly Color CORRECT_COLOR = Color.green;
        static readonly Color WRONG_COLOR = Color.red;
        static readonly Color DEFAULT_COLOR = Color.white;

        const string QUESTION_INDEX_SAVE_KEY = Constants.QUESTION_INDEX_SAVE_KEY;
        const string SLIDE_OUT_TRIGGER = "Slide Out Trigger";
        const string SLIDE_IN_TRIGGER = "Slide In Trigger";
        const string BOUNCE_TRIGGER = "Bounce Trigger";
        const float TEXT_FADE_SPEED = Constants.TEXT_FADE_SPEED;
        const float SLIDE_DURATION = 0.664f; // animation lengths

        #endregion

        void Start()
        {
            defaultBgImageColor = bgImage.color;

            UpdateQuestionText();
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

        void Update()
        {
            // state machine
            switch (state)
            {
                case GameState.SlideIn:
                    if (delayTimer <= 0)
                        ChangeGameState(GameState.Play);
                    break;

                case GameState.SlideOut:
                    if (delayTimer <= 0)
                        ChangeGameState(GameState.SlideIn);
                    break;

                case GameState.Correct:
                    if (delayTimer <= 0)
                    {
                        counter++;
                        if (counter > lastAnswer)
                        {
                            AdvanceQuestion_Discrete();
                            return;
                        }
                        if (counter >= 0)
                        {
                            if (counter == lastAnswer)
                                BounceQuestionText();
                            BounceAnswerButton(counter);
                            SFX.PlaySound(GetIndexTone(counter));
                        }
                        delayTimer = 0.5f;

                    }
                    break;
            }

            // fade question text back to white
            questionText.color = Color.Lerp(questionText.color, DEFAULT_COLOR, TEXT_FADE_SPEED * Time.smoothDeltaTime);
            bgImage.color = Color.Lerp(bgImage.color, defaultBgImageColor, TEXT_FADE_SPEED * Time.smoothDeltaTime);
            if (delayTimer > 0)
                delayTimer -= Time.deltaTime;
        }

#if UNITY_EDITOR

        void OnGUI()
        {
            GUILayout.Label("State = " + state.ToString());
        }

#endif

        void UpdateQuestionText()
        {
            questionText.text = questionNumber.ToString();
        }

        void NextQuestion()
        {
            //questionText.color = Color.yellow;
            questionNumber += 1;
            if (questionNumber >= 10)
                questionNumber = 1;
        }

        void FinalizeQuestionSwap()
        {
            UpdateQuestionText();
            ResetAnswerTexts();
            SaveSaveState();
        }

        void VerifyAnswer(int answerIndex)
        {
            if (IsBusy())
                return;

            lastAnswer = answerIndex;
            if (answerIndex == (questionNumber - 1))
            {
                //SFX.PlaySound(4.0f);
                //Debug.Log("Correct!");
                BounceQuestionText();
                bgImage.color = CORRECT_COLOR;
                answerTexts[answerIndex].color = CORRECT_COLOR;
                questionText.color = CORRECT_COLOR;
                ChangeGameState(GameState.Correct);
                // auto-advance with a correct answer
                //AdvanceQuestion();
            }
            else
            {
                questionText.color = WRONG_COLOR;
                bgImage.color = WRONG_COLOR;
                SFX.PlaySound(GetIndexTone(lastAnswer));
                //Debug.Log("Wrong!");
                answerTexts[answerIndex].color = WRONG_COLOR;
            }
        }

        void ResetAnswerTexts()
        {
            for (int i = 0; i < answerTexts.Length; i++)
            {
                answerTexts[i].color = DEFAULT_COLOR;
                answerTexts[i].gameObject.SetActive(true);
            }
        }

        void SetLastVisibleAnswerText(int k)
        {
            for (int i = 0; i < answerTexts.Length; i++)
            {
                if (i < k && i != k)
                    answerTexts[i].color = Color.yellow;
                else
                    answerTexts[i].gameObject.SetActive(i == k);
            }
        }

        void ChangeGameState(GameState nextState)
        {
            // on exit
            switch (state)
            {
                /*case GameState.SlideIn:
                    // reset slide in animation
                    if (nextState == GameState.SlideIn)
                    {
                        delayTimer = SLIDE_DURATION / 2f;
                        bgAnimator.SetTrigger(SLIDE_IN_TRIGGER);
                    }
                    break;*/

                case GameState.SlideOut:
                    FinalizeQuestionSwap();
                    break;
            }

            // prevent entering the same state again, resetting current animation
            if (state == nextState)
                return;

            // assign next state
            state = nextState;

            // on enter
            switch (state)
            {
                case GameState.Play:
                    BounceQuestionText();
                    SFX.PlaySound( GetIndexTone(questionNumber-1) );
                    break;

                case GameState.SlideIn:
                    delayTimer = SLIDE_DURATION / 2f;
                    bgAnimator.SetTrigger(SLIDE_IN_TRIGGER);
                    break;

                case GameState.SlideOut:
                    delayTimer = SLIDE_DURATION / 2f;
                    bgAnimator.SetTrigger(SLIDE_OUT_TRIGGER);
                    break;

                case GameState.Correct:
                    SFX.PlaySound(GetIndexTone(lastAnswer));
                    SetLastVisibleAnswerText(lastAnswer);
                    counter = -2; // give clicked button a chance to play with an extra negative iteration
                    break;
            }
        }

        #region SAVE_LOAD

        internal static void ResetSaveState()
        {
            questionNumber = 1;
            SaveSaveState();
        }

        internal static void LoadSaveState()
        {
            questionNumber = PlayerPrefs.GetInt(QUESTION_INDEX_SAVE_KEY);
            questionNumber = Mathf.Clamp(questionNumber, 1, 10);
        }

        static void SaveSaveState()
        {
            PlayerPrefs.SetInt(QUESTION_INDEX_SAVE_KEY, questionNumber);
        }

        #endregion

        #region UNITY_EVENT_TARGETS

        public void AdvanceQuestion()
        {
            SFX.PlaySound();
            AdvanceQuestion_Discrete();
        }

        void AdvanceQuestion_Discrete()
        {
            NextQuestion();
            ChangeGameState(GameState.SlideOut);
        }

        public void AnswerButton(Transform t)
        {
            //SFX.PlaySound();
            // order of children should directly correspond to the number they represent
            int answerIndex = t.GetSiblingIndex();

            Animator answerAnimator = t.GetComponent<Animator>();
            if (answerAnimator)
                answerAnimator.SetTrigger(BOUNCE_TRIGGER);

            VerifyAnswer(answerIndex);
        }

        public void QuitGame()
        {
            SFX.PlaySound();
            SceneManager.LoadScene(0);
        }

        #endregion

        void BounceQuestionText()
        {
            Animator animator = questionText.GetComponent<Animator>();
            if (animator)
                animator.SetTrigger(BOUNCE_TRIGGER);
        }

        void BounceAnswerButton(int i)
        {
            Debug.Assert(i >= 0);
            Debug.Assert(i < answerTexts.Length);
            Animator answerAnimator = answerTexts[i].GetComponentInParent<Animator>();
            if (answerAnimator)
                answerAnimator.SetTrigger(BOUNCE_TRIGGER);
        }

        bool IsBusy()
        {
            return state != GameState.Play;
        }

        float GetIndexTone(int i)
        {
            return 2f + (i / 8f);
        }

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