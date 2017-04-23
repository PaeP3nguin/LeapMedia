using System;

namespace LeapMedia {
    /// <summary>
    ///     A general-purpose class for detecting continuous hand gestures
    ///     A continuous gesture is one that can be triggered multiple times as some continuous quantity changes
    ///     An example of a continuous gesture is rotating the hand while pinching to adjust volume
    ///     Handles all the nitty-gritty stuff of debouncing, only triggering once per gesture, etc.
    /// </summary>
    internal class ContinuousGestureDetector : IGestureDetector {
        private readonly Func<HandStats, bool> canGesture;
        private readonly Func<HandStats, float> discreteValue;
        private readonly Action<bool> onGesture;
        private readonly float triggerThreshold;

        private float lastTriggerValue = float.NaN;

        /// <summary>
        ///     Create a continuous gesture detector
        /// </summary>
        /// <param name="canGesture">A function that returns true if the hand is currently able to make the gesture</param>
        /// <param name="discreteValue">A function that returns the current discrete value to monitor</param>
        /// <param name="triggerThreshold">The interval of change in the discrete value at which to trigger the gesture</param>
        /// <param name="onGesture">A function that is called when the gesture is triggered, parameter is true if the discrete value has increased</param>
        public ContinuousGestureDetector(Func<HandStats, bool> canGesture, Func<HandStats, float> discreteValue,
            float triggerThreshold, Action<bool> onGesture) {
            this.canGesture = canGesture;
            this.discreteValue = discreteValue;
            this.triggerThreshold = triggerThreshold;
            this.onGesture = onGesture;
        }
        
        public void OnHand(HandStats hand, long timestamp) {
            if (!canGesture(hand)) return;

            float currentValue = discreteValue(hand);
            if (float.IsNaN(lastTriggerValue)) {
                lastTriggerValue = currentValue;
                return;
            }

            float diff = lastTriggerValue - currentValue;
            if (Math.Abs(diff) < triggerThreshold) return;

            lastTriggerValue = currentValue;

            onGesture(diff >= 0);
        }
    }
}