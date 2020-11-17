using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class FrigateAIComponent : AIComponent {
    public static readonly string ENTITY_GROUP = "FRIGATE_AI_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private ActivationGroup _activationGroup;

    private int _reverserCooldown = 2;
    private int _currentReverserCooldown = 0;

    public FrigateAIComponent(ActivationGroup activationGroup) {
      _activationGroup = activationGroup;
    }

    public List<EncounterAction> DecideNextAction(EncounterState state, Entity parent) {
      // TODO: Pull this out & don't copy/paste in every AI
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

      // Always attempt to close distance
      var path = Pathfinder.AStarWithNewGrid(parentPos, playerPos, state);
      if (path != null) {
        actions.Add(new MoveAction(parent.EntityId, path[0]));
      }
      // Fire reverser + cannon every 3rd turn, and shotgun + secondary batteries otherwise
      if (this._currentReverserCooldown == 0) {
        actions.Add(FireProjectileAction.CreateReverserAction(parent.EntityId, playerPos));
        actions.Add(FireProjectileAction.CreateSmallCannonAction(parent.EntityId, playerPos));
        this._currentReverserCooldown += _reverserCooldown;
      } else {
        actions.Add(FireProjectileAction.CreateSmallCannonAction(parent.EntityId, playerPos));
        actions.Add(FireProjectileAction.CreateSmallCannonAction(parent.EntityId, playerPos));
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 2, spread: 3, state.EncounterRand));
        this._currentReverserCooldown -= 1;
      }

      return actions;
    }
  }
}