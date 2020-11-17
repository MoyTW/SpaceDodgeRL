using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class PathAIComponent : AIComponent {
    public static readonly string ENTITY_GROUP = "PATH_AI_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public EncounterPath Path { get; private set; }

    public static PathAIComponent Create(EncounterPath path) {
      var component = new PathAIComponent();

      component.Path = path;

      return component;
    }

    public List<EncounterAction> DecideNextAction(EncounterState state, Entity parent) {
      if (Path.AtEnd) {
        return new List<EncounterAction>() { new SelfDestructAction(parent.EntityId) };
      } else {
        var nextPosition = Path.Step();
        return new List<EncounterAction>() { new MoveAction(parent.EntityId, nextPosition) };
      }
    }
  }
}