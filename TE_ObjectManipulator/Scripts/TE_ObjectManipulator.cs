using UnityEngine;

public class TE_ObjectManipulator : MonoBehaviour
{
  EntityAlive _EntityAlive;
  EntityPlayerLocal _Player;
  PlayerMoveController _PlayerMoveController;
  Animator _Animator;
  AvatarLocalPlayerController _AvatarLocalPlayerController;
  Transform _currentTarget;

  string _LocalPosX;
  string _LocalPosY;
  string _LocalPosZ;

  string _LocalRotX;
  string _LocalRotY;
  string _LocalRotZ;

  string _MoveAmount;
  string _FindTransformText;
  string _FoundResult;
  Transform _FindTransform;
  Translate _TranslateSel;
  string[] _TranslateTypes;
  GameObject _Cube;
  bool _CursorEnabled;

  public Transform CurrentTarget
  {
    get
    {
      return _currentTarget;
    }

    set
    {
      _currentTarget = value;
      var rootEntityT = RootTransformRefEntity.FindEntityUpwards(_currentTarget);
      if (rootEntityT != null)
      {
        foreach (var c in rootEntityT.GetComponentsInChildren<Component>())
        {
          if (c is Animator)
          {
            _Animator = c as Animator;
            break;
          }
        }
      }
      UpdatePrimitive();
    }
  }

  public void Init(EntityAlive entityAlive)
  {
    _EntityAlive = entityAlive;

    if (_currentTarget == null)
    {
      _currentTarget = _EntityAlive.transform;
    }

    if (_Player == null && GameManager.Instance.World != null)
    {
      _Player = GameManager.Instance.World.GetPrimaryPlayer();
      var playerRagdoll = _currentTarget.FindInChilds("player_" + (_Player.IsMale ? "male" : "female") + "Ragdoll", false);
      _Animator = playerRagdoll.GetComponent<Animator>();
      _AvatarLocalPlayerController = _currentTarget.GetComponent<AvatarLocalPlayerController>();
    }

    if (_Cube == null)
    {
      _Cube = AddPrimitive(PrimitiveType.Cube, 0.2f, Color.blue);
      _Cube.transform.parent = _currentTarget;
      _Cube.transform.position = _currentTarget.position;
    }
  }

  public void ToggleActive(bool active)
  {
    gameObject.SetActive(active);
    _Cube.SetActive(active);
  }

  void Awake()
  {
    _MoveAmount = "0.1";
    _TranslateSel = Translate.PosX;
    _TranslateTypes = new string[] { "Pos X", "Pos Y", "Pos Z", "Rot X", "Rot Y", "Rot Z", "Scale X", "Scale Y", "Scale Z", "Scale All", };
    _FindTransformText = "Find";
    _FoundResult = "";
    _CursorEnabled = false;
  }

  void Update()
  {
    if (Input.GetKeyUp(KeyCode.F2) && _Player != null)
    {
      _CursorEnabled = !_CursorEnabled;
      GameManager.Instance.SetCursorEnabledOverride(_CursorEnabled, _CursorEnabled);
      _Player.SetControllable(!_CursorEnabled);
      _Player.playerInput.Enabled = !_CursorEnabled;
    }

    if (_CursorEnabled && Cursor.visible == false)
    {
      GameManager.Instance.SetCursorEnabledOverride(_CursorEnabled, _CursorEnabled);
    }
  }

