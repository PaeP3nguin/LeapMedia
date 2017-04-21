using System;

namespace LeapMedia {
    /// <summary>
    ///     A general-purpose class for detecting hand gestures
    ///     Handles all the nitty-gritty stuff of debouncing, only triggering once per gesture, etc.
    /// </summary>
    internal class GestureDetector : IGestureDetector {
        private const int MIN_TRIGGER_DEBOUNCE = 500 * 1000;
        private const int MIN_DETECTION_TIME = 100 * 1000;
        private const int MIN_COOLDOWN_TIME = 100 * 1000;

        private readonly Func<HandStats, bool> isGesture;
        private readonly Action onGesture;

        private bool isMakingGesture;
        private long lastActionTime;
        private long lastGestureTime;
        private int lastHandId;
        private int lastTriggerHandId;

        /// <summary>
        ///     Create a gesture detector
        /// </summary>
        /// <param name="isGesture">A function that returns true if the gesture is detected</param>
        /// <param name="onGesture">A function to be run when the gesture is triggered</param>
        public GestureDetector(Func<HandStats, bool> isGesture, Action onGesture) {
            this.isGesture = isGesture;
            this.onGesture = onGesture;
        }

        /// <summary>
        ///     Call this function whenever a valid hand is found in a frame that you want to detect gestures on
        /// </summary>
        /// <param name="hand">Hand to detect gestures on</param>
        /// <param name="timestamp">Timestamp of the frame, in microseconds</param>
        public void OnHand(HandStats hand, long timestamp) {
            bool wasMakingGesture = isMakingGesture && lastHandId == hand.Id;
            isMakingGesture = isGesture(hand);

            lastHandId = hand.Id;

            if (!wasMakingGesture && isMakingGesture) {
                lastGestureTime = timestamp;
            } else if (!isMakingGesture) {
                if (wasMakingGesture && timestamp >= lastGestureTime + MIN_COOLDOWN_TIME) {
                    lastTriggerHandId = -1;
                }
                return;
            }

            // Make sure the trigger is detected for a bit, not just random noise
            if (timestamp <= lastGestureTime + MIN_DETECTION_TIME) return;

            // Don't let the same hand trigger gesture again
            if (lastTriggerHandId == hand.Id) return;

            // Only trigger action at most once every second
            if (timestamp <= lastActionTime + MIN_TRIGGER_DEBOUNCE) return;

            // Trigger a gesture!
            lastActionTime = timestamp;
            lastTriggerHandId = hand.Id;
            onGesture();
        }
    }
}