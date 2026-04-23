using UnityEngine;

namespace MK8.Shared
{
    [CreateAssetMenu(menuName = "MK8/Track", fileName = "MK8_Track_")]
    public class MK8TrackData : ScriptableObject
    {
        public string displayName;
        public Sprite preview;
        public string raceSceneName;
        public bool isLocked;
    }
}