using Raylib_cs;

namespace MonopolyDeal
{
    public static class Time
    {
        public static float TimeScale = 1f;
        public static float DeltaTime { get; private set; }
        public static float Now { get; private set; }
        public static float UnscaledDeltaTime { get; private set; }
        public static float UnscaledNow { get; private set; }

        public static void Update()
        {
            UnscaledDeltaTime = Raylib.GetFrameTime();
            UnscaledNow += UnscaledDeltaTime;

            DeltaTime = UnscaledDeltaTime * TimeScale;
            Now += UnscaledNow;
        }
    }
}

