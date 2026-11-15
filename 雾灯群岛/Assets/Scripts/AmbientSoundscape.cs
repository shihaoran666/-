using UnityEngine;

namespace FoglightIsles
{
    /// <summary>Small generated wind-and-sea loop so the prototype has ambience without external audio assets.</summary>
    public sealed class AmbientSoundscape : MonoBehaviour
    {
        private const int SampleRate = 44100;
        private const int DurationSeconds = 18;

        private void Awake()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = CreateLoop();
            source.loop = true;
            source.volume = .72f;
            source.spatialBlend = 0f;
            source.Play();
        }

        private void Update()
        {
            AudioListener.pause = StartScreen.IsOpen || PauseMenu.IsPaused || MapOverlay.IsOpen || QuestJournalPanel.IsOpen;
        }

        private void OnDestroy()
        {
            AudioListener.pause = false;
        }

        private static AudioClip CreateLoop()
        {
            int sampleCount = SampleRate * DurationSeconds;
            var samples = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)SampleRate;
                float wind = Mathf.Sin(time * Mathf.PI * 2f * .075f) * .06f +
                             Mathf.Sin(time * Mathf.PI * 2f * .19f) * .025f;
                float surf = Mathf.Sin(time * Mathf.PI * 2f * .31f) * .035f;
                float foam = (Mathf.PerlinNoise(index * .0017f, 2.4f) - .5f) * .038f;
                float fade = Mathf.Min(1f, time * .9f, (DurationSeconds - time) * .9f);
                samples[index] = Mathf.Clamp((wind + surf + foam) * fade, -.18f, .18f);
            }

            var clip = AudioClip.Create("Morning Bay Wind and Tide", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
