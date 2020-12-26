using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Audio;
using GUI_2;
using UnityEngine;

public class ItemActionTE_TextureBlock : ItemActionRanged
{
  public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
  {
    return new ItemActionTextureBlock.ItemActionTextureBlockData(_invData, _indexInEntityOfAction, "Muzzle/Particle1");
  }

  public override void ReadFrom(DynamicProperties _props)
  {
    base.ReadFrom(_props);

    if (_props.Values.ContainsKey("RemoveTexture"))
    {
      this.bRemoveTexture = StringParsers.ParseBool(_props.Values["RemoveTexture"], 0, -1, true);
    }

    if (_props.Values.ContainsKey("DefaultTextureID"))
    {
      this.DefaultTextureID = Convert.ToInt32(_props.Values["DefaultTextureID"]);
    }
  }

  protected override int getUserData(ItemActionData _actionData)
  {
    //Log.Out("getUserData");
    ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_actionData;
    //Log.Out("itemActionTextureBlockData.idx: {0}", itemActionTextureBlockData.idx);
    int textureID = (int)TE_BlockTextureData.list[itemActionTextureBlockData.idx].TextureID;
    //Log.Out("textureID: {0}", textureID);
    Color color;

    if (textureID == 0)
    {
      color = Color.gray;
    }
    else
    {
      if (textureID >= MeshDescription.meshes[10].textureAtlas.uvMapping.Length)
      {
        color = Color.gray;
      }
      else
      {
        color = MeshDescription.meshes[10].textureAtlas.uvMapping[textureID].color;
      }
    }

    return ((int)(color.r * 255f) & 255) | ((int)(color.g * 255f) << 8 & 65280) | ((int)(color.b * 255f) << 16 & 16711680);
  }

  public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
  {
    base.ItemActionEffects(_gameManager, _actionData, _firingState, _startPos, _direction, _userData);
    if (_firingState != 0 && _actionData.invData.model != null)
    {
      ParticleSystem[] componentsInChildren = _actionData.invData.model.GetComponentsInChildren<ParticleSystem>();
      for (int i = 0; i < componentsInChildren.Length; i++)
      {
        Renderer component = componentsInChildren[i].GetComponent<Renderer>();
        if (component != null)
        {
          component.material.SetColor("_Color", new Color32((byte)(_userData & 255), (byte)(_userData >> 8 & 255), (byte)(_userData >> 16 & 255), byte.MaxValue));
        }
      }
    }
  }

  public override void StartHolding(ItemActionData _data)
  {
    base.StartHolding(_data);

    ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_data;
    itemActionTextureBlockData.idx = itemActionTextureBlockData.invData.itemValue.Meta;
  }

  public override bool ConsumeScrollWheel(ItemActionData _actionData, float _scrollWheelInput, PlayerActionsLocal _playerInput)
  {
    return false;
  }

  protected override bool checkAmmo(ItemActionData _actionData)
  {
    if (this.InfiniteAmmo || GameStats.GetInt(EnumGameStats.GameModeId) == 2 || GameStats.GetInt(EnumGameStats.GameModeId) == 8)
    {
      return true;
    }

    ItemValue holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
    EntityAlive holdingEntity = _actionData.invData.holdingEntity;
    ItemValue item = ItemClass.GetItem(this.MagazineItemNames[(int)holdingItemItemValue.SelectedAmmoTypeIndex], false);
    return holdingEntity.bag.GetItemCount(item, -1, -1, false) > 0 || holdingEntity.inventory.GetItemCount(item, false, -1, -1) > 0;
  }

  private bool decreaseAmmo(ItemActionData _actionData)
  {
    if (this.InfiniteAmmo || GameStats.GetInt(EnumGameStats.GameModeId) == 2 || GameStats.GetInt(EnumGameStats.GameModeId) == 8)
    {
      return true;
    }

    ItemValue holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
    ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_actionData;
    int num = (int)TE_BlockTextureData.list[itemActionTextureBlockData.idx].PaintCost;
    EntityAlive holdingEntity = _actionData.invData.holdingEntity;
    ItemValue item = ItemClass.GetItem(this.MagazineItemNames[(int)holdingItemItemValue.SelectedAmmoTypeIndex], false);
    bool result = false;
    int itemCount = holdingEntity.bag.GetItemCount(item, -1, -1, false);
    int itemCount2 = holdingEntity.inventory.GetItemCount(item, false, -1, -1);

    if (itemCount + itemCount2 >= num)
    {
      num -= holdingEntity.bag.DecItem(item, num, false);
      if (num > 0)
      {
        num -= holdingEntity.inventory.DecItem(item, num, false);
      }
      result = true;
    }

    return result;
  }

