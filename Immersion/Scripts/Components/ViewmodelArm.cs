using OWML.Common;
using OWML.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Immersion.Scripts.Components
{
    public class ViewmodelArm : MonoBehaviour
    {
        private static GameObject s_viewmodelArmAsset;

        [SerializeField]
        private SkinnedMeshRenderer _noSuitMesh;

        [SerializeField]
        private SkinnedMeshRenderer _noSuitMeshPrepass;

        [SerializeField]
        private SkinnedMeshRenderer _suitMesh;

        [SerializeField]
        private SkinnedMeshRenderer _suitMeshPrepass;

        private Dictionary<string, Transform> _bones;

        private PlayerTool _playerTool;

        private OWItem _owItem;

        private ItemTool _itemCarryTool;

        private GameObject _playerNoSuitMesh;

        private GameObject _playerSuitMesh;

        /// <summary>
        /// Outputs this ViewmodelArm's information in JSON format.
        /// </summary>
        public void OutputArmData()
        {
            string indent = "    ";

            var armPos = transform.localPosition;
            var armRot = transform.localEulerAngles;

            string boneEulersString = "";
            int i = 0;
            foreach (var keyValuePair in _bones)
            {
                var eulers = keyValuePair.Value.localEulerAngles;
                boneEulersString += $"{indent}{indent}{indent}\"{keyValuePair.Key}\": {{ \"x\": {eulers.x}, \"y\": {eulers.y}, \"z\": {eulers.z} }}{(i < _bones.Count - 1 ? ",\n" : "\n")}";
                i++;
            }

            string output = $"{indent}\"[ARM DATA ID HERE]\" {{\n" +
                $"{indent}{indent}\"arm_local_position\": {{ \"x\": {armPos.x}, \"y\": {armPos.y}, \"z\": {armPos.z} }},\n" +
                $"{indent}{indent}\"arm_local_euler_angles\": {{ \"x\": {armRot.x}, \"y\": {armRot.y}, \"z\": {armRot.z} }},\n" +
                $"{indent}{indent}\"arm_scale\": {10f * transform.localScale.x},\n" +
                $"{indent}{indent}\"arm_shader\": \"{_noSuitMesh.material.shader.name}\",\n" +
                $"{indent}{indent}\"bones_local_euler_angles\": {{\n" +
                boneEulersString +
                $"{indent}{indent}}}\n{indent}}}";

            ModMain.Console.WriteLine(output);
        }

        internal static void LoadAsset()
        {
            var assetBundle = ModMain.Assets.LoadBundle("AssetBundles/viewmodelarm");
            s_viewmodelArmAsset = assetBundle.LoadAsset<GameObject>("Assets/ViewmodelArm.prefab");
        }

        internal static void OnEquipTool(PlayerTool playerTool)
        {
            if (Config.EnableViewmodelArms && ArmData.Exists(playerTool))
            {
                // check for existing arm and enable if found (PlayerTool has no event for being equipped, so this is required)
                var existingArm = playerTool.transform.Find("ViewmodelArm");
                if (existingArm != null)
                {
                    existingArm.gameObject.SetActive(true);
                    return;
                }

                NewViewmodelArm(playerTool.transform, ArmData.Find(playerTool));
            }
        }

        internal static void OnPickUpItem(OWItem owItem)
        {
            if (Config.EnableViewmodelArms && ArmData.Exists(owItem) && owItem.transform.Find("ViewmodelArm") == null)
            {
                NewViewmodelArm(owItem.transform, ArmData.Find(owItem));
            }

            // some items need to be adjusted
            switch (owItem.GetItemType().GetName())
            {
                case "ConversationStone":
                    owItem.transform.localPosition += 0.2f * Vector3.forward;
                    break;
                case "Lantern":
                    owItem.transform.localEulerAngles = new Vector3(0f, 327f, 0f);
                    break;
                case "GhostbirdSkull":
                    ModMain.Events.Unity.FireOnNextUpdate(() =>
                    {
                        owItem.transform.localScale = 0.6f * Vector3.one;
                    });
                    break;
            }
        }

        private static ViewmodelArm NewViewmodelArm(Transform parent, ArmData armData = null)
        {
            var viewmodelArm = Instantiate(s_viewmodelArmAsset).GetComponent<ViewmodelArm>();
            viewmodelArm.name = "ViewmodelArm";
            viewmodelArm.transform.parent = parent;
            viewmodelArm.transform.localPosition = Vector3.zero;
            viewmodelArm.transform.localRotation = Quaternion.identity;

            // get the ingame shaders
            viewmodelArm.SetShader("Standard");
            var prepassShader = Shader.Find("Outer Wilds/Utility/View Model Prepass");
            viewmodelArm._noSuitMeshPrepass.materials[0].shader = prepassShader;
            viewmodelArm._noSuitMeshPrepass.materials[1].shader = prepassShader;
            viewmodelArm._suitMeshPrepass.material.shader = prepassShader;

            if (armData != null)
            {
                viewmodelArm.SetArmData(armData);
            }

            return viewmodelArm;
        }

        private void SetShader(string shaderName)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                ModMain.Console.WriteLine($"\"{shaderName}\" is not a valid shader.", MessageType.Error);
                return;
            }

            _noSuitMesh.materials[0].shader = shader;
            _noSuitMesh.materials[1].shader = shader;
            _suitMesh.material.shader = shader;

            // if using the viewmodel shader, the prepass meshes must be enabled to prevent viewmodel arms from appearing behind things
            bool isViewmodel = shaderName == "Outer Wilds/Utility/View Model" || shaderName == "Outer Wilds/Utility/View Model (Cutoff)";
            _noSuitMeshPrepass.gameObject.SetActive(isViewmodel);
            _suitMeshPrepass.gameObject.SetActive(isViewmodel);
        }

        private void SetBoneEulers(Dictionary<string, Vector3> boneEulers)
        {
            foreach (var boneEuler in boneEulers)
            {
                _bones[boneEuler.Key].localEulerAngles = boneEuler.Value;
            }
        }

        private void SetArmData(ArmData armData)
        {
            transform.localPosition = armData.armPosition;
            transform.localEulerAngles = armData.armRotation;
            transform.localScale = 0.1f * armData.armScale * Vector3.one;
            SetShader(armData.armShader);
            SetBoneEulers(armData.boneEulers);
        }

        private void SetArmData(string armDataId)
        {
            var armData = ArmData.Find(armDataId);
            if (armData != null)
            {
                SetArmData(armData);
            }
        }

        private void Awake()
        {
            // grab the bones that matter
            _bones = new Dictionary<string, Transform>
            {
                ["shoulder"] = _noSuitMesh.bones[5],
                ["elbow"] = _noSuitMesh.bones[6],
                ["wrist"] = _noSuitMesh.bones[7],
                ["finger_01_01"] = _noSuitMesh.bones[8],
                ["finger_01_02"] = _noSuitMesh.bones[9],
                ["finger_01_03"] = _noSuitMesh.bones[10],
                ["finger_01_04"] = _noSuitMesh.bones[11],
                ["finger_02_01"] = _noSuitMesh.bones[12],
                ["finger_02_02"] = _noSuitMesh.bones[13],
                ["finger_02_03"] = _noSuitMesh.bones[14],
                ["finger_02_04"] = _noSuitMesh.bones[15],
                ["thumb_01"] = _noSuitMesh.bones[16],
                ["thumb_02"] = _noSuitMesh.bones[17],
                ["thumb_03"] = _noSuitMesh.bones[18],
                ["thumb_04"] = _noSuitMesh.bones[19]
            };
        }

        private void Start()
        {
            _playerTool = transform.parent.GetComponent<PlayerTool>();
            if (_playerTool == null)
            {
                _owItem = transform.parent.GetComponent<OWItem>();
                _owItem.onPickedUp.AddListener((_) => gameObject.SetActive(true));
                _itemCarryTool = Locator.GetToolModeSwapper().GetItemCarryTool();
            }

            var playerBody = Locator.GetPlayerBody();
            _playerNoSuitMesh = playerBody.transform.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_RightArm").gameObject;
            _playerSuitMesh = playerBody.transform.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject;
        }

        private void LateUpdate()
        {
            if (!Config.EnableViewmodelArms)
            {
                gameObject.SetActive(false);
                return;
            }

            if (_playerTool != null && ((!_playerTool.IsEquipped() && !_playerTool.IsPuttingAway()) || OWInput.IsInputMode(InputMode.ShipCockpit)))
            {
                gameObject.SetActive(false);
                return;
            }
            else if (_owItem != null && _itemCarryTool.GetHeldItem() != _owItem)
            {
                gameObject.SetActive(false);
                return;
            }

            _noSuitMesh.gameObject.SetActive(_playerNoSuitMesh.activeInHierarchy);
            _suitMesh.gameObject.SetActive(_playerSuitMesh.activeInHierarchy);
        }
    }
}