using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.Hands.Samples.VisualizerSample
{
    public class HandVisualizer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("If this is enabled, this component will enable the Input System internal feature flag 'USE_OPTIMIZED_CONTROLS'. You must have at least version 1.5.0 of the Input System and have its backend enabled for this to take effect.")]
        bool m_UseOptimizedControls = true;

        [SerializeField]
        XROrigin m_Origin;

        [SerializeField]
        GameObject m_LeftHandMesh;

        [SerializeField]
        GameObject m_RightHandMesh;

        [SerializeField]
        Material m_HandMeshMaterial;

        public bool drawMeshes
        {
            get => m_DrawMeshes;
            set
            {
                m_DrawMeshes = value;

                if (m_LeftHandGameObjects != null)
                    m_LeftHandGameObjects.ToggleDrawMesh(value);

                if (m_RightHandGameObjects != null)
                    m_RightHandGameObjects.ToggleDrawMesh(value);
            }
        }
        [SerializeField]
        bool m_DrawMeshes;

        [SerializeField]
        GameObject m_DebugDrawPrefab;

        public bool debugDrawJoints
        {
            get => m_DebugDrawJoints;
            set
            {
                m_DebugDrawJoints = value;

                if (m_LeftHandGameObjects != null)
                    m_LeftHandGameObjects.ToggleDebugDrawJoints(value);

                if (m_RightHandGameObjects != null)
                    m_RightHandGameObjects.ToggleDebugDrawJoints(value);
            }
        }
        [SerializeField]
        bool m_DebugDrawJoints;

        [SerializeField]
        GameObject m_VelocityPrefab;

        public enum VelocityType
        {
            Linear,
            Angular,
            None
        }
        public VelocityType velocityType
        {
            get => m_VelocityType;
            set
            {
                m_VelocityType = value;

                if (m_LeftHandGameObjects != null)
                    m_LeftHandGameObjects.SetVelocityType(value);

                if (m_RightHandGameObjects != null)
                    m_RightHandGameObjects.SetVelocityType(value);
            }
        }
        [SerializeField]
        VelocityType m_VelocityType;

        void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            if (m_UseOptimizedControls)
                InputSystem.InputSystem.settings.SetInternalFeatureFlag("USE_OPTIMIZED_CONTROLS", true);
