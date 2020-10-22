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

    private static void MovePlayer(EncounterState state, int dx, int dy) {
      var positionComponent = state.Player.GetComponent<PositionComponent>();
      var oldPos = positionComponent.EncounterPosition;
      Rulebook.ResolveAction(new MoveAction(state.Player.EntityId, new EncounterPosition(oldPos.X + dx, oldPos.Y + dy)), state);
    }

    private static void PassTime(EncounterState state, int time) {
      var actionEntities = state.ActionEntities();
      foreach (Entity entity in actionEntities) {
        var actionTimeComponent = entity.GetComponent<ActionTimeComponent>();
        actionTimeComponent.PassTime(time);
      }
    }

    private static void RunTurn(EncounterState state, InputHandler inputHandler) {
      var entity = state.NextEntity();
      var actionTimeComponent = entity.GetComponent<ActionTimeComponent>();
      if (actionTimeComponent.TicksUntilTurn > 0) {
        EncounterRunner.PassTime(state, actionTimeComponent.TicksUntilTurn);
      }

      if (entity.IsInGroup(PlayerComponent.ENTITY_GROUP)) {
        // TODO: dirty bit so it shows firing as well & not just instant vanish
        if (state.DangerMapDirty) {
          state.UpdateDangerMap();
          state.DangerMapDirty = false;
        }

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
        if (action != null) {
          state.DangerMapDirty = true;
        }
      } else {
        AIComponent aIComponent = entity.GetComponent<AIComponent>();
        var aIActions = aIComponent.DecideNextAction(state);
        Rulebook.ResolveActions(aIActions, state);
      }
    }
  }
}