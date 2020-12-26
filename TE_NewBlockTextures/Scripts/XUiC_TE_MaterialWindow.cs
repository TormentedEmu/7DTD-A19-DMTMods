using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

public class XUiC_TE_MaterialWindow : XUiController
{
  public event XUiEvent_PageNumberChangedEventHandler PageNumberChanged;

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			this.page = value;
			this.pageNumber.Text = (this.page + 1).ToString();
      this.PageNumberChanged?.Invoke(this.page);
    }
	}

	public override void Init()
	{
    LoadModPainting.Init();
		base.Init();

    //Log.Out("XUiC_TE_MaterialWindow {0}, {1}", WindowGroup.ID, WindowGroup.Id);
		this.windowicon = base.GetChildById("windowicon");
		this.headerName = (XUiV_Label)base.GetChildById("windowName").ViewComponent;
		this.resultCount = (XUiV_Label)base.GetChildById("resultCount").ViewComponent;
		base.GetChildById("pageUp").OnPress += new XUiEvent_OnPressEventHandler(this.HandlePageUpPress);
		base.GetChildById("pageDown").OnPress += new XUiEvent_OnPressEventHandler(this.HandlePageDownPress);
		this.handlePageDownAction = new Action(this.HandlePageDown);
		this.handlePageUpAction = new Action(this.HandlePageUp);
		this.pageNumber = (XUiV_Label)base.GetChildById("pageNumber").ViewComponent;
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnScroll += this.HandleOnScroll;
		}
		base.OnScroll += this.HandleOnScroll;
		this.paintbrushButton = base.GetChildById("paintbrush");
		this.paintbrushButton.OnPress += new XUiEvent_OnPressEventHandler(this.paintBrushButton_OnPress);
		this.paintrollerButton = base.GetChildById("paintroller");
		this.paintrollerButton.OnPress += new XUiEvent_OnPressEventHandler(this.paintrollerButton_OnPress);
		this.materialGrid = base.Parent.GetChildByType<XUiC_TE_MaterialStackGrid>();
		XUiController[] childrenByType = this.materialGrid.GetChildrenByType<XUiC_TE_MaterialStack>(null);
		XUiController[] array = childrenByType;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].OnScroll += this.HandleOnScroll;
		}
		this.length = array.Length;
		this.txtInput = (XUiC_TextInput)this.windowGroup.Controller.GetChildById("searchInput");
		if (this.txtInput != null)
		{
			this.txtInput.OnChangeHandler += this.HandleOnChangedHandler;
			this.txtInput.Text = "";
		}
		this.lblPaintBrush = Localization.Get("lblPaintBrush");
		this.lblPaintRoller = Localization.Get("lblPaintRoller");
		this.lblTotal = Localization.Get("lblTotalItems");
		this.SelectedButton = this.paintbrushButton;
	}

	private void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.Page = 0;
		this.materialGrid.Page = 0;
		this.GetMaterialData(this.txtInput.Text);
		this.materialGrid.SetMaterials(this.currentItems, ((ItemActionTextureBlock.ItemActionTextureBlockData)base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1]).idx);
	}

	public XUiController SelectedButton
	{
		get
		{
			return this.selectedButton;
		}
		set
		{
			if (this.selectedButton != null)
			{
				(this.selectedButton.ViewComponent as XUiV_Button).Selected = false;
			}
			this.selectedButton = value;
			if (this.selectedButton != null)
			{
				(this.selectedButton.ViewComponent as XUiV_Button).Selected = true;
			}
		}
	}

	private void paintBrushButton_OnPress(XUiController _sender, EventArgs _e)
	{
		this.SelectedButton = this.paintbrushButton;
		this.IsDirty = true;
	}

	private void paintrollerButton_OnPress(XUiController _sender, EventArgs _e)
	{
		this.SelectedButton = this.paintrollerButton;
		this.IsDirty = true;
	}

	private void HandleOnScroll(XUiController _sender, OnScrollEventArgs _e)
	{
		if (_e.Delta > 0f)
		{
			this.HandlePageDown();
			return;
		}
		this.HandlePageUp();
	}

	private void HandlePageDownPress(XUiController _sender, EventArgs _e)
	{
		this.HandlePageDown();
	}

	private void HandlePageDown()
	{
		if (this.page > 0)
		{
			int num = this.Page;
			this.Page = num - 1;
			this.materialGrid.Page = this.page;
		}
	}

	private void HandlePageUpPress(XUiController _sender, EventArgs _e)
	{
		this.HandlePageUp();
	}

	private void HandlePageUp()
	{
		if ((this.page + 1) * this.length < this.currentItems.Count)
		{
			int num = this.Page;
			this.Page = num + 1;
			this.materialGrid.Page = this.page;
		}
	}

	private void GetMaterialData(string _name)
	{
		if (_name == null)
		{
			_name = "";
		}
		this.currentItems.Clear();
		this.length = this.materialGrid.Length;
		this.Page = 0;
		this.materialGrid.Page = 0;
		this.FilterByName(_name);
	}

	public void FilterByName(string _name)
	{
		bool flag = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) && GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		this.currentItems.Clear();
		for (int i = 0; i < TE_BlockTextureData.list.Length; i++)
		{
			TE_BlockTextureData blockTextureData = TE_BlockTextureData.list[i];
			if (blockTextureData != null && (!blockTextureData.Hidden || flag))
			{
				if (_name != "")
				{
					string name = blockTextureData.Name;
					if (_name == "" || name.ContainsCaseInsensitive(_name))
					{
						this.currentItems.Add(blockTextureData);
					}
				}
				else
				{
					this.currentItems.Add(blockTextureData);
				}
			}
		}
		this.resultCount.Text = string.Format(this.lblTotal, this.currentItems.Count.ToString());
	}

	public override void OnOpen()
	{
		base.OnOpen();

		this.GetMaterialData(this.txtInput.Text);
		this.materialCount = this.currentItems.Count;
		this.IsDirty = true;
		this.materialGrid.SetMaterials(this.currentItems, ((ItemActionTextureBlock.ItemActionTextureBlockData)base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1]).idx);
		int holdingItemIdx = base.xui.playerUI.entityPlayer.inventory.holdingItemIdx;
		XUiController childByType = ((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>();
		base.xui.dragAndDrop.InMenu = true;
		if (childByType != null)
		{
			//(childByType as XUiC_Toolbelt).GetSlotControl(holdingItemIdx).AssembleLock = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();

		base.xui.currentToolTip.ToolTip = "";
		int holdingItemIdx = base.xui.playerUI.entityPlayer.inventory.holdingItemIdx;
		XUiController childByType = ((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>();
		base.xui.dragAndDrop.InMenu = false;
		if (childByType != null)
		{
			//(childByType as XUiC_Toolbelt).GetSlotControl(holdingItemIdx).AssembleLock = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);

		if (this.viewComponent.IsVisible)
		{
			XUi.HandlePaging(base.xui, this.handlePageUpAction, this.handlePageDownAction);
			if (null != base.xui.playerUI && base.xui.playerUI.playerInput != null && base.xui.playerUI.playerInput.GUIActions != null)
			{
				PlayerActionsGUI guiactions = base.xui.playerUI.playerInput.GUIActions;
			}
			if (this.IsDirty)
			{
				base.RefreshBindings(false);
				this.materialGrid.IsDirty = true;
			}
		}
	}

	public XUiC_TE_MaterialWindow()
	{
	}

	private XUiV_Label headerName;

	private XUiV_Label pageNumber;

	private XUiV_Label resultCount;

	private XUiC_TE_MaterialStackGrid materialGrid;

	private int page;

	private int length;

	private int materialCount;

	private XUiC_TextInput txtInput;

	private XUiController paintbrushButton;

	private XUiController paintrollerButton;

	private XUiController windowicon;

	private string lblPaintBrush;

	private string lblPaintRoller;

	private string lblPaintEyeDropper;

	private string lblCopyBlock;

	private string lblTotal;

	private Action handlePageDownAction;

	private Action handlePageUpAction;

	private XUiController selectedButton;

	private List<TE_BlockTextureData> currentItems = new List<TE_BlockTextureData>();
}
