using Newtonsoft.Json;
using OWML.Common;
using OWML.Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Immersion.Scripts
{
    public class ArmData
    {
        [JsonProperty("arm_position")]
        public readonly Vector3 armPosition;

        [JsonProperty("arm_rotation")]
        public readonly Vector3 armRotation;

        [JsonProperty("arm_scale")]
        public readonly float armScale;

        [JsonProperty("arm_shader")]
        public readonly string armShader;

        [JsonProperty("bone_eulers")]
        public readonly Dictionary<string, Vector3> boneEulers;

        private static Dictionary<string, ArmData> s_armData;

        /// <summary>
        /// Determines if there is an ArmData of a certain ID.
        /// </summary>
        /// <param name="armDataId">The ID to determine if there is ArmData for.</param>
        /// <returns>True if there is an ArmData of this ID, False if there is not.</returns>
        public static bool Exists(string armDataId)
        {
            return !string.IsNullOrEmpty(armDataId) && s_armData != null && s_armData.ContainsKey(armDataId);
        }

        /// <summary>
        /// Determines if there is an ArmData for a certain PlayerTool.
        /// </summary>
        /// <param name="playerTool">The PlayerTool to determine if there is ArmData for.</param>
        /// <returns>True if there is an ArmData for this PlayerTool, False if there is not.</returns>
        public static bool Exists(PlayerTool playerTool)
        {
            string armDataId = FindId(playerTool);
            return ArmData.Exists(armDataId);
        }

        /// <summary>
        /// Determines if there is an ArmData for a certain OWItem.
        /// </summary>
        /// <param name="owItem">The OWItem to determine if there is ArmData for.</param>
        /// <returns>True if there is an ArmData for this OWItem, False if there is not.</returns>
        public static bool Exists(OWItem owItem)
        {
            string armDataId = FindId(owItem);
            return ArmData.Exists(armDataId);
        }

        /// <summary>
        /// Returns the ArmData linked to the given ID, if it exists.
        /// </summary>
        /// <param name="armDataId">The ID of the desired ArmData.</param>
        /// <returns>The ArmData of this ID, or null if there is no ArmData of this ID.</returns>
        public static ArmData Find(string armDataId)
        {
            if (ArmData.Exists(armDataId))
            {
                return s_armData[armDataId];
            }

            ModMain.Console.WriteLine($"No Arm Data found for {armDataId}", MessageType.Error);
            return null;
        }

        /// <summary>
        /// Returns the ArmData for a PlayerTool, if it exists.
        /// </summary>
        /// <param name="playerTool">The PlayerTool to find ArmData for.</param>
        /// <returns>The ArmData for the PlayerTool, or null if there is no ArmData for that tool.</returns>
        public static ArmData Find(PlayerTool playerTool)
        {
            if (!ArmData.Exists(playerTool))
            {
                ModMain.Console.WriteLine($"No ArmData exists for PlayerTool \"{playerTool.name}\"");
                return null;
            }

            string armDataId = ArmData.FindId(playerTool);
            return ArmData.Find(armDataId);
        }

        /// <summary>
        /// Returns the ArmData for an OWItem, if it exists.
        /// </summary>
        /// <param name="playerTool">The OWItem to find ArmData for.</param>
        /// <returns>The ArmData for the OWItem, or null if there is no ArmData for that tool.</returns>
        public static ArmData Find(OWItem owItem)
        {
            if (!ArmData.Exists(owItem))
            {
                ModMain.Console.WriteLine($"No ArmData exists for OWItem \"{owItem.name}\"");
                return null;
            }

            string armDataId = ArmData.FindId(owItem);
            return ArmData.Find(armDataId);
        }

        /// <summary>
        /// Returns the ID of the ArmData for a PlayerTool, if it exists.
        /// </summary>
        /// <param name="playerTool">The PlayerTool to find the ArmData ID for.</param>
        /// <returns>The ID of the ArmData for the PlayerTool, or null if there is no ArmData for that tool.</returns>
        public static string FindId(PlayerTool playerTool)
        {
            return playerTool.name;
        }

        /// <summary>
        /// Returns the ID of the ArmData for a OWItem, if it exists.
        /// </summary>
        /// <param name="owItem">The OWItem to find the ArmData ID for.</param>
        /// <returns>The ID of the ArmData for the OWItem, or null if there is no ArmData for that tool.</returns>
        public static string FindId(OWItem owItem)
        {
            var itemTypeName = owItem.GetItemType().GetName();
            switch (itemTypeName)
            {
                // vanilla items
                case "SharedStone":
                case "SlideReel":
                case "Lantern":
                case "VisionTorch":
                    return itemTypeName;

                // vanilla items with variants
                case "Scroll":
                    return owItem.name switch
                    {
                        "Prefab_NOM_Scroll_egg" => "Scroll_Egg",
                        "Prefab_NOM_Scroll_Jeff" => "Scroll_Jeff",
                        _ => "Scroll"
                    };
                case "ConversationStone":
                    if (owItem is NomaiConversationStone conversationStone)
                    {
                        return conversationStone.GetWord() switch
                        {
                            NomaiWord.Identify or NomaiWord.Explain => "ConversationStone_Big",
                            _ => "ConversationStone"
                        };
                    }
                    return null;
                case "WarpCore":
                    if (owItem is WarpCoreItem warpCore)
                    {
                        return warpCore.GetWarpCoreType() switch
                        {
                            WarpCoreType.Vessel or WarpCoreType.VesselBroken => "WarpCore",
                            _ => "WarpCore_Simple"
                        };
                    }
                    return null;
                case "DreamLantern":
                    if (owItem is DreamLanternItem dreamLantern)
                    {
                        return dreamLantern.GetLanternType() switch
                        {
                            DreamLanternType.Nonfunctioning => "DreamLantern_Nonfunctioning",
                            DreamLanternType.Malfunctioning => "DreamLantern_Malfunctioning",
                            _ => "DreamLantern"
                        };
                    }
                    return null;

                // TSTA items
                case "CloakMineral":
                case "StrangerSeal":
                case "GhostbirdSkull":
                    return $"TSTA_{itemTypeName}";

                // Dreambound compass
                case "Compass":
                    return "Dreambound_Compass";
            }

            return null;
        }

        internal static void Load()
        {
            ModMain.Console.WriteLine($"Loading Arm Data...", MessageType.Info);
            string json = File.ReadAllText($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/viewmodel-arm-data.json");
            s_armData = JsonConvert.DeserializeObject<Dictionary<string, ArmData>>(json);
            ModMain.Console.WriteLine($"Arm Data loaded successfully!", MessageType.Success);
        }
    }
}