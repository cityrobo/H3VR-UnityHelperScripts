using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FistVR;
using OpenScripts2;
using System.Linq;
using Alloy;
using System;

namespace Cityrobo
{
    [CustomEditor(typeof(BulkAlloyMapCreator))]
    public class BulkAlloyMapCreatorEditor : Editor
    {
        private BulkAlloyMapCreator t;

        private const string BASECOLOR_NAME = "_MainTex";
        private const string ALLOYMAP_NAME = "_SpecTex";
        private const string NORMALMAP_NAME = "_BumpMap";

        public override void OnInspectorGUI()
        {
            if (t == null) t = target as BulkAlloyMapCreator;

            if (BulkAlloyMapCreator.PackerDefinition == null) BulkAlloyMapCreator.PackerDefinition = AssetDatabase.LoadAssetAtPath<PackedMapDefinition>("Assets/Alloy/Scripts/MaterialMapChannelPacker/Config/PackedPack.asset");

            GUILayout.Label("Bulk AlloyMap Creator", EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = 300f;

            // Display the array elements
            var serializedObject = new SerializedObject(target);
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetFolder"), true);
            GUILayout.Label("Naming", EditorStyles.boldLabel);
            t.UsingCustomTextureNames = EditorGUILayout.Toggle(new GUIContent("Using custom output texture names?", "If not checked, Output texture names will be dependent on the first valid texture from arrays in the order: Metallic, AO, Specular, Roughness."), t.UsingCustomTextureNames);
            if (t.UsingCustomTextureNames)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TextureNames"), true);
            else t.UsingAutomaticTexureNameTruncation = EditorGUILayout.Toggle(new GUIContent("Using automatic texture name truncation?", "If checked anything after a existing last \"_\" in the automatically found texture name will be truncated automatically. For example if something like \"_Metallic\" exists at the end of the name it will be automatically removed from the final AlloyMap name."), t.UsingAutomaticTexureNameTruncation);
            
            GUILayout.Label("Metallic Textures", EditorStyles.boldLabel);
            t.UsingMetallicTextures = EditorGUILayout.Toggle("Using metallic textures?", t.UsingMetallicTextures);
            if (t.UsingMetallicTextures)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MetallicTextures"), true);
            else t.DefaultMetallicIsWhite = EditorGUILayout.Toggle("Is default metallic all white instead?", t.DefaultMetallicIsWhite);

            GUILayout.Label("Ambient Occlusion Textures", EditorStyles.boldLabel);
            t.UsingAmbientOcclusionTextures = EditorGUILayout.Toggle("Using ambient occlusion textures?", t.UsingAmbientOcclusionTextures);
            if (t.UsingAmbientOcclusionTextures)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AmbientOcclusionTextures"), true);
            else t.DefaultAmbientOcclusionIsBlack = EditorGUILayout.Toggle("Is default ambient occlusion all black instead?", t.DefaultAmbientOcclusionIsBlack);

            GUILayout.Label("Specular Textures", EditorStyles.boldLabel);
            t.UsingSpecularTextures = EditorGUILayout.Toggle("Using specular textures?", t.UsingSpecularTextures);
            if (t.UsingSpecularTextures)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SpecularTextures"), true);
            else t.DefaultSpecularIsBlack = EditorGUILayout.Toggle("Is default specular all black instead?", t.DefaultSpecularIsBlack);

            GUILayout.Label("Roughness Textures", EditorStyles.boldLabel);
            t.UsingRoughnessTextures = EditorGUILayout.Toggle("Using roughness textures?", t.UsingRoughnessTextures);
            if (t.UsingRoughnessTextures)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RoughnessTextures"), true);
            else t.DefaultRoughnessIsBlack = EditorGUILayout.Toggle("Is default roughness all black instead?", t.DefaultRoughnessIsBlack);

            GUILayout.Label("Material AutoComplete", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaterialsList"), true);
            t.UseMaterialNameForTextureName = EditorGUILayout.Toggle("Use material name for texture name?", t.UseMaterialNameForTextureName);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseColorTextures"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NormalMapTextures"), true);

            serializedObject.ApplyModifiedProperties();

            //Store Texture2D arrays in a list
            List<List<Texture2D>> textureLists = new List<List<Texture2D>>();

            // Add arrays to the list based on the corresponding boolean flags
            if (t.UsingMetallicTextures) textureLists.Add(t.MetallicTextures);
            if (t.UsingAmbientOcclusionTextures) textureLists.Add(t.AmbientOcclusionTextures);
            if (t.UsingSpecularTextures) textureLists.Add(t.SpecularTextures);
            if (t.UsingRoughnessTextures) textureLists.Add(t.RoughnessTextures);

            List<List<Texture2D>> allTexureLists = new List<List<Texture2D>>();
            allTexureLists.AddRange(textureLists);

            if (t.BaseColorTextures.Count > 0) allTexureLists.Add(t.BaseColorTextures);
            if (t.NormalMapTextures.Count > 0) allTexureLists.Add (t.NormalMapTextures);

            // Check some things for errors
            bool listsHaveNullEntry = allTexureLists.Any(textureList => textureList.Any(texture => texture == null)) || t.MaterialsList.Count > 0 && t.MaterialsList.Any(material => material == null);
            bool listsHaveEqualLength = allTexureLists.All(arr => arr.Count == textureLists[0].Count) && (t.MaterialsList.Count == 0 || t.MaterialsList.Count > 0 && t.MaterialsList.Count == textureLists[0].Count);
            bool usingAnyCustomTextures = t.UsingMetallicTextures || t.UsingAmbientOcclusionTextures || t.UsingSpecularTextures || t.UsingRoughnessTextures;
            bool materialNameCheck = !t.UseMaterialNameForTextureName || t.UseMaterialNameForTextureName && t.MaterialsList.Count > 0;

            if (usingAnyCustomTextures && !listsHaveNullEntry && listsHaveEqualLength && materialNameCheck && AssetDatabase.IsValidFolder(t.TargetFolder) && (!t.UsingCustomTextureNames || textureLists[0].Count == t.TextureNames.Length) && GUILayout.Button("Create AlloyPM files", GUILayout.ExpandWidth(true)))
            {
                CreateAssets(textureLists);
            }
            else if (!usingAnyCustomTextures) EditorGUILayout.HelpBox("No custom textures are used, so this becomes fairly poinless!", MessageType.Warning);
            else if (listsHaveNullEntry) EditorGUILayout.HelpBox("There is at least one empty spot in your lists!", MessageType.Error);
            else if (!listsHaveEqualLength) EditorGUILayout.HelpBox("Textures arrays do not share the same amount of textures!", MessageType.Error);
            else if (!AssetDatabase.IsValidFolder(t.TargetFolder)) EditorGUILayout.HelpBox("Output folder is not valid!", MessageType.Error);
            else if (t.UsingCustomTextureNames && textureLists[0].Count != t.TextureNames.Length) EditorGUILayout.HelpBox("Not enough names in the names array! " + textureLists[0].Count + " Textures vs " + t.TextureNames.Length + " Names", MessageType.Error);
            else if (!materialNameCheck) EditorGUILayout.HelpBox("You want to use material names, but no materials are supplied!", MessageType.Error);

            if (GUILayout.Button("Assign Textures"))
            {
                FindFiles();
            }
        }

