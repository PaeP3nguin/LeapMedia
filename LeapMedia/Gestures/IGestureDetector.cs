namespace LeapMedia.Gestures {
    /// <summary>
    ///     Generic interface for a gesture detector
    /// </summary>
    internal interface IGestureDetector {
        /// <summary>
        ///     Call this function whenever a valid hand is found in a frame that you want to detect gestures on
        /// </summary>
        /// <param name="hand">Hand to detect gestures on</param>
        /// <param name="timestamp">Timestamp of the frame, in microseconds</param>
        void OnHand(HandStats hand, long timestamp);
    }
}