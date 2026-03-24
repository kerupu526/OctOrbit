using System.Collections.Generic;
using OctOrbit.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace OctOrbit.Gameplay
{
    public class NotePoolManager : MonoBehaviour
    {
        public static NotePoolManager Instance { get; private set; }

        [Header("References")]
        [Tooltip("노트가 생성될 부모 캔버스 트랜스폼")]
        public RectTransform gameplayCanvasRect; 
        [Tooltip("노트 프리팹 (UI Image 기반)")]
        public GameObject notePrefab;

        [Header("Spawning Settings")]
        [Tooltip("판정선으로부터 노트가 생성되는 거리 (픽셀 단위)")]
        public float spawnDistance = 1500f; 

        // 8방향 정규화 벡터 및 실제 스폰 위치 캐싱
        private Vector2[] _directionVectors = new Vector2[8];
        private Vector2[] _spawnPositions = new Vector2[8];

        // 유니티 내장 오브젝트 풀
        private IObjectPool<GameObject> _notePool;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            InitializeDirections();
            InitializePool();
        }

        // 1. 8방향 벡터 및 스폰 좌표 미리 계산 (성능 최적화)
        private void InitializeDirections()
        {
            // 0: Up, 1: UpRight, 2: Right, 3: DownRight, 4: Down, 5: DownLeft, 6: Left, 7: UpLeft
            // 시계 방향으로 45도씩 회전
            for (int i = 0; i < 8; i++)
            {
                // 12시 방향(Up)을 기준으로 계산하기 위해 90도에서 시작하여 빼나감
                float angleDegrees = 90f - (i * 45f);
                float angleRadians = angleDegrees * Mathf.Deg2Rad;

                _directionVectors[i] = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians)).normalized;
                _spawnPositions[i] = _directionVectors[i] * spawnDistance;
            }
        }

        // 2. 오브젝트 풀 초기화
        private void InitializePool()
        {
            _notePool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(notePrefab, gameplayCanvasRect),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false, // 최적화를 위해 중복 반환 체크 해제
                defaultCapacity: 50,
                maxSize: 200 // 최대 동시 노트 수
            );
        }

        [Header("Note Management")]
        [Tooltip("판정선에 도달하기까지 걸리는 시간 (2.5초)")]
        public float noteTravelTime = 2.5f; 

        // 매니저가 관리할 활성화된 노트 리스트
        private List<NoteController> _activeNotes = new List<NoteController>(200);

        // 노트 소환 메서드 수정
        public void SpawnNote(int directionIndex, double hitTime)
        {
            if (directionIndex < 0 || directionIndex > 7) return;

            GameObject noteObj = _notePool.Get();
            NoteController note = noteObj.GetComponent<NoteController>();
            
            double spawnTime = hitTime - noteTravelTime;
            
            // 데이터 주입
            note.Initialize(directionIndex, hitTime, spawnTime, _spawnPositions[directionIndex]);
            
            // 리스트에 추가하여 매니저가 관리하도록 함
            _activeNotes.Add(note);
        }

        private void Update()
        {
            // 음악이 재생 중이 아니라면 연산하지 않음
            if (TimingManager.Instance == null) return;

            double currentTime = TimingManager.Instance.CurrentAudioPosSeconds;

            // 🌟 10년 차의 팁: 리스트 요소 삭제가 일어날 수 있으므로 역순(Reverse) for문 사용
            for (int i = _activeNotes.Count - 1; i >= 0; i--)
            {
                NoteController note = _activeNotes[i];

                // 진행도(0.0 ~ 1.0) 계산: (현재시간 - 생성시간) / 이동총시간
                float progress = (float)((currentTime - note.SpawnTime) / noteTravelTime);

                // 1. 노트 이동 (시작점 -> 중심점 Vector2.zero)
                note.RectTransform.anchoredPosition = Vector2.Lerp(note.StartPosition, Vector2.zero, progress);

                // 2. 노트가 판정선(진행도 1.0)을 지나쳤는지 확인
                // ※ 나중에 여기에 입력(Input) 처리 및 판정(Perfect, Miss 등) 로직이 들어갑니다.
                if (progress >= 1.0f)
                {
                    // 지금은 일단 닿으면 바로 삭제되도록 처리
                    DespawnNote(note, i);
                }
            }
        }

        // 노트를 풀로 돌려보내고 리스트에서 제거
        private void DespawnNote(NoteController note, int listIndex)
        {
            _activeNotes.RemoveAt(listIndex);
            _notePool.Release(note.gameObject);
        }
    }
}