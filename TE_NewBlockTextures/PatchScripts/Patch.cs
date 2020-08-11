using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DMT;

// The purpose of this DMT patch is to increase the mesh array count by 1 and add a new field to define our custom texture array
public class PatchMeshDesc : IPatcherMod
{
  public bool Patch(ModuleDefinition module)
  {
    Log("Patch MeshDescription start...");

    var gm = module.Types.First(d => d.Name == "MeshDescription");
    var field = gm.Fields.First(d => d.Name == "MESH_LENGTH");
    var newMeshField = new FieldDefinition("MESH_NEWBLOCKTEX", FieldAttributes.Public | FieldAttributes.Static, field.FieldType);
    gm.Fields.Add(newMeshField);

    var staticctor = gm.Methods.FirstOrDefault(m => m.Name == ".cctor");
    if (staticctor == null)
    {
      Log("staticctor is null: {0}, field: {1}", staticctor, field);
      return false;
    }

    Instruction ip = null;

    foreach (var inst in staticctor.Body.Instructions)
    {
      if (inst.Operand == field && inst.Previous != null && inst.Previous.OpCode != null && inst.Previous.Operand != null && inst.Previous.OpCode == OpCodes.Ldc_I4_S && inst.Previous.Operand.Equals((sbyte)10))
      {
        inst.Previous.Operand = (sbyte)11;
        ip = inst.Previous;
        break;
      }
    }

    if (ip == null)
    {
      Log("Did not find suitable insertion point.");
      return false;
    }

    var il = staticctor.Body.GetILProcessor();
    il.InsertBefore(ip, il.Create(OpCodes.Ldc_I4_S, (sbyte)10));
    il.InsertBefore(ip, il.Create(OpCodes.Stsfld, newMeshField));

    Log("Patch MeshDescription complete!");
    return true;
  }

  public void Log(string msg, params object[] args)
  {
    Logging.Log(string.Format(msg, args));
  }

  public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
  {
    return true;
  }


  // Helper functions to allow us to access and change variables that are otherwise unavailable.
  private void SetMethodToVirtual(MethodDefinition meth)
  {
    meth.IsVirtual = true;
  }

  private void SetFieldToPublic(FieldDefinition field)
  {
    field.IsFamily = false;
    field.IsPrivate = false;
    field.IsPublic = true;
  }
  private void SetMethodToPublic(MethodDefinition field)
  {
    field.IsFamily = false;
    field.IsPrivate = false;
    field.IsPublic = true;
  }
}
