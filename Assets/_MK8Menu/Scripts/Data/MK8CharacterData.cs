using UnityEngine;

namespace MK8.Shared
{
    [CreateAssetMenu(menuName = "MK8/Character", fileName = "MK8_Character_")]
    public class MK8CharacterData : ScriptableObject
    {
        public string displayName;
        public Sprite portrait;
        public GameObject modelPrefab;
        public bool isLocked;
    }
}