using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Animations;
#else
using UnityEngine.Experimental.Animations;
#endif
namespace animationJobs {
    public struct DampingJob : IAnimationJob {
        public TransformStreamHandle rootHandle;
        public NativeArray<TransformStreamHandle> jointHandles;
        public NativeArray<Vector3> localPositions;
        public NativeArray<Quaternion> localRotations;
        public NativeArray<Vector3> positions;
        public NativeArray<Vector3> velocities;


        public void ProcessRootMotion(AnimationStream stream) {
            var rootPosition = rootHandle.GetPosition(stream);
            var rootRotation = rootHandle.GetRotation(stream);

            rootHandle.SetPosition(stream, rootPosition);
            rootHandle.SetRotation(stream, rootRotation);
        }

        public void ProcessAnimation(AnimationStream stream) {
            if (jointHandles.Length < 2)
                return;

            ComputeDampedPositions(stream);
            ComputeJointLocalRotations(stream);
        }

        private void ComputeDampedPositions(AnimationStream stream) {
            var rootPosition = rootHandle.GetPosition(stream);
            var rootRotation = rootHandle.GetRotation(stream);
            var parentPosition = rootPosition + rootRotation * localPositions[0];
            var parentRotation = rootRotation * localRotations[0];
            positions[0] = parentPosition;
            for (var i = 1; i < jointHandles.Length; ++i) {
                var newPosition = parentPosition + (parentRotation * localPositions[i]);
                var velocity = velocities[i];
                newPosition = Vector3.SmoothDamp(positions[i], newPosition,
                    ref velocity, 0.15f, Mathf.Infinity, stream.deltaTime);

                newPosition = parentPosition +
                    (newPosition - parentPosition).normalized * localPositions[i].magnitude;

                velocities[i] = velocity;
                positions[i] = newPosition;

                parentPosition = newPosition;
                parentRotation = parentRotation * localRotations[i];
            }
        }

        private void ComputeJointLocalRotations(AnimationStream stream) {
            var parentRotation = rootHandle.GetRotation(stream);
            for (var i = 0; i < jointHandles.Length - 1; ++i) {

                var rotation = parentRotation * localRotations[i];

                var direction = (rotation * localPositions[i + 1]).normalized;

                var newDirection = (positions[i + 1] - positions[i]).normalized;

                var currentToNewRotation = Quaternion.FromToRotation(direction, newDirection);

                rotation = currentToNewRotation * rotation;

                var newLocalRotation = Quaternion.Inverse(parentRotation) * rotation;
                jointHandles[i].SetLocalRotation(stream, newLocalRotation);

                parentRotation = rotation;
            }
        }
    }
    public struct DampingJobTemp : IJobTemp{
        public NativeArray<TransformStreamHandle> Handles;
        public NativeArray<Vector3> LocalPositions;
        public NativeArray<Quaternion> LocalRotations;
        public NativeArray<Vector3> Positions;
        public NativeArray<Vector3> Velocities;

        public void DisposeData() {
            Handles.Dispose();
            LocalPositions.Dispose();
            LocalRotations.Dispose();
            Positions.Dispose();
            Velocities.Dispose();
        }
    }
}

