using System;
using UnityEngine;

public class TE_ObjectManipulatorItem : ItemClass
{
  ItemAction_TE_ObjectManipulator _itemAction_TE_ObjectManipulator;

  public override void Init()
  {
    base.Init();

    _itemAction_TE_ObjectManipulator = Actions[0] as ItemAction_TE_ObjectManipulator;
    _itemAction_TE_ObjectManipulator.Init();
    for (int i = 1; i < Actions.Length; i++)
    {
      Actions[i] = _itemAction_TE_ObjectManipulator;
    }
  }

  public override void ExecuteAction(int _actionIdx, ItemInventoryData _data, bool _bReleased, PlayerActionsLocal _playerActions)
  {
    if (Actions[_actionIdx] == null)
    {
      return;
    }

    if (!_bReleased)
      return;

    EntityAlive ea = _data.holdingEntity;
    if (_actionIdx == 0)
    {
      _itemAction_TE_ObjectManipulator.ExecuteAction(_data.actionData[_actionIdx], _bReleased);
    }
    else if (_actionIdx == 1 && ea is EntityPlayerLocal)
    {
      _itemAction_TE_ObjectManipulator.ToggleActive(_data.actionData[_actionIdx]);
    }
    else
    {
      // I didn't see anywhere in the code where this was possible, but maybe in the future.  A20??
      Log.Out($"TE_ObjectManipulatorItem::ExecuteAction index: {_actionIdx} pressed.");
    }
  }
}