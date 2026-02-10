using HarmonyLib;
using Immersion.Objects;
using UnityEngine;

namespace Immersion.Components;

[HarmonyPatch]
public class ViewmodelArm : MonoBehaviour
{
    private static GameObject s_viewmodelArmAsset;
    
    [SerializeField]
    private GameObject _viewmodelArmNoSuit;

    [SerializeField]
    private GameObject _viewmodelArmSuit;

    private Transform[] _bones;

    private PlayerTool _playerTool;

    private OWItem _owItem;

    private ItemTool _itemCarryTool;

    private GameObject _playerRightArmNoSuit;

    private GameObject _playerRightArmSuit;

    public static ViewmodelArm NewViewmodelArm(PlayerTool playerTool)
    {
        var viewmodelArm = LoadViewmodelArm(playerTool.transform);
        viewmodelArm._playerTool = playerTool;
        return viewmodelArm;
    }

    public static ViewmodelArm NewViewmodelArm(OWItem owItem)
    {
        var viewmodelArm = LoadViewmodelArm(owItem.transform);
        viewmodelArm._owItem = owItem;
        return viewmodelArm;
    }

    public void SetArmData(string itemName)
    {
        var armData = ArmData.GetArmData(itemName);
        if (armData == null) return;

        SetBonePoses(armData.bonePositions, armData.boneEulers);
        transform.localScale = 0.1f * Vector3.one * armData.scale;
        SetShader(armData.shaderName);
    }

    public void OutputArmData()
    {
        string output = "Bone Positions:\n";
        for (int i = 0; i < _bones.Length; i++)
        {
            var bonePositions = _bones[i].localPosition;
            output += $"[ {Mathf.Round(bonePositions.x * 1000f) / 1000f}, {Mathf.Round(bonePositions.y * 1000f) / 1000f}, {Mathf.Round(bonePositions.z * 1000f) / 1000f} ],\n";
        }

        output += "\nBone Eulers:\n";
        for (int i = 0; i < _bones.Length; i++)
        {
            var boneEulers = _bones[i].localEulerAngles;
            output += $"[ {Mathf.Round(boneEulers.x * 100f) / 100f}, {Mathf.Round(boneEulers.y * 100f) / 100f}, {Mathf.Round(boneEulers.z * 100f) / 100f} ],\n";
        }

        output += $"\nArm Scale: {transform.localScale.x * 10f}\n";
        output += $"\nShader: \"{_viewmodelArmNoSuit.GetComponent<SkinnedMeshRenderer>().materials[0].shader.name}\"";

        ModMain.Log(output);
    }

