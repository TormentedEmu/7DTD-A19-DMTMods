using System;
using System.Runtime.CompilerServices;
using InControl;
using UnityEngine;

public class XUiC_TE_MaterialStack : XUiC_SelectableEntry
{
	public bool IsLocked
	{
    get;
    private set;
	}

	public TE_BlockTextureData TextureData
	{
		get
		{
			return this.textureData;
		}
		set
		{
			this.textMaterial.IsVisible = false;
			base.ViewComponent.Enabled = (value != null);
			if (this.textureData != value)
			{
				this.textureData = value;
				this.isDirty = true;
				if (this.textureData == null)
				{
					this.SetItemNameText("");
					this.IsLocked = false;
				}
				else
				{
					this.textMaterial.IsVisible = true;
					MeshDescription meshDescription = MeshDescription.meshes[10];
					int textureID = (int)this.textureData.TextureID;
          //Log.Out("textureID: {0}", textureID);
					Rect uvrect;
					if (textureID == 0)
					{
						uvrect = WorldConstants.uvRectZero;
					}
					else
					{
            if (meshDescription.textureAtlas.uvMapping.Length <= textureID)
            {
              Log.Out("Length {0} <= {1}", meshDescription.textureAtlas.uvMapping.Length, textureID);
            }
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
					this.SetItemNameText(string.Format("({0}) {1}", this.textureData.ID, this.textureData.LocalizedName));
				}
			}
			if (this.textureData != null)
			{
				if (!(this.textureData.LockedByPerk != ""))
				{
					this.IsLocked = false;
				}
				this.textMaterial.IsVisible = true;
			}
			base.RefreshBindings(false);
		}
	}

	public XUiC_TE_MaterialInfoWindow InfoWindow
	{
    get;
    set;
	}

	protected override void SelectedChanged(bool isSelected)
	{
		if (isSelected)
		{
			this.SetColor(this.selectColor);
			if (base.xui.currentSelectedEntry == this)
			{
				this.InfoWindow.SetMaterial(this.textureData);
				return;
			}
		}
		else
		{
			this.SetColor(XUiC_TE_MaterialStack.backgroundColor);
		}
	}

	private void SetColor(Color32 color)
	{
		this.background.Color = color;
	}

	public override void Init()
	{
		base.Init();

		this.tintedOverlay = base.GetChildById("tintedOverlay");
		this.highlightOverlay = (base.GetChildById("highlightOverlay").ViewComponent as XUiV_Sprite);
		this.background = (base.GetChildById("background").ViewComponent as XUiV_Sprite);
		this.textMaterial = (base.GetChildById("textMaterial").ViewComponent as XUiV_Texture);
		this.textMaterial.Material = new Material(Shader.Find("Unlit/Transparent Colored Emissive TextureArray"));
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		LocalPlayerUI playerUI = base.xui.playerUI;
		UICamera uiCamera = playerUI.uiCamera;
		PlayerActionsGUI guiactions = playerUI.playerInput.GUIActions;
		if (base.WindowGroup.isShowing)
		{
			uiCamera.GetMousePosition();
			bool mouseButtonUp = uiCamera.GetMouseButtonUp(UICamera.MouseButton.LeftButton, false);
			uiCamera.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
			uiCamera.GetMouseButton(UICamera.MouseButton.LeftButton);
			uiCamera.GetMouseButtonUp(UICamera.MouseButton.RightButton, false);
			uiCamera.GetMouseButtonDown(UICamera.MouseButton.RightButton);
			uiCamera.GetMouseButton(UICamera.MouseButton.RightButton);
			if (this.isOver && base.xui.playerUI.uiCamera.hoveredObject == base.ViewComponent.UiTransform.gameObject && base.ViewComponent.EventOnPress)
			{
				if (guiactions.LastInputType == BindingSourceType.DeviceBindingSource)
				{
					bool wasReleased = guiactions.Submit.WasReleased;
					bool wasReleased2 = guiactions.HalfStack.WasReleased;
					bool wasReleased3 = guiactions.Inspect.WasReleased;
					bool wasReleased4 = guiactions.RightStick.WasReleased;
					if (wasReleased && this.textureData != null)
					{
						this.SetSelectedTextureForItem();
					}
				}
				else if (mouseButtonUp && this.textureData != null)
				{
					this.SetSelectedTextureForItem();
				}
			}
			else
			{
				this.currentColor = XUiC_TE_MaterialStack.backgroundColor;
				if (this.highlightOverlay != null)
				{
					this.highlightOverlay.Color = XUiC_TE_MaterialStack.backgroundColor;
				}
				if (!base.Selected)
				{
					this.background.Color = this.currentColor;
				}
				this.lastClicked = false;
				if (this.isOver)
				{
					this.isOver = false;
				}
			}
		}
		if (this.isDirty)
		{
			this.isDirty = false;
		}
	}

	public void SetSelectedTextureForItem()
	{
		if (!this.IsLocked)
		{
			((ItemActionTextureBlock.ItemActionTextureBlockData)base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1]).idx = this.textureData.ID;
			base.xui.playerUI.entityPlayer.inventory.holdingItemItemValue.Meta = (int)((byte)this.textureData.ID);
			base.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[1].invData.itemValue = base.xui.playerUI.entityPlayer.inventory.holdingItemItemValue;
		}
		base.Selected = true;
	}

