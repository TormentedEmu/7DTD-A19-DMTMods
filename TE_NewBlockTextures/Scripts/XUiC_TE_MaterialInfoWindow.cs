using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class XUiC_TE_MaterialInfoWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.textMaterial = (base.GetChildById("textMaterial").ViewComponent as XUiV_Texture);
		this.textMaterial.Material = new Material(Shader.Find("Unlit/Transparent Colored Emissive TextureArray"));
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty && base.ViewComponent.IsVisible)
		{
			this.IsDirty = false;
		}
	}

	public override bool GetBindingValue(ref string value, BindingItem binding)
	{
		string fieldName = binding.FieldName;

    switch (fieldName)
    {
      case "materialname":
        {
          value = ((this.TextureData != null) ? Localization.Get(this.TextureData.Name) : "");
          return true;
        }

      case "perklevel":
        {
          value = ((this.TextureData != null) ? this.perklevelFormatter.Format(this.TextureData.RequiredLevel) : "");
          return true;
        }

      case "paintcost":
        {
          value = ((this.TextureData != null) ? this.paintcostFormatter.Format(this.TextureData.PaintCost) : "");
          return true;
        }

      case "paintunit":
        {
          value = ((this.TextureData != null) ? Localization.Get("xuiPaintUnit") : "");
          return true;
        }

      case "paintcosttitle":
        {
          value = ((this.TextureData != null) ? Localization.Get("xuiPaintCost") : "");
          return true;
        }

      case "requiredtitle":
        {
          value = ((this.TextureData != null) ? Localization.Get("xuiRequired") : "");
          return true;
        }

      case "group":
        {
          value = ((this.TextureData != null) ? Localization.Get(this.TextureData.Group) : "");
          return true;
        }

      case "perk":
        {
          value = "";
          if (this.TextureData != null && this.TextureData.LockedByPerk != "")
          {
            ProgressionValue progressionValue = base.xui.playerUI.entityPlayer.Progression.GetProgressionValue(this.TextureData.LockedByPerk);
            value = Localization.Get(progressionValue.ProgressionClass.NameKey);
          }
          return true;
        }

      case "hasperklock":
        {
          value = ((this.TextureData != null) ? (this.TextureData.LockedByPerk != "").ToString() : "false");
          return true;
        }

      case "grouptitle":
        {
          value = ((this.TextureData != null) ? Localization.Get("xuiMaterialGroup") : "");
          return true;
        }
    }

    return false;
  }

	public void SetMaterial(TE_BlockTextureData newTexture)
	{
		this.TextureData = newTexture;
		this.textMaterial.IsVisible = false;
		if (this.TextureData != null)
		{
			this.textMaterial.IsVisible = true;
			MeshDescription meshDescription = MeshDescription.meshes[10];
			int textureID = (int)this.TextureData.TextureID;
			Rect uvrect;
			if (textureID == 0)
			{
				uvrect = WorldConstants.uvRectZero;
			}
			else
			{
				uvrect = meshDescription.textureAtlas.uvMapping[textureID].uv;
			}
			this.textMaterial.Texture = meshDescription.textureAtlas.diffuseTexture;
			if (meshDescription.bTextureArray)
			{
				this.textMaterial.Material.SetTexture("_BumpMap", meshDescription.textureAtlas.normalTexture);
				this.textMaterial.Material.SetFloat("_Index", (float)meshDescription.textureAtlas.uvMapping[textureID].index);
				this.textMaterial.Material.SetFloat("_Size", (float)meshDescription.textureAtlas.uvMapping[textureID].blockW);
			}
			else
			{
				this.textMaterial.UVRect = uvrect;
			}
		}
		base.RefreshBindings(false);
	}

	public XUiC_TE_MaterialInfoWindow()
	{
	}

	private TE_BlockTextureData TextureData;

	private XUiV_Texture textMaterial;

	private readonly CachedStringFormatter<ushort> paintcostFormatter = new CachedStringFormatter<ushort>((ushort _i) => _i.ToString());

	private readonly CachedStringFormatter<ushort> perklevelFormatter = new CachedStringFormatter<ushort>((ushort _i) => _i.ToString());


}
