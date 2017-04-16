using System;

namespace LeapMedia {
    internal class ScrubDetector {
        public const float MIN_TRIGGER_DISTANCE = 30;

        public float lastXTriggered = float.NaN;

        public void OnHand(HandStats hand, long timestamp) {
            float currentX = hand.PalmPosition.x;
            if (lastXTriggered == float.NaN) {
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