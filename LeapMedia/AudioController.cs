using LeapMedia.Properties;
using NAudio.Wave;

namespace LeapMedia {
    internal class AudioController {
        private readonly IWavePlayer audioPlayer;
        private readonly WaveFileReader beepUp;

        public AudioController() {
            audioPlayer = new WaveOut {
                DesiredLatency = 700,
                NumberOfBuffers = 3,
                DeviceNumber = -1
            };
            beepUp = new WaveFileReader(Resources.beep_up);
            audioPlayer.Init(beepUp);
        }

        public void PlayBeepUp() {
            beepUp.Position = 0;
            audioPlayer.Play();
        }
    }
}