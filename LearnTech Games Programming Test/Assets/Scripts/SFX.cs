namespace LearnTechGamesTest
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    // SFX Singleton
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class SFX : MonoBehaviour
    {
        static SFX instance;
        AudioSource audioPlayer;

        private void Awake()
        {
            if (instance)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            audioPlayer = GetComponent<AudioSource>();
        }

        private void OnDestroy()
        {
            //if (instance == this)
            //    instance = null;
        }

        public static void PlaySound(float pitch = 2.0f)
        {
            instance.audioPlayer.Stop();
            instance.audioPlayer.pitch = pitch;
            instance.audioPlayer.Play();
        }
    }
}