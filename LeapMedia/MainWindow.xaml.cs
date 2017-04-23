using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Leap;
using LeapMedia.Gestures;
using Frame = Leap.Frame;

namespace LeapMedia {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly AudioController audioController;
        private readonly List<IGestureDetector> gestureDetectors;
        private readonly Controller leapController;
        private TaskbarIcon taskbarIcon;

        private int currentHand;
        private bool isTracking;
        private MenuItem toggleMenuItem;

        public MainWindow() {
            InitializeComponent();
            InitializeTaskbarIcon();

            gestureDetectors = new List<IGestureDetector> {
                DiscreteGestureDetector.OpenHandToggleMusicGesture(),
                DiscreteGestureDetector.HandLeftPrevTrackGesture(),
                DiscreteGestureDetector.HandRightNextTrackGesture(),
                ContinuousGestureDetector.RotateHandChangeVolumeGesture()
            };

            audioController = new AudioController();
            leapController = new Controller();

            StartTracking();

            // I hate this. Why don't the events work?
            while (!leapController.IsConnected) {
                Thread.Sleep(100);
            }
            Debug.WriteLine("Connected");

            leapController.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);
            leapController.SetPolicy(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);
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
            leapController.FrameReady += frameHandler;
        }

        private void StopTracking() {
            isTracking = false;
            leapController.FrameReady -= frameHandler;
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
                HandYaw.Text = $"Hand yaw: {hand.Yaw}";
                HandPitch.Text = $"Hand pitch: {hand.Pitch}";
                HandRoll.Text = $"Hand roll: {hand.Roll}";

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
            if (!hand.IsInBounds || hand.IsUsingMouse) {
                currentHand = 0;
                return;
            }

            if (hand.Id != currentHand) {
                // Play a sound to indicate a new hand was detected
                audioController.PlayBeepUp();
            }

            foreach (var detector in gestureDetectors) {
                detector.OnHand(hand, frame.Timestamp);
            }

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