  protected override void ConsumeAmmo(ItemActionData _actionData)
  {
  }

  public override void OnHoldingUpdate(ItemActionData _actionData)
  {
    base.OnHoldingUpdate(_actionData);

    ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_actionData;

    if (itemActionTextureBlockData.bReplacePaintNextTime && Time.time - itemActionTextureBlockData.lastTimeReplacePaintShown > 5f)
    {
      itemActionTextureBlockData.lastTimeReplacePaintShown = Time.time;
      GameManager.ShowTooltip(GameManager.Instance.World.GetLocalPlayers()[0], Localization.Get("ttPaintedTextureReplaced"));
    }
  }

  public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
  {
    if ((double)_actionData.invData.holdingEntity.speedForward > 0.009)
    {
      this.rayCastDelay = AnimationDelayData.AnimationDelay[_actionData.invData.item.HoldType.Value].RayCastMoving;
    }
    else
    {
      this.rayCastDelay = AnimationDelayData.AnimationDelay[_actionData.invData.item.HoldType.Value].RayCast;
    }

    base.ExecuteAction(_actionData, _bReleased);
  }

  protected override Vector3 fireShot(int _shotIdx, ItemActionRanged.ItemActionDataRanged _actionData)
  {
    GameManager.Instance.StartCoroutine(this.fireShotLater(_shotIdx, _actionData));
    return Vector3.zero;
  }

  private static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
  {
    return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
  }

  private bool checkBlockCanBeChanged(World _world, Vector3i _blockPos, BlockValue _blockValue, PersistentPlayerData lpRelative)
  {
    return _world.CanPlaceBlockAt(_blockPos, lpRelative, false);
  }

