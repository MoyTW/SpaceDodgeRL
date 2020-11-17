using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class ScoutAIComponent : AIComponent {
    public static readonly string ENTITY_GROUP = "SCOUT_AI_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private ActivationGroup _activationGroup;

    public ScoutAIComponent(ActivationGroup activationGroup) {
      this._activationGroup = activationGroup;
    }

    public List<EncounterAction> DecideNextAction(EncounterState state, Entity parent) {
      if (!_activationGroup.IsActive) {
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

      // Close distance if further than 5
      if (parentPos.DistanceTo(playerPos) >= 5f) {
        // TODO: We may want to make pathfinding stateful/cache it or something, to save on turn times
        var path = Pathfinder.AStarWithNewGrid(parentPos, playerPos, state);
        if (path != null) {
          actions.Add(new MoveAction(parent.EntityId, path[0]));
        }
      }
      // Always fire shotgun (spread=2, pellets=3)
      var fire = FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 3, spread: 2, state.EncounterRand);
      actions.AddRange(fire);

      return actions;
    }
  }
}