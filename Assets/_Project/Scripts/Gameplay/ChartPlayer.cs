using System.Collections.Generic;
using System.Collections; // 이거 추가 확인!
using UnityEngine;
using OctOrbit.Core;
using OctOrbit.Models; // ChartData, NoteData

namespace OctOrbit.Gameplay
{
    public class ChartPlayer : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("노트가 화면에 나타나서 판정선까지 도달하는 시간")]
        public float lookaheadTime = 2.5f;

        private ChartData _currentChart;
        private int _noteIndex = 0;
        private List<NoteSpawnData> _spawnList = new List<NoteSpawnData>();

        // 런타임에서 사용할 계산된 노트 데이터
        private struct NoteSpawnData
        {
            public NoteData rawData;
            public double hitTimeSeconds;
        }

        // Start를 IEnumerator로 변경하여 코루틴으로 만듭니다.
        private IEnumerator Start()
        {
            // 1. 차트 데이터 준비 (메모리에 올리기)
            CreateDummyChart();
            PrepareSpawnList();

            // 2. 엔진 웜업 및 FMOD 뱅크 로딩 대기 시간
            // 이 시간 동안 유니티는 버벅거림(초기화)을 끝내고 안정화됩니다.
            Debug.Log("<b>[OctOrbit]</b> 시스템 웜업 중... (1.5초 대기)");
            yield return new WaitForSeconds(1.5f);

            // 3. 안정화된 후 음악 및 채보 재생 시작
            Debug.Log("<b>[OctOrbit]</b> 게임 시작!");
            TimingManager.Instance.PlayMusic();
        }

        private void CreateDummyChart()
        {
            _currentChart = new ChartData
            {
                bpm = 120f, // 120 BPM이면 1박자에 0.5초
                offset = 0f,
                notes = new List<NoteData>
                {
                    // 1마디 0박자 (0초) - 사실상 게임 시작하자마자 바로 맞아야 하는 노트 (테스트용)
                    new NoteData { measure = 0, beat = 0.0f, direction = 0, type = NoteType.Tap },
                    // 1마디 1박자 (0.5초 뒤)
                    new NoteData { measure = 0, beat = 1.0f, direction = 1, type = NoteType.Tap },
                    // 1마디 2박자 (1.0초 뒤)
                    new NoteData { measure = 0, beat = 2.0f, direction = 2, type = NoteType.Tap },
                    // 1마디 3박자 (1.5초 뒤)
                    new NoteData { measure = 0, beat = 3.0f, direction = 3, type = NoteType.Tap },
                    // 2마디 0박자 (2.0초 뒤)
                    new NoteData { measure = 1, beat = 0.0f, direction = 4, type = NoteType.Tap },
                }
            };
            
            // TimingManager BPM 동기화
            TimingManager.Instance.bpm = _currentChart.bpm;
        }

        private void PrepareSpawnList()
        {
            double secPerBeat = 60.0 / _currentChart.bpm;

            foreach (var note in _currentChart.notes)
            {
                // totalBeats = measure * 4 + beat (4/4박자 기준)
                double totalBeats = note.measure * 4.0 + note.beat;
                double hitTime = (totalBeats * secPerBeat) + _currentChart.offset;

                _spawnList.Add(new NoteSpawnData { rawData = note, hitTimeSeconds = hitTime });
            }

            // 혹시 모르니 판정 시간 순으로 오름차순 정렬 (차트 에디터에서 순서가 섞일 수 있으므로)
            _spawnList.Sort((a, b) => a.hitTimeSeconds.CompareTo(b.hitTimeSeconds));
        }

        private void Update()
        {
            if (TimingManager.Instance == null || _spawnList == null) return;

            double currentTime = TimingManager.Instance.CurrentAudioPosSeconds;

            // 스폰해야 할 노트를 찾음 (현재 시간 + 미리보기 시간이 노트의 판정 시간보다 크거나 같을 때)
            while (_noteIndex < _spawnList.Count && 
                   currentTime + lookaheadTime >= _spawnList[_noteIndex].hitTimeSeconds)
            {
                var spawnData = _spawnList[_noteIndex];
                
                // NotePoolManager에게 노트 소환 지시
                NotePoolManager.Instance.SpawnNote(spawnData.rawData.direction, spawnData.hitTimeSeconds);
                
                _noteIndex++;
            }
        }
    }
}