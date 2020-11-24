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

  public class FrigateAIComponent : ActivatableAIComponent, Savable {
    public static readonly string ENTITY_GROUP = "FRIGATE_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public int ReverserCooldown { get; private set; } = 2;
    [JsonInclude] public int CurrentReverserCooldown { get; private set; } = 0;

    public FrigateAIComponent(string activationGroupId) : base(activationGroupId) { }

    public static FrigateAIComponent Create(string saveData) {
      return JsonSerializer.Deserialize<FrigateAIComponent>(saveData);
    }

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
      if (this.CurrentReverserCooldown == 0) {
        actions.Add(FireProjectileAction.CreateReverserAction(parent.EntityId, playerPos));
        actions.Add(FireProjectileAction.CreateSmallCannonAction(parent.EntityId, playerPos));
        this.CurrentReverserCooldown += ReverserCooldown;
      } else {
        actions.Add(FireProjectileAction.CreateSmallCannonAction(parent.EntityId, playerPos));
        actions.Add(FireProjectileAction.CreateSmallCannonAction(parent.EntityId, playerPos));
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 2, spread: 3, state.EncounterRand));
        if (this.CurrentReverserCooldown > 0) {
          this.CurrentReverserCooldown -= 1;
        }
      }

      return actions;
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}