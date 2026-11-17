using UnityEngine;

namespace FoglightIsles
{
    public enum InteractionCue { BeaconSuccess, BeaconFailure, Discovery, QuestHandIn }

    /// <summary>Short generated confirmation sounds; one voice avoids overlap during repeated interactions.</summary>
    public sealed class InteractionCueAudio : MonoBehaviour
    {
        private const int SampleRate = 44100;
        private const float MinimumInterval = .09f;
        private AudioSource _source;
        private AudioClip[] _clips;
        private float _lastPlayedAt = -10f;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = false;
            _source.spatialBlend = 0f;
            _source.ignoreListenerPause = false;
            _source.volume = .34f;
            _clips = new[]
            {
                CreateClip("Beacon success", .19f, new[] { 620f, 880f }),
                CreateClip("Beacon failure", .20f, new[] { 390f, 260f }),
                CreateClip("Discovery", .24f, new[] { 740f, 990f, 1240f }),
                CreateClip("Quest hand-in", .28f, new[] { 440f, 660f, 880f })
            };
        }

        public void Play(InteractionCue cue)
        {
            if (_source == null || _clips == null || AudioListener.pause ||
                StartScreen.IsOpen || PauseMenu.IsPaused || MapOverlay.IsOpen || QuestJournalPanel.IsOpen ||
                Time.unscaledTime - _lastPlayedAt < MinimumInterval) return;

            int index = (int)cue;
            if (index < 0 || index >= _clips.Length) return;
            _lastPlayedAt = Time.unscaledTime;
            _source.Stop();
            _source.clip = _clips[index];
            _source.Play();
        }

        private void OnDestroy()
        {
            if (_clips == null) return;
            foreach (AudioClip clip in _clips)
                if (clip != null) Destroy(clip);
        }

        private static AudioClip CreateClip(string name, float duration, float[] notes)
        {
            int count = Mathf.CeilToInt(SampleRate * duration);
            var samples = new float[count];
            float noteDuration = duration / notes.Length;
            for (int i = 0; i < count; i++)
            {
                float time = i / (float)SampleRate;
                int note = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(time / noteDuration));
                float notePhase = (time - note * noteDuration) / noteDuration;
                float noteEnvelope = Mathf.Sin(Mathf.PI * notePhase);
                float envelope = Mathf.Min(1f, Mathf.Min(time / .012f, (duration - time) / .04f));
                float wave = Mathf.Sin(2f * Mathf.PI * notes[note] * time);
                samples[i] = wave * noteEnvelope * envelope * .72f;
            }

            var clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
