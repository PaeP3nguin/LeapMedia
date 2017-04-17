using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AudioSwitcher.AudioApi.CoreAudio;

namespace LeapMedia {
    internal class VolumeController {
        private const int KEYEVENTF_EXTENDEDKEY = 1;

        private readonly CoreAudioDevice playbackDevice;

        public VolumeController() {
            playbackDevice = new CoreAudioController().DefaultPlaybackDevice;
        }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

        public static void VolumeUp() {
            keybd_event((byte) Keys.VolumeUp, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
        }

        public static void VolumeDown() {
            keybd_event((byte) Keys.VolumeDown, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
        }

        public static void Mute() {
            keybd_event((byte) Keys.VolumeMute, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
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