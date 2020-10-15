using Godot;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.scenes.encounter;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class TestAIComponent : Node, Component, AIComponent {
    public static string ENTITY_GROUP = "TEST_AI_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    // TODO: sight-lines & group activation
    public bool IsActive => true;

    public List<EncounterAction> DecideNextAction(EncounterState state) {
      // TODO: Provide nicer syntax for a component to get its parents maybe...?
      return new List<EncounterAction>() { new EndTurnAction((GetParent() as Entity).EntityId) };
    }
  }
}