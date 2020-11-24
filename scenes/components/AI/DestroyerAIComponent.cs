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

  public class DestroyerAIComponent : ActivatableAIComponent, Savable {
    public static readonly string ENTITY_GROUP = "DESTROYER_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public int VolleyCooldown { get; private set; } = 4;
    [JsonInclude] public int CurrentVolleyCooldown { get; private set; } = 0;

    public DestroyerAIComponent(string activationGroupId) : base(activationGroupId) { }

    public static DestroyerAIComponent Create(string saveData) {
      return JsonSerializer.Deserialize<DestroyerAIComponent>(saveData);
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
      // Fire comically dense & inaccurate shotgun volley every 4 turns, and shotgun otherwise
      if (this.CurrentVolleyCooldown == 0) {
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 30, spread: 7, state.EncounterRand));
        actions.Add(FireProjectileAction.CreateSmallCannonAction(parent.EntityId, playerPos));
        this.CurrentVolleyCooldown += VolleyCooldown;
      } else {
        actions.AddRange(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos, numPellets: 2, spread: 1, state.EncounterRand));
        if (this.CurrentVolleyCooldown > 0) {
          this.CurrentVolleyCooldown -= 1;
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