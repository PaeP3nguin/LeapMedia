using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Leap;
using NAudio.Wave;
using Frame = Leap.Frame;

namespace LeapMedia {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly WaveOut beepUp;
        private readonly List<GestureDetector> gestureDetectors;
        private readonly WaveFileReader waveFileReader;
        private readonly Controller controller;
        private readonly ScrubDetector scrubDetector;
        private TaskbarIcon taskbarIcon;

        private int currentHand;
        private bool isTracking;
        private MenuItem toggleMenuItem;

        public MainWindow() {
            InitializeComponent();
            InitializeTaskbarIcon();

            gestureDetectors = new List<GestureDetector> {
                new GestureDetector(hand => hand.IsOpen,
                    PlaybackUtil.ToggleMusic),
                new GestureDetector(hand => hand.Pointing == HandStats.PointingDirection.Left,
                    PlaybackUtil.PreviousTrack),
                new GestureDetector(hand => hand.Pointing == HandStats.PointingDirection.Right,
                    PlaybackUtil.NextTrack),
                new GestureDetector(hand => hand.PalmPosition.y >= 200,
                    VolumeController.Mute)
            };

            scrubDetector = new ScrubDetector();

            beepUp = new WaveOut {
                DesiredLatency = 700,
                NumberOfBuffers = 3
            };
            waveFileReader = new WaveFileReader(Properties.Resources.beep_up);
            beepUp.Init(waveFileReader);

            controller = new Controller();
            StartTracking();

            // I hate this. Why don't the events work?
            while (!controller.IsConnected) {
                Thread.Sleep(100);
            }
            Debug.WriteLine("Connected");

            controller.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
        }

        private void InitializeTaskbarIcon() {
            taskbarIcon = (TaskbarIcon) FindResource("TaskbarIcon");
            toggleMenuItem = (MenuItem) taskbarIcon.ContextMenu?.Items[0];
        }

        private void TaskbarIconClick(object sender, RoutedEventArgs e) {
            Show();
            WindowState = WindowState.Normal;
        }

        protected override void OnStateChanged(EventArgs e) {
            if (WindowState == WindowState.Minimized) Hide();

            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e) {
            taskbarIcon.Dispose();
            base.OnClosing(e);
        }

        private void StartTracking() {
            isTracking = true;
            controller.FrameReady += frameHandler;
        }

        private void StopTracking() {
            isTracking = false;
            controller.FrameReady -= frameHandler;
        }

        private void frameHandler(object sender, FrameEventArgs eventArgs) {
            Frame frame = eventArgs.frame;
            // Only watch for one-handed gestures
            if (frame.Hands.Count != 1) return;

            var hand = new HandStats(frame.Hands[0]);

            if (WindowState != WindowState.Minimized) {
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
                SameHand.Text = $"Same hand: {hand.Id == currentHand}";
                HandInBounds.Text = $"In bounds: {hand.IsInBounds}";
                UsingMouse.Text = $"Using mouse: {hand.IsUsingMouse}";
            }

            // Only use right hand
            if (hand.IsLeft) return;
            if (!hand.IsInBounds) return;
            if (hand.IsUsingMouse) return;

            if (hand.Id != currentHand) {
                // Play a sound to indicate a new hand was detected
                waveFileReader.Position = 0;
                beepUp.Play();
            }

            foreach (var detector in gestureDetectors) {
                detector.OnHand(hand, frame.Timestamp);
            }

            scrubDetector.OnHand(hand, frame.Timestamp);

            currentHand = hand.Id;
        }

        private void ToggleTracking(object sender, RoutedEventArgs e) {
            if (isTracking) {
                StopTracking();
                toggleMenuItem.Header = "Resume";
                ToggleButton.Content = "Resume";
            } else {
                StartTracking();
                toggleMenuItem.Header = "Pause";
                ToggleButton.Content = "Pause";
            }
        }

        private void Quit(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
    }
}