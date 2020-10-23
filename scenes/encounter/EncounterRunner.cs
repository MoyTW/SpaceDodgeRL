using System.Collections.Generic;
using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.encounter {
  public class EncounterRunner : Node {

    public InputHandler inputHandlerRef = null;

    private EncounterState _encounterState;
    public void SetEncounterState(EncounterState encounterState) {
      this._encounterState = encounterState;
    }

    public override void _Process(float delta) {
      EncounterRunner.RunTurn(this._encounterState, inputHandlerRef);
    }

    private static void PassTime(EncounterState state, int time) {
      var actionEntities = state.ActionEntities();
      foreach (Entity entity in actionEntities) {
        var actionTimeComponent = entity.GetComponent<ActionTimeComponent>();
        actionTimeComponent.PassTime(time);
      }
    }

    // TODO: Write a system that has a component which does this instead of hard-coding it into the player's turn end
    private static void PlayerExecuteTurnEndingAction(EncounterAction action, EncounterState state) {
      var playerPosition = state.Player.GetComponent<PositionComponent>().EncounterPosition;
      var actions = new List<EncounterAction>() { action };

      // TODO: Pick a target in range and fire the projectile
      // TODO: Create a Faction component?
      // TODO: Iterating literally every action entity every time is very silly
      PositionComponent closestEnemyPosition = null;
      float closestEnemyDistance = int.MaxValue;
      foreach (Entity actionEntity in state.ActionEntities()) {
        if (actionEntity.GetComponent<AIComponent>() != null && actionEntity.GetComponent<PathAIComponent>() == null) {
          var actionEntityPositionComponent = actionEntity.GetComponent<PositionComponent>();
          var distance = actionEntityPositionComponent.EncounterPosition.DistanceTo(playerPosition);
          if (distance < closestEnemyDistance) {
            closestEnemyPosition = actionEntityPositionComponent;
            closestEnemyDistance = distance;
          }
        }
      }
      if (closestEnemyPosition != null) {
        var fireAction = FireProjectileAction.CreateSmallShotgunAction(state.Player.EntityId, closestEnemyPosition.EncounterPosition);
        actions.Add(fireAction);
      }

      Rulebook.ResolveActions(actions, state);
    }

    private static void PlayerMove(EncounterState state, int dx, int dy) {
      var positionComponent = state.Player.GetComponent<PositionComponent>();
      var oldPos = positionComponent.EncounterPosition;
      var moveAction = new MoveAction(state.Player.EntityId, new EncounterPosition(oldPos.X + dx, oldPos.Y + dy));
      PlayerExecuteTurnEndingAction(moveAction, state);
    }

    private static void PlayerWait(EncounterState state) {
      var waitAction = new WaitAction(state.Player.EntityId);
      PlayerExecuteTurnEndingAction(waitAction, state);
    }

    private static void RunTurn(EncounterState state, InputHandler inputHandler) {
      var entity = state.NextEntity();
      var actionTimeComponent = entity.GetComponent<ActionTimeComponent>();
      if (actionTimeComponent.TicksUntilTurn > 0) {
        EncounterRunner.PassTime(state, actionTimeComponent.TicksUntilTurn);
      }

      if (entity.IsInGroup(PlayerComponent.ENTITY_GROUP)) {
        var action = inputHandler.PopQueue();
        // Super not a fan of the awkwardness of checking this twice! Switch string -> enum, maybe?
        if (action == InputHandler.ActionMapping.MOVE_N) {
          PlayerMove(state, 0, -1);
        } else if (action == InputHandler.ActionMapping.MOVE_NE) {
          PlayerMove(state, 1, -1);
        } else if (action == InputHandler.ActionMapping.MOVE_E) {
          PlayerMove(state, 1, 0);
        } else if (action == InputHandler.ActionMapping.MOVE_SE) {
          PlayerMove(state, 1, 1);
        } else if (action == InputHandler.ActionMapping.MOVE_S) {
          PlayerMove(state, 0, 1);
        } else if (action == InputHandler.ActionMapping.MOVE_SW) {
          PlayerMove(state, -1, 1);
        } else if (action == InputHandler.ActionMapping.MOVE_W) {
          PlayerMove(state, -1, 0);
        } else if (action == InputHandler.ActionMapping.MOVE_NW) {
          PlayerMove(state, -1, -1);
        } else if (action == InputHandler.ActionMapping.WAIT) {
          PlayerWait(state);
        }
      } else {
        AIComponent aIComponent = entity.GetComponent<AIComponent>();
        var aIActions = aIComponent.DecideNextAction(state);
        Rulebook.ResolveActions(aIActions, state);
        state.UpdateDangerMap();
      }
    }
  }
}