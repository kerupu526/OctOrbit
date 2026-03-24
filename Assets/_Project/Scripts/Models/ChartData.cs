using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OctOrbit.Models
{
    [Serializable]
    public class ChartData
    {
        public string songTitle;
        public string songArtist;
        public float bpm;
        public float offset; // 초 단위 오프셋
        public int timeSignatureNumerator = 4;
        public int timeSignatureDenominator = 4;

        public List<NoteData> notes = new List<NoteData>();
    }

    [Serializable]
    public class NoteData
    {
        public int measure;      // 마디 (0부터 시작)
        public float beat;       // 박자 (0.0 ~ 3.99... 4/4박 기준)
        public NoteType type;    // Tap, Hold, Slide
        public int direction;    // 0~7
        
        // Optional Fields
        public float length;     // Hold/Slide 지속 시간 (박자 단위)
        public SlideDirection slideDir; // Slide 전용

        [JsonIgnore]
        public double totalBeats => measure * 4.0 + beat; 
    }

    public enum NoteType { Tap, Hold, Slide }
    public enum SlideDirection { None, Left, Right }
}