using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;
using System.Text.Json;

namespace SpaceDodgeRL.scenes.components.AI {

  public class DiplomatAIComponent : ActivatableAIComponent {
    public static readonly string ENTITY_GROUP = "DIPLOMAT_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public DiplomatAIComponent(string activationGroupId) : base(activationGroupId) { }

    public static DiplomatAIComponent Create(string saveData) {
      return JsonSerializer.Deserialize<DiplomatAIComponent>(saveData);
    }

    public override List<EncounterAction> _DecideNextAction(EncounterState state, Entity parent) {
      return new List<EncounterAction>() { new WaitAction(parent.EntityId) };
    }

    public override string Save() {
      return JsonSerializer.Serialize(this);
    }

    public override void NotifyAttached(Entity parent) { }

    public override void NotifyDetached(Entity parent) { }
  }
}