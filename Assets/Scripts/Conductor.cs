using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

using Starpelly;

namespace RhythmHeavenMania
{
    // [RequireComponent(typeof(AudioSource))]
    public class Conductor : MonoBehaviour
    {
        // Song beats per minute
        // This is determined by the song you're trying to sync up to
        public float songBpm;

        // The number of seconds for each song beat
        public float secPerBeat;

        // Current song position, in seconds
        public float songPosition;

        // Current song position, in beats
        public float songPositionInBeats;

        // Current time of the song
        private float time;

        // an AudioSource attached to this GameObject that will play the music.
        public AudioSource musicSource;

        // The offset to the first beat of the song in seconds
        public float firstBeatOffset;

        // Conductor instance
        public static Conductor instance;

        public bool isPlaying;
        public bool isPaused;

        // private AudioDspTimeKeeper timeKeeper;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            musicSource = GetComponent<AudioSource>();

            secPerBeat = 60f / songBpm;
        }

        public void SetBeat(float beat)
        {
            float secFromBeat = GetSongPosFromBeat(beat);

            if (musicSource.clip != null)
            {
                if (secFromBeat < musicSource.clip.length)
                    musicSource.time = secFromBeat;
                else
                    musicSource.time = 0;
            }

            GameManager.instance.SetCurrentEventToClosest(beat);
        }

        public void Play(float beat)
        {
            this.time = GetSongPosFromBeat(beat);

            isPlaying = true;
            isPaused = false;

            if (SongPosLessThanClipLength(time))
            {
                musicSource.time = time;
                musicSource.PlayScheduled(Time.time);
            }

            // GameManager.instance.SetCurrentEventToClosest(songPositionInBeats);
        }

        public void Pause()
        {
            isPlaying = false;
            isPaused = true;

            musicSource.Pause();
        }

        public void Stop(float time)
        {
            this.time = time;
            isPlaying = false;
            isPaused = false;


            musicSource.Stop();
        }

        /*public void SetTime(float startBeat)
        {
            musicSource.time = GetSongPosFromBeat(startBeat);
            songPositionInBeats = musicSource.time / secPerBeat;
            GameManager.instance.SetCurrentEventToClosest(songPositionInBeats);
        }*/

        public void Update()
        {
            if (isPlaying)
            {
                time += Time.deltaTime * musicSource.pitch;

                songPosition = time - firstBeatOffset;

                songPositionInBeats = songPosition / secPerBeat;
            }
        }

        public float GetLoopPositionFromBeat(float startBeat, float length)
        {
            float a = Mathp.Normalize(songPositionInBeats, startBeat, startBeat + length);
            return a;
        }

        public float GetSongPosFromBeat(float beat)
        {
            return secPerBeat * beat;
        }

        public void SetBpm(float bpm)
        {
            this.songBpm = bpm;
            secPerBeat = 60f / songBpm;
        }

        public float SongLengthInBeats()
        {
            if (!musicSource.clip) return 0;
            return musicSource.clip.length / secPerBeat;
        }

        public bool SongPosLessThanClipLength(float t)
        {
            if (musicSource.clip != null)
                return t < musicSource.clip.length;
            else
                return false;
        }

        public bool NotStopped()
        {
            return Conductor.instance.isPlaying == true || Conductor.instance.isPaused;
        }
    }
}