namespace LeapMedia {
    /// <summary>
    ///     Generic interface for a gesture detector
    /// </summary>
    internal interface IGestureDetector {
        void OnHand(HandStats hand, long timestamp);
    }
}