using Leap;
using System;
using System.Windows;
using System.Runtime.InteropServices;

namespace LeapMagic {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private const float MIN_PINCH_DIST = 60;
        private const float MAX_PINCH_STRENGTH = 0f;
        private const float MAX_GRAB_STRENGTH = 0f;
        private const int MIN_ACTION_DEBOUNCE = 1000 * 1000;

        private Controller controller = new Controller();
        private int currentHand;
        private long lastActionTime;

        public MainWindow() {
            InitializeComponent();

            controller.FrameReady += frameHandler;
        }

        public Hand CurrentHand { get; set; }

        void frameHandler(object sender, FrameEventArgs eventArgs) {
            Frame frame = eventArgs.frame;
            // Only watch for one-handed gestures
            if (frame.Hands.Count != 1) return;

            Hand hand = frame.Hands[0];
            CurrentHand = hand;

            handInfo.Text = hand.ToString();
            pinchStrength.Text = "Pinch strength: " + hand.PinchStrength;
            pinchDistance.Text = "Pinch distance: " + hand.PinchDistance;
            grabStrength.Text = "Grab strength: " + hand.GrabStrength;
            confidence.Text = "Confidence: " + hand.Confidence;
            timeVisible.Text = "Time visible: " + hand.TimeVisible;
            palmPosition.Text = "Palm position: " + hand.PalmPosition.ToString();

            if (hand.Id == currentHand) {

            } else {
                // Ignore hands that have not been visible for long
                // if (hand.TimeVisible <= 500 * 1000) return;
                // Ignore the hand if moving a mouse
                if (hand.PinchDistance <= MIN_PINCH_DIST
                    || hand.PinchStrength > MAX_PINCH_STRENGTH
                    || hand.GrabStrength > MAX_GRAB_STRENGTH) return;
                // Another heuristic to ignore mouse/keyboard usage
                if (Math.Abs(hand.PalmPosition.x) >= 100 || Math.Abs(hand.PalmPosition.z) >= 100) return;
                // Only toggle music at most once every second
                if (frame.Timestamp <= lastActionTime + MIN_ACTION_DEBOUNCE) return;
                MediaController.toggleMusic();
                lastActionTime = frame.Timestamp;
                currentHand = hand.Id;
            }
        }
    }

    public class MediaController {
        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int NEXT_TRACK = 0xB0; // code to jump to next track
        private const int PLAY_PAUSE = 0xB3; // code to play or pause a song
        private const int PREV_TRACK = 0xB1; // code to jump to prev track

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        public static void toggleMusic() {
            keybd_event(PLAY_PAUSE, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
        }

        public static void previousTrack() {
            keybd_event(PREV_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
        }

        public static void nextTrack() {
            keybd_event(NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
        }
    }
}
