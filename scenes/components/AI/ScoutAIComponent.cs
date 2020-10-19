using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class ScoutAIComponent : AIComponent {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/AI/ScoutAIComponent.tscn");

    public static readonly string ENTITY_GROUP = "SCOUT_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    // TODO: sight-lines & group activation
    public override bool IsActive => true;

    public static ScoutAIComponent Create() {
      return _componentPrefab.Instance() as ScoutAIComponent;
    }

    public override List<EncounterAction> DecideNextAction(EncounterState state) {
      Entity parent = GetParent() as Entity;

      if (!IsActive) { return new List<EncounterAction>() { new EndTurnAction(parent.EntityId) }; }

      var actions = new List<EncounterAction>();

      var parentPos = parent.GetComponent<PositionComponent>().EncounterPosition;
      var playerPos = state.Player.GetComponent<PositionComponent>().EncounterPosition;

      // Close distance if further than 5
      if (parentPos.DistanceTo(playerPos) >= 5f) {
        // TODO: We may want to make pathfinding stateful/cache it or something, to save on turn times
        var path = Pathfinder.AStarWithNewGrid(parentPos, playerPos, state);
        if (path != null) {
          actions.Add(new MoveAction(parent.EntityId, path[0]));
        }
      }
      // Always fire
      var fire = new FireProjectileAction(
        parent.EntityId,
        1,
        (sourcePos) => EncounterPathBuilder.BuildStraightLinePath(sourcePos, playerPos, 25),
        20,
        ProjectileType.SMALL_SHOTGUN
      );
      actions.Add(fire);

      return actions;
    }
  }
}