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

    // We use this to determine whether the UI should refresh; this will cause a comical number of UI updates since we'll be
    // sending it every end turn, but I don't want to have to go in and effectively instrument all my state changes to set up
    // "change on data update" changes.
    [Signal] public delegate void TurnEnded();
    [Signal] public delegate void PositionScanned(int x, int y, Entity scannedEntity);

    public InputHandler inputHandlerRef = null;

    private EncounterState _encounterState;
    public void SetEncounterState(EncounterState encounterState) {
      this._encounterState = encounterState;
    }

    public override void _Process(float delta) {
      RunTurn(this._encounterState, inputHandlerRef);
    }

    // TODO: Write a system that has a component which does this instead of hard-coding it into the player's turn end
    private void PlayerExecuteTurnEndingAction(EncounterAction action, EncounterState state) {
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
      EmitSignal(nameof(EncounterRunner.TurnEnded));
      state.UpdateFoVAndFoW();
      state.UpdatePlayerOverlays();
    }

    private void PlayerMove(EncounterState state, int dx, int dy) {
      var positionComponent = state.Player.GetComponent<PositionComponent>();
      var oldPos = positionComponent.EncounterPosition;
      var moveAction = new MoveAction(state.Player.EntityId, new EncounterPosition(oldPos.X + dx, oldPos.Y + dy));
      PlayerMove(state, moveAction);
    }

    private void PlayerMove(EncounterState state, MoveAction moveAction) {
      PlayerExecuteTurnEndingAction(moveAction, state);
    }

    private void PlayerWait(EncounterState state) {
      var waitAction = new WaitAction(state.Player.EntityId);
      PlayerExecuteTurnEndingAction(waitAction, state);
    }

    private void RunTurn(EncounterState state, InputHandler inputHandler) {
      if (state.RunStatus == EncounterState.RUN_STATUS_PLAYER_DEFEAT) {
        var sceneManager = (SceneManager)GetNode("/root/SceneManager");
        sceneManager.ShowDefeatMenu(state);
        return;
      } else if (state.RunStatus == EncounterState.RUN_STATUS_PLAYER_VICTORY) {
        var sceneManager = (SceneManager)GetNode("/root/SceneManager");
        sceneManager.ShowVictoryMenu(state);
        return;
      }

      var entity = state.NextEntity;
      var actionTimeComponent = entity.GetComponent<ActionTimeComponent>();

      if (entity.IsInGroup(PlayerComponent.ENTITY_GROUP)) {
        // We force the player to pick a level-up if they have any available.
        if (entity.GetComponent<XPTrackerComponent>().UnusedLevelUps.Count > 0) {
          ShowCharacterMenu(state);
        }

        var action = inputHandler.PopQueue();

        // If you interrupt autopilot in any way it immediately shuts off
        if (action != null && entity.GetComponent<PlayerComponent>().IsAutopiloting) {
          Rulebook.ResolveAction(new AutopilotEndAction(entity.EntityId, AutopilotEndReason.PLAYER_INPUT), state);
        }

        // Super not a fan of the awkwardness of checking this twice! Switch string -> enum, maybe?
        // TODO: this is a jank if & the conditions are hard to read
        if (action != null && action.Mapping == InputHandler.ActionMapping.MOVE_N) {
          PlayerMove(state, 0, -1);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.MOVE_NE) {
          PlayerMove(state, 1, -1);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.MOVE_E) {
          PlayerMove(state, 1, 0);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.MOVE_SE) {
          PlayerMove(state, 1, 1);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.MOVE_S) {
          PlayerMove(state, 0, 1);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.MOVE_SW) {
          PlayerMove(state, -1, 1);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.MOVE_W) {
          PlayerMove(state, -1, 0);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.MOVE_NW) {
          PlayerMove(state, -1, -1);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.WAIT) {
          PlayerWait(state);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.AUTOPILOT) {
          ShowAutopilotMenu(state);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.CHARACTER) {
          ShowCharacterMenu(state);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.ESCAPE_MENU) {
          ShowEscapeMenu(state);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.INVENTORY) {
          ShowInventoryMenu(state);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.USE_STAIRS) {
          PlayerExecuteTurnEndingAction(new UseStairsAction(entity.EntityId), state);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.GET_ITEM) {
          PlayerExecuteTurnEndingAction(new GetItemAction(entity.EntityId), state);
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.USE_ITEM) {
          GD.Print("Select an item via the inventory menu instead!");
        } else if (action != null && action.Mapping == InputHandler.ActionMapping.SCAN_POSITION) {
          var scanAction = action as InputHandler.ScanInputAction;
          var blockingEntity = state.BlockingEntityAtPosition(scanAction.X, scanAction.Y);
          var allEntities = state.EntitiesAtPosition(scanAction.X, scanAction.Y);
          if (blockingEntity != null) {
            EmitSignal(nameof(PositionScanned), scanAction.X, scanAction.Y, blockingEntity);
          } else if (allEntities.Count > 0) {
            EmitSignal(nameof(PositionScanned), scanAction.X, scanAction.Y, allEntities[0]);
          } else {
            EmitSignal(nameof(PositionScanned), scanAction.X, scanAction.Y, null);
          }
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
          StatusEffectTrackerComponent statusTracker = entity.GetComponent<StatusEffectTrackerComponent>();

          List<EncounterAction> aIActions;
          if (statusTracker != null && statusTracker.HasDisabledEffect()) {
            aIActions = new List<EncounterAction>() { new WaitAction(entity.EntityId) };
          } else {
            aIActions = aIComponent.DecideNextAction(state, entity);
          }
          Rulebook.ResolveActionsAndEndTurn(aIActions, state);
          EmitSignal(nameof(EncounterRunner.TurnEnded));

          // TODO: this seems...fragile?
          if (aIComponent is PathAIComponent) {
            state.UpdateDangerMap();
          }

          entity = state.NextEntity;
          numTurnsRan += 1;
        }
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

    private void ShowEscapeMenu(EncounterState state) {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowEscapeMenu(state);
    }

    private void ShowInventoryMenu(EncounterState state) {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowInventoryMenu(state);
    }

    // Instead of calling into runner like this, put it into InputHandler!
    public void HandleAutopilotSelection(string selectedZoneId) {
      var playerId = this._encounterState.Player.EntityId;
      Rulebook.ResolveAction(new AutopilotBeginAction(playerId, selectedZoneId), this._encounterState);
      Rulebook.ResolveEndTurn(this._encounterState.Player.EntityId, this._encounterState);
    }

    public void HandleUseItemSelection(string itemIdToUse) {
      var playerId = this._encounterState.Player.EntityId;
      PlayerExecuteTurnEndingAction(new UseAction(playerId, itemIdToUse, true), this._encounterState);
    }
  }
}