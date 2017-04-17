using System;

namespace LeapMedia {
    internal class ScrubDetector {
        public const float MIN_TRIGGER_DISTANCE = 30;
        public const float MIN_TIME_VISIBLE = 750 * 1000;

        private float lastXTriggered = float.NaN;

        public void OnHand(HandStats hand, long timestamp) {
            if (hand.TimeVisible <= MIN_TIME_VISIBLE) return;

            float currentX = hand.PalmPosition.x;
            if (float.IsNaN(lastXTriggered)) {
                lastXTriggered = currentX;
                return;
            }

            float diff = lastXTriggered - currentX;
            if (Math.Abs(diff) < MIN_TRIGGER_DISTANCE) return;

            lastXTriggered = currentX;

            if (diff < 0) {
                VolumeController.VolumeDown();
            } else {
                VolumeController.VolumeUp();
            }
        }
    }
}