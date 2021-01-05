using Godot;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.components.use;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceDodgeRL.library.encounter.rulebook {

  public static class Rulebook {
    // ...can't I just do <> like I can in Kotlin? C# why you no let me do this. Probably because "evolving languages are hard".
    private static Dictionary<ActionType, Func<EncounterAction, EncounterState, bool>> _actionMapping = new Dictionary<ActionType, Func<EncounterAction, EncounterState, bool>>() {
      { ActionType.AUTOPILOT_BEGIN, (a, s) => ResolveAutopilotBegin(a as AutopilotBeginAction, s) },
      { ActionType.AUTOPILOT_CONTINUE, (a, s) => ResolveAutopilotContinue(a as AutopilotContinueAction, s) },
      { ActionType.AUTOPILOT_END, (a, s) => ResolveAutopilotEnd(a as AutopilotEndAction, s) },
      { ActionType.MOVE, (a, s) => ResolveMove(a as MoveAction, s) },
      { ActionType.FIRE_PROJECTILE, (a, s) => ResolveFireProjectile(a as FireProjectileAction, s) },
      { ActionType.GET_ITEM, (a, s) => ResolveGetItem(a as GetItemAction, s) },
      { ActionType.DESTROY, (a, s) => ResolveDestroy(a as DestroyAction, s) },
      { ActionType.SPAWN_ENTITY, (a, s) => ResolveSpawnEntity(a as SpawnEntityAction, s) },
      { ActionType.USE, (a, s) => ResolveUse(a as UseAction, s) },
      { ActionType.USE_STAIRS, (a, s) => ResolveUseStairs(a as UseStairsAction, s) },
      { ActionType.WAIT, (a, s) => ResolveWait(a as WaitAction, s) }
    };

    public static bool ResolveAction(EncounterAction action, EncounterState state) {
      // If I had C# 8.0 I'd use the new, nifty switch! I'm using a dictionary because I *always* forget to break; out of a switch
      // and that usually causes an annoying bug that I spend way too long mucking around with. Instead here it will just EXPLODE!
      return _actionMapping[action.ActionType].Invoke(action, state);
    }

    public static bool ResolveEndTurn(string entityId, EncounterState state) {
      Entity entity = state.GetEntityById(entityId);
      if (entity != null) {
        var actionTimeComponent = entity.GetComponent<ActionTimeComponent>();
        actionTimeComponent.EndTurn(entity.GetComponent<SpeedComponent>(), entity.GetComponent<StatusEffectTrackerComponent>());
        state.EntityHasEndedTurn(entity);
        return true;
      } else {
        return false;
      }
    }

    /**
     * Resolves a list of actions, and then ends the turn. Does not check the results of the actions, so should only really be
     * used when you're confident the actions will be successful and don't need to be able to interject additional logic.
     */
    public static void ResolveActionsAndEndTurn(List<EncounterAction> actions, EncounterState state) {
      actions.ForEach((action) => ResolveAction(action, state));
      if (actions.Count > 0) {
        ResolveEndTurn(actions[0].ActorId, state);
      } else {
        throw new NotImplementedException("should never resolve zero actions");
      }
    }

    private static bool ResolveAutopilotBeginTravel(AutopilotBeginAction action, EncounterState state) {
      var playerPosition = state.Player.GetComponent<PositionComponent>().EncounterPosition;
      EncounterZone zone = state.GetZoneById(action.ZoneId);

      var foundPath = Pathfinder.AStarWithNewGrid(playerPosition, zone.Center, state, 9000);
      int autopilotTries = 0;
      while (foundPath == null && autopilotTries < 5) {
        foundPath = Pathfinder.AStarWithNewGrid(playerPosition, zone.RandomEmptyPosition(state.EncounterRand, state), state, 9000);
        autopilotTries++;
      }
      if (foundPath != null && autopilotTries > 0) {
        state.LogMessage(String.Format("Autopilot could not find path to center of [b]{0}[/b]; autopiloting to randomly chosen position in [b]{0}[/b].", zone.ZoneName));
        state.Player.GetComponent<PlayerComponent>().LayInAutopilotPathForTravel(new EncounterPath(foundPath));
        return true;
      } else if (foundPath != null) {
        state.LogMessage(String.Format("Autopiloting to [b]{0}[/b]", zone.ZoneName));
        state.Player.GetComponent<PlayerComponent>().LayInAutopilotPathForTravel(new EncounterPath(foundPath));
        return true;
      } else {
        state.LogMessage(String.Format("Autopilot failed to plot course to to [b]{0}[/b]", zone.ZoneName), failed: true);
        return false;
      }
    }

    private static bool ResolveAutopilotBeginExplore(AutopilotBeginAction action, EncounterState state) {
      var playerComponent = state.Player.GetComponent<PlayerComponent>();
      playerComponent.BeginAutoexploring(action.ZoneId);
      return true;
    }

    private static bool ResolveAutopilotBegin(AutopilotBeginAction action, EncounterState state) {
      if (action.Mode == AutopilotMode.TRAVEL) {
        return ResolveAutopilotBeginTravel(action, state);
      } else if (action.Mode == AutopilotMode.EXPLORE) {
        return ResolveAutopilotBeginExplore(action, state);
      } else {
        throw new NotImplementedException(string.Format("Rulebook doesn't know how to handle autopilot mode {0}", action.Mode));
      }
    }

    private static bool PlayerSeesEnemies(EncounterState state) {
      return state.FoVCache.VisibleCells
          .Select(cell => state.EntitiesAtPosition(cell.X, cell.Y))
          .Any(entitiesAtPosition => entitiesAtPosition.Any(e => e.GetComponent<AIComponent>() != null &&
                                                                 !(e.GetComponent<PathAIComponent>() is PathAIComponent)));
    }

    private static bool ResolveAutopilotContinueTravel(EncounterState state) {
      var player = state.Player;
      var path = state.Player.GetComponent<PlayerComponent>().AutopilotPath;

      if (PlayerSeesEnemies(state)) {
        ResolveAction(new AutopilotEndAction(player.EntityId, AutopilotEndReason.ENEMY_DETECTED), state);
        return false;
      } else if (!path.AtEnd) {
        Rulebook.ResolveAction(new MoveAction(player.EntityId, path.Step()), state);
        return true;
      } else {
        ResolveAction(new AutopilotEndAction(player.EntityId, AutopilotEndReason.TASK_COMPLETED), state);
        return false;
      }
    }

    /**
     * A zone is considered "explored" when:
     * 1: All storables have been found
     *   1a: If your inventory is not full, all storable items have been picked up
     * 2: All non-storable features have been seen
     *
     * Actual % of FoW revealed is not important. This is kinda cheaty, because it implies the autopilot knows intel for the
     * area, but also, who cares? It's fine. Likewise it doesn't factor into account having destroyed the enemies because if
     * there is an encounter, you'll definitely run into it!
     */
    private static bool ResolveAutopilotContinueExplore(EncounterState state) {
      var player = state.Player;
      var playerComponent = player.GetComponent<PlayerComponent>();
      var playerPos = player.GetComponent<PositionComponent>().EncounterPosition;
      var path = playerComponent.AutopilotPath;

      var zone = state.GetZoneById(playerComponent.AutopilotZoneId);
      var nextUngottenStorable =
        zone.ReadoutItems.Concat(zone.ReadoutFeatures)
                         .FirstOrDefault(i => state.GetEntityById(i.EntityId) != null && 
                                              state.GetEntityById(i.EntityId).GetComponent<StorableComponent>() != null);
      // We rely on the fact that there's no ungettable features other than stairs, and that there's only one stair, to make this
      // work. If there were multiple ungettable features you'd want to put them into a list and autopilot between them so you
      // could cycle the 'x' button to find the one you want.
      var stairs = zone.ReadoutFeatures.FirstOrDefault(r =>
        state.GetEntityById(r.EntityId).GetComponent<StairsComponent>() != null &&
        state.GetEntityById(r.EntityId).GetComponent<PositionComponent>().EncounterPosition != playerPos
      );

      if (PlayerSeesEnemies(state)) {
        ResolveAction(new AutopilotEndAction(player.EntityId, AutopilotEndReason.ENEMY_DETECTED), state);
        return false;
      } // If you are already on a path, progress on the path
      else if (path != null) {
        if (path.AtEnd) {
          playerComponent.ClearAutopilotPath();
          return false;
        } else {
          Rulebook.ResolveAction(new MoveAction(player.EntityId, path.Step()), state);
          return true;
        }
      } // If you're on top of a storable item, get it
      else if (state.EntitiesAtPosition(playerPos.X, playerPos.Y).Any(e => e.GetComponent<StorableComponent>() != null)) {
        var nextStorable = state.EntitiesAtPosition(playerPos.X, playerPos.Y).First(e => e.GetComponent<StorableComponent>() != null);
        if (ResolveAction(new GetItemAction(player.EntityId), state)) {
          return true;
        } else {
          ResolveAction(new AutopilotEndAction(player.EntityId, AutopilotEndReason.INVENTORY_FULL), state);
          return false;
        }
      } // If there are any storable items, move towards them
      else if (nextUngottenStorable != null) {
        var nextPos = state.GetEntityById(nextUngottenStorable.EntityId).GetComponent<PositionComponent>().EncounterPosition;
        var foundPath = Pathfinder.AStarWithNewGrid(playerPos, nextPos, state, 900);
        if (foundPath == null) {
          ResolveAction(new AutopilotEndAction(player.EntityId, AutopilotEndReason.NO_PATH), state);
          return false;
        } else {
          foundPath.Add(nextPos);
          playerComponent.LayInAutopilotPathForExploration(new EncounterPath(foundPath));
          return true;
        }
      } // If this is the stairs zone, go to the stairs
      else if (stairs != null) {
        var nextPos = state.GetEntityById(stairs.EntityId).GetComponent<PositionComponent>().EncounterPosition;
        var foundPath = Pathfinder.AStarWithNewGrid(playerPos, nextPos, state, 900);
        if (foundPath == null) {
          ResolveAction(new AutopilotEndAction(player.EntityId, AutopilotEndReason.NO_PATH), state);
          return false;
        } else {
          foundPath.Add(nextPos);
          playerComponent.LayInAutopilotPathForExploration(new EncounterPath(foundPath));
          return true;
        }
      } // Otherwise you're done!
      else {
        ResolveAction(new AutopilotEndAction(player.EntityId, AutopilotEndReason.TASK_COMPLETED), state);
        return false;
      }
    }

    /**
     * Handles autopilot according to player's internal state. Returns true if autopilot continues, false if it is ended.
     */
    private static bool ResolveAutopilotContinue(AutopilotContinueAction action, EncounterState state) {
      var mode = state.Player.GetComponent<PlayerComponent>().ActiveAutopilotMode;
      if (mode == AutopilotMode.TRAVEL) {
        return ResolveAutopilotContinueTravel(state);
      } else if (mode == AutopilotMode.EXPLORE) {
        return ResolveAutopilotContinueExplore(state);
      } else {
        throw new NotImplementedException(string.Format("Rulebook doesn't know how to handle autopilot mode {0}", mode));
      }
    }

    private static bool ResolveAutopilotEnd(AutopilotEndAction action, EncounterState state) {
      var playerComponent = state.Player.GetComponent<PlayerComponent>();

      if (action.Reason == AutopilotEndReason.PLAYER_INPUT) {
        state.LogMessage(String.Format("Autopilot ending - [b]overridden by pilot[/b]"));
      } else if (action.Reason == AutopilotEndReason.ENEMY_DETECTED) {
        state.LogMessage(String.Format("Autopilot ending - [b]enemy detected[/b]"));
      } else if (action.Reason == AutopilotEndReason.TASK_COMPLETED) {
        if (playerComponent.ActiveAutopilotMode == AutopilotMode.TRAVEL) {
          state.LogMessage(String.Format("Autopilot ending - [b]travel completed[/b]"));
        } else if (playerComponent.ActiveAutopilotMode == AutopilotMode.EXPLORE) {
          state.LogMessage(String.Format("Autopilot ending - [b]exploration completed[/b]"));
        } else {
          throw new NotImplementedException("no such explore mode known");
        }
      } else if (action.Reason == AutopilotEndReason.INVENTORY_FULL) {
        state.LogMessage(String.Format("Autopilot ending - [b]inventory was full[/b]"));
      } else if (action.Reason == AutopilotEndReason.NO_PATH) {
        state.LogMessage(String.Format("Autopilot ending - [b]path to next location blocked[/b]"));
      } else {
        throw new NotImplementedException("no such matching clause for enum");
      }

      state.Player.GetComponent<PlayerComponent>().StopAutopiloting();

      return true;
    }

    private static void LogAttack(DefenderComponent defenderComponent, string message, EncounterState state) {
      if (defenderComponent.ShouldLogDamage) {
        state.LogMessage(message);
      }
    }

    private static void Attack(Entity attacker, Entity defender, EncounterState state) {
      var attackerComponent = attacker.GetComponent<AttackerComponent>();
      var defenderComponent = defender.GetComponent<DefenderComponent>();

      if(defenderComponent.IsInvincible) {
        var logMessage = string.Format("[b]{0}[/b] hits [b]{1}[/b], but the attack has no effect!",
          attacker.EntityName, defender.EntityName);
        LogAttack(defenderComponent, logMessage, state);
      } else {
        // We don't allow underflow damage, though that could be a pretty comical mechanic...
        int damage = Math.Max(0, attackerComponent.Power - defenderComponent.Defense);
        defenderComponent.RemoveHp(damage);
        if (defenderComponent.CurrentHp <= 0) {
          var logMessage = string.Format("[b]{0}[/b] hits [b]{1}[/b] for {2} damage, destroying it!",
            attacker.EntityName, defender.EntityName, damage);

          // Assign XP to the entity that fired the projectile
          var projectileSource = state.GetEntityById(attackerComponent.SourceEntityId);
          var xpValueComponent = defender.GetComponent<XPValueComponent>();
          if (projectileSource != null && xpValueComponent != null && projectileSource.GetComponent<XPTrackerComponent>() != null) {
            projectileSource.GetComponent<XPTrackerComponent>().AddXP(xpValueComponent.XPValue);
            logMessage += String.Format(" [b]{0}[/b] gains {1} XP!", projectileSource.EntityName, xpValueComponent.XPValue);
          }

          LogAttack(defenderComponent, logMessage, state);
          ResolveAction(new DestroyAction(defender.EntityId), state);
        } else {
          var logMessage = string.Format("[b]{0}[/b] hits [b]{1}[/b] for {2} damage!",
            attacker.EntityName, defender.EntityName, damage);
            LogAttack(defenderComponent, logMessage, state);
        }
      }
    }

    private static bool ResolveMove(MoveAction action, EncounterState state) {
      Entity actor = state.GetEntityById(action.ActorId);
      var positionComponent = state.GetEntityById(action.ActorId).GetComponent<PositionComponent>();

      if (positionComponent.EncounterPosition == action.TargetPosition) {
        GD.PrintErr(string.Format("Entity {0}:{1} tried to move to its current position {2}", actor.EntityName, actor.EntityId, action.TargetPosition));
        return false;
      } else if (state.IsPositionBlocked(action.TargetPosition)) {
        var blocker = state.BlockingEntityAtPosition(action.TargetPosition.X, action.TargetPosition.Y);
        var actorCollision = actor.GetComponent<CollisionComponent>();

        if (actorCollision.OnCollisionAttack) {
          Attack(actor, blocker, state);
        }
        if (actorCollision.OnCollisionSelfDestruct) {
          state.TeleportEntity(actor, action.TargetPosition, ignoreCollision: true);
          if (state.FoVCache.IsVisible(action.TargetPosition)) {
            positionComponent.PlayExplosion();
          }
          ResolveAction(new DestroyAction(action.ActorId), state);
        }
        return true;
      } else {
        state.TeleportEntity(actor, action.TargetPosition, ignoreCollision: false);
        return true;
      }
    }

    private static bool ResolveFireProjectile(FireProjectileAction action, EncounterState state) {
      var actorPosition = state.GetEntityById(action.ActorId).GetComponent<PositionComponent>().EncounterPosition;
      Entity projectile = EntityBuilder.CreateProjectileEntity(
        state.GetEntityById(action.ActorId),
        action.ProjectileType,
        action.Power,
        action.PathFunction(actorPosition),
        action.Speed,
        state.CurrentTick
      );
      state.PlaceEntity(projectile, actorPosition, true);
      return true;
    }

    private static bool ResolveGetItem(GetItemAction action, EncounterState state) {
      var actor = state.GetEntityById(action.ActorId);
      var inventoryComponent = actor.GetComponent<InventoryComponent>();
      var actorPosition = actor.GetComponent<PositionComponent>().EncounterPosition;
      var item = state.EntitiesAtPosition(actorPosition.X, actorPosition.Y)
                      .FirstOrDefault(e => e.GetComponent<StorableComponent>() != null);

      if (item == null) {
        state.LogMessage("No item found!", failed: true);
        return false;
      } else if (item.GetComponent<UsableComponent>() != null && item.GetComponent<UsableComponent>().UseOnGet) {
        // The responsibility for removing/not removing the usable from the EncounterState is in the usage code.
        bool successfulUsage = ResolveUse(new UseAction(actor.EntityId, item.EntityId, false), state);
        if (!successfulUsage) {
          GD.PrintErr(string.Format("Item {0} was not successfully used after being picked up!", item.EntityName));
        }
        return true;
      } else if (!inventoryComponent.CanFit(item)) {
        state.LogMessage(string.Format("[b]{0}[/b] can't fit the [b]{1}[/b] in its inventory!",
          actor.EntityName, item.EntityName), failed: true);
        return false;
      } else {
        state.RemoveEntity(item);
        actor.GetComponent<InventoryComponent>().AddEntity(item);

        var logMessage = string.Format("[b]{0}[/b] has taken the [b]{1}[/b]", actor.EntityName, item.EntityName);
        state.LogMessage(logMessage);
        return true;
      }
    }

    private static bool ResolveOnDeathEffect(string effectType, EncounterState state) {
      if (effectType == OnDeathEffectType.PLAYER_VICTORY) {
        state.NotifyPlayerVictory();
        return false;
      } else if (effectType == OnDeathEffectType.PLAYER_DEFEAT) {
        state.NotifyPlayerDefeat();
        return false;
      } else {
        throw new NotImplementedException(String.Format("Don't know how to resolve on death effect type {0}", effectType));
      }
    }

    private static bool ResolveDestroy(DestroyAction action, EncounterState state) {
      Entity entity = state.GetEntityById(action.ActorId);

      var onDeathComponent = entity.GetComponent<OnDeathComponent>();
      // this 'shouldRemoveEntity' code is slightly confusing, simplify it if you come back to it
      bool shouldRemoveEntity = true;
      if (onDeathComponent != null) {
        foreach (var effectType in onDeathComponent.ActiveEffectTypes) {
          var effectStopsRemoval = !ResolveOnDeathEffect(effectType, state);
          if (effectStopsRemoval) {
            shouldRemoveEntity = false;
          }
        }
      }

      if (shouldRemoveEntity) {
        state.RemoveEntity(entity);
        return true;
      } else {
        return false;
      }
    }

    private static bool ResolveSpawnEntity(SpawnEntityAction action, EncounterState state) {
      state.PlaceEntity(action.EntityToSpawn, action.Position, action.IgnoreCollision);
      return true;
    }

    private static void ResolveUseEffects(Entity user, Entity usable, EncounterState state) {
      // We keep this logic here instead of in the component itself because the component should have only state data. That said
      // we shouldn't keep it, like, *here* here, 'least not indefinitely.
      var useEffectHeal = usable.GetComponent<UseEffectHealComponent>();
      if (useEffectHeal != null) {
        var restored = user.GetComponent<DefenderComponent>().RestoreHP(useEffectHeal.Healpower);
        state.LogMessage(string.Format("{0} restored {1} HP to {2}!", usable.EntityName, restored, user.EntityName));
      }

      var useEffectAddIntel = usable.GetComponent<UseEffectAddIntelComponent>();
      if (useEffectAddIntel != null) {
        // Kinda funny because player's the only one for which this effect is meaningful so we just grab player it's fiiiiiine
        state.Player.GetComponent<PlayerComponent>().RegisterIntel(useEffectAddIntel.TargetDungeonLevel);
        state.LogMessage(string.Format("Discovered intel for [b]sector {0}[/b]!", useEffectAddIntel.TargetDungeonLevel));
      }

      var useEffectBoostPower = usable.GetComponent<UseEffectBoostPowerComponent>();
      if (useEffectBoostPower != null) {
        state.LogMessage(String.Format("Attack power boosted by {0} for duration {1}!",
          useEffectBoostPower.BoostPower, useEffectBoostPower.Duration));
        var statusEffectTracker = user.GetComponent<StatusEffectTrackerComponent>();
        statusEffectTracker.AddEffect(new StatusEffectTimedPowerBoost(
          boostPower: useEffectBoostPower.BoostPower,
          startTick: state.CurrentTick,
          endTick: state.CurrentTick + useEffectBoostPower.Duration
        ));
      }

      var useEffectBoostSpeed = usable.GetComponent<UseEffectBoostSpeedComponent>();
      if (useEffectBoostSpeed != null) {
        state.LogMessage(String.Format("Speed boosted by {0} for duration {1}!",
          useEffectBoostSpeed.BoostPower, useEffectBoostSpeed.Duration));
        var statusEffectTracker = user.GetComponent<StatusEffectTrackerComponent>();
        statusEffectTracker.AddEffect(new StatusEffectTimedSpeedBoost(
          boostPower: useEffectBoostSpeed.BoostPower,
          startTick: state.CurrentTick,
          endTick: state.CurrentTick + useEffectBoostSpeed.Duration
        ));
      }

      var useEffectEMP = usable.GetComponent<UseEffectEMPComponent>();
      if (useEffectEMP != null) {
        state.LogMessage(String.Format("EMP detonated in radius {0} - disables {1} turns!",
          useEffectEMP.Radius, useEffectEMP.DisableTurns));
        var userPosition = user.GetComponent<PositionComponent>().EncounterPosition;
        for (int x = userPosition.X - useEffectEMP.Radius; x <= userPosition.X + useEffectEMP.Radius; x++) {
          for (int y = userPosition.Y - useEffectEMP.Radius; y <= userPosition.Y + useEffectEMP.Radius; y++) {
            var distance = userPosition.DistanceTo(x, y);
            if (distance <= useEffectEMP.Radius && state.IsInBounds(x, y)) {
              var entitiesAtPosition = state.EntitiesAtPosition(x, y);
              foreach (var entity in entitiesAtPosition) {
                var speedComponent = entity.GetComponent<SpeedComponent>();
                var statusTracker = entity.GetComponent<StatusEffectTrackerComponent>();
                if (entity != user && speedComponent != null && statusTracker != null) {
                  var disableTicks = speedComponent.Speed * useEffectEMP.DisableTurns;
                  statusTracker.AddEffect(new StatusEffectTimedDisable(state.CurrentTick, state.CurrentTick + disableTicks));
                  state.LogMessage(String.Format("{0} was disabled for {1} ticks!", entity.EntityName, disableTicks));
                }
              }
            }
          }
        }
      }
    }

    // Currently, each use effect is its own component. If we run into a case where we have too many effects, we can push the
    // effects into the usable component itself, similarly to status effects (though status effects are their own mess right now)
    // which would probably be better for building on.
    private static bool ResolveUse(UseAction action, EncounterState state) {
      var user = state.GetEntityById(action.ActorId);

      // This is another issue that'd be solved with a global Entity lookup - though not the removal part.
      Entity usable = null;
      if (action.FromInventory) {
        var userInventory = user.GetComponent<InventoryComponent>();
        usable = userInventory.StoredEntityById(action.UsableId);
        userInventory.RemoveEntity(usable);
      } else {
        usable = state.GetEntityById(action.UsableId);
        state.RemoveEntity(usable);
      }

      if (usable.GetComponent<UsableComponent>() == null) {
        throw new NotImplementedException("can't use non-usable thing TODO: Handle better!");
      }

      state.LogMessage(string.Format("{0} used {1}!", user.EntityName, usable.EntityName));

      ResolveUseEffects(user, usable, state);

      // We assume all items are single-use; this will change if I deviate from the reference implementation!
      usable.QueueFree();
      return true;
    }

    private static bool ResolveUseStairs(UseStairsAction action, EncounterState state) {
      var actorPosition = state.GetEntityById(action.ActorId).GetComponent<PositionComponent>().EncounterPosition;
      var stairs = state.EntitiesAtPosition(actorPosition.X, actorPosition.Y)
                        .FirstOrDefault(e => e.GetComponent<StairsComponent>() != null);
      if (stairs != null) {
        state.ResetStateForNewLevel(state.Player, state.DungeonLevel + 1);
        state.WriteToFile();
        return true;
      } else {
        state.LogMessage("No jump point found!", failed: true);
        return false;
      }
    }

    private static bool ResolveWait(WaitAction action, EncounterState state) {
      return true;
    }
  }
}