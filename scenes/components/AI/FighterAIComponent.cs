using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class FighterAIComponent : ActivatableAIComponent {
    public static readonly string ENTITY_GROUP = "FIGHTER_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public FighterAIComponent(string activationGroupId) : base(activationGroupId) { }

    public override List<EncounterAction> _DecideNextAction(EncounterState state, Entity parent) {
      var actions = new List<EncounterAction>();
      var parentPos = parent.GetComponent<PositionComponent>().EncounterPosition;
      var playerPos = state.Player.GetComponent<PositionComponent>().EncounterPosition;

      // Always close distance
      var path = Pathfinder.AStarWithNewGrid(parentPos, playerPos, state);
      if (path != null) {
        actions.Add(new MoveAction(parent.EntityId, path[0]));
      }
      // Always fire
      actions.Add(FireProjectileAction.CreateSmallGatlingAction(parent.EntityId, playerPos));
      actions.Add(FireProjectileAction.CreateSmallGatlingAction(parent.EntityId, playerPos));
      actions.Add(FireProjectileAction.CreateSmallGatlingAction(parent.EntityId, playerPos));

      return actions;
    }
  }
}