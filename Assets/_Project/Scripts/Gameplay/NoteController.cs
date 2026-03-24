using UnityEngine;

namespace OctOrbit.Gameplay
{
    public class NoteController : MonoBehaviour
    {
        public int DirectionIndex { get; private set; }
        public double HitTime { get; private set; } // 판정선에 닿아야 하는 정확한 시간(초)
        public double SpawnTime { get; private set; } // 생성된 시간(초)
        public Vector2 StartPosition { get; private set; }
        
        // 캐싱용 트랜스폼
        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        // 매니저가 노트를 소환할 때 데이터를 주입해주는 메서드
        public void Initialize(int directionIndex, double hitTime, double spawnTime, Vector2 startPos)
        {
            DirectionIndex = directionIndex;
            HitTime = hitTime;
            SpawnTime = spawnTime;
            StartPosition = startPos;
            
            // 초기 위치 세팅
            RectTransform.anchoredPosition = startPos;
        }
    }
}