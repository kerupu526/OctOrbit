using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace OctOrbit.Core
{
    public class TimingManager : MonoBehaviour
    {
        public static TimingManager Instance { get; private set; }

        [Header("Settings")]
        public float bpm = 120f;
        public int beatsPerMeasure = 4;

        [Header("Audio Reference")]
        public EventReference musicEvent; // FMOD Music Event
        private EventInstance _musicInstance;

        // 실시간 타이밍 데이터
        public double CurrentAudioPosSeconds { get; private set; }
        public double CurrentBeat { get; private set; }
        public int CurrentMeasure { get; private set; }

        private PLAYBACK_STATE _playbackState;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void PlayMusic()
        {
            _musicInstance = RuntimeManager.CreateInstance(musicEvent);
            _musicInstance.start();
        }

        private void Update()
        {
            // 인스턴스가 유효한지 먼저 체크
            if (!_musicInstance.isValid()) return;

            // FMOD 재생 상태 확인 (멈춰있는지 디버깅 용도)
            _musicInstance.getPlaybackState(out _playbackState);

            // 1. FMOD에서 현재 재생 위치(ms)를 가져와 초(s)로 변환
            _musicInstance.getTimelinePosition(out int timelinePosMs);
            CurrentAudioPosSeconds = timelinePosMs / 1000.0;

            // 2. 현재 비트 계산
            CurrentBeat = CurrentAudioPosSeconds * (bpm / 60.0);

            // 3. 현재 마디 계산
            CurrentMeasure = (int)(CurrentBeat / beatsPerMeasure);
        }

        private void OnDestroy()
        {
            if (_musicInstance.isValid())
            {
                // 반드시 Stop을 먼저 하고 Release를 해야 메모리 릭(누수)이 발생하지 않습니다.
                _musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _musicInstance.release();
            }
        }
    }
}