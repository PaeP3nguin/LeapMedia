using System;
using System.Collections.Generic;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Leap;
using NAudio.Wave;

namespace LeapMagic {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const int MIN_ACTION_DEBOUNCE = 1000 * 1000;
        private readonly WaveOut beepUp;
        private readonly List<IController> controllers;
        private readonly WaveFileReader waveFileReader;

        private int currentHand;
        private long lastActionTime;

        public MainWindow() {
            InitializeComponent();

            var icon = (TaskbarIcon) FindResource("TaskbarIcon");
            icon.TrayLeftMouseUp += Icon_TrayLeftMouseUp;
            Closing += delegate { icon.Dispose(); };

            controllers = new List<IController> {new PlayPauseController(), new SkipController()};

            beepUp = new WaveOut();
            waveFileReader = new WaveFileReader(Properties.Resources.beep_up);
            beepUp.Init(waveFileReader);

            new Controller().FrameReady += frameHandler;
        }

        private void Icon_TrayLeftMouseUp(object sender, RoutedEventArgs e) {
            Show();
            WindowState = WindowState.Normal;
        }

        protected override void OnStateChanged(EventArgs e) {
            if (WindowState == WindowState.Minimized) Hide();

            base.OnStateChanged(e);
        }

        private void frameHandler(object sender, FrameEventArgs eventArgs) {
            Frame frame = eventArgs.frame;
            // Only watch for one-handed gestures
            if (frame.Hands.Count != 1) return;

            var hand = new HandStats(frame.Hands[0]);

            HandInfo.Text = hand.ToString();
            Confidence.Text = "Confidence: " + hand.Confidence;
            TimeVisible.Text = "Time visible: " + hand.TimeVisible;
            PinchStrength.Text = "Pinch strength: " + hand.PinchStrength;
            PinchDistance.Text = "Pinch distance: " + hand.PinchDistance;
            GrabStrength.Text = "Grab strength: " + hand.GrabStrength;
            PalmPosition.Text = $"Palm position: {hand.PalmPosition}";
            HandYaw.Text = $"Hand yaw: {hand.Direction.Yaw}";
            HandPitch.Text = $"Hand pitch: {hand.Direction.Pitch}";
            HandRoll.Text = $"Hand roll: {hand.PalmNormal.Roll}";

            FingerSpread.Text = "Finger spread: " + hand.AngleSum;
            HandOpen.Text = $"Hand open: {hand.IsOpen}";
            HandDirection.Text = $"Hand direction: {hand.Pointing}";
            CurrentTime.Text = $"Current time: {frame.Timestamp}";
            NextActionTime.Text = $"Next action time: {lastActionTime + MIN_ACTION_DEBOUNCE}";
            SameHand.Text = $"Same hand: {hand.Id == currentHand}";
            HandInBounds.Text = $"In bounds: {hand.IsInBounds}";
            UsingMouse.Text = $"Using mouse: {hand.IsUsingMouse}";

            // Only using right hand
//            if (hand.IsLeft) return;

            if (!hand.IsInBounds) return;
            if (hand.IsUsingMouse) return;

            if (hand.Id != currentHand) {
                // Play a sound to indicate a new hand was detected
                waveFileReader.Position = 0;
                beepUp.Play();
            }

            foreach (var controller in controllers) {
                controller.OnHand(hand, frame.Timestamp);
            }

            currentHand = hand.Id;
        }
    }
}