	private void HandleItemInspect()
	{
		if (this.textureData != null && this.InfoWindow != null)
		{
			base.Selected = true;
		}
	}

	private void SetItemNameText(string name)
	{
		this.viewComponent.ToolTip = ((this.textureData != null) ? name : string.Empty);
	}

	protected override void OnHovered(OnHoverEventArgs _e)
	{
		this.isOver = _e.IsOver;
		if (!base.Selected)
		{
			if (_e.IsOver)
			{
				this.background.Color = XUiC_TE_MaterialStack.highlightColor;
			}
			else
			{
				this.background.Color = XUiC_TE_MaterialStack.backgroundColor;
			}
		}
		base.OnHovered(_e);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.RefreshBindings(false);
	}

	public void ClearSelectedInfoWindow()
	{
		if (base.Selected)
		{
			this.InfoWindow.SetMaterial(null);
		}
	}

	public override bool GetBindingValue(ref string value, BindingItem binding)
	{
		string fieldName = binding.FieldName;
		if (fieldName == "islocked")
		{
			value = this.IsLocked.ToString();
			return true;
		}
		return false;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "select_color"))
			{
				if (!(name == "press_color"))
				{
					if (!(name == "background_color"))
					{
						if (!(name == "highlight_color"))
						{
							if (!(name == "select_sound"))
							{
								return false;
							}
							base.xui.LoadData<AudioClip>(value, delegate(AudioClip o)
							{
								this.selectSound = o;
							});
						}
						else
						{
							XUiC_TE_MaterialStack.highlightColor = StringParsers.ParseColor32(value);
						}
					}
					else
					{
						XUiC_TE_MaterialStack.backgroundColor = StringParsers.ParseColor32(value);
					}
				}
				else
				{
					this.pressColor = StringParsers.ParseColor32(value);
				}
			}
			else
			{
				this.selectColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	public XUiC_TE_MaterialStack()
	{
	}

	// Note: this type is marked as 'beforefieldinit'.
	static XUiC_TE_MaterialStack()
	{
	}


	private bool isDirty = true;

	private bool bHighlightEnabled;

	private bool bDropEnabled = true;

	private AudioClip selectSound;

	private AudioClip placeSound;

	private bool isOver;

	private Color32 currentColor;

	private Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private bool lastClicked;

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	private XUiController tintedOverlay;

	private XUiV_Label stackValue;

	private XUiV_Sprite highlightOverlay;

	private XUiV_Sprite background;

	private XUiV_Texture textMaterial;


	private TE_BlockTextureData textureData;


	private Vector3 startMousePos = Vector3.zero;
}
