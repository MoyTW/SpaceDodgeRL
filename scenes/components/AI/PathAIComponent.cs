using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceDodgeRL.scenes.components.AI {

  public class PathAIComponent : AIComponent {
    public static readonly string ENTITY_GROUP = "PATH_AI_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public EncounterPath Path { get; private set; }

    public static PathAIComponent Create(EncounterPath path) {
      var component = new PathAIComponent();

      component.Path = path;

      return component;
    }

    public static PathAIComponent Create(string saveData) {
      return JsonSerializer.Deserialize<PathAIComponent>(saveData);
    }

    public List<EncounterAction> DecideNextAction(EncounterState state, Entity parent) {
      if (Path.AtEnd) {
        return new List<EncounterAction>() { new DestroyAction(parent.EntityId) };
      } else {
        var nextPosition = Path.Step();
        return new List<EncounterAction>() { new MoveAction(parent.EntityId, nextPosition) };
      }
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}