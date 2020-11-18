using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.resources.gamedata;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class CarrierAIComponent : AIComponent {
    public static readonly string ENTITY_GROUP = "CARRIER_AI_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private ActivationGroup _activationGroup;

    private int _flakCooldown = 9;
    private int _currentFlakCooldown = 0;
    private string[] _launchTable = new string[] { EntityDefId.SCOUT, EntityDefId.FIGHTER, EntityDefId.FIGHTER };

    public CarrierAIComponent(ActivationGroup activationGroup) {
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

      // Carrier never moves

      // Always launch randomly chosen strike craft
      var chosenEntityDef = this._launchTable[state.EncounterRand.Next(3)];
      var strikeCraft = EntityBuilder.CreateEnemyByEntityDefId(chosenEntityDef, this._activationGroup, state.CurrentTick);
      GD.Print("TODO: Launch fighter craft entity action");

      // If player is close and flak is off cooldown, fire (comically this can, and will, friendly-fire its fighter wing)
      // TODO: friendly fire, maybe? It is a bit silly.
      if (parentPos.DistanceTo(playerPos) <= 4 && this._currentFlakCooldown == 0) {
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 30, spread: 5, state.EncounterRand));
        this._currentFlakCooldown = this._flakCooldown;
      } else if (this._currentFlakCooldown > 0) {
        this._currentFlakCooldown -= 1;
      }

      if (actions.Count == 0) {
        actions.Add(new WaitAction(parent.EntityId));
      }

      return actions;
    }
  }
}