  private IEnumerator fireShotLater(int _shotIdx, ItemActionRanged.ItemActionDataRanged _actionData)
  {
    yield return new WaitForSeconds(this.rayCastDelay);

    EntityAlive holdingEntity = _actionData.invData.holdingEntity;
    PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(holdingEntity.entityId);
    Vector3 direction = holdingEntity.GetLookVector((_actionData.muzzle != null) ? _actionData.muzzle.forward : Vector3.zero);
    Vector3i vector3i;
    BlockValue blockValue;
    BlockFace blockFace;
    WorldRayHitInfo worldRayHitInfo;

    if (this.getHitBlockFace(_actionData, out vector3i, out blockValue, out blockFace, out worldRayHitInfo) == -1 || worldRayHitInfo == null || !worldRayHitInfo.bHitValid)
    {
      //Log.Out("getHitBlockFace");
      yield break;
    }

    //Log.Out("BlockFace: {0}, blockValue: {1}", blockFace, blockValue);
    ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_actionData;

    ItemInventoryData invData = itemActionTextureBlockData.invData;
    if (this.bRemoveTexture)
    {
      //Log.Out("bRemoveTexture");
      itemActionTextureBlockData.idx = 0;
    }

    ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[worldRayHitInfo.hit.clrIdx];
    if (chunkCluster == null)
    {
      //Log.Out("chunkCluster == null");
      yield break;
    }

    if (!itemActionTextureBlockData.bReplacePaintNextTime)
    {
      //Log.Out("!itemActionTextureBlockData.bReplacePaintNextTime");

      bool flag = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
      switch (itemActionTextureBlockData.paintMode)
      {
        case ItemActionTextureBlock.EnumPaintMode.Single:
        case ItemActionTextureBlock.EnumPaintMode.SingleAllSides:
          if (!this.checkBlockCanBeChanged(GameManager.Instance.World, vector3i, worldRayHitInfo.hit.blockValue, playerDataFromEntityID))
          {
            //Log.Out("!this.checkBlockCanBeChanged");
            yield break;
          }
          if (itemActionTextureBlockData.paintMode == ItemActionTextureBlock.EnumPaintMode.SingleAllSides)
          {
            blockFace = BlockFace.None;
            if (chunkCluster.GetTextureFull(vector3i) == Chunk.TextureIdxToTextureFullValue64(itemActionTextureBlockData.idx))
            {
              //Log.Out("chunkCluster.GetTextureFull(vector3i) == Chunk.TextureIdxToTextureFullValue64(itemActionTextureBlockData.idx)");
              yield break;
            }
          }
          else
          {
            int blockFaceTexture = chunkCluster.GetBlockFaceTexture(vector3i, blockFace);
            //Log.Out("blockFaceTextures: {0}, iatb.idx: {1}", blockFaceTexture, itemActionTextureBlockData.idx);
            if (itemActionTextureBlockData.idx == blockFaceTexture)
            {
              //Log.Out("itemActionTextureBlockData.idx == blockFaceTexture");
              //GameManager.Instance.SetBlockTextureServer(vector3i, blockFace, 7, invData.holdingEntity.entityId);
              yield break;
            }
          }
          if (flag || this.decreaseAmmo(_actionData))
          {
            //Log.Out("flag || this.decreaseAmmo(_actionData)");
            BlockToolSelection.Instance.BeginUndo(chunkCluster.ClusterIdx);
            GameManager.Instance.SetBlockTextureServer(vector3i, blockFace, itemActionTextureBlockData.idx, invData.holdingEntity.entityId);
            BlockToolSelection.Instance.EndUndo(chunkCluster.ClusterIdx, false);
          }
          break;
        case ItemActionTextureBlock.EnumPaintMode.Multiple:
        case ItemActionTextureBlock.EnumPaintMode.Spray:
          {
            float num = (itemActionTextureBlockData.paintMode == ItemActionTextureBlock.EnumPaintMode.Spray) ? 7.5f : 1.25f;
            if (worldRayHitInfo.hitTriangleIdx != -1)
            {
              Vector3 normalized = GameUtils.GetNormalFromHitInfo(worldRayHitInfo.hitCollider, worldRayHitInfo.hitTriangleIdx).normalized;
              Vector3 vector;
              Vector3 vector2;
              if (Utils.FastAbs(normalized.x) >= Utils.FastAbs(normalized.y) && Utils.FastAbs(normalized.x) >= Utils.FastAbs(normalized.z))
              {
                vector = Vector3.up;
                vector2 = Vector3.forward;
              }
              else if (Utils.FastAbs(normalized.y) >= Utils.FastAbs(normalized.x) && Utils.FastAbs(normalized.y) >= Utils.FastAbs(normalized.z))
              {
                vector = Vector3.right;
                vector2 = Vector3.forward;
              }
              else
              {
                vector = Vector3.right;
                vector2 = Vector3.up;
              }
              vector = ItemActionTE_TextureBlock.ProjectVectorOnPlane(normalized, vector).normalized;
              vector2 = ItemActionTE_TextureBlock.ProjectVectorOnPlane(normalized, vector2).normalized;
              Vector3 pos = worldRayHitInfo.hit.pos;
              Vector3 origin = worldRayHitInfo.ray.origin;
              BlockToolSelection.Instance.BeginUndo(chunkCluster.ClusterIdx);
              for (float num2 = -num; num2 <= num; num2 += 0.5f)
              {
                for (float num3 = -num; num3 <= num; num3 += 0.5f)
                {
                  direction = pos + num2 * vector + num3 * vector2 - origin;
                  int hitMask = 69;
                  if (Voxel.Raycast(GameManager.Instance.World, new Ray(origin, direction), this.Range, -555528197, hitMask, 0f))
                  {
                    WorldRayHitInfo worldRayHitInfo2 = Voxel.voxelRayHitInfo.Clone();
                    Vector3i blockPos = worldRayHitInfo2.hit.blockPos;
                    blockValue = worldRayHitInfo2.hit.blockValue;
                    vector3i = worldRayHitInfo2.hit.blockPos;
                    if (blockValue.type != 0 && Block.list[blockValue.type].shape is BlockShapeNew && ((int)Block.list[blockValue.type].MeshIndex == MeshDescription.MESH_OPAQUE || (int)Block.list[blockValue.type].MeshIndex == 10))
                    {
                      Block block = Block.list[blockValue.type];
                      if (blockValue.ischild)
                      {
                        vector3i = block.multiBlockPos.GetParentPos(vector3i, blockValue);
                        blockValue = chunkCluster.GetBlock(vector3i);
                      }
                      if ((int)Block.list[blockValue.type].MeshIndex == MeshDescription.MESH_OPAQUE || (int)Block.list[blockValue.type].MeshIndex == 10)
                      {
                        blockFace = BlockFace.Top;
                        if (Block.list[blockValue.type].shape is BlockShapeNew)
                        {
                          blockFace = GameUtils.GetBlockFaceFromHitInfo(vector3i, blockValue, worldRayHitInfo2.hitCollider, worldRayHitInfo2.hitTriangleIdx);
                        }
                        if (blockFace != BlockFace.None && this.checkBlockCanBeChanged(GameManager.Instance.World, blockPos, worldRayHitInfo2.hit.blockValue, playerDataFromEntityID))
                        {
                          int blockFaceTexture2 = chunkCluster.GetBlockFaceTexture(blockPos, blockFace);
                          if (itemActionTextureBlockData.idx != blockFaceTexture2 && (flag || this.decreaseAmmo(_actionData)))
                          {
                            GameManager.Instance.SetBlockTextureServer(blockPos, blockFace, itemActionTextureBlockData.idx, invData.holdingEntity.entityId);
                          }
                        }
                      }
                    }
                  }
                }
              }

              BlockToolSelection.Instance.EndUndo(chunkCluster.ClusterIdx, false);
            }
            break;
          }
      }

      //Log.Out("End yield break;");
      yield break;
    }

    itemActionTextureBlockData.bReplacePaintNextTime = false;
    if (!this.checkBlockCanBeChanged(GameManager.Instance.World, vector3i, worldRayHitInfo.hit.blockValue, playerDataFromEntityID))
    {
      yield break;
    }

    int blockFaceTexture3 = chunkCluster.GetBlockFaceTexture(vector3i, blockFace);
    if (itemActionTextureBlockData.idx == blockFaceTexture3)
    {
      yield break;
    }

    int num4 = GameManager.Instance.World.ChunkClusters[0].GetBlockFaceTexture(vector3i, blockFace);
    if (num4 == 0)
    {
      string text;
      num4 = TE_Utils.FindPaintIdForBlockFace(blockValue, blockFace, out text);
    }

    if (num4 != itemActionTextureBlockData.idx)
    {
      BlockToolSelection blockToolSelection = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
      if (blockToolSelection == null || !blockToolSelection.SelectionActive)
      {
        this.replacePaintInCurrentPrefab(vector3i, blockFace, num4, itemActionTextureBlockData.idx);
      }
      else
      {
        this.replacePaintInCurrentSelection(vector3i, blockFace, num4, itemActionTextureBlockData.idx);
      }
    }

    yield break;
  }

