using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class FighterAIComponent : AIComponent {
    public static readonly string ENTITY_GROUP = "FIGHTER_AI_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private ActivationGroup _activationGroup;
    public bool IsActive => _activationGroup.IsActive;

    public FighterAIComponent(ActivationGroup activationGroup) {
      _activationGroup = activationGroup;
    }

    public List<EncounterAction> DecideNextAction(EncounterState state, Entity parent) {
      if (!IsActive) {
        var position = parent.GetComponent<PositionComponent>().EncounterPosition;
        if (state.FoVCache.Contains(position.X, position.Y)) {
          _activationGroup.Activate();
        } else {
          return new List<EncounterAction>() { new WaitAction(parent.EntityId) };
        }
      }

      var actions = new List<EncounterAction>();
      var parentPos = parent.GetComponent<PositionComponent>().EncounterPosition;
      var playerPos = state.Player.GetComponent<PositionComponent>().EncounterPosition;

      // Always close distance
      var path = Pathfinder.AStarWithNewGrid(parentPos, playerPos, state);
      if (path != null) {
        actions.Add(new MoveAction(parent.EntityId, path[0]));
      }
      // Always fire
      // TODO: These should be small_gatling, not small_shotgun
      actions.Add(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos));
      actions.Add(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos));
      actions.Add(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos));

      return actions;
    }
  }
}