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

  public class GunshipAIComponent : ActivatableAIComponent {
    public static readonly string ENTITY_GROUP = "GUNSHIP_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public int ShotgunCooldown { get; private set; } = 3;
    [JsonInclude] public int CurrentShotgunCooldown { get; private set; } = 0;

    public GunshipAIComponent(string activationGroupId) : base (activationGroupId) { }

    public static GunshipAIComponent Create(string saveData) {
      return JsonSerializer.Deserialize<GunshipAIComponent>(saveData);
    }

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
      if (this.CurrentShotgunCooldown == 0) {
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 5, spread: 5, state.EncounterRand));
        this.CurrentShotgunCooldown += this.ShotgunCooldown;
      } else {
        actions.Add(FireProjectileAction.CreateSmallCannonAction(parent.EntityId, playerPos));
        if (this.CurrentShotgunCooldown > 0) {
          this.CurrentShotgunCooldown -= 1;
        }
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