using UnityEngine;

namespace MK8.Shared
{
    [CreateAssetMenu(menuName = "MK8/Database", fileName = "MK8_Database")]
    public class MK8Database : ScriptableObject
    {
        public MK8CharacterData[] characters;
        public MK8KartBodyData[] kartBodies;
        public MK8WheelsData[] wheels;
        public MK8GliderData[] gliders;
        public MK8CupData[] cups;

        public static MK8Database Instance { get; private set; }

        private void OnEnable()
        {
            Instance = this;
        }
    }
}