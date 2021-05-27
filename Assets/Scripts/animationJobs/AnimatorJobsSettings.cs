using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

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
        public TwoBoneIKJobSettings TwoBoneIKSettings;
        public DampingJobSettings DampingJobSettings;
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

    [System.Serializable]
    public struct TwoBoneIKJobSettings {
        public Transform EndJoint;
        public GameObject EffectorModel;
    }

    [System.Serializable]
    public struct DampingJobSettings {
        public Transform[] Joints;
        public GameObject EffectorModel;
    }
    public struct DampingJobData {
        public AnimationScriptPlayable ScriptPlayable;
        public NativeArray<TransformStreamHandle> Handles;
        public NativeArray<Vector3> LocalPositions;
        public NativeArray<Quaternion> LocalRotations;
        public NativeArray<Vector3> Positions;
        public NativeArray<Vector3> Velocities;

        public void  DisposeData() {
            Handles.Dispose();
            LocalPositions.Dispose();
            LocalRotations.Dispose();
            Positions.Dispose();
            Velocities.Dispose();

        }
    }

}