    private static ViewmodelArm LoadViewmodelArm(Transform parent)
    {
        if (s_viewmodelArmAsset == null)
            s_viewmodelArmAsset = ModMain.Instance.ModHelper.Assets.LoadBundle("AssetBundles/viewmodelarm").LoadAsset<GameObject>("Assets/ViewmodelArm.prefab");

        var viewmodelArm = Instantiate(s_viewmodelArmAsset).GetComponent<ViewmodelArm>();
        viewmodelArm.name = "ViewmodelArm";
        viewmodelArm.transform.parent = parent;
        viewmodelArm.transform.localPosition = Vector3.zero;
        viewmodelArm.transform.localRotation = Quaternion.identity;
        return viewmodelArm;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.EquipTool))]
    private static void PlayerTool_EquipTool_Postfix(PlayerTool __instance)
    {
        // don't try to add viewmodel arm if disabled in config or if this tool already has one
        if (!Config.EnableViewmodelHands) return;

        // check for existing arm and enable if found (PlayerTool has no event for tool being equipped, so this is required)
        var existingArm = __instance.transform.Find("ViewmodelArm");
        if (existingArm != null)
        {
            existingArm.gameObject.SetActive(true);
            return;
        }

        if (__instance is Signalscope)
        {
            NewViewmodelArm(__instance);//?;.SetArmData("Signalscope");
            return;
        }

        if (__instance is NomaiTranslator)
        {
            NewViewmodelArm(__instance);//?.SetArmData("NomaiTranslator");
            return;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
    private static void OWItem_PickUpItem_Postfix(OWItem __instance)
    {
        // don't try to add viewmodel arm if disabled in config or if this item already has one
        if (!Config.EnableViewmodelHands || __instance.transform.Find("ViewmodelArm")) return;

        switch (__instance._type)
        {
            case ItemType.SharedStone:
                if (__instance is not SharedStone) break;
                NewViewmodelArm(__instance)?.SetArmData("SharedStone");
                break;

            case ItemType.Scroll:
                if (__instance is not ScrollItem) break;

                if (__instance.name == "Prefab_NOM_Scroll_Jeff")
                {
                    NewViewmodelArm(__instance)?.SetArmData("Scroll_Jeff");
                }
                else
                {
                    NewViewmodelArm(__instance)?.SetArmData("Scroll");
                }

                break;

            case ItemType.ConversationStone:
                if (__instance is not NomaiConversationStone) break;
                var word = (__instance as NomaiConversationStone)._word;
                if (word == NomaiWord.Identify || word == NomaiWord.Explain)
                    NewViewmodelArm(__instance)?.SetArmData("ConversationStone_Big");
                else
                    NewViewmodelArm(__instance)?.SetArmData("ConversationStone");
                break;

            case ItemType.WarpCore:
                if (__instance is not WarpCoreItem) break;
                var warpCoreType = (__instance as WarpCoreItem)._warpCoreType;
                if (warpCoreType == WarpCoreType.Vessel || warpCoreType == WarpCoreType.VesselBroken)
                    NewViewmodelArm(__instance)?.SetArmData("WarpCore");
                else
                    NewViewmodelArm(__instance)?.SetArmData("WarpCore_Simple");
                break;

            case ItemType.Lantern:
                if (__instance is not SimpleLanternItem) break;
                NewViewmodelArm(__instance)?.SetArmData("Lantern");
                break;

            case ItemType.SlideReel:
                if (__instance is not SlideReelItem) break;
                NewViewmodelArm(__instance)?.SetArmData("SlideReel");
                break;

            case ItemType.DreamLantern:
                if (__instance is not DreamLanternItem) break;
                switch ((__instance as DreamLanternItem)._lanternType)
                {
                    case DreamLanternType.Nonfunctioning:
                        NewViewmodelArm(__instance)?.SetArmData("DreamLantern_Nonfunctioning");
                        break;

                    case DreamLanternType.Malfunctioning:
                        NewViewmodelArm(__instance)?.SetArmData("DreamLantern_Malfunctioning");
                        break;

                    default:
                        NewViewmodelArm(__instance)?.SetArmData("DreamLantern");
                        break;
                }

                break;

            case ItemType.VisionTorch:
                if (__instance is not VisionTorchItem) break;
                NewViewmodelArm(__instance)?.SetArmData("VisionTorch");
                break;
        }
    }

    private void SetBonePoses(Vector3[] positions, Vector3[] eulers)
    {
        for (int i = 0; i < _bones.Length; i++)
        {
            _bones[i].localPosition = positions[i];
            _bones[i].localEulerAngles = eulers[i];
        }
    }

    private void SetShader(string shaderName)
    {
        var shader = Shader.Find(shaderName);
        var noSuitMesh = _viewmodelArmNoSuit.GetComponent<SkinnedMeshRenderer>();
        noSuitMesh.materials[0].shader = shader;
        noSuitMesh.materials[1].shader = shader;
        _viewmodelArmSuit.GetComponent<SkinnedMeshRenderer>().material.shader = shader;
    }

    private void Awake()
    {
        _itemCarryTool = Locator.GetToolModeSwapper().GetItemCarryTool();
        _bones = _viewmodelArmNoSuit.GetComponent<SkinnedMeshRenderer>().bones;

        var player = Locator.GetPlayerController().transform;
        _playerRightArmNoSuit = player.transform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
        _playerRightArmSuit = player.transform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
    }

    private void LateUpdate()
    {
        if (!Config.EnableViewmodelHands)
        {
            gameObject.SetActive(false);
            return;
        }

        if (_playerTool != null)
        {
            bool isHoldingTool = _playerTool.IsEquipped() || _playerTool.IsPuttingAway();
            if (!isHoldingTool || OWInput.IsInputMode(InputMode.ShipCockpit))
            {
                gameObject.SetActive(false);
                return;
            }
        }
        else if (_owItem != null && _itemCarryTool._heldItem != _owItem)
        {
            gameObject.SetActive(false);
            return;
        }

        _viewmodelArmNoSuit.SetActive(_playerRightArmNoSuit.activeInHierarchy);
        _viewmodelArmSuit.SetActive(_playerRightArmSuit.activeInHierarchy);
    }
}