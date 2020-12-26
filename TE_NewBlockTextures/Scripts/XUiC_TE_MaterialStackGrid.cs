using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UniLinq;

public class XUiC_TE_MaterialStackGrid : XUiController
{
  protected int curPageIdx;
  protected int numPages;
  private int page;
  private bool isDirty;
  protected XUiController[] materialControllers;
  protected int[] materialIndices;
  private bool bAwakeCalled;
  private TE_BlockTextureData selectedMaterial;
  private List<TE_BlockTextureData> currentList;

  public int Length { get; private set; }

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			this.page = value;
			this.isDirty = true;
		}
	}

  public XUiC_TE_MaterialStackGrid()
  {
  }

  public override void Init()
	{
		base.Init();

		XUiController[] childrenByType = base.GetChildrenByType<XUiC_TE_MaterialStack>(null);
		this.materialControllers = childrenByType;
		this.Length = this.materialControllers.Length;
		this.bAwakeCalled = true;
		this.IsDirty = false;
		this.IsDormant = true;
	}

	public void SetMaterials(List<TE_BlockTextureData> materialIndexList, int newSelectedMaterial = -1)
	{
		bool isCreative = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) && GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		materialIndexList = (from m in materialIndexList
		orderby m.GetLocked(this.xui.playerUI.entityPlayer), m.ID != 0 & isCreative, m.Group, m.ID, m.LocalizedName
		select m).ToList<TE_BlockTextureData>();
		XUiC_TE_MaterialInfoWindow childByType = base.xui.GetChildByType<XUiC_TE_MaterialInfoWindow>();
		int count = materialIndexList.Count;
		this.currentList = materialIndexList;
		if (newSelectedMaterial != -1)
		{
			this.selectedMaterial = TE_BlockTextureData.list[newSelectedMaterial];
			if (selectedMaterial != null && this.selectedMaterial.Hidden && !isCreative)
			{
				this.selectedMaterial = null;
			}
		}
		for (int i = 0; i < this.Length; i++)
		{
			int num = i + this.Length * this.page;
			XUiC_TE_MaterialStack xuiC_MaterialStack = (XUiC_TE_MaterialStack)this.materialControllers[i];
			xuiC_MaterialStack.InfoWindow = childByType;
			if (num < count)
			{
				xuiC_MaterialStack.TextureData = materialIndexList[num];
				if (xuiC_MaterialStack.TextureData == this.selectedMaterial)
				{
					xuiC_MaterialStack.Selected = true;
				}
				if (xuiC_MaterialStack.Selected && xuiC_MaterialStack.TextureData != this.selectedMaterial)
				{
					xuiC_MaterialStack.Selected = false;
				}
			}
			else
			{
				xuiC_MaterialStack.TextureData = null;
				if (xuiC_MaterialStack.Selected)
				{
					xuiC_MaterialStack.Selected = false;
				}
			}
		}
		if (this.selectedMaterial == null && newSelectedMaterial != -1)
		{
			for (int j = 0; j < this.materialControllers.Length; j++)
			{
				XUiC_TE_MaterialStack xuiC_MaterialStack2 = this.materialControllers[j] as XUiC_TE_MaterialStack;
				if (xuiC_MaterialStack2.TextureData != null && !xuiC_MaterialStack2.IsLocked)
				{
					xuiC_MaterialStack2.SetSelectedTextureForItem();
					xuiC_MaterialStack2.Selected = true;
					return;
				}
			}
		}
		this.IsDirty = false;
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		this.IsDormant = false;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
		this.IsDormant = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.isDirty)
		{
			this.SetMaterials(this.currentList, ((ItemActionTextureBlock.ItemActionTextureBlockData)base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1]).idx);
			this.isDirty = false;
		}
	}
}
