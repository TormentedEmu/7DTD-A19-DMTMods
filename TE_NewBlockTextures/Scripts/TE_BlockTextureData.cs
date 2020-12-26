using System;
using System.IO;
using System.Xml;

public class TE_BlockTextureData
{
  public static TE_BlockTextureData[] list;

  public int ID;
  public ushort TextureID;
  public string Name;
  public string LocalizedName;
  public string Group;
  public ushort PaintCost;
  public bool Hidden;
  public byte SortIndex = byte.MaxValue;
  public string LockedByPerk = "";
  public ushort RequiredLevel;

  public TE_BlockTextureData()
	{
	}

	public static void InitStatic()
	{
    TE_BlockTextureData.list = new TE_BlockTextureData[256];
	}

	public void Init()
	{
		TE_BlockTextureData.list[ID] = this;
	}

	public static void Cleanup()
	{
    TE_BlockTextureData.list = null;
	}

	public static TE_BlockTextureData GetDataByTextureID(int textureID)
	{
		for (int i = 0; i < TE_BlockTextureData.list.Length; i++)
		{
			if (TE_BlockTextureData.list[i] != null && TE_BlockTextureData.list[i].TextureID == textureID)
			{
				return TE_BlockTextureData.list[i];
			}
		}
		return null;
	}

	public bool GetLocked(EntityPlayerLocal player)
	{
		if (LockedByPerk != "")
		{
			ProgressionValue progressionValue = player.Progression.GetProgressionValue(LockedByPerk);
			if (progressionValue != null && ProgressionClass.GetCalculatedLevel(player, progressionValue) >= RequiredLevel)
			{
				return true;
			}
		}
		return false;
	}

	internal static void AddLockedGroup(string _perkName, ushort _requiredLevel, string _materialGroupName)
	{
		for (int i = 0; i < TE_BlockTextureData.list.Length; i++)
		{
			if (TE_BlockTextureData.list[i] != null && TE_BlockTextureData.list[i].Group == _materialGroupName)
			{
        TE_BlockTextureData.list[i].LockedByPerk = _perkName;
        TE_BlockTextureData.list[i].RequiredLevel = _requiredLevel;
			}
		}
	}
}

public class LoadModPainting
{
  public static string MyModFolder = "TE_NewBlockTextures";

  public static void Init()
  {
    TE_BlockTextureData.InitStatic();
    CreateModBlockTextures();
  }

  public static void CreateModBlockTextures()
  {
    string myResourcesPath = Path.Combine(Utils.GetGamePath(), "Mods", MyModFolder, "Config");
    XmlFile xml = new XmlFile(File.ReadAllBytes(Path.Combine(myResourcesPath, "modpainting.xml")));
    XmlElement ele = xml.XmlDoc.DocumentElement;

    if (ele.ChildNodes.Count == 0)
    {
      Log.Out("Error loading modpainting.xml");
      return;
    }

    foreach (XmlNode xmlNode in ele.ChildNodes)
    {
      if (xmlNode.NodeType == XmlNodeType.Comment)
      {
        continue;
      }

      if (xmlNode.NodeType == XmlNodeType.Element && xmlNode.Name.Equals("modpaint"))
      {
        XmlElement xmlElement = (XmlElement)xmlNode;
        DynamicProperties dynamicProperties = new DynamicProperties();

        foreach (object obj2 in xmlElement.ChildNodes)
        {
          XmlNode xmlNode2 = (XmlNode)obj2;
          if (xmlNode2.NodeType == XmlNodeType.Element && xmlNode2.Name.Equals("property"))
          {
            dynamicProperties.Add(xmlNode2, true);
          }
        }

        TE_BlockTextureData blockTextureData = new TE_BlockTextureData();
        blockTextureData.Name = xmlElement.GetAttribute("name");
        blockTextureData.LocalizedName = Localization.Get(blockTextureData.Name);
        blockTextureData.ID = int.Parse(xmlElement.GetAttribute("id"));

        if (dynamicProperties.Values.ContainsKey("Group"))
        {
          blockTextureData.Group = dynamicProperties.Values["Group"];
        }

        if (dynamicProperties.Values.ContainsKey("PaintCost"))
        {
          blockTextureData.PaintCost = Convert.ToUInt16(dynamicProperties.Values["PaintCost"]);
        }
        else
        {
          blockTextureData.PaintCost = 1;
        }

        if (dynamicProperties.Values.ContainsKey("TextureId"))
        {
          blockTextureData.TextureID = Convert.ToUInt16(dynamicProperties.Values["TextureId"]);
        }

        if (dynamicProperties.Values.ContainsKey("Hidden"))
        {
          blockTextureData.Hidden = Convert.ToBoolean(dynamicProperties.Values["Hidden"]);
        }

        if (dynamicProperties.Values.ContainsKey("SortIndex"))
        {
          blockTextureData.SortIndex = Convert.ToByte(dynamicProperties.Values["SortIndex"]);
        }

        blockTextureData.Init();
      }
    }

  }
}
