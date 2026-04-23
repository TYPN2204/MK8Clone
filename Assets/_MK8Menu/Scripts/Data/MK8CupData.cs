using UnityEngine;

namespace MK8.Shared
{
    [CreateAssetMenu(menuName = "MK8/Cup", fileName = "MK8_Cup_")]
    public class MK8CupData : ScriptableObject
    {
        public string displayName;
        public Sprite icon;
        public MK8TrackData[] tracks;
        public bool isLocked;
    }
}