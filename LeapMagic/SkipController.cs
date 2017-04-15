using static LeapMagic.HandStats;

namespace LeapMagic {
    class SkipController : IController {
        private const int MIN_ACTION_DEBOUNCE = 500 * 1000;
        private const int MIN_UNCENTERED_TIME = 100 * 1000;
        private const int MIN_CENTERED_TIME = 100 * 1000;

        private int lastTriggeredHandId;
        private int lastHandId;
        private long lastActionTime;
        private long lastUncenteredTime;
        private PointingDirection direction;

        public void OnHand(HandStats hand, long timestamp) {
            PointingDirection lastDirection = lastHandId == hand.Id ? direction : PointingDirection.Center;
            direction = hand.Pointing;

            lastHandId = hand.Id;

            if (direction != PointingDirection.Center && lastDirection != direction) {
                lastUncenteredTime = timestamp;
            } else if (direction == PointingDirection.Center) {
                if (lastDirection != PointingDirection.Center && timestamp >= lastUncenteredTime + MIN_CENTERED_TIME) {
                    lastTriggeredHandId = -1;
                }
                return;
            }

            // Make sure the hand is uncentered for a bit, not just random noise
            if (timestamp <= lastUncenteredTime + MIN_UNCENTERED_TIME) return;

            // Don't let the same uncentered hand skip music again
            if (lastTriggeredHandId == hand.Id) return;

            // Only skip music at most once every second
            if (timestamp <= lastActionTime + MIN_ACTION_DEBOUNCE) return;

            lastActionTime = timestamp;
            lastTriggeredHandId = hand.Id;
            if (direction == PointingDirection.Left) {
                PlaybackUtil.PreviousTrack();
            } else if (direction == PointingDirection.Right) {
                PlaybackUtil.NextTrack();
            }
        }
    }
}
