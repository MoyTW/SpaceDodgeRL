using Godot;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.scenes.encounter;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class TestAIComponent : AIComponent {
    public static readonly string ENTITY_GROUP = "TEST_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    // TODO: sight-lines & group activation
    public override bool IsActive => true;

    public override List<EncounterAction> DecideNextAction(EncounterState state) {
      // TODO: Provide nicer syntax for a component to get its parents maybe...?
      return new List<EncounterAction>() { new EndTurnAction((GetParent() as Entity).EntityId) };
    }
  }
}