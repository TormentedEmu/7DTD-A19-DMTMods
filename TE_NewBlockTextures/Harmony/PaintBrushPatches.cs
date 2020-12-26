using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DMT;
using UnityEngine;

namespace TormentedEmu_Mods_A19
{
  public class TE_PaintBrush : IHarmony
  {
    public void Start()
    {
      LoadModPainting.Init();
    }

    [HarmonyPatch(typeof(BlockShapeNew), "renderFace")]
    class BlockShapeNew_renderFace
    {
      /*
        118	0118	ldarg.0
        119	0119	ldfld	int32 BlockShape::MeshIndex
        120	011E	ldc.i4.s	10
        121	0120	bne.un.s	124 (0126) ldc.r4 0
        122	0122	ldloc.s	num2 (7)
        123	0124	stloc.s	num3 (8)
      */
      static int mi;
      static int id;
      static int texid;

      static FieldInfo meshIndex = AccessTools.Field(typeof(BlockShapeNew), "MeshIndex");
      static MethodInfo m_MyExtraMethod = SymbolExtensions.GetMethodInfo(() => GetTextureId(mi, id, ref texid));

      static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
      {
        var found = false;
        int count = -1;
        foreach (var instruction in instructions)
        {
          count++;

          if (!found && count == 119)
          {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, meshIndex);
            yield return new CodeInstruction(OpCodes.Ldloc_S, 7);
            yield return new CodeInstruction(OpCodes.Ldloca_S, 8);
            yield return new CodeInstruction(OpCodes.Call, m_MyExtraMethod);
            found = true;
          }

          yield return instruction;
        }
      }

      static void GetTextureId(int meshIndex, int id, ref int textureId)
      {
        if (meshIndex == 10)
        {
          if (TE_BlockTextureData.list == null)
          {
            Log.Error("TE_BlockTextureData.list == null, id: {0}", id);
            textureId = 1;
            return;
          }
          
          if (TE_BlockTextureData.list[id] == null)
          {
            Log.Error("ModPaintBrush: Missing paint ID XML entry: {0}", id);
            textureId = 1;
          }
          else
          {
            textureId = (int)TE_BlockTextureData.list[id].TextureID;
          }
        }
      }
    }

    [HarmonyPatch(typeof(XUiC_ItemStack), "set_ItemStack")]
    public class XUiC_ItemStack_set_ItemStack
    {
      static AccessTools.FieldRef<XUiC_ItemStack, ItemClass> itemClass = AccessTools.FieldRefAccess<XUiC_ItemStack, ItemClass>("itemClass");
      static AccessTools.FieldRef<XUiC_ItemStack, ItemStack> itemStack = AccessTools.FieldRefAccess<XUiC_ItemStack, ItemStack>("itemStack");
      static XUiC_ItemStack _ItemStack = null;
      static MethodInfo _MyCheckMethod = SymbolExtensions.GetMethodInfo(() => CheckIfModPaintBrush(ref _ItemStack));

      static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
      {
        var foundFirst = false;
        var foundBranch = false;
        var next = false;
        var branchEndIndex = -1;
        Label myNewBranch = ilGenerator.DefineLabel();        
        Label label = default(Label);
        var codes = new List<CodeInstruction>(instructions);
        
        for (int i = 0; i < codes.Count; i++)
        {
          if (next && codes[i].opcode == OpCodes.Brfalse)
          {
            label = (Label)codes[i].operand;
            codes[i].operand = myNewBranch;
            next = false;
            foundBranch = true;
          }

          if (!foundFirst && codes[i].Is(OpCodes.Isinst, typeof(ItemActionTextureBlock)))
          {
            foundFirst = true;
            next = true;
          }
          
          if (foundBranch && codes[i].labels != null && codes[i].labels.Count > 0)
          {
            foreach (var l in codes[i].labels)
            {
              if (l.Equals(label))
              {
                branchEndIndex = i;
                break;
              }
            }
          }
        }

        if (branchEndIndex > -1)
        {
          var newCodes = new List<CodeInstruction>()
          {
            new CodeInstruction(OpCodes.Ldarga, 0) { labels = new List<Label>() { myNewBranch } },
            new CodeInstruction(OpCodes.Call, _MyCheckMethod),
          };

          codes.InsertRange(branchEndIndex, newCodes.AsEnumerable());
        }
        
        return codes.AsEnumerable();
      }

      static void CheckIfModPaintBrush(ref XUiC_ItemStack __instance)
      {        
        if (itemClass(__instance).Actions[0] is ItemActionTE_TextureBlock)
        {
          __instance.backgroundTexture.IsVisible = false;
          if (itemStack(__instance).itemValue.Meta == 0)
          {
            itemStack(__instance).itemValue.Meta = (itemClass(__instance).Actions[0] as ItemActionTE_TextureBlock).DefaultTextureID;
          }

          __instance.backgroundTexture.IsVisible = true;
          MeshDescription meshDescription = MeshDescription.meshes[10];

          if (TE_BlockTextureData.list[itemStack(__instance).itemValue.Meta] == null)
          {
            Log.Error("TE_BlockTextureData List at {0} is null", itemStack(__instance).itemValue.Meta);
            return;
          }

          int textureID = (int)TE_BlockTextureData.list[itemStack(__instance).itemValue.Meta].TextureID;
          Rect uvrect;

          if (textureID == 0)
          {
            uvrect = WorldConstants.uvRectZero;
          }
          else
          {
            if (textureID >= meshDescription.textureAtlas.uvMapping.Length)
            {
              return;
            }

            uvrect = meshDescription.textureAtlas.uvMapping[textureID].uv;
          }

          __instance.backgroundTexture.Texture = meshDescription.textureAtlas.diffuseTexture;

          if (meshDescription.bTextureArray)
          {
            __instance.backgroundTexture.Material.SetTexture("_BumpMap", meshDescription.textureAtlas.normalTexture);
            __instance.backgroundTexture.Material.SetFloat("_Index", (float)meshDescription.textureAtlas.uvMapping[textureID].index);
            __instance.backgroundTexture.Material.SetFloat("_Size", (float)meshDescription.textureAtlas.uvMapping[textureID].blockW);
          }
          else
          {
            __instance.backgroundTexture.UVRect = uvrect;
          }
        }        
      }
    }
  }
}