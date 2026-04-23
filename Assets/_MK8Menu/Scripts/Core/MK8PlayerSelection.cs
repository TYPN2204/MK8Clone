namespace MK8.Shared
{
    public static class MK8PlayerSelection
    {
        public static MK8CharacterData Character;
        public static MK8KartBodyData KartBody;
        public static MK8WheelsData Wheels;
        public static MK8GliderData Glider;
        public static MK8CupData Cup;
        public static MK8TrackData Track;
        public static MK8SpeedClass SpeedClass;
        public static MK8GameMode GameMode;

        public static void Clear()
        {
            Character = null;
            KartBody = null;
            Wheels = null;
            Glider = null;
            Cup = null;
            Track = null;
            SpeedClass = MK8SpeedClass.CC50;
            GameMode = MK8GameMode.GrandPrix;
        }
    }
}