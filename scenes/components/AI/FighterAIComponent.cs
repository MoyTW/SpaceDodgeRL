using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.components.AI {

  public class FighterAIComponent : AIComponent {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/AI/FighterAIComponent.tscn");

    public static readonly string ENTITY_GROUP = "FIGHTER_AI_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    // TODO: sight-lines & group activation
    public override bool IsActive => false;

    public static FighterAIComponent Create() {
      return _componentPrefab.Instance() as FighterAIComponent;
    }

    public override List<EncounterAction> DecideNextAction(EncounterState state) {
      Entity parent = GetParent() as Entity;

      if (!IsActive) { return new List<EncounterAction>() { new WaitAction(parent.EntityId) }; }

      var actions = new List<EncounterAction>();
      var parentPos = parent.GetComponent<PositionComponent>().EncounterPosition;
      var playerPos = state.Player.GetComponent<PositionComponent>().EncounterPosition;

      // Always close distance
      var path = Pathfinder.AStarWithNewGrid(parentPos, playerPos, state);
      if (path != null) {
        actions.Add(new MoveAction(parent.EntityId, path[0]));
      }
      // Always fire
      // TODO: These should be small_gatling, not small_shotgun
      actions.Add(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos));
      actions.Add(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos));
      actions.Add(FireProjectileAction.CreateSmallShotgunAction(parent.EntityId, playerPos));

      return actions;
    }
  }
}