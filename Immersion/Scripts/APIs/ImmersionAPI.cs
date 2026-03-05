using Immersion.Scripts.Components;
using System;
using UnityEngine;

namespace Immersion.Scripts.APIs
{
    public class ImmersionAPI
    {
        /// <summary>
        /// Whether or not the Viewmodel Arms feature is enabled.
        /// </summary>
        /// <returns>true if Viewmodel Arms are enabled, false if not.</returns>
        public bool AreViewmodelArmsEnabled()
        {
            return Config.EnableViewmodelArms;
        }

        /// <summary>
        /// Loads Arm Data from JSON.
        /// </summary>
        /// <param name="jsonPath">The path to the JSON containing the custom ArmData information.</param>
        public void LoadCustomArmData(string jsonPath)
        {
            ArmData.LoadCustomArmData(jsonPath);
        }

        /// <summary>
        /// Creates a Viewmodel Arm on a PlayerTool.
        /// </summary>
        /// <param name="tool">The PlayerTool to add a Viewmodel Arm to.</param>
        /// <returns>The Viewmodel Arm's GameObject.</returns>
        public GameObject CreateViewmodelArm(PlayerTool tool)
        {
            return ViewmodelArm.NewViewmodelArm(tool).gameObject;
        }

        /// <summary>
        /// Creates a Viewmodel Arm on an OWItem.
        /// </summary>
        /// <param name="item">The OWItem to add a Viewmodel Arm to.</param>
        /// <returns>The Viewmodel Arm's GameObject.</returns>
        public GameObject CreateViewmodelArm(OWItem item)
        {
            return ViewmodelArm.NewViewmodelArm(item).gameObject;
        }

        /// <summary>
        /// Sets the Arm Data for a Viewmodel Arm.
        /// </summary>
        /// <param name="viewmodelArmObject">The GameObject of the ViewmodelArm.</param>
        /// <param name="itemName">The name (from the JSON) of the Arm Data information.</param>
        public void SetArmData(GameObject viewmodelArmObject, string armDataId)
        {
            viewmodelArmObject.GetComponent<ViewmodelArm>()?.SetArmData(armDataId);
        }

        [Obsolete("Immersion no longer changes AnimSpeed.")]
        public float GetAnimSpeed()
        {
            return 1f;
        }

        [Obsolete("No longer works. Use CreateViewmodelArm() instead.")]
        public GameObject NewViewmodelArm(PlayerTool playerTool, Vector3 localPos, Quaternion localRot, Vector3 localScale, int armShader = 0)
        {
            return null; // ViewmodelArm.NewViewmodelArm(playerTool, localPos, localRot, localScale, (ViewmodelArm.ArmShader)armShader).gameObject;
        }

        [Obsolete("No longer works. Use CreateViewmodelArm() instead.")]
        public GameObject NewViewmodelArm(OWItem owItem, Vector3 localPos, Quaternion localRot, Vector3 localScale, int armShader = 0)
        {
            return null; // ViewmodelArm.NewViewmodelArm(owItem, localPos, localRot, localScale, (ViewmodelArm.ArmShader)armShader).gameObject;
        }
    }
}