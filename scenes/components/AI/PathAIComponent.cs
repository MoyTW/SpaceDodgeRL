using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class PathAIComponent : AIComponent {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/AI/PathAIComponent.tscn");

    public static readonly string ENTITY_GROUP = "PATH_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    // Preset path AIs are always active
    public override bool IsActive => true;

    public EncounterPath Path { get; private set; }

    public static PathAIComponent Create(EncounterPath path) {
      var component = _componentPrefab.Instance() as PathAIComponent;

      component.Path = path;

      return component;
    }

    public override List<EncounterAction> DecideNextAction(EncounterState state) {
      if (Path.AtEnd) {
        return new List<EncounterAction>() { new SelfDestructAction((GetParent() as Entity).EntityId) };
      } else {
        var nextPosition = Path.Step();
        return new List<EncounterAction>() { new MoveAction((GetParent() as Entity).EntityId, nextPosition) };
      }
    }
  }
}