using Alloy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cityrobo
{
    [CreateAssetMenu(fileName = "New BulkAlloyMapCreator", menuName = "Tools/BulkAlloyMapCreator", order = 0)]
    public class BulkAlloyMapCreator : ScriptableObject
    {
        public bool UsingMetallicTextures;
        public bool DefaultMetallicIsWhite;
        public bool UsingAmbientOcclusionTextures;
        public bool DefaultAmbientOcclusionIsBlack;
        public bool UsingSpecularTextures;
        public bool DefaultSpecularIsBlack;
        public bool UsingRoughnessTextures;
        public bool DefaultRoughnessIsBlack;

        public bool UsingCustomTextureNames;
        public bool UsingAutomaticTexureNameTruncation;
        public string[] TextureNames;

        public List<Texture2D> MetallicTextures = new List<Texture2D>();
        public List<Texture2D> AmbientOcclusionTextures = new List<Texture2D>();
        public List<Texture2D> SpecularTextures = new List<Texture2D>();
        public List<Texture2D> RoughnessTextures = new List<Texture2D>();

        public string TargetFolder;

        public static PackedMapDefinition PackerDefinition;

        public bool UseMaterialNameForTextureName;
        public List<Material> MaterialsList = new List<Material>();

        public List<Texture2D> BaseColorTextures = new List<Texture2D>();
        public List<Texture2D> NormalMapTextures = new List<Texture2D>();
    }
}