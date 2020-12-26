using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using DMT;
using UnityEngine;

namespace TormentedEmu_Mods_A19
{
  /// <summary>
  /// Loads a new set of texture arrays(diff, norm, spec) and appends it to the fun pimps mesh array
  /// </summary>
  public class TE_NewBlockTextures : IHarmony
  {
    public static string MyModFolder = "TE_NewBlockTextures";
    public static string MyUnityBundle = "NewBlockTest.unity3d";
    public static string MyUVMappingXml = "NewBlockTestUVMapping.xml";
    public static string MyTextureArrayMeshName = "TE_TextureSample";
    public static string MyTextureArrayDiffuse = "ta_NewBlockTest";
    public static string MyTextureArrayNormal = "ta_NewBlockTest_n";
    public static string MyTextureArraySpecular = "ta_NewBlockTest_s";

    public void Start()
    {
      var harmony = new Harmony("TormentedEmu.Mods.A19");
      harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    [HarmonyPatch(typeof(MeshDescriptionCollection), "LoadTextureArraysForQuality", new Type[] { typeof(bool) })]
    public class MDCLoadTex
    {
      public static void Postfix(ref MeshDescriptionCollection __instance)
      {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        string myResourcesPath = Path.Combine(Utils.GetGamePath(), "Mods", MyModFolder, "Resources");
        string myConfigPath = Path.Combine(Utils.GetGamePath(), "Mods", MyModFolder, "Config");
        string myBundlePath = Path.Combine(myResourcesPath, MyUnityBundle);

        MeshDescription newBlockTexMD = new MeshDescription(__instance.meshes[0]);
        newBlockTexMD.Name = MyTextureArrayMeshName;
        newBlockTexMD.ShaderName = __instance.meshes[0].ShaderName;
        newBlockTexMD.SecondaryShader = __instance.meshes[0].SecondaryShader;
        newBlockTexMD.bTextureArray = true;
        newBlockTexMD.MetaData = new TextAsset(File.ReadAllText(Path.Combine(myConfigPath, MyUVMappingXml)));

        AssetBundleManager.Instance.LoadAssetBundle(myBundlePath, false);

        newBlockTexMD.TexDiffuse = AssetBundleManager.Instance.Get<Texture2DArray>(myBundlePath, MyTextureArrayDiffuse);
        newBlockTexMD.TexNormal = AssetBundleManager.Instance.Get<Texture2DArray>(myBundlePath, MyTextureArrayNormal);
        newBlockTexMD.TexSpecular = AssetBundleManager.Instance.Get<Texture2DArray>(myBundlePath, MyTextureArraySpecular);

        Array.Resize(ref __instance.meshes, 11);
        __instance.meshes[10] = newBlockTexMD;
        MeshDescription.MESH_LENGTH = 11;

        sw.Stop();
        Log.Out("Load new texture arrays complete.  Elapsed time: {0}", sw.Elapsed);
      }
    }

    [HarmonyPatch(typeof(MeshDescription), "LoadTextureArraysForQuality")]
    public class MeshDescription_LoadTextureArraysForQuality
    {
      public static bool Prefix(ref MeshDescription __instance, MeshDescriptionCollection _meshDescriptionCollection, int _index, int _quality, bool _isReload = false)
      {
        if (_index > MeshDescription.MESH_OPAQUE2)
        {
          Log.Out("_index > MESH_OPAQUE2(9) so let's not load/reload.  Skipping LoadTextureArraysForQuality for index {0}", _index);
          return false;
        }

        return true;
      }
    }
  }
}