  private int getHitBlockFace(ItemActionRanged.ItemActionDataRanged _actionData, out Vector3i blockPos, out BlockValue bv, out BlockFace blockFace, out WorldRayHitInfo hitInfo)
  {
    bv = BlockValue.Air;
    blockFace = BlockFace.None;
    hitInfo = null;
    blockPos = Vector3i.zero;
    hitInfo = this.GetExecuteActionTarget(_actionData);

    if (hitInfo == null || !hitInfo.bHitValid || hitInfo.tag == null || !GameUtils.IsBlockOrTerrain(hitInfo.tag))
    {
      //Log.Out("hitInfo == null || !hitInfo.bHitValid || hitInfo.tag == null || !GameUtils.IsBlockOrTerrain(hitInfo.tag)");
      return -1;
    }

    ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[hitInfo.hit.clrIdx];
    if (chunkCluster == null)
    {
      //Log.Out("chunkCluster == null");
      return -1;
    }

    bv = hitInfo.hit.blockValue;
    blockPos = hitInfo.hit.blockPos;
    Block block = Block.list[bv.type];
    if (bv.ischild)
    {
      blockPos = block.multiBlockPos.GetParentPos(blockPos, bv);
      bv = chunkCluster.GetBlock(blockPos);
    }

    if ((int)Block.list[bv.type].MeshIndex != 10)
    {
      //Log.Out("!= Mesh opaque and 10");
      return -1;
    }

    blockFace = BlockFace.Top;
    if (Block.list[bv.type].shape is BlockShapeNew)
    {
      blockFace = GameUtils.GetBlockFaceFromHitInfo(blockPos, bv, hitInfo.hitCollider, hitInfo.hitTriangleIdx);
    }

    if (blockFace == BlockFace.None)
    {
      //Log.Out("blockFace == BlockFace.None");
      return -1;
    }

    //Log.Out("return chunkCluster.GetBlockFaceTexture(blockPos, blockFace): {0}", chunkCluster.GetBlockFaceTexture(blockPos, blockFace));
    return chunkCluster.GetBlockFaceTexture(blockPos, blockFace);
  }

