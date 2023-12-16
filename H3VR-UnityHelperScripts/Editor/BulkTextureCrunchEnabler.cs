using Alloy;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Cityrobo
{
    [CreateAssetMenu(fileName = "New BulkTextureCrunchEnabler", menuName = "Tools/BulkTextureCrunchEnabler", order = 0)]
    public class BulkTextureCrunchEnabler : ScriptableObject
    {
        public Texture2D ReferenceTextureInFolder;
        public bool EnableCrunch = true;
        [Range(0, 100)]
        public int CompressionQuality = 50;

        [ContextMenu("Enable Crunch in Folder")]
        private void EnableCrunchinFolder()
        {
            string folderPath = AssetDatabase.GetAssetPath(GetInstanceID());
            folderPath = Path.GetDirectoryName(folderPath); // Get the folder containing the ScriptableObject.

            string[] textureGUIDs = AssetDatabase.FindAssets("t:Texture", new[] { folderPath });
            foreach (string textureGUID in textureGUIDs)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(textureGUID);
                TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (textureImporter.crunchedCompression != EnableCrunch)
                {
                    textureImporter.crunchedCompression = EnableCrunch;
                    textureImporter.compressionQuality = CompressionQuality;
                    AssetDatabase.ImportAsset(texturePath);
                }
            }
        }
    }
}