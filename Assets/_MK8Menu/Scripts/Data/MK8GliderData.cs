using UnityEngine;

namespace MK8.Shared
{
    [CreateAssetMenu(menuName = "MK8/Glider", fileName = "MK8_Glider_")]
    public class MK8GliderData : ScriptableObject
    {
        public string displayName;
        public Sprite icon;
        public GameObject gliderPrefab;
        public bool isLocked;
    }
}