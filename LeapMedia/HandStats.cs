using System;
using Leap;

namespace LeapMedia {
    public class HandStats : Hand {
        public enum PointingDirection {
            Left,
            Center,
            Right
        }

        private const float MIN_PINCH_DIST = 35;
        private const float MAX_PINCH_STRENGTH = 0.5f;
        private const float MAX_GRAB_STRENGTH = 0;

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
            Roll = hand.PalmNormal.Roll;
            Pitch = hand.Direction.Pitch;
            Yaw = hand.Direction.Yaw;
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

            if (hand.Direction.Yaw >= 0.5) {
                Pointing = PointingDirection.Left;
            } else if (hand.Direction.Yaw <= -0.4) {
                Pointing = PointingDirection.Right;
            } else {
                Pointing = PointingDirection.Center;
            }
        }

        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public new bool IsRight => !IsLeft;
        public float AngleSum { get; }
        public bool IsOpen { get; set; }
        public bool IsInBounds { get; }
        public bool IsUsingMouse { get; }
        public PointingDirection Pointing { get; set; }
    }
}