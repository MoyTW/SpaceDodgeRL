using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class CruiserAIComponent : AIComponent {
    public static readonly string ENTITY_GROUP = "CRUISER_AI_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private ActivationGroup _activationGroup;

    private int _railgunCooldown = 2;
    private int _currentRailgunCooldown = 0;
    private int _flakCooldown = 9;
    private int _currentFlakCooldown = 0;

    public CruiserAIComponent(ActivationGroup activationGroup) {
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
      var distanceToPlayer = parentPos.DistanceTo(playerPos);

      // Only attempt to close distance if >=7 squares away
      if (distanceToPlayer >= 7f) {
        var path = Pathfinder.AStarWithNewGrid(parentPos, playerPos, state);
        if (path != null) {
          actions.Add(new MoveAction(parent.EntityId, path[0]));
        }
      }

      // Always fire cannon
      actions.Add(FireProjectileAction.CreateSmallCannonAction(parent.EntityId, playerPos));

      // Fire railgun on cooldown (every 3rd turn)
      if (this._currentRailgunCooldown == 0) {
        actions.Add(FireProjectileAction.CreateRailgunAction(parent.EntityId, playerPos));
        this._currentRailgunCooldown += this._railgunCooldown;
      } else if (this._currentRailgunCooldown > 0) {
        this._currentRailgunCooldown -= 1;
      }

      // If player is close and flak is off cooldown, fire
      if (distanceToPlayer <= 4 && this._currentFlakCooldown == 0) {
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 30, spread: 5, state.EncounterRand));
        this._currentFlakCooldown = this._flakCooldown;
      } else if (this._currentFlakCooldown > 0) {
        this._currentFlakCooldown -= 1;
      }

      return actions;
    }
  }
}