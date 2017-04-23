using WindowsInput.Native;

namespace LeapMedia.Util {
    public class PlaybackUtil {
        public static void ToggleMusic() {
            App.InputSimulator.Keyboard.KeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE);
        }

        public static void PreviousTrack() {
            App.InputSimulator.Keyboard.KeyPress(VirtualKeyCode.MEDIA_PREV_TRACK);
        }

        public static void NextTrack() {
            App.InputSimulator.Keyboard.KeyPress(VirtualKeyCode.MEDIA_NEXT_TRACK);
        }
    }
}