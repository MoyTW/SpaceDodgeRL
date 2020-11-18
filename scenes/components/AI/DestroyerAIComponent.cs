using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class DestroyerAIComponent : AIComponent {
    public static readonly string ENTITY_GROUP = "DESTROYER_AI_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private ActivationGroup _activationGroup;

    private int _volleyCooldown = 4;
    private int _currentVolleyCooldown = 0;

    public DestroyerAIComponent(ActivationGroup activationGroup) {
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
      // Fire comically dense & inaccurate shotgun volley every 4 turns, and shotgun otherwise
      if (this._currentVolleyCooldown == 0) {
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 30, spread: 7, state.EncounterRand));
        actions.Add(FireProjectileAction.CreateSmallCannonAction(parent.EntityId, playerPos));
        this._currentVolleyCooldown += _volleyCooldown;
      } else {
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 2, spread: 1, state.EncounterRand));
        if (this._currentVolleyCooldown > 0) {
          this._currentVolleyCooldown -= 1;
        }
      }

      return actions;
    }
  }
}