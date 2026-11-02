namespace DeadAir.Core
{
    // Общий флаг отладочной отрисовки (конус, состояние, последняя позиция).
    // По умолчанию включено, чтобы всё было видно сразу.
    public static class DebugOverlay
    {
        public static bool Visible { get; private set; } = true;

        public static void Toggle()
        {
            Visible = !Visible;
        }
    }
}
