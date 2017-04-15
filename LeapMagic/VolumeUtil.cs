using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Observables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioSwitcher.AudioApi;

namespace LeapMedia {
    class VolumeUtil {
        CoreAudioDevice playbackDevice;

        public VolumeUtil() {
            playbackDevice = new CoreAudioController().DefaultPlaybackDevice;
            playbackDevice.Volume = 80;
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
