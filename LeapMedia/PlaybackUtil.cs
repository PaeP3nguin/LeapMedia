using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LeapMedia {
    public class PlaybackUtil {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        public static void ToggleMusic() {
            keybd_event((byte) Keys.MediaPlayPause, 0, 0, IntPtr.Zero);
        }

        public static void PreviousTrack() {
            keybd_event((byte) Keys.MediaPreviousTrack, 0, 0, IntPtr.Zero);
        }

        public static void NextTrack() {
            keybd_event((byte) Keys.MediaNextTrack, 0, 0, IntPtr.Zero);
        }
    }
}