#endif // ENABLE_INPUT_SYSTEM
        }

        void Update() => TryEnsureInitialized();

        void OnEnable()
        {
            m_LeftHandGameObjects?.OnEnable();
            m_RightHandGameObjects?.OnEnable();
        }

        void OnDisable()
        {
            if (m_Subsystem != null)
            {
                m_Subsystem.trackingAcquired -= OnTrackingAcquired;
                m_Subsystem.trackingLost -= OnTrackingLost;
                m_Subsystem.updatedHands -= OnUpdatedHands;
                m_Subsystem = null;
            }

            m_LeftHandGameObjects?.OnDisable();
            m_RightHandGameObjects?.OnDisable();
        }

        void OnDestroy()
        {
            if (m_LeftHandGameObjects != null)
            {
                m_LeftHandGameObjects.OnDestroy();
                m_LeftHandGameObjects = null;
            }

            if (m_RightHandGameObjects != null)
            {
                m_RightHandGameObjects.OnDestroy();
                m_RightHandGameObjects = null;
            }
        }

        bool TryEnsureInitialized()
        {
            if (m_Subsystem != null)
                return true;

            m_Subsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRHandSubsystem>();
            if (m_Subsystem == null)
                return false;

            if (m_LeftHandGameObjects == null)
            {
                var leftHandTracked = m_Subsystem.leftHand.isTracked;
                m_LeftHandGameObjects = new HandGameObjects(true, transform, m_LeftHandMesh, m_HandMeshMaterial, m_DebugDrawPrefab, m_VelocityPrefab);
                m_LeftHandGameObjects.ForceToggleDrawMesh(m_DrawMeshes && leftHandTracked);
                m_LeftHandGameObjects.ForceToggleDebugDrawJoints(m_DebugDrawJoints && leftHandTracked);
                m_LeftHandGameObjects.ForceSetVelocityType(leftHandTracked ? m_VelocityType : VelocityType.None);
            }

            var rightHandTracked = m_Subsystem.rightHand.isTracked;
            if (m_RightHandGameObjects == null)
            {
                m_RightHandGameObjects = new HandGameObjects(false, transform, m_RightHandMesh, m_HandMeshMaterial, m_DebugDrawPrefab, m_VelocityPrefab);
                m_RightHandGameObjects.ForceToggleDrawMesh(m_DrawMeshes && rightHandTracked);
                m_RightHandGameObjects.ForceToggleDebugDrawJoints(m_DebugDrawJoints && rightHandTracked);
                m_RightHandGameObjects.ForceSetVelocityType(rightHandTracked ? m_VelocityType : VelocityType.None);
            }

            m_Subsystem.trackingAcquired += OnTrackingAcquired;
            m_Subsystem.trackingLost += OnTrackingLost;
            m_Subsystem.updatedHands += OnUpdatedHands;
            return true;
        }

        void OnTrackingAcquired(XRHand hand)
        {
            switch (hand.handedness)
            {
                case Handedness.Left:
                    m_LeftHandGameObjects.ForceToggleDrawMesh(m_DrawMeshes);
                    m_LeftHandGameObjects.ForceToggleDebugDrawJoints(m_DebugDrawJoints);
                    m_LeftHandGameObjects.ForceSetVelocityType(m_VelocityType);
                    break;

                case Handedness.Right:
                    m_RightHandGameObjects.ForceToggleDrawMesh(m_DrawMeshes);
                    m_RightHandGameObjects.ForceToggleDebugDrawJoints(m_DebugDrawJoints);
                    m_RightHandGameObjects.ForceSetVelocityType(m_VelocityType);
                    break;
            }
        }

        void OnTrackingLost(XRHand hand)
        {
            switch (hand.handedness)
            {
                case Handedness.Left:
                    m_LeftHandGameObjects.ForceToggleDrawMesh(false);
                    m_LeftHandGameObjects.ForceToggleDebugDrawJoints(false);
                    m_LeftHandGameObjects.ForceSetVelocityType(VelocityType.None);
                    break;

                case Handedness.Right:
                    m_RightHandGameObjects.ForceToggleDrawMesh(false);
                    m_RightHandGameObjects.ForceToggleDebugDrawJoints(false);
                    m_RightHandGameObjects.ForceSetVelocityType(VelocityType.None);
                    break;
            }
        }

        void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            // we have no game logic depending on the Transforms, so early out here
            // (add game logic before this return here, directly querying from
            // m_Subsystem.leftHand and m_Subsystem.rightHand using GetJoint on each hand)
            if (updateType == XRHandSubsystem.UpdateType.Dynamic)
                return;

            // account for changes in the Inspector
#if UNITY_EDITOR
            var leftHandTracked = subsystem.leftHand.isTracked;
            m_LeftHandGameObjects.ToggleDrawMesh(m_DrawMeshes && leftHandTracked);
            m_LeftHandGameObjects.ToggleDebugDrawJoints(m_DebugDrawJoints && leftHandTracked);
            m_LeftHandGameObjects.SetVelocityType(leftHandTracked ? m_VelocityType : VelocityType.None);
            
            var rightHandTracked = subsystem.rightHand.isTracked;
            m_RightHandGameObjects.ToggleDrawMesh(m_DrawMeshes && rightHandTracked);
            m_RightHandGameObjects.ToggleDebugDrawJoints(m_DebugDrawJoints && rightHandTracked);
            m_RightHandGameObjects.SetVelocityType(rightHandTracked ? m_VelocityType : VelocityType.None);
