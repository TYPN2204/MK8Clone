using UnityEngine;

namespace MK8.Shared
{
    [CreateAssetMenu(menuName = "MK8/Kart Body", fileName = "MK8_KartBody_")]
    public class MK8KartBodyData : ScriptableObject
    {
        public string displayName;
        public Sprite icon;
        public GameObject bodyPrefab;
        public bool isLocked;
    }
}