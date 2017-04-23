using WindowsInput.Native;
using AudioSwitcher.AudioApi.CoreAudio;

namespace LeapMedia.Util {
    internal class VolumeUtil {
        private readonly CoreAudioDevice playbackDevice;

        public VolumeUtil() {
            playbackDevice = new CoreAudioController().DefaultPlaybackDevice;
        }

        public static void VolumeUp() {
            App.InputSimulator.Keyboard.KeyPress(VirtualKeyCode.VOLUME_UP);
        }

        public static void VolumeDown() {
            App.InputSimulator.Keyboard.KeyPress(VirtualKeyCode.VOLUME_DOWN);
        }

        public static void Mute() {
            App.InputSimulator.Keyboard.KeyPress(VirtualKeyCode.VOLUME_MUTE);
        }

        public void VolumeUp(int increment) {
            playbackDevice.Volume += increment;
        }

        public void VolumeDown(int increment) {
            playbackDevice.Volume -= increment;
        }

        public void SetVolume(int volume) {
            playbackDevice.Volume = volume;
        }
    }
}