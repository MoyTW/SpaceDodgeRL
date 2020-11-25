using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceDodgeRL.scenes.components.AI {

  public class CruiserAIComponent : ActivatableAIComponent {
    public static readonly string ENTITY_GROUP = "CRUISER_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public int RailgunCooldown { get; private set; } = 2;
    [JsonInclude] public int CurrentRailgunCooldown { get; private set; } = 0;
    [JsonInclude] public int FlakCooldown { get; private set; } = 9;
    [JsonInclude] public int CurrentFlakCooldown { get; private set; } = 0;

    public CruiserAIComponent(string activationGroupId) : base(activationGroupId) { }

    public static CruiserAIComponent Create(string saveData) {
      return JsonSerializer.Deserialize<CruiserAIComponent>(saveData);
    }

    public override List<EncounterAction> _DecideNextAction(EncounterState state, Entity parent) {
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
      if (this.CurrentRailgunCooldown == 0) {
        actions.Add(FireProjectileAction.CreateRailgunAction(parent.EntityId, playerPos));
        this.CurrentRailgunCooldown += this.RailgunCooldown;
      } else if (this.CurrentRailgunCooldown > 0) {
        this.CurrentRailgunCooldown -= 1;
      }

      // If player is close and flak is off cooldown, fire
      if (distanceToPlayer <= 4 && this.CurrentFlakCooldown == 0) {
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 30, spread: 5, state.EncounterRand));
        this.CurrentFlakCooldown = this.FlakCooldown;
      } else if (this.CurrentFlakCooldown > 0) {
        this.CurrentFlakCooldown -= 1;
      }

      return actions;
    }

    public override string Save() {
      return JsonSerializer.Serialize(this);
    }

    public override void NotifyAttached(Entity parent) { }

    public override void NotifyDetached(Entity parent) { }
  }
}