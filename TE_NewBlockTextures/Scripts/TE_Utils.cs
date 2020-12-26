using System;
using HarmonyLib;
using DMT;
using UnityEngine;

public class TE_Utils
{
  public static int FindPaintIdForBlockFace(BlockValue _bv, BlockFace blockFace, out string _name)
  {
    int meshIndex = Block.list[_bv.type].MeshIndex;

    if (meshIndex == 10)
    {
      int sideTextureId = Block.list[_bv.type].GetSideTextureId(_bv, blockFace);

      for (int i = 0; i < TE_BlockTextureData.list.Length; i++)
      {
        if (TE_BlockTextureData.list[i] != null && (int)TE_BlockTextureData.list[i].TextureID == sideTextureId)
        {
          _name = TE_BlockTextureData.list[i].Name;
          return i;
        }
      }
    }

    _name = string.Empty;
    return 0;
  }
}