  public void CopyTextureFromWorld(ItemActionRanged.ItemActionDataRanged _actionData)
  {
    if (!(_actionData.invData.holdingEntity is EntityPlayerLocal))
    {
      return;
    }

    Vector3i vector3i;
    BlockValue bv;
    BlockFace blockFace;
    WorldRayHitInfo worldRayHitInfo;

    int num = this.getHitBlockFace(_actionData, out vector3i, out bv, out blockFace, out worldRayHitInfo);

    if (num == -1)
    {
      return;
    }

    if (num == 0)
    {
      string text;
      num = TE_Utils.FindPaintIdForBlockFace(bv, blockFace, out text);
    }

    ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_actionData;
    EntityPlayerLocal player = itemActionTextureBlockData.invData.holdingEntity as EntityPlayerLocal;
    TE_BlockTextureData blockTextureData = TE_BlockTextureData.list[num];

    if (blockTextureData != null && !blockTextureData.GetLocked(player))
    {
      itemActionTextureBlockData.idx = num;
      itemActionTextureBlockData.invData.itemValue.Meta = num;
      itemActionTextureBlockData.invData.itemValue = itemActionTextureBlockData.invData.itemValue;
      return;
    }

    Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
    GameManager.ShowTooltip(player, Localization.Get("ttPaintTextureIsLocked"));
  }

  public void CopyBlockFromWorld(ItemActionRanged.ItemActionDataRanged _actionData)
  {
    if (!(_actionData.invData.holdingEntity is EntityPlayerLocal))
    {
      return;
    }

    WorldRayHitInfo executeActionTarget = this.GetExecuteActionTarget(_actionData);

    if (executeActionTarget == null || !executeActionTarget.bHitValid || executeActionTarget.tag == null || !GameUtils.IsBlockOrTerrain(executeActionTarget.tag))
    {
      return;
    }

    ChunkCluster chunkCluster = GameManager.Instance.World.ChunkClusters[executeActionTarget.hit.clrIdx];

    if (chunkCluster == null)
    {
      return;
    }

    BlockValue blockValue = executeActionTarget.hit.blockValue;
    Vector3i vector3i = executeActionTarget.hit.blockPos;
    Block block = Block.list[blockValue.type];

    if (blockValue.ischild)
    {
      vector3i = block.multiBlockPos.GetParentPos(vector3i, blockValue);
      blockValue = chunkCluster.GetBlock(vector3i);
    }

    if ((int)Block.list[blockValue.type].MeshIndex != MeshDescription.MESH_OPAQUE && (int)Block.list[blockValue.type].MeshIndex != 10)
    {
      return;
    }

    ItemValue itemValue = executeActionTarget.hit.blockValue.ToItemValue();
    itemValue.Texture = chunkCluster.GetTextureFull(vector3i);
    ItemStack itemStack = new ItemStack(itemValue, 99);
    _actionData.invData.holdingEntity.inventory.AddItem(itemStack);
  }

  protected override void onHoldingEntityFired(ItemActionData _actionData)
  {
    if (_actionData.indexInEntityOfAction == 0)
    {
      _actionData.invData.holdingEntity.RightArmAnimationUse = true;
      return;
    }
    _actionData.invData.holdingEntity.RightArmAnimationAttack = true;
  }

  private void replacePaintInCurrentPrefab(Vector3i _blockPos, BlockFace _blockFace, int _searchPaintId, int _replacePaintId)
  {
    //Log.Out("replacePaintInCurrentPrefab");

    World world = GameManager.Instance.World;
    DynamicPrefabDecorator dynamicPrefabDecorator = world.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator();
    if (dynamicPrefabDecorator == null)
    {
      return;
    }

    PrefabInstance prefabInstance = GameUtils.FindPrefabForBlockPos(dynamicPrefabDecorator.GetDynamicPrefabs(), _blockPos);
    if (prefabInstance == null)
    {
      return;
    }

    for (int i = prefabInstance.boundingBoxPosition.x; i <= prefabInstance.boundingBoxPosition.x + prefabInstance.boundingBoxSize.x; i++)
    {
      for (int j = prefabInstance.boundingBoxPosition.z; j <= prefabInstance.boundingBoxPosition.z + prefabInstance.boundingBoxSize.z; j++)
      {
        for (int k = 0; k < 256; k++)
        {
          BlockValue block = world.GetBlock(i, k, j);
          if (block.type != 0)
          {
            long num = world.GetTexture(i, k, j);
            bool flag = false;
            for (int l = 0; l < 6; l++)
            {
              int num2 = (int)(num >> l * 8 & 255L);
              if (num2 == 0)
              {
                string text;
                num2 = TE_Utils.FindPaintIdForBlockFace(block, (BlockFace)l, out text);
              }
              if (num2 == _searchPaintId)
              {
                num &= ~(255L << l * 8);
                num |= (long)_replacePaintId << l * 8;
                flag = true;
              }
            }
            if (flag)
            {
              world.SetTexture(0, i, k, j, num);
            }
          }
        }
      }
    }
  }

