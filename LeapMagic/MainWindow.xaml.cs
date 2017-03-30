using Leap;
using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;

namespace LeapMagic {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private const float MIN_PINCH_DIST = 60;
        private const float MAX_PINCH_STRENGTH = 0;
        private const float MAX_GRAB_STRENGTH = 0;
        private const int MIN_ACTION_DEBOUNCE = 1000 * 1000;

        private TaskbarIcon icon;

        private Controller controller = new Controller();
        private int currentHand;
        private long lastActionTime;

        public MainWindow() {
            InitializeComponent();

            icon = (TaskbarIcon) FindResource("taskbarIcon");
            icon.TrayLeftMouseUp += Icon_TrayLeftMouseUp;

            Closing += delegate { icon.Dispose(); };

            controller.FrameReady += frameHandler;
        }

        private void Icon_TrayLeftMouseUp(object sender, RoutedEventArgs e) {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        protected override void OnStateChanged(EventArgs e) {
            if (WindowState == WindowState.Minimized) {
                this.Hide();
            }

            base.OnStateChanged(e);
        }

        public Hand CurrentHand { get; set; }

        void frameHandler(object sender, FrameEventArgs eventArgs) {
            Frame frame = eventArgs.frame;
            // Only watch for one-handed gestures
            if (frame.Hands.Count != 1) return;

            Hand hand = frame.Hands[0];
            // Only using right hand
            if (hand.IsLeft) return;
            CurrentHand = hand;

            handInfo.Text = hand.ToString();
            pinchStrength.Text = "Pinch strength: " + hand.PinchStrength;
            pinchDistance.Text = "Pinch distance: " + hand.PinchDistance;
            grabStrength.Text = "Grab strength: " + hand.GrabStrength;
            confidence.Text = "Confidence: " + hand.Confidence;
            timeVisible.Text = "Time visible: " + hand.TimeVisible;
            // TODO: Use position to extablish bounding box
            palmPosition.Text = $"Palm position: {hand.PalmPosition}";
            handYaw.Text = $"Hand yaw: {hand.Direction.Yaw}";
            handPitch.Text = $"Hand pitch: {hand.Direction.Pitch}";
            handRoll.Text = $"Hand roll: {hand.PalmNormal.Roll}";

            float angleSum = 0f;
            Leap.Vector lastAngle = hand.Fingers[0].Direction;
            foreach (Finger finger in hand.Fingers) {
                angleSum += finger.Direction.AngleTo(lastAngle);
                lastAngle = finger.Direction;
            }

            fingerSpread.Text = "Finger spread: " + angleSum;
            bool isHandOpen = angleSum >= 1;
            handOpen.Text = $"Hand open: {isHandOpen}";
            currentTime.Text = $"Current time: {frame.Timestamp}";
            nextActionTime.Text = $"Next action time: {lastActionTime + MIN_ACTION_DEBOUNCE}";
            sameHand.Text = $"Same hand: {hand.Id == currentHand}";

            bool outOfBounds = Math.Abs(hand.PalmPosition.x) >= 100 || Math.Abs(hand.PalmPosition.z) >= 100;
            handInBounds.Text = $"Out of bounds: {outOfBounds}";

            if (outOfBounds) return;

            if (hand.Id == currentHand) {
                // Only accept continuing gestures with closed hand
                if (isHandOpen) return;
                if (hand.Direction.Yaw >= 0.6) {
                    if (frame.Timestamp <= lastActionTime + MIN_ACTION_DEBOUNCE) return;
                    MediaController.previousTrack();
                    lastActionTime = frame.Timestamp;
                    lastActionText.Text = "prev";
                }
                if (hand.Direction.Yaw <= -0.4) {
                    if (frame.Timestamp <= lastActionTime + MIN_ACTION_DEBOUNCE) return;
                    MediaController.nextTrack();
                    lastActionTime = frame.Timestamp;
                    lastActionText.Text = "next";
                }
            } else {
                currentHand = hand.Id;
                // Ignore one-time gestures with closed hand
                if (!isHandOpen) return;
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
                lastActionText.Text = "pause";
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
