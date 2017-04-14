using Leap;

namespace LeapMagic {
    internal interface IController {
        void OnHand(HandStats hand, long timestamp);
    }
}