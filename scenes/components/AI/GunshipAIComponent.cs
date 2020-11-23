using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class GunshipAIComponent : ActivatableAIComponent {
    public static readonly string ENTITY_GROUP = "GUNSHIP_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    private int _shotgunCooldown = 3;
    private int _currentShotgunCooldown = 0;

    public GunshipAIComponent(string activationGroupId) : base (activationGroupId) { }

    public override List<EncounterAction> _DecideNextAction(EncounterState state, Entity parent) {
      var actions = new List<EncounterAction>();
      var parentPos = parent.GetComponent<PositionComponent>().EncounterPosition;
      var playerPos = state.Player.GetComponent<PositionComponent>().EncounterPosition;

      // Close distance if further than 5
      if (parentPos.DistanceTo(playerPos) >= 5f) {
        // PERF: We may want to make pathfinding stateful/cache it or something, to save on turn times
        var path = Pathfinder.AStarWithNewGrid(parentPos, playerPos, state);
        if (path != null) {
          actions.Add(new MoveAction(parent.EntityId, path[0]));
        }
      }
      // Fire shotgun pellets=5, spread=5 every 4th turn, else fire single cannon (this is honestly a comical amount of spread though)
      if (this._currentShotgunCooldown == 0) {
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 5, spread: 5, state.EncounterRand));
        this._currentShotgunCooldown += this._shotgunCooldown;
      } else {
        actions.Add(FireProjectileAction.CreateSmallCannonAction(parent.EntityId, playerPos));
        if (this._currentShotgunCooldown > 0) {
          this._currentShotgunCooldown -= 1;
        }
      }


      return actions;
    }
  }
}