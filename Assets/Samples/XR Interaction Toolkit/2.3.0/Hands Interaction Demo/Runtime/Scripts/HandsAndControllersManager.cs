using System.Collections.Generic;
#if XR_HANDS
using UnityEngine.XR.Hands;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Samples.Hands
{
    /// <summary>
    /// Manages swapping between hands and controllers at runtime based on whether hands are tracked.
    /// </summary>
    public class HandsAndControllersManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("GameObject to toggle on when the left hand is tracked and off when it is not tracked.")]
        GameObject m_LeftHand;

        [SerializeField]
        [Tooltip("GameObject to toggle on when the right hand is tracked and off when it is not tracked.")]
        GameObject m_RightHand;

        [SerializeField]
        [Tooltip("GameObject to toggle off when the left hand is tracked and on when it is not tracked.")]
        GameObject m_LeftController;

        [SerializeField]
        [Tooltip("GameObject to toggle off when the right hand is tracked and on when it is not tracked.")]
        GameObject m_RightController;

#if XR_HANDS
        XRHandSubsystem m_Subsystem;

        static readonly List<XRHandSubsystem> s_Subsystems = new List<XRHandSubsystem>();
#endif

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
#if XR_HANDS
            SubsystemManager.GetSubsystems(s_Subsystems);
            if (s_Subsystems.Count == 0)
            {
                Debug.LogWarning("Hand Tracking Subsystem not found, can't subscribe to hand tracking status. Enable that feature in the OpenXR project settings and ensure OpenXR is enabled as the plug-in provider.", this);
                ToggleLeftHand(false);
                ToggleRightHand(false);
                return;
            }

            m_Subsystem = s_Subsystems[0];
            m_Subsystem.trackingAcquired += OnHandTrackingAcquired;
            m_Subsystem.trackingLost += OnHandTrackingLost;

            var leftHand = m_Subsystem.leftHand;
            ToggleHand(leftHand.handedness, leftHand.isTracked);
            
            var rightHand = m_Subsystem.rightHand;
            ToggleHand(rightHand.handedness, rightHand.isTracked);
#else
            Debug.LogError("Script requires XR Hands (com.unity.xr.hands) package. Install using Window > Package Manager or click Fix on the related issue in Edit > Project Settings > XR Plug-in Management > Project Validation.", this);
            ToggleLeftHand(false);
            ToggleRightHand(false);
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
#if XR_HANDS
            if (m_Subsystem == null)
                return;

            m_Subsystem.trackingAcquired -= OnHandTrackingAcquired;
            m_Subsystem.trackingLost -= OnHandTrackingLost;
            m_Subsystem = null;
#endif
        }

        void ToggleLeftHand(bool handOn)
        {
            if (m_LeftHand != null)
                m_LeftHand.SetActive(handOn);

            if (m_LeftController != null)
                m_LeftController.SetActive(!handOn);
        }

        void ToggleRightHand(bool handOn)
        {
            if (m_RightHand != null)
                m_RightHand.SetActive(handOn);

            if (m_RightController != null)
                m_RightController.SetActive(!handOn);
        }

#if XR_HANDS
        void OnHandTrackingAcquired(XRHand hand)
        {
            ToggleHand(hand.handedness, true);
        }

        void OnHandTrackingLost(XRHand hand)
        {
            ToggleHand(hand.handedness, false);
        }

        void ToggleHand(Handedness handedness, bool handOn)
        {
            switch (handedness)
            {
                case Handedness.Left:
                    ToggleLeftHand(handOn);
                    break;
                case Handedness.Right:
                    ToggleRightHand(handOn);
                    break;
            }
        }
#endif
    }
}