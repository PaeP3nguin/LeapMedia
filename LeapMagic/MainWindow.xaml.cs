using Leap;
using System;
using System.Windows;
using System.Runtime.InteropServices;

namespace LeapMagic {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private Controller controller = new Controller();
        private int currentHand;
        private long lastActionTime;

        public MainWindow() {
            InitializeComponent();

            controller.FrameReady += frameHandler;
        }

        void frameHandler(object sender, FrameEventArgs eventArgs) {
            Frame frame = eventArgs.frame;
            if (frame.Hands.Count != 1) {
                // Only watch for one-handed gestures
                return;
            }
            Hand hand = frame.Hands[0];
            if (hand.Id == currentHand) {

            } else {
                // Only toggle music at most once every second
                if (frame.Timestamp <= lastActionTime + 1000 * 1000) return;                
                MediaController.toggleMusic();
                lastActionTime = frame.Timestamp;
                currentHand = hand.Id;
            }
        }
    }

    public class MediaController {
        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int NEXT_TRACK = 0xB0;// code to jump to next track
        private const int PLAY_PAUSE = 0xB3;// code to play or pause a song
        private const int PREV_TRACK = 0xB1;// code to jump to prev track

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