  private void replacePaintInCurrentSelection(Vector3i _blockPos, BlockFace _blockFace, int _searchPaintId, int _replacePaintId)
  {
    BlockToolSelection blockToolSelection = GameManager.Instance.GetActiveBlockTool() as BlockToolSelection;
    if (blockToolSelection == null)
    {
      return;
    }

    World world = GameManager.Instance.World;
    Vector3i selectionMin = blockToolSelection.SelectionMin;

    for (int i = selectionMin.x; i < selectionMin.x + blockToolSelection.SelectionSize.x; i++)
    {
      for (int j = selectionMin.z; j < selectionMin.z + blockToolSelection.SelectionSize.z; j++)
      {
        for (int k = selectionMin.y; k < selectionMin.y + blockToolSelection.SelectionSize.y; k++)
        {
          BlockValue block = world.GetBlock(i, k, j);
          if (block.type != 0)
          {
            long num = world.GetTexture(i, k, j);
            bool flag = false;
            for (int l = 0; l < 6; l++)
            {
              int num2 = (int)(num >> l * 8 & 255L);
              if (num2 == 0)
              {
                string text;
                num2 = TE_Utils.FindPaintIdForBlockFace(block, (BlockFace)l, out text);
              }
              if (num2 == _searchPaintId)
              {
                num &= ~(255L << l * 8);
                num |= (long)_replacePaintId << l * 8;
                flag = true;
              }
            }

            if (flag)
            {
              world.SetTexture(0, i, k, j, num);
            }
          }
        }
      }
    }
  }

  public override EnumCameraShake GetCameraShakeType(ItemActionData _actionData)
  {
    return EnumCameraShake.None;
  }

  public override bool ShowAmmoInUI()
  {
    return true;
  }

  public override void SetupRadial(XUiC_Radial _xuiRadialWindow, EntityPlayerLocal _epl)
  {
    ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)_epl.inventory.holdingItemData.actionData[1];
    _xuiRadialWindow.ResetRadialEntries();
    object obj = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
    _xuiRadialWindow.CreateRadialEntry(0, "ui_game_symbol_paint_bucket", "UIAtlas", "", Localization.Get("xuiMaterials"), false);
    _xuiRadialWindow.CreateRadialEntry(1, "ui_game_symbol_paint_brush", "UIAtlas", "", Localization.Get("xuiPaintBrush"), itemActionTextureBlockData.paintMode == ItemActionTextureBlock.EnumPaintMode.Single);
    _xuiRadialWindow.CreateRadialEntry(2, "ui_game_symbol_paint_roller", "UIAtlas", "", Localization.Get("xuiPaintRoller"), itemActionTextureBlockData.paintMode == ItemActionTextureBlock.EnumPaintMode.Multiple);
    object obj2 = obj;
    if (obj2 != null)
    {
      _xuiRadialWindow.CreateRadialEntry(3, "ui_game_symbol_paint_spraygun", "UIAtlas", "", Localization.Get("xuiSprayGun"), itemActionTextureBlockData.paintMode == ItemActionTextureBlock.EnumPaintMode.Spray);
      _xuiRadialWindow.CreateRadialEntry(4, "ui_game_symbol_paint_allsides", "UIAtlas", "", Localization.Get("xuiPaintAllSides"), itemActionTextureBlockData.paintMode == ItemActionTextureBlock.EnumPaintMode.SingleAllSides);
    }
    _xuiRadialWindow.CreateRadialEntry(5, "ui_game_symbol_paint_eyedropper", "UIAtlas", "", Localization.Get("xuiTexturePicker"), false);
    if (obj2 != null)
    {
      _xuiRadialWindow.CreateRadialEntry(6, "ui_game_symbol_paint_copy_block", "UIAtlas", "", Localization.Get("xuiCopyBlock"), false);
      _xuiRadialWindow.CreateRadialEntry(7, "ui_game_symbol_book", "UIAtlas", "", Localization.Get("xuiReplacePaint"), itemActionTextureBlockData.bReplacePaintNextTime);
    }

