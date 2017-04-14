using Leap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using NAudio.Wave;
using Vector = Leap.Vector;

namespace LeapMagic {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const int MIN_ACTION_DEBOUNCE = 1000 * 1000;
        
        private int currentHand;
        private long lastActionTime;
        private List<IController> controllers;
        private WaveOut audioOut;

        public MainWindow() {
            InitializeComponent();

            TaskbarIcon icon = (TaskbarIcon) FindResource("TaskbarIcon");
            icon.TrayLeftMouseUp += Icon_TrayLeftMouseUp;
            Closing += delegate { icon.Dispose(); };

            controllers = new List<IController> { new PlayPauseController() };

            audioOut = new WaveOut();
            audioOut.Init(new WaveFileReader(Properties.Resources.beep_up));

            new Controller().FrameReady += frameHandler;
        }

        private void Icon_TrayLeftMouseUp(object sender, RoutedEventArgs e) {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        protected override void OnStateChanged(EventArgs e) {
            if (WindowState == WindowState.Minimized) {
                this.Hide();
            }

            base.OnStateChanged(e);
        }

        private void frameHandler(object sender, FrameEventArgs eventArgs) {
            Frame frame = eventArgs.frame;
            // Only watch for one-handed gestures
            if (frame.Hands.Count != 1) return;

            HandStats hand = new HandStats(frame.Hands[0]);

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
            CurrentTime.Text = $"Current time: {frame.Timestamp}";
            NextActionTime.Text = $"Next action time: {lastActionTime + MIN_ACTION_DEBOUNCE}";
            SameHand.Text = $"Same hand: {hand.Id == currentHand}";
            
            HandInBounds.Text = $"In bounds: {hand.IsInBounds}";
            UsingMouse.Text = $"Using mouse: {hand.IsUsingMouse}";

            // Only using right hand
            if (hand.IsLeft) return;

            if (!hand.IsInBounds) return;
            if (hand.IsUsingMouse) return;

            foreach (var controller in controllers) {
                controller.OnHand(hand, frame.Timestamp);
            }

            if (hand.Id == currentHand) {
                // Only accept continuing gestures with closed hand
                if (hand.IsOpen) return;
                if (hand.Direction.Yaw >= 0.6) {
                    if (frame.Timestamp <= lastActionTime + MIN_ACTION_DEBOUNCE) return;
                    PlaybackUtil.PreviousTrack();
                    lastActionTime = frame.Timestamp;
                    LastActionText.Text = "prev";
                }
                if (hand.Direction.Yaw <= -0.4) {
                    if (frame.Timestamp <= lastActionTime + MIN_ACTION_DEBOUNCE) return;
                    PlaybackUtil.NextTrack();
                    lastActionTime = frame.Timestamp;
                    LastActionText.Text = "next";
                }
            } else {
                // Play a sound to indicate a new hand was detected
                audioOut.Play();
            }

            currentHand = hand.Id;
        }
    }

    public class HandStats : Hand {
        private const float MIN_PINCH_DIST = 45;
        private const float MAX_PINCH_STRENGTH = 0;
        private const float MAX_GRAB_STRENGTH = 0;

        public new bool IsRight => !IsLeft;
        public float AngleSum { get; }
        public bool IsOpen { get; set; }
        public bool IsInBounds { get; }
        public bool IsUsingMouse { get; }

        public HandStats(Hand hand) {
            Arm = hand.Arm;
            Confidence = hand.Confidence;
            Direction = hand.Direction;
            Fingers = hand.Fingers;
            FrameId = hand.FrameId;
            GrabAngle = hand.GrabAngle;
            GrabStrength = hand.GrabStrength;
            Id = hand.Id;
            IsLeft = hand.IsLeft;
            PalmNormal = hand.PalmNormal;
            PalmPosition = hand.PalmPosition;
            PalmVelocity = hand.PalmVelocity;
            PalmWidth = hand.PalmWidth;
            PinchDistance = hand.PinchDistance;
            PinchStrength = hand.PinchStrength;
            Rotation = hand.Rotation;
            StabilizedPalmPosition = hand.StabilizedPalmPosition;
            TimeVisible = hand.TimeVisible;
            WristPosition = hand.WristPosition;

            AngleSum = 0;

            Vector middle = hand.Fingers[2].Direction;
            for (var i = 0; i < hand.Fingers.Count; i++) {
                if (i == 2) continue;
                Vector finger = hand.Fingers[i].Direction;
                double angle = Math.Atan2(middle.y, middle.x) - Math.Atan2(finger.y, finger.x);
                if (hand.IsLeft) {
                    if (i < 2) {
                        AngleSum -= (float) angle;
                    } else {
                        AngleSum += (float) angle;
                    }
                } else {
                    if (i < 2) {
                        AngleSum += (float) angle;
                    } else {
                        AngleSum -= (float) angle;
                    }
                }
            }

            IsOpen = AngleSum >= 2;

            IsInBounds = Math.Abs(hand.PalmPosition.x) <= 100 && Math.Abs(hand.PalmPosition.z) <= 100;

            IsUsingMouse = hand.PinchDistance <= MIN_PINCH_DIST
                           || hand.PinchStrength > MAX_PINCH_STRENGTH
                           || hand.GrabStrength > MAX_GRAB_STRENGTH;
        }
    }
}