  private void OnGUI()
  {
    if (GameManager.Instance.World == null)
    {
      return;
    }

    if (_currentTarget != null)
    {
      GUILayout.TextField(_currentTarget.GetThisPath());

      GUI.Box(new Rect(5, 185, 500, 1000), "");

      if (GUI.Button(new Rect(8, 580, 100, 25), "Increase"))
      {
        TranslatePos(_MoveAmount, _TranslateSel);
        GUI.changed = false;
      }

      if (GUI.Button(new Rect(108, 580, 100, 25), "Decrease"))
      {
        TranslatePos("-" + _MoveAmount, _TranslateSel);
        GUI.changed = false;
      }

      if (GUI.Button(new Rect(8, 190, 50, 25), "Root"))
      {
        ChangeTargetToRoot();
        GUI.changed = false;
      }

      if (GUI.Button(new Rect(58, 190, 50, 25), "Parent"))
      {
        ChangeTargetToParent();
        GUI.changed = false;
      }

      if (GUI.Button(new Rect(108, 190, 50, 25), "Sibling"))
      {
        ChangeTargetToNextSibling();
        GUI.changed = false;
      }

      if (GUI.Button(new Rect(158, 190, 50, 25), "Child 0"))
      {
        ChangeTargetToFirstChild();
        GUI.changed = false;
      }

      if (GUI.Button(new Rect(220, 190, 80, 25), "Primitive"))
      {
        _Cube.SetActive(!_Cube.activeSelf);
        GUI.changed = false;
      }

      if (GUI.Button(new Rect(310, 190, 80, 25), "Animator"))
      {
        if (_Animator != null)
          _Animator.enabled = !_Animator.enabled;

        GUI.changed = false;
      }

      if (GUI.Button(new Rect(8, 650, 80, 25), "Find"))
      {
        _FindTransform = FindTransform(_FindTransformText);

        if (_FindTransform != null)
        {
          _FoundResult = _FindTransform.GetThisPath();
        }
        else
        {
          _FoundResult = "Not Found";
        }

        GUI.changed = false;
      }

      if (GUI.Button(new Rect(89, 650, 80, 25), "Set") && _FindTransform != null)
      {
        _currentTarget = _FindTransform;
        GUI.changed = false;
      }

      GUI.Label(new Rect(8, 215, 1000, 25), "Current Transform: " + _currentTarget.ToString());
      GUI.TextField(new Rect(8, 240, 500, 25), "   Position: " + _currentTarget.position.ToString("N5"));
      GUI.TextField(new Rect(8, 265, 500, 25), "   Local Position: " + _currentTarget.localPosition.ToString("N5"));
      GUI.TextField(new Rect(8, 290, 500, 25), "   Local Rotation: " + _currentTarget.localRotation.eulerAngles.ToString("N5"));
      GUI.TextField(new Rect(8, 315, 500, 25), "   Local Scale: " + _currentTarget.localScale.ToString("N5"));

      if (_currentTarget.parent != null)
      {
        GUI.Label(new Rect(8, 340, 500, 25), "Parent: " + _currentTarget.parent.ToString());
        GUI.TextField(new Rect(8, 365, 500, 25), "   Local Pos: " + _currentTarget.parent.localPosition.ToString("N5"));
        GUI.TextField(new Rect(8, 390, 500, 25), "   Local Rot: " + _currentTarget.parent.localRotation.eulerAngles.ToString("N5"));
      }

      if (_currentTarget.root != null)
      {
        GUI.Label(new Rect(8, 415, 500, 25), "Root Transform: " + _currentTarget.root.ToString());
      }

      _TranslateSel = (Translate)GUI.SelectionGrid(new Rect(8, 445, 300, 100), (int)_TranslateSel, _TranslateTypes, 3);
      _MoveAmount = GUI.TextField(new Rect(10, 550, 100, 25), _MoveAmount);
      _FindTransformText = GUI.TextField(new Rect(8, 625, 200, 25), _FindTransformText);
      GUI.TextField(new Rect(210, 626, 200, 22), _FoundResult);
      GUI.Label(new Rect(8, 680, 300, 25), "Press F2 to toggle cursor");
    }

  }

  void ChangeTargetToParent()
  {
    if (_currentTarget.parent != null)
    {
      Log.Out($"Changing target to parent: {_currentTarget.parent}");
      _currentTarget = _currentTarget.parent;
      UpdatePrimitive();
    }
  }

  void ChangeTargetToNextSibling()
  {
    var curParent = _currentTarget.parent;
    var curIndex = _currentTarget.GetSiblingIndex();
    var siblingCount = curParent != null ? curParent.childCount : 1;
    var nextIndex = curIndex + 1 >= siblingCount ? 0 : curIndex + 1;

    if (curParent != null)
    {
      Log.Out($"Changing target to sibling at index: {nextIndex}, Sibling count: {siblingCount}");
      _currentTarget = curParent.GetChild(nextIndex);
      UpdatePrimitive();
    }
  }

