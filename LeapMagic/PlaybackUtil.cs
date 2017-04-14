using System;
using System.Runtime.InteropServices;

namespace LeapMagic {
    public class PlaybackUtil {
        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int NEXT_TRACK = 0xB0; // code to jump to next track
        private const int PLAY_PAUSE = 0xB3; // code to play or pause a song
        private const int PREV_TRACK = 0xB1; // code to jump to prev track

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        public static void ToggleMusic() {
            keybd_event(PLAY_PAUSE, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
        }

        public static void PreviousTrack() {
            keybd_event(PREV_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
        }

        public static void NextTrack() {
            keybd_event(NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
        }
    }
}
