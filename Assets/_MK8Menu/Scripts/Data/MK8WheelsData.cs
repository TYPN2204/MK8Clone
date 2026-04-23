using UnityEngine;

namespace MK8.Shared
{
    [CreateAssetMenu(menuName = "MK8/Wheels", fileName = "MK8_Wheels_")]
    public class MK8WheelsData : ScriptableObject
    {
        public string displayName;
        public Sprite icon;
        public GameObject wheelsPrefab;
        public bool isLocked;
    }
}