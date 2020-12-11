using Godot;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.components;
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
      { ActionType.AUTOPILOT_END, (a, s) => ResolveAutopilotEnd(a as AutopilotEndAction, s) },
      { ActionType.MOVE, (a, s) => ResolveMove(a as MoveAction, s) },
      { ActionType.FIRE_PROJECTILE, (a, s) => ResolveFireProjectile(a as FireProjectileAction, s) },
      { ActionType.GET_ITEM, (a, s) => ResolveGetItem(a as GetItemAction, s) },
      { ActionType.SELF_DESTRUCT, (a, s) => ResolveSelfDestruct(a as SelfDestructAction, s) },
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

    private static bool ResolveAutopilotBegin(AutopilotBeginAction action, EncounterState state) {
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
        state.Player.GetComponent<PlayerComponent>().LayInAutopilotPath(new EncounterPath(foundPath));
        return true;
      } else if (foundPath != null) {
        state.LogMessage(String.Format("Autopiloting to [b]{0}[/b]", zone.ZoneName));
        state.Player.GetComponent<PlayerComponent>().LayInAutopilotPath(new EncounterPath(foundPath));
        return true;
      } else {
        state.LogMessage(String.Format("Autopilot failed to plot course to to [b]{0}[/b]", zone.ZoneName));
        return false;
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
          // TODO: If the source has been destroyed in between the firing of the projectile and it destroying something else,
          // then projectileSource will be null and this will crash.
          var projectileSource = state.GetEntityById(attackerComponent.SourceEntityId);
          var xpValueComponent = defender.GetComponent<XPValueComponent>();
          if (projectileSource != null && xpValueComponent != null && projectileSource.GetComponent<XPTrackerComponent>() != null) {
            projectileSource.GetComponent<XPTrackerComponent>().AddXP(xpValueComponent.XPValue);
            logMessage += String.Format(" [b]{0}[/b] gains {1} XP!", projectileSource.EntityName, xpValueComponent.XPValue);
          }

          LogAttack(defenderComponent, logMessage, state);
          // TODO: Change "SelfDestructAction" to "RemoveAction" or something & add a toggle/line for log text?
          ResolveAction(new SelfDestructAction(defender.EntityId), state);
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
        // TODO: This causes the projectile to vanish, which is...awkward, visually, since we're Tweening it at the time!
        if (actorCollision.OnCollisionSelfDestruct) {
          ResolveAction(new SelfDestructAction(action.ActorId), state);
        }
        return true;
      } else {
        state.TeleportEntity(actor, action.TargetPosition);
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
          actor.EntityName, item.EntityName));
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

    private static bool ResolveSelfDestruct(SelfDestructAction action, EncounterState state) {
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
        // TODO: Make util code for "for each cell in radius"? we have at least one VERY similar invocation in calculating player
        // overlays, and probably also would have it in the player laser targeting too
        // TODO: boy is this just flat-out ugly
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

    // TODO: Consider putting all these effects under UsableComponent, instead of keeping them as components
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
        return false;
      }
    }

    private static bool ResolveWait(WaitAction action, EncounterState state) {
      return true;
    }
  }
}