#endif

            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose) != XRHandSubsystem.UpdateSuccessFlags.None)
                m_LeftHandGameObjects.UpdateRootPose(subsystem.leftHand);

            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints) != XRHandSubsystem.UpdateSuccessFlags.None)
                m_LeftHandGameObjects.UpdateJoints(m_Origin, m_Subsystem.leftHand);

            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose) != XRHandSubsystem.UpdateSuccessFlags.None)
                m_RightHandGameObjects.UpdateRootPose(subsystem.rightHand);

            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandJoints) != XRHandSubsystem.UpdateSuccessFlags.None)
                m_RightHandGameObjects.UpdateJoints(m_Origin, m_Subsystem.rightHand);
        }

        class HandGameObjects
        {
            GameObject m_HandRoot;
            GameObject m_DrawJointsParent;

            Transform[] m_JointXforms = new Transform[XRHandJointID.EndMarker.ToIndex()];
            GameObject[] m_DrawJoints = new GameObject[XRHandJointID.EndMarker.ToIndex()];
            GameObject[] m_VelocityParents = new GameObject[XRHandJointID.EndMarker.ToIndex()];
            LineRenderer[] m_Lines = new LineRenderer[XRHandJointID.EndMarker.ToIndex()];

            bool m_DrawMesh;
            bool m_DebugDrawJoints;
            VelocityType m_VelocityType;

            Vector3[] m_LinePointsReuse = new Vector3[2];
            const float k_LineWidth = 0.005f;

            public HandGameObjects(
                bool isLeft,
                Transform parent,
                GameObject meshPrefab,
                Material meshMaterial,
                GameObject debugDrawPrefab,
                GameObject velocityPrefab)
            {
                void AssignJoint(
                    XRHandJointID jointId,
                    Transform jointXform,
                    Transform drawJointsParent)
                {
                    int jointIndex = jointId.ToIndex();
                    m_JointXforms[jointIndex] = jointXform;

                    m_DrawJoints[jointIndex] = GameObject.Instantiate(debugDrawPrefab);
                    m_DrawJoints[jointIndex].transform.parent = drawJointsParent;
                    m_DrawJoints[jointIndex].name = jointId.ToString();

                    m_VelocityParents[jointIndex] = GameObject.Instantiate(velocityPrefab);
                    m_VelocityParents[jointIndex].transform.parent = jointXform;

                    m_Lines[jointIndex] = m_DrawJoints[jointIndex].GetComponent<LineRenderer>();
                    m_Lines[jointIndex].startWidth = m_Lines[jointIndex].endWidth = k_LineWidth;
                    m_LinePointsReuse[0] = m_LinePointsReuse[1] = jointXform.position;
                    m_Lines[jointIndex].SetPositions(m_LinePointsReuse);
                }

                m_HandRoot = GameObject.Instantiate(meshPrefab);
                m_HandRoot.transform.parent = parent;
                m_HandRoot.transform.localPosition = Vector3.zero;
                m_HandRoot.transform.localRotation = Quaternion.identity;

                Transform wristRootXform = null;
                for (int childIndex = 0; childIndex < m_HandRoot.transform.childCount; ++childIndex)
                {
                    var child = m_HandRoot.transform.GetChild(childIndex);
                    if (child.gameObject.name.EndsWith(XRHandJointID.Wrist.ToString()))
                        wristRootXform = child;
                    else if (child.gameObject.name.EndsWith("Hand"))
                        child.GetComponent<SkinnedMeshRenderer>().sharedMaterial = meshMaterial;
                }

                m_DrawJointsParent = new GameObject();
                m_DrawJointsParent.transform.parent = parent;
                m_DrawJointsParent.transform.localPosition = Vector3.zero;
                m_DrawJointsParent.transform.localRotation = Quaternion.identity;
                m_DrawJointsParent.name = (isLeft ? "Left" : "Right") + "HandDebugDrawJoints";

                AssignJoint(XRHandJointID.Wrist, wristRootXform, m_DrawJointsParent.transform);
                for (int childIndex = 0; childIndex < wristRootXform.childCount; ++childIndex)
                {
                    var child = wristRootXform.GetChild(childIndex);

                    if (child.name.EndsWith(XRHandJointID.Palm.ToString()))
                    {
                        AssignJoint(XRHandJointID.Palm, child, m_DrawJointsParent.transform);
                        continue;
                    }

                    for (int fingerIndex = (int)XRHandFingerID.Thumb;
                        fingerIndex <= (int)XRHandFingerID.Little;
                        ++fingerIndex)
                    {
                        var fingerId = (XRHandFingerID)fingerIndex;

                        var jointIdFront = fingerId.GetFrontJointID();
                        if (!child.name.EndsWith(jointIdFront.ToString()))
                            continue;

                        AssignJoint(jointIdFront, child, m_DrawJointsParent.transform);
                        var lastChild = child;

                        int jointIndexBack = fingerId.GetBackJointID().ToIndex();
                        for (int jointIndex = jointIdFront.ToIndex() + 1;
                            jointIndex <= jointIndexBack;
                            ++jointIndex)
                        {
                            Transform nextChild = null;
                            for (int nextChildIndex = 0; nextChildIndex < lastChild.childCount; ++nextChildIndex)
                            {
                                nextChild = lastChild.GetChild(nextChildIndex);
                                if (nextChild.name.EndsWith(XRHandJointIDUtility.FromIndex(jointIndex).ToString()))
                                {
                                    lastChild = nextChild;
                                    break;
                                }
                            }

                            if (!lastChild.name.EndsWith(XRHandJointIDUtility.FromIndex(jointIndex).ToString()))
                                throw new InvalidOperationException("Hand transform hierarchy not set correctly - couldn't find " + XRHandJointIDUtility.FromIndex(jointIndex).ToString() + " joint!");

                            var jointId = XRHandJointIDUtility.FromIndex(jointIndex);
                            AssignJoint(jointId, lastChild, m_DrawJointsParent.transform);
                        }
                    }
                }

                for (int fingerIndex = (int)XRHandFingerID.Thumb;
                    fingerIndex <= (int)XRHandFingerID.Little;
                    ++fingerIndex)
                {
                    var fingerId = (XRHandFingerID)fingerIndex;

                    var jointId = fingerId.GetFrontJointID();
                    if (m_JointXforms[jointId.ToIndex()] == null)
                        Debug.LogWarning("Hand transform hierarchy not set correctly - couldn't find " + jointId.ToString() + " joint!");
                }
            }

            public void OnDestroy()
            {
                GameObject.Destroy(m_HandRoot);
                m_HandRoot = null;

                for (int jointIndex = 0; jointIndex < m_DrawJoints.Length; ++jointIndex)
                {
                    GameObject.Destroy(m_DrawJoints[jointIndex]);
                    m_DrawJoints[jointIndex] = null;
                }

                for (int jointIndex = 0; jointIndex < m_VelocityParents.Length; ++jointIndex)
                {
                    GameObject.Destroy(m_VelocityParents[jointIndex]);
                    m_VelocityParents[jointIndex] = null;
                }

                GameObject.Destroy(m_DrawJointsParent);
                m_DrawJointsParent = null;
            }

            public void OnEnable()
            {
                if (m_DrawMesh)
                    ForceToggleDrawMeshNoCache(true);

                if (m_DebugDrawJoints)
                    ForceToggleDebugDrawJointsNoCache(true);

                if (m_VelocityType != VelocityType.None)
                    ForceSetVelocityTypeNoCache(m_VelocityType);
            }

            public void OnDisable()
            {
                if (m_DrawMesh)
                    ForceToggleDrawMeshNoCache(false);

                if (m_DebugDrawJoints)
                    ForceToggleDebugDrawJointsNoCache(false);

                if (m_VelocityType != VelocityType.None)
                    ForceSetVelocityTypeNoCache(VelocityType.None);
            }

            public void ToggleDrawMesh(bool drawMesh)
            {
                if (drawMesh != m_DrawMesh)
                    ForceToggleDrawMesh(drawMesh);
            }

            public void ForceToggleDrawMesh(bool drawMesh)
            {
                m_DrawMesh = drawMesh;
                ForceToggleDrawMeshNoCache(drawMesh);
            }

            void ForceToggleDrawMeshNoCache(bool drawMesh)
            {
                for (int childIndex = 0; childIndex < m_HandRoot.transform.childCount; ++childIndex)
                {
                    var xform = m_HandRoot.transform.GetChild(childIndex);
                    if (xform.TryGetComponent<SkinnedMeshRenderer>(out var renderer))
                        renderer.enabled = drawMesh;
                }
            }

            public void ToggleDebugDrawJoints(bool debugDrawJoints)
            {
                if (debugDrawJoints != m_DebugDrawJoints)
                    ForceToggleDebugDrawJoints(debugDrawJoints);
            }

            public void ForceToggleDebugDrawJoints(bool debugDrawJoints)
            {
                m_DebugDrawJoints = debugDrawJoints;
                ForceToggleDebugDrawJointsNoCache(debugDrawJoints);
            }

            void ForceToggleDebugDrawJointsNoCache(bool debugDrawJoints)
            {
                for (int jointIndex = 0; jointIndex < m_DrawJoints.Length; ++jointIndex)
                {
                    ToggleRenderers<MeshRenderer>(debugDrawJoints, m_DrawJoints[jointIndex].transform);
                    m_Lines[jointIndex].enabled = debugDrawJoints;
                }

                m_Lines[0].enabled = false;
            }

            public void SetVelocityType(VelocityType velocityType)
            {
                if (velocityType != m_VelocityType)
                    ForceSetVelocityType(velocityType);
            }

            public void ForceSetVelocityType(VelocityType velocityType)
            {
                m_VelocityType = velocityType;
                ForceSetVelocityTypeNoCache(velocityType);
            }

            void ForceSetVelocityTypeNoCache(VelocityType velocityType)
            {
                for (int jointIndex = 0; jointIndex < m_VelocityParents.Length; ++jointIndex)
                    ToggleRenderers<LineRenderer>(velocityType != VelocityType.None, m_VelocityParents[jointIndex].transform);
            }

            public void UpdateRootPose(XRHand hand)
            {
                var xform = m_JointXforms[XRHandJointID.Wrist.ToIndex()];
                xform.localPosition = hand.rootPose.position;
                xform.localRotation = hand.rootPose.rotation;
            }

            public void UpdateJoints(XROrigin origin, XRHand hand)
            {
                var originPose = new Pose(origin.transform.position, origin.transform.rotation);

                var wristPose = Pose.identity;
                UpdateJoint(originPose, hand.GetJoint(XRHandJointID.Wrist), ref wristPose);
                UpdateJoint(originPose, hand.GetJoint(XRHandJointID.Palm), ref wristPose, false);

                for (int fingerIndex = (int)XRHandFingerID.Thumb;
                    fingerIndex <= (int)XRHandFingerID.Little;
                    ++fingerIndex)
                {
                    var parentPose = wristPose;
                    var fingerId = (XRHandFingerID)fingerIndex;

                    int jointIndexBack = fingerId.GetBackJointID().ToIndex();
                    for (int jointIndex = fingerId.GetFrontJointID().ToIndex();
                        jointIndex <= jointIndexBack;
                        ++jointIndex)
                    {
                        if (m_JointXforms[jointIndex] != null)
                            UpdateJoint(originPose, hand.GetJoint(XRHandJointIDUtility.FromIndex(jointIndex)), ref parentPose);
                    }
                }
            }

            void UpdateJoint(
                Pose originPose,
                XRHandJoint joint,
                ref Pose parentPose,
                bool cacheParentPose = true)
            {
                int jointIndex = joint.id.ToIndex();
                var xform = m_JointXforms[jointIndex];
                if (xform == null || !joint.TryGetPose(out var pose))
                    return;

                m_DrawJoints[jointIndex].transform.localPosition = pose.position;
                m_DrawJoints[jointIndex].transform.localRotation = pose.rotation;

                if (m_DebugDrawJoints && joint.id != XRHandJointID.Wrist)
                {
                    m_LinePointsReuse[0] = parentPose.GetTransformedBy(originPose).position;
                    m_LinePointsReuse[1] = pose.GetTransformedBy(originPose).position;
                    m_Lines[jointIndex].SetPositions(m_LinePointsReuse);
                }

                var inverseParentRotation = Quaternion.Inverse(parentPose.rotation);
                xform.localPosition = inverseParentRotation * (pose.position - parentPose.position);
                xform.localRotation = inverseParentRotation * pose.rotation;
                if (cacheParentPose)
                    parentPose = pose;

                if (m_VelocityType != VelocityType.None && m_VelocityParents[jointIndex].TryGetComponent<LineRenderer>(out var renderer))
                {
                    m_VelocityParents[jointIndex].transform.localPosition = Vector3.zero;
                    m_VelocityParents[jointIndex].transform.localRotation = Quaternion.identity;

                    m_LinePointsReuse[0] = m_LinePointsReuse[1] = m_VelocityParents[jointIndex].transform.position;
                    if (m_VelocityType == VelocityType.Linear)
                    {
                        if (joint.TryGetLinearVelocity(out var velocity))
                            m_LinePointsReuse[1] += velocity;
                    }
                    else if (m_VelocityType == VelocityType.Angular)
                    {
                        if (joint.TryGetAngularVelocity(out var velocity))
                            m_LinePointsReuse[1] += 0.05f * velocity.normalized;
                    }

                    renderer.SetPositions(m_LinePointsReuse);
                }
            }

            static void ToggleRenderers<TRenderer>(bool toggle, Transform xform)
                where TRenderer : Renderer
            {
                if (xform.TryGetComponent<TRenderer>(out var renderer))
                    renderer.enabled = toggle;

                for (int childIndex = 0; childIndex < xform.childCount; ++childIndex)
                    ToggleRenderers<TRenderer>(toggle, xform.GetChild(childIndex));
            }
        }

        XRHandSubsystem m_Subsystem;
        HandGameObjects m_LeftHandGameObjects, m_RightHandGameObjects;
    }
}
