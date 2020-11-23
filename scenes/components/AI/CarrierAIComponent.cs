using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.resources.gamedata;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class CarrierAIComponent : ActivatableAIComponent {
    public static readonly string ENTITY_GROUP = "CARRIER_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    private int _flakCooldown = 9;
    private int _currentFlakCooldown = 0;
    private string[] _launchTable = new string[] { EntityDefId.SCOUT, EntityDefId.FIGHTER, EntityDefId.FIGHTER };

    public CarrierAIComponent(string activationGroupId) : base(activationGroupId) { }

    public override List<EncounterAction> _DecideNextAction(EncounterState state, Entity parent) {
      var actions = new List<EncounterAction>();
      var parentPos = parent.GetComponent<PositionComponent>().EncounterPosition;
      // TODO: Player death! if you die this just NPEs, which aside from being inelegant kills the program.
      var playerPos = state.Player.GetComponent<PositionComponent>().EncounterPosition;

      // Carrier never moves

      // Always launch randomly chosen strike craft - immediately paths one square to get it out of the carrier's square
      var chosenEntityDef = this._launchTable[state.EncounterRand.Next(3)];
      var strikeCraft = EntityBuilder.CreateEnemyByEntityDefId(chosenEntityDef, this.ActivationGroupId, state.CurrentTick);
      actions.Add(new SpawnEntityAction(parent.EntityId, strikeCraft, parentPos, true));
      var path = Pathfinder.AStarWithNewGrid(parentPos, playerPos, state);
      // TODO: If you park adjacent the carrier, spawns will try to path literally into you and be stuck on the carrier square.
      // Won't crash anything it'll just put some error logs out and be deeply comical.
      if (path != null) {
        actions.Add(new MoveAction(strikeCraft.EntityId, path[0]));
      }

      // If player is close and flak is off cooldown, fire (comically this can, and will, friendly-fire its fighter wing)
      // TODO: friendly fire, maybe? It is a bit silly.
      if (parentPos.DistanceTo(playerPos) <= 4 && this._currentFlakCooldown == 0) {
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 30, spread: 5, state.EncounterRand));
        this._currentFlakCooldown = this._flakCooldown;
      } else if (this._currentFlakCooldown > 0) {
        this._currentFlakCooldown -= 1;
      }

      return actions;
    }
  }
}