using Godot;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;

namespace SpaceDodgeRL.library.encounter.rulebook {

  public static class Rulebook {
    // ...can't I just do <> like I can in Kotlin? C# why you no let me do this. Probably because "evolving languages are hard".
    private static Dictionary<ActionType, Action<EncounterAction, EncounterState>> _actionMapping = new Dictionary<ActionType, Action<EncounterAction, EncounterState>>() {
    { ActionType.MOVE, (a, s) => ResolveMove(a as MoveAction, s) },
    { ActionType.END_TURN, (a, s) => ResolveEndTurn(a as EndTurnAction, s) },
    { ActionType.SELF_DESTRUCT, (a, s) => ResolveSelfDestruct(a as SelfDestructAction, s) }
  };

    public static void ResolveActions(List<EncounterAction> actions, EncounterState state) {
      actions.ForEach((action) => ResolveAction(action, state));
    }

    public static void ResolveAction(EncounterAction action, EncounterState state) {
      var entity = state.GetEntityById(action.ActorId);
      GD.Print(string.Format("Processing action type {0} for entity {1}:{2}", action.ActionType, entity.EntityName, entity.EntityId));

      // If I had C# 8.0 I'd use the new, nifty switch! I'm using a dictionary because I *always* forget to break; out of a switch
      // and that usually causes an annoying bug that I spend way too long mucking around with. Instead here it will just EXPLODE!
      _actionMapping[action.ActionType].Invoke(action, state);
    }

    private static void ResolveMove(MoveAction action, EncounterState state) {
      Entity entity = state.GetEntityById(action.ActorId);
      var positionComponent = state.GetEntityById(action.ActorId).GetComponent<PositionComponent>();

      if (positionComponent.EncounterPosition == action.TargetPosition) {
        GD.PrintErr(string.Format("Entity {0}:{1} tried to move to its current position {2}", entity.EntityName, entity.EntityId, action.TargetPosition));
      } else if (state.IsPositionBlocked(action.TargetPosition)) {
        // TODO: Resolve attacks
        GD.PrintErr(string.Format("Entity {0}:{1} could not move to {2}, blocked!", entity.EntityName, entity.EntityId, action.TargetPosition));
      } else {
        state.TeleportEntity(entity, action.TargetPosition);
        ResolveAction(new EndTurnAction(action.ActorId), state);
      }
    }

    private static void ResolveEndTurn(EndTurnAction action, EncounterState state) {
      Entity entity = state.GetEntityById(action.ActorId);
      var actionTimeComponent = entity.GetComponent<ActionTimeComponent>();
      actionTimeComponent.EndTurn(entity.GetComponent<SpeedComponent>());
    }

    private static void ResolveSelfDestruct(SelfDestructAction action, EncounterState state) {
      Entity entity = state.GetEntityById(action.ActorId);
      state.RemoveEntity(entity);
    }
  }
}