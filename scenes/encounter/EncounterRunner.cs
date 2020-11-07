using System.Collections.Generic;
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

    // TODO: Switch from tracking TTL to tracking "next tick", and then comparing to "current tick" in state - will save on having to iterate entities
    // So, basically, should have each entity continuously update to "next tick" instead of subtracting "time to next tick" until 0
    // will make passing time O(1)  - we will just update EncounterState's CurrentTick
    // if we combine that with an internal storage in CalculateNextEntity() or remove CalculateNextEntity() it can basically cut out the whole
    // series of entity iterations that we use to manage time
    private static void PassTime(EncounterState state, int time) {
      var actionEntities = state.ActionEntities();
      foreach (Entity entity in actionEntities) {
        var actionTimeComponent = entity.GetComponent<ActionTimeComponent>();
        actionTimeComponent.PassTime(time);
      }
    }

    // TODO: Write a system that has a component which does this instead of hard-coding it into the player's turn end
    private static void PlayerExecuteTurnEndingAction(EncounterAction action, EncounterState state) {
      var player = state.Player;
      var playerPosition = player.GetComponent<PositionComponent>().EncounterPosition;
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
      var playerComponent = player.GetComponent<PlayerComponent>();
      if (closestEnemyPosition != null && closestEnemyDistance <= playerComponent.CuttingLaserRange) {
        var fireAction = FireProjectileAction.CreateCuttingLaserAction(
          player.EntityId, playerComponent.CuttingLaserPower, closestEnemyPosition.EncounterPosition);
        actions.Add(fireAction);
      }

      // TODO: This actually doesn't fire on moving towards an enemy, because the "determine fire" happens BEFORE you enter the radius
      // so if you spend a turn moving in, you don't know to fire! this is badly incorrect!
      // The issue is that the ResolveActions() bundles all state changes into one invocation and then ends the turn, so you can't have something like
      // 1. move towards your enemy
      // 2. if moving means you're close enough to see your enemy, activate your shields
      // because there's no time for logic in between. It also means you can't really have 'free' actions without restructuring how ResolveActions works.
      // One thing we could do is unbundle ResolveActions(...) into ResolveAction, explicitly add an END_TURN action, and then just expect the caller to
      // do the appropriate thing with turn endings.
      // This ALSO lets us do things like hook an "end turn", uh, hook.
      Rulebook.ResolveActions(actions, state);

      // After the player executes their turn we need to update the UI
      state.CalculateNextEntity();
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
      if (actionTimeComponent.TicksUntilTurn > 0) {
        EncounterRunner.PassTime(state, actionTimeComponent.TicksUntilTurn);
      }

      if (entity.IsInGroup(PlayerComponent.ENTITY_GROUP)) {
        // TODO: Not on every process()
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
        } else if (action == InputHandler.ActionMapping.AUTOPILOT) {
          ShowAutopilotMenu(state);
        } else if (action == InputHandler.ActionMapping.USE_STAIRS) {
          // TODO: Attempting to use stairs definitely shouldn't end your turn lol
          PlayerExecuteTurnEndingAction(new UseStairsAction(entity.EntityId), state);
        } else if (action == InputHandler.ActionMapping.GET_ITEM) {
          // TODO: Shouldn't end turn if fails due to no gettable items
          PlayerExecuteTurnEndingAction(new GetItemAction(entity.EntityId), state);
        } else if (action == InputHandler.ActionMapping.USE_ITEM) {
          // TODO: Query player for "what item"
          var items = state.Player.GetComponent<InventoryComponent>().StoredItems;
          if (items.Count > 0) {
            PlayerExecuteTurnEndingAction(new UseAction(entity.EntityId, (items[0] as Entity).EntityId), state);
          } else {
            GD.Print("TODO: you have no items, fix this janky 'use first' prototype");
          }
        } else if (entity.GetComponent<PlayerComponent>().IsAutopiloting) {
          // TODO: Allow player to interrupt to turn off autopiloting
          // TODO: If your machine is slow and you buffer your move inputs/use autopilot the targeting reticule and FoW sort
          // of don't keep up!
          // TODO: Add in termination condition of "enemy enters FoV"
          // TODO: It NPEs when you try to autopilot to Zone 2 on 3cbebd008eca2f139a119c2f05adbd4fcebcfb01
          var path = entity.GetComponent<PlayerComponent>().AutopilotPath;
          if (!path.AtEnd) {
            PlayerMove(state, new MoveAction(entity.EntityId, path.Step()));
          } else {
            // TODO: STOP_AUTOPILOTING action, don't state change here
            entity.GetComponent<PlayerComponent>().StopAutopiloting();
          }
        } else if (action != null) {
          GD.Print("No handler yet for ", action);
        }
      } else {
        AIComponent aIComponent = entity.GetComponent<AIComponent>();
        var aIActions = aIComponent.DecideNextAction(state);
        Rulebook.ResolveActions(aIActions, state);
        state.CalculateNextEntity();
        state.UpdateDangerMap();
      }
    }

    private void ShowAutopilotMenu(EncounterState state) {
      // TODO: We could probably make the cleaner by using signals?
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ShowAutopilotMenu(state);
    }

    public void HandleAutopilotSelection(EncounterZone selectedZone) {
      var actions = new List<EncounterAction>() { new AutopilotAction(this._encounterState.Player.EntityId, selectedZone.ZoneId) };
      Rulebook.ResolveActions(actions, this._encounterState);
    }
  }
}