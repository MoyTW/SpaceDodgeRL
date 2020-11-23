using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class FrigateAIComponent : ActivatableAIComponent {
    public static readonly string ENTITY_GROUP = "FRIGATE_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    private int _reverserCooldown = 2;
    private int _currentReverserCooldown = 0;

    public FrigateAIComponent(string activationGroupId) : base(activationGroupId) { }

    public override List<EncounterAction> _DecideNextAction(EncounterState state, Entity parent) {
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
        if (this._currentReverserCooldown > 0) {
          this._currentReverserCooldown -= 1;
        }
      }

      return actions;
    }
  }
}