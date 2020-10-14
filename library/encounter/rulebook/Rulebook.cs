using System;
using System.Collections.Generic;

public static class Rulebook {
  // ...can't I just do <> like I can in Kotlin? C# why you no let me do this. Probably because "evolving languages are hard".
  private static Dictionary<ActionType, Action<EncounterAction, EncounterState>> _actionMapping = new Dictionary<ActionType, Action<EncounterAction, EncounterState>>() {
    { ActionType.MOVE, (EncounterAction action, EncounterState state) => ResolveMove(action as MoveAction, state) }
  };

  public static void ResolveActions(List<EncounterAction> actions, EncounterState state) {
    actions.ForEach((action) => ResolveAction(action, state));
  }

  public static void ResolveAction(EncounterAction action, EncounterState state) {
    // If I had C# 8.0 I'd use the new, nifty switch! I'm using a dictionary because I *always* forget to break; out of a switch
    // and that usually causes an annoying bug that I spend way too long mucking around with. Instead here it will just EXPLODE!
    _actionMapping[action.ActionType].Invoke(action, state);
  }

  private static void ResolveMove(MoveAction action, EncounterState state) {
    // TODO: We're throwing away the entityId here, because only the player moves.
    var positionComponent = state.Player.GetNode<PositionComponent>("PositionComponent");
    positionComponent.GamePosition = action.TargetPosition;

    // TODO: create an EndTurnAction!
    var actionTimeComponent = state.Player.GetNode<ActionTimeComponent>("ActionTimeComponent");
    actionTimeComponent.EndTurn(state.Player.GetNode<SpeedComponent>("SpeedComponent"));
  }
}