  void ChangeTargetToFirstChild()
  {
    var childCount = _currentTarget.childCount;

    if (childCount > 0)
    {
      _currentTarget = _currentTarget.GetChild(0);
      Log.Out($"Changing target to first child {_currentTarget}");
      UpdatePrimitive();
    }
    else
    {
      Log.Out($"Transform {_currentTarget} has no children.");
    }
  }

  void ChangeTargetToRoot()
  {
    if (_currentTarget.root != null)
    {
      Log.Out($"Changing target to root: {_currentTarget.root}");
      _currentTarget = _currentTarget.root;
      UpdatePrimitive();
    }
  }

  void TranslatePos(string input, Translate t)
  {
    float newVal = 0f;
    if (float.TryParse(input, out newVal))
    {
      Log.Out($"Adjusting: {t} by {newVal}");

      var lpos = _currentTarget.localPosition;
      var lrot = _currentTarget.localRotation.eulerAngles;
      var lscale = _currentTarget.localScale;

      switch (t)
      {
        case Translate.PosX:
          {
            _currentTarget.localPosition = new Vector3(lpos.x + newVal, lpos.y, lpos.z);
            break;
          }

        case Translate.PosY:
          {
            _currentTarget.localPosition = new Vector3(lpos.x, lpos.y + newVal, lpos.z);
            break;
          }

        case Translate.PosZ:
          {
            _currentTarget.localPosition = new Vector3(lpos.x, lpos.y, lpos.z + newVal);
            break;
          }

        case Translate.RotX:
          {
            _currentTarget.localRotation = Quaternion.Euler(lrot.x + newVal, lrot.y, lrot.z);
            break;
          }


        case Translate.RotY:
          {
            _currentTarget.localRotation = Quaternion.Euler(lrot.x, lrot.y + newVal, lrot.z);
            break;
          }

        case Translate.RotZ:
          {
            _currentTarget.localRotation = Quaternion.Euler(lrot.x, lrot.y, lrot.z + newVal);
            break;
          }

        case Translate.ScaleX:
          {
            _currentTarget.localScale = new Vector3(lscale.x + newVal, lscale.y, lscale.z);
            break;
          }
        case Translate.ScaleY:
          {
            _currentTarget.localScale = new Vector3(lscale.x, lscale.y + newVal, lscale.z);
            break;
          }

        case Translate.ScaleZ:
          {
            _currentTarget.localScale = new Vector3(lscale.x, lscale.y, lscale.z + newVal);
            break;
          }

        case Translate.ScaleAll:
          {
            _currentTarget.localScale = new Vector3(lscale.x + newVal, lscale.y + newVal, lscale.z + newVal);
            break;
          }
      }
    }
  }

  Transform FindTransform(string input)
  {
    return _currentTarget.FindInChilds(input, false);
  }

  void UpdatePrimitive()
  {
    if (_currentTarget != null && _Cube != null)
    {
      _Cube.transform.parent = _currentTarget;
      _Cube.transform.position = _currentTarget.position;
      _Cube.transform.localPosition = _currentTarget.localPosition;
      _Cube.transform.rotation = _currentTarget.rotation;
      _Cube.transform.localRotation = _currentTarget.localRotation;
      _Cube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
    }
  }

  enum Translate
  {
    PosX,
    PosY,
    PosZ,
    RotX,
    RotY,
    RotZ,
    ScaleX,
    ScaleY,
    ScaleZ,
    ScaleAll,
  }


  public static GameObject AddPrimitive(PrimitiveType primitiveType, float scale, Color color)
  {
    GameObject prim = GameObject.CreatePrimitive(primitiveType);
    Object.Destroy(prim.transform.GetComponent<Collider>());
    prim.layer = 0;
    var renderer = prim.GetComponent<Renderer>();
    renderer.material = Resources.Load<Material>("Materials/TerrainSmoothing");
    color.a = 0.2f;
    renderer.material.color = color;
    prim.transform.localPosition = Vector3.zero;
    prim.transform.localScale = Vector3.one * scale;
    return prim;
  }
}