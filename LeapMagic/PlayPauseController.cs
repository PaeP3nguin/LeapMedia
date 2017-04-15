namespace LeapMagic {
    class PlayPauseController : IController {
        private const int MIN_ACTION_DEBOUNCE = 1000 * 1000;
        private const int MIN_OPEN_TIME = 100 * 1000;
        private const int MIN_CLOSE_TIME = 100 * 1000;

        private int lastToggleHandId;
        private int lastHandId;
        private long lastActionTime;
        private long lastHandOpenTime;
        private bool isOpen;

        public void OnHand(HandStats hand, long timestamp) {
            bool wasOpen = isOpen && lastHandId == hand.Id;
            isOpen = hand.IsOpen;

            lastHandId = hand.Id;

            if (!wasOpen && isOpen) {
                lastHandOpenTime = timestamp;
            } else if (!isOpen) {
                if (wasOpen && timestamp >= lastHandOpenTime + MIN_CLOSE_TIME) {
                    lastToggleHandId = -1;
                }
                return;
            }

            // Make sure the hand is open for a bit, not just random noise
            if (timestamp <= lastHandOpenTime + MIN_OPEN_TIME) return;

            // Don't let the same open hand toggle music again
            if (lastToggleHandId == hand.Id) return;

            // Only toggle music at most once every second
            if (timestamp <= lastActionTime + MIN_ACTION_DEBOUNCE) return;

            lastActionTime = timestamp;
            lastToggleHandId = hand.Id;
            PlaybackUtil.ToggleMusic();
        }
    }
}
