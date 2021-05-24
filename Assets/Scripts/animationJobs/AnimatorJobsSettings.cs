using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Axis {
    Forward,
    Back,
    Up,
    Down,
    Left,
    Right
}

namespace animationJobs {
    [System.Serializable]
    public struct AnimatorJobsSettings {
        public LookAtJobSettings LookAtSettings;
    }
    [System.Serializable]
    public struct LookAtJobSettings {
        public Transform Joint;
        public Axis Axis;
        public GameObject Target;
        public float MinAngle;
        public float MaxAngle;

        public Vector3 GetAxisVector(Axis axis) {
            switch (axis) {
                case Axis.Forward:
                    return Vector3.forward;
                case Axis.Back:
                    return Vector3.back;
                case Axis.Up:
                    return Vector3.up;
                case Axis.Down:
                    return Vector3.down;
                case Axis.Left:
                    return Vector3.left;
                case Axis.Right:
                    return Vector3.right;
            }

            return Vector3.forward;
        }

    }

}
