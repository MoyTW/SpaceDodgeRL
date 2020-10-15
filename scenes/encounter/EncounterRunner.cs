using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
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

    private static void MovePlayer(EncounterState state, int dx, int dy) {
      var positionComponent = state.Player.GetNode<PositionComponent>("PositionComponent");
      var oldPos = positionComponent.GamePosition;
      Rulebook.ResolveAction(new MoveAction(state.Player.EntityId, new GamePosition(oldPos.X + dx, oldPos.Y + dy)), state);
    }

    private static void PassTime(EncounterState state, int time) {
      var actionEntities = state.ActionEntities();
      foreach (Entity entity in actionEntities) {
        var actionTimeComponent = entity.GetNode<ActionTimeComponent>("ActionTimeComponent");
        actionTimeComponent.PassTime(time);
      }
    }

    private static void RunTurn(EncounterState state, InputHandler inputHandler) {
      var entity = state.NextEntity();
      var actionTimeComponent = entity.GetNode<ActionTimeComponent>("ActionTimeComponent");
      if (actionTimeComponent.TicksUntilTurn > 0) {
        EncounterRunner.PassTime(state, actionTimeComponent.TicksUntilTurn);
      }

      if (entity.IsInGroup(PlayerComponent.ENTITY_GROUP)) {
        var action = inputHandler.PopQueue();
        // Super not a fan of the awkwardness of checking this twice! Switch string -> enum, maybe?
        if (action == InputHandler.ActionMapping.MOVE_N) {
          MovePlayer(state, 0, -1);
        } else if (action == InputHandler.ActionMapping.MOVE_NE) {
          MovePlayer(state, 1, -1);
        } else if (action == InputHandler.ActionMapping.MOVE_E) {
          MovePlayer(state, 1, 0);
        } else if (action == InputHandler.ActionMapping.MOVE_SE) {
          MovePlayer(state, 1, 1);
        } else if (action == InputHandler.ActionMapping.MOVE_S) {
          MovePlayer(state, 0, 1);
        } else if (action == InputHandler.ActionMapping.MOVE_SW) {
          MovePlayer(state, -1, 1);
        } else if (action == InputHandler.ActionMapping.MOVE_W) {
          MovePlayer(state, -1, 0);
        } else if (action == InputHandler.ActionMapping.MOVE_NW) {
          MovePlayer(state, -1, -1);
        }
      } else {
        // TODO: Take any AI component & restrict it to direct children
        AIComponent aIComponent = entity.GetNode<TestAIComponent>("TestAIComponent");
        var aIActions = aIComponent.DecideNextAction(state);
        Rulebook.ResolveActions(aIActions, state);
      }
    }


  }
}