    _xuiRadialWindow.SetCommonData(UIUtils.ButtonIcon.FaceButtonNorth, new Action<XUiC_Radial, int, XUiC_Radial.RadialContextAbs>(this.handleRadialCommand), null, -1, false);
  }

  private void handleRadialCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
  {
    EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
    ItemClass holdingItem = entityPlayer.inventory.holdingItem;
    ItemInventoryData holdingItemData = entityPlayer.inventory.holdingItemData;
    if (!(holdingItem.Actions[0] is ItemActionTE_TextureBlock) || !(holdingItem.Actions[1] is ItemActionTE_TextureBlock))
    {
      return;
    }

    ItemActionTE_TextureBlock itemActionTextureBlock = (ItemActionTE_TextureBlock)holdingItem.Actions[0];
    ItemActionTE_TextureBlock itemActionTextureBlock2 = (ItemActionTE_TextureBlock)holdingItem.Actions[1];
    ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData = (ItemActionTextureBlock.ItemActionTextureBlockData)holdingItemData.actionData[0];
    ItemActionTextureBlock.ItemActionTextureBlockData itemActionTextureBlockData2 = (ItemActionTextureBlock.ItemActionTextureBlockData)holdingItemData.actionData[1];
    if (_commandIndex != 0)
    {
      itemActionTextureBlockData2.bReplacePaintNextTime = false;
    }

    switch (_commandIndex)
    {
      case 0:
        _sender.xui.playerUI.windowManager.Open("TE_Materials", true, false, true);
        return;
      case 1:
        itemActionTextureBlockData.paintMode = ItemActionTextureBlock.EnumPaintMode.Single;
        itemActionTextureBlockData2.paintMode = ItemActionTextureBlock.EnumPaintMode.Single;
        return;
      case 2:
        itemActionTextureBlockData.paintMode = ItemActionTextureBlock.EnumPaintMode.Multiple;
        itemActionTextureBlockData2.paintMode = ItemActionTextureBlock.EnumPaintMode.Multiple;
        return;
      case 3:
        itemActionTextureBlockData.paintMode = ItemActionTextureBlock.EnumPaintMode.Spray;
        itemActionTextureBlockData2.paintMode = ItemActionTextureBlock.EnumPaintMode.Spray;
        return;
      case 4:
        itemActionTextureBlockData.paintMode = ItemActionTextureBlock.EnumPaintMode.SingleAllSides;
        itemActionTextureBlockData2.paintMode = ItemActionTextureBlock.EnumPaintMode.SingleAllSides;
        return;
      case 5:
        itemActionTextureBlock.CopyTextureFromWorld(itemActionTextureBlockData);
        itemActionTextureBlock2.CopyTextureFromWorld(itemActionTextureBlockData2);
        return;
      case 6:
        itemActionTextureBlock.CopyBlockFromWorld(itemActionTextureBlockData);
        itemActionTextureBlock2.CopyBlockFromWorld(itemActionTextureBlockData2);
        return;
      case 7:
        itemActionTextureBlockData2.bReplacePaintNextTime = true;
        return;
      default:
        return;
    }
  }

  public ItemActionTE_TextureBlock()
  {
  }

  private float rayCastDelay;

  private bool bRemoveTexture;

  public int DefaultTextureID = 1;

  public enum EnumPaintMode
  {
    Single,
    Multiple,
    SingleAllSides,
    Spray
  }

  /*
  public static void InitBlockTextureData()
  {
    TE_list = new BlockTextureData[256];
  }

  public static BlockTextureData[] TE_list;

  public class ItemActionTextureBlockData : ItemActionRanged.ItemActionDataRanged
  {
    public ItemActionTextureBlockData(ItemInventoryData _invData, int _indexInEntityOfAction, string _particleTransform) : base(_invData, _indexInEntityOfAction)
    {
    }

    public int idx = 1;

    public ItemActionTE_TextureBlock.EnumPaintMode paintMode;

    public bool bReplacePaintNextTime;

    public float lastTimeReplacePaintShown;
  }
  */
}