        private void CreateAssets(List<List<Texture2D>> textureLists)
        {
            bool textureImportSettingsChanged = false;

            for (int i = 0; i < textureLists[0].Count; i++)
            {
                string textureName;
                if (t.UsingCustomTextureNames) textureName = t.TextureNames[i];
                else if (t.UseMaterialNameForTextureName && t.MaterialsList.Count != 0) textureName = t.MaterialsList[i].name;
                else if (t.UsingAutomaticTexureNameTruncation)
                {
                    string input = textureLists[0][i].name;
                    int lastIndex = input.LastIndexOf("_");

                    if (lastIndex != -1)
                    {
                        textureName = input.Substring(0, lastIndex);
                    }
                    else
                    {
                        textureName = textureLists[0][i].name;
                    }
                }
                else textureName = textureLists[0][i].name;

                string assetPath = t.TargetFolder + "/" + textureName + "_AlloyPM.asset";
                string texturePath = t.TargetFolder + "/" + textureName + "_AlloyPM.png";

                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.DeleteAsset(texturePath);

                string[] GUIDs = GetTextureGUID(i);

                AlloyCustomImportObject alloyPM = CreateInstance<AlloyCustomImportObject>();

                alloyPM.PackMode = BulkAlloyMapCreator.PackerDefinition;

                TextureValueChannelMode metallicChannelMode = t.UsingMetallicTextures ? TextureValueChannelMode.Texture : t.DefaultMetallicIsWhite ? TextureValueChannelMode.White : TextureValueChannelMode.Black;
                TextureValueChannelMode aoChannelMode = t.UsingAmbientOcclusionTextures ? TextureValueChannelMode.Texture : t.DefaultAmbientOcclusionIsBlack ? TextureValueChannelMode.Black : TextureValueChannelMode.White;
                TextureValueChannelMode specularChannelMode = t.UsingSpecularTextures ? TextureValueChannelMode.Texture : t.DefaultSpecularIsBlack ? TextureValueChannelMode.Black : TextureValueChannelMode.White;
                TextureValueChannelMode roughnessChannelMode = t.UsingRoughnessTextures ? TextureValueChannelMode.Texture : t.DefaultRoughnessIsBlack ? TextureValueChannelMode.Black : TextureValueChannelMode.White;

                float metallicValue = t.DefaultMetallicIsWhite ? 1f : 0f;
                float aoValue = t.DefaultAmbientOcclusionIsBlack ? 0f : 1f;
                float specularValue = t.DefaultSpecularIsBlack ? 0f : 1f;
                float roughnessValue = t.DefaultRoughnessIsBlack ? 0f : 1f;

                alloyPM.SelectedModes = new TextureValueChannelMode[] { metallicChannelMode, aoChannelMode, specularChannelMode, roughnessChannelMode };
                alloyPM.ChannelValues = new Vector4(metallicValue, aoValue, specularValue, roughnessValue);

                alloyPM.TexturesGUID = GUIDs;

                alloyPM = Instantiate(alloyPM);
                AlloyCustomImportAction.CreatePostProcessingInformation(assetPath, alloyPM);

                AssetDatabase.SaveAssets();

                if (t.MaterialsList.Count > 0)
                {
                    Texture2D alloyMap = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    Material material = t.MaterialsList[i];
                    material.shader = Shader.Find("Alloy/Core");

                    material.SetTexture(ALLOYMAP_NAME, alloyMap);
                    if (t.BaseColorTextures.Count > 0) material.SetTexture(BASECOLOR_NAME, t.BaseColorTextures[i]);
                    if (t.NormalMapTextures.Count > 0)
                    {
                        string normalMapPath = AssetDatabase.GetAssetPath(t.NormalMapTextures[i]);
                        TextureImporter textureImporter = AssetImporter.GetAtPath(normalMapPath) as TextureImporter;
                        if (textureImporter.textureType != TextureImporterType.NormalMap)
                        {
                            textureImporter.textureType = TextureImporterType.NormalMap;

                            AssetDatabase.ImportAsset(normalMapPath);
                        }

                        material.SetTexture(NORMALMAP_NAME, t.NormalMapTextures[i]);
                    }
                }
            }

            if (textureImportSettingsChanged) AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private string[] GetTextureGUID(int index)
        {
            string assetPath;
            string[] GUIDs = new string[4];

            if (t.UsingMetallicTextures)
            {
                assetPath = AssetDatabase.GetAssetPath(t.MetallicTextures[index]);
                GUIDs[0] = AssetDatabase.AssetPathToGUID(assetPath);
            }

            if (t.UsingAmbientOcclusionTextures)
            {
                assetPath = AssetDatabase.GetAssetPath(t.AmbientOcclusionTextures[index]);
                GUIDs[1] = AssetDatabase.AssetPathToGUID(assetPath);
            }

            if (t.UsingSpecularTextures)
            {
                assetPath = AssetDatabase.GetAssetPath(t.SpecularTextures[index]);
                GUIDs[2] = AssetDatabase.AssetPathToGUID(assetPath);
            }

            if (t.UsingRoughnessTextures)
            {
                assetPath = AssetDatabase.GetAssetPath(t.RoughnessTextures[index]);
                GUIDs[3] = AssetDatabase.AssetPathToGUID(assetPath);
            }

            return GUIDs;
        }

        private void FindFiles()
        {
            string folderPath = AssetDatabase.GetAssetPath(t.GetInstanceID());
            folderPath = Path.GetDirectoryName(folderPath); // Get the folder containing the ScriptableObject.

            string[] textureGUIDs = AssetDatabase.FindAssets("t:Texture", new[] { folderPath });

            t.BaseColorTextures.Clear();
            t.MetallicTextures.Clear();
            t.AmbientOcclusionTextures.Clear();
            t.RoughnessTextures.Clear();
            t.NormalMapTextures.Clear();
            t.MaterialsList.Clear();

            foreach (string textureGUID in textureGUIDs)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(textureGUID);

                if (Path.GetFileNameWithoutExtension(texturePath).Contains("_1BaseColor"))
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    t.BaseColorTextures.Add(texture);
                    EditorUtility.SetDirty(t);
                }
                else if (Path.GetFileNameWithoutExtension(texturePath).Contains("_2AO"))
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    t.AmbientOcclusionTextures.Add(texture);
                    EditorUtility.SetDirty(t);
                }
                else if (Path.GetFileNameWithoutExtension(texturePath).Contains("_3Metallic"))
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    t.MetallicTextures.Add(texture);
                    EditorUtility.SetDirty(t);
                }
                else if (Path.GetFileNameWithoutExtension(texturePath).Contains("_4Roughness"))
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    t.RoughnessTextures.Add(texture);
                    EditorUtility.SetDirty(t);
                }
                else if (Path.GetFileNameWithoutExtension(texturePath).Contains("_5Normal"))
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    t.NormalMapTextures.Add(texture);
                    EditorUtility.SetDirty(t);
                }
            }

            string[] materialGUIDs = AssetDatabase.FindAssets("t:Material", new[] { folderPath });

            foreach (string materialGUID in materialGUIDs)
            {
                string materialPath = AssetDatabase.GUIDToAssetPath(materialGUID);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                t.MaterialsList.Add(material);
                EditorUtility.SetDirty(t);
            }
        }
    }
}