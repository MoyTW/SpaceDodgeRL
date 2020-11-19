using System.Collections.Generic;
using System.Linq;
using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.encounter {
  public class EncounterRunner : Node {

    public InputHandler inputHandlerRef = null;

    private EncounterState _encounterState;
    public void SetEncounterState(EncounterState encounterState) {
      this._encounterState = encounterState;
    }

    public override void _Process(float delta) {
      RunTurn(this._encounterState, inputHandlerRef);
    }

    // TODO: Write a system that has a component which does this instead of hard-coding it into the player's turn end
    private static void PlayerExecuteTurnEndingAction(EncounterAction action, EncounterState state) {
      var player = state.Player;

      bool actionResolvedSuccessfully = Rulebook.ResolveAction(action, state);
      if (!actionResolvedSuccessfully) {
        return;
      }

      // Picks a target in range & fires a projectile
      // PERF: Iterating literally every action entity every time is very silly
      var playerPosition = player.GetComponent<PositionComponent>().EncounterPosition;
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
      var playerComponent = player.GetComponent<PlayerComponent>();
      if (closestEnemyPosition != null && closestEnemyDistance <= playerComponent.CuttingLaserRange) {
        var fireAction = FireProjectileAction.CreateCuttingLaserAction(
          player.EntityId, playerComponent.CuttingLaserPower, closestEnemyPosition.EncounterPosition);
        Rulebook.ResolveAction(fireAction, state);
      }
      Rulebook.ResolveEndTurn(player.EntityId, state);

      // After the player executes their turn we need to update the UI
      state.UpdateFoVAndFoW();
      state.UpdatePlayerOverlays();
    }

    private static void PlayerMove(EncounterState state, int dx, int dy) {
      var positionComponent = state.Player.GetComponent<PositionComponent>();
      var oldPos = positionComponent.EncounterPosition;
      var moveAction = new MoveAction(state.Player.EntityId, new EncounterPosition(oldPos.X + dx, oldPos.Y + dy));
      PlayerMove(state, moveAction);
    }

    private static void PlayerMove(EncounterState state, MoveAction moveAction) {
      PlayerExecuteTurnEndingAction(moveAction, state);
    }

    private static void PlayerWait(EncounterState state) {
      var waitAction = new WaitAction(state.Player.EntityId);
      PlayerExecuteTurnEndingAction(waitAction, state);
    }

    private void RunTurn(EncounterState state, InputHandler inputHandler) {
      var entity = state.NextEntity;
      var actionTimeComponent = entity.GetComponent<ActionTimeComponent>();

      if (entity.IsInGroup(PlayerComponent.ENTITY_GROUP)) {
        // We force the player to pick a level-up if they have any available.
        if (entity.GetComponent<XPTrackerComponent>().UnusedLevelUps.Count > 0) {
          ShowCharacterMenu(state);
        }

        // TODO: Not on every process()
        var action = inputHandler.PopQueue();

        // If you interrupt autopilot in any way it immediately shuts off
        if (action != null && entity.GetComponent<PlayerComponent>().IsAutopiloting) {
          Rulebook.ResolveAction(new AutopilotEndAction(entity.EntityId, AutopilotEndReason.PLAYER_INPUT), state);
        }

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
        } else if (action == InputHandler.ActionMapping.AUTOPILOT) {
          ShowAutopilotMenu(state);
        } else if (action == InputHandler.ActionMapping.CHARACTER) {
          ShowCharacterMenu(state);
        } else if (action == InputHandler.ActionMapping.INVENTORY) {
          ShowInventoryMenu(state);
        } else if (action == InputHandler.ActionMapping.USE_STAIRS) {
          PlayerExecuteTurnEndingAction(new UseStairsAction(entity.EntityId), state);
        } else if (action == InputHandler.ActionMapping.GET_ITEM) {
          PlayerExecuteTurnEndingAction(new GetItemAction(entity.EntityId), state);
        } else if (action == InputHandler.ActionMapping.USE_ITEM) {
          GD.Print("Select an item via the inventory menu instead!");
        } else if (entity.GetComponent<PlayerComponent>().IsAutopiloting) {
          // TODO: The player sprite lags the true position significantly because the Tween can't keep up
          var path = entity.GetComponent<PlayerComponent>().AutopilotPath;
          var seesEnemies = state.FoVCache.VisibleCells
              .Select(cell => state.EntitiesAtPosition(cell.X, cell.Y))
              .Any(entitiesAtPosition => entitiesAtPosition.Any(e => e.GetComponent<AIComponent>() != null && !(e.GetComponent<PathAIComponent>() is PathAIComponent)));

          if (seesEnemies) {
            Rulebook.ResolveAction(new AutopilotEndAction(entity.EntityId, AutopilotEndReason.ENEMY_DETECTED), state);
          } else if (!path.AtEnd) {
            PlayerMove(state, new MoveAction(entity.EntityId, path.Step()));
          } else {
            Rulebook.ResolveAction(new AutopilotEndAction(entity.EntityId, AutopilotEndReason.DESTINATION_REACHED), state);
          }
        } else if (action != null) {
          GD.Print("No handler yet for ", action);
        }
      } else {
        // TODO: Figure out using the delta in Process how many of these we permit to run?
        int numTurnsToRun = 15;
        int numTurnsRan = 0;
        while (!entity.IsInGroup(PlayerComponent.ENTITY_GROUP) && numTurnsRan < numTurnsToRun) {
          AIComponent aIComponent = entity.GetComponent<AIComponent>();
          var aIActions = aIComponent.DecideNextAction(state, entity);
          Rulebook.ResolveActionsAndEndTurn(aIActions, state);

          // TODO: this seems...fragile?
          if (aIComponent is PathAIComponent) {
            state.UpdateDangerMap();
          }

          entity = state.NextEntity;
          numTurnsRan += 1;
        }
        GD.Print("You should save here!");
      }
    }

    // TODO: These are basically identical we can conslidate
    private void ShowAutopilotMenu(EncounterState state) {
      // TODO: We could probably make the cleaner by using signals?
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowAutopilotMenu(state);
    }

    private void ShowCharacterMenu(EncounterState state) {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowCharacterMenu(state);
    }

    private void ShowInventoryMenu(EncounterState state) {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowInventoryMenu(state);
    }

    public void HandleAutopilotSelection(EncounterZone selectedZone) {
      var playerId = this._encounterState.Player.EntityId;
      Rulebook.ResolveAction(new AutopilotBeginAction(playerId, selectedZone.ZoneId), this._encounterState);
      Rulebook.ResolveEndTurn(this._encounterState.Player.EntityId, this._encounterState);
    }

    public void HandleUseItemSelection(string itemIdToUse) {
      var playerId = this._encounterState.Player.EntityId;
      PlayerExecuteTurnEndingAction(new UseAction(playerId, itemIdToUse), this._encounterState);
    }
  }
}