using System;
using LeapMedia.Util;

namespace LeapMedia.Gestures {
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
        private long lastPositiveTriggerTime;
        private long lastNegativeTriggerTime;

        /// <summary>
        ///     Amount of time in microseconds to wait between triggering gesture going in different directions
        /// </summary>
        public int DirectionDebounceTime { get; set; }

        /// <summary>
        ///     Create a continuous gesture detector
        /// </summary>
        /// <param name="canGesture">A function that returns true if the hand is currently able to make the gesture</param>
        /// <param name="discreteValue">A function that returns the current discrete value to monitor</param>
        /// <param name="triggerThreshold">The interval of change in the discrete value at which to trigger the gesture</param>
        /// <param name="onGesture">
        ///     A function that is called when the gesture is triggered,
        ///     parameter is true if the discrete value has increased
        /// </param>
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

            bool isPositiveChange = diff >= 0;

            if (isPositiveChange) {
                if (lastNegativeTriggerTime + DirectionDebounceTime >= timestamp) {
                    return;
                }
                lastPositiveTriggerTime = timestamp;
            } else {
                if (lastPositiveTriggerTime + DirectionDebounceTime >= timestamp) {
                    return;
                }
                lastNegativeTriggerTime = timestamp;
            }

            lastTriggerValue = currentValue;
            onGesture(isPositiveChange);
        }

        /// <summary>
        ///     Create a gesture detector that changes the volume when the hand is rotated along the Z axis
        /// </summary>
        public static ContinuousGestureDetector RotateHandChangeVolumeGesture() {
            return new ContinuousGestureDetector(
                // Only allow rotation if hand is closed
                hand => !hand.IsOpen,
                // Track hand roll (angle along Z axis) in radians, from 0 to 2PI
                delegate(HandStats hand) {
                    if (hand.Roll >= 0) {
                        return hand.Roll;
                    }
                    // Transform the -PI to 0 space to be from PI to 2 * PI
                    return (float) (Math.PI * 2 + hand.Roll);
                },
                // Trigger every 0.35 radians / 20 degrees
                0.35f,
                // Increase volume if rotating clockwise
                delegate(bool isPositive) {
                    if (isPositive) {
                        VolumeUtil.VolumeUp();
                    } else {
                        VolumeUtil.VolumeDown();
                    }
                }) {
                DirectionDebounceTime = 500 * 1000
            };
        }

        /// <summary>
        ///     Create a gesture detector that changes the volume when the hand is moved left and right (along the X axis)
        /// </summary>
        public static ContinuousGestureDetector MoveHandXChangeVolumeGesture() {
            return new ContinuousGestureDetector(
                // Only acknowledge hand if it's visible for a bit first
                hand => hand.TimeVisible >= 500 * 1000,
                // Track lateral hand movement along long axis of Leap
                hand => hand.PalmPosition.x,
                // Trigger every 30 millimeters
                30,
                // Increase volume if moving right
                delegate(bool isPositive) {
                    if (isPositive) {
                        VolumeUtil.VolumeUp();
                    } else {
                        VolumeUtil.VolumeDown();
                    }
                });
        }
    }
}