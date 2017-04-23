using System;
using LeapMedia.Util;

namespace LeapMedia.Gestures {
    /// <summary>
    ///     A general-purpose class for detecting discrete hand gestures
    ///     A discrete gesture is one where the hand is either making or not making the gesture and an action should be
    ///     triggered only once when the hand first starts making the gesture
    ///     An example of a discrete gesture is rotating the hand past a certain angle to change the song
    ///     Handles all the nitty-gritty stuff of debouncing, only triggering once per gesture, etc.
    /// </summary>
    internal class DiscreteGestureDetector : IGestureDetector {
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
        ///     Create a discrete gesture detector
        /// </summary>
        /// <param name="isGesture">A function that returns true if the gesture is detected</param>
        /// <param name="onGesture">A function to be run when the gesture is triggered</param>
        public DiscreteGestureDetector(Func<HandStats, bool> isGesture, Action onGesture) {
            this.isGesture = isGesture;
            this.onGesture = onGesture;
        }

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

        /// <summary>
        ///     Create a gesture detector that toggles the play/pause state of music when the hand's fingers are spread wide
        /// </summary>
        /// <returns></returns>
        public static DiscreteGestureDetector OpenHandToggleMusicGesture() {
            return new DiscreteGestureDetector(hand => hand.IsOpen,
                PlaybackUtil.ToggleMusic);
        }

        /// <summary>
        ///     Create a gesture detector that skips to the previous track when the hand points left (rotating along the Y axis)
        /// </summary>
        public static DiscreteGestureDetector HandLeftPrevTrackGesture() {
            return new DiscreteGestureDetector(hand => hand.Pointing == HandStats.PointingDirection.Left,
                PlaybackUtil.PreviousTrack);
        }

        /// <summary>
        ///     Create a gesture detector that skips to the previous track when the hand points left (rotating along the Y axis)
        /// </summary>
        public static DiscreteGestureDetector HandRightNextTrackGesture() {
            return new DiscreteGestureDetector(hand => hand.Pointing == HandStats.PointingDirection.Right,
                PlaybackUtil.NextTrack);
        }

        /// <summary>
        ///     Create a gesture detector that mutes the audio when the hand moves far away from the Leap
        /// </summary>
        public static DiscreteGestureDetector HandDownMuteMusicGesture() {
            return new DiscreteGestureDetector(hand => hand.PalmPosition.y >= 200,
                VolumeUtil.Mute);
        }
    }
}