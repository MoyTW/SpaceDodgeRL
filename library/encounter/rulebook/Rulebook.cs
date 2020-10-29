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
    private static Dictionary<ActionType, Action<EncounterAction, EncounterState>> _actionMapping = new Dictionary<ActionType, Action<EncounterAction, EncounterState>>() {
      { ActionType.AUTOPILOT, (a, s) => ResolveAutopilot(a as AutopilotAction, s) },
      { ActionType.MOVE, (a, s) => ResolveMove(a as MoveAction, s) },
      { ActionType.FIRE_PROJECTILE, (a, s) => ResolveFireProjectile(a as FireProjectileAction, s) },
      { ActionType.GET_ITEM, (a, s) => ResolveGetItem(a as GetItemAction, s) },
      { ActionType.SELF_DESTRUCT, (a, s) => ResolveSelfDestruct(a as SelfDestructAction, s) },
      { ActionType.USE, (a, s) => ResolveUse(a as UseAction, s) },
      { ActionType.USE_STAIRS, (a, s) => ResolveUseStairs(a as UseStairsAction, s) },
      { ActionType.WAIT, (a, s) => ResolveWait(a as WaitAction, s) }
    };

    private static void EndTurnIfStillExists(string entityId, EncounterState state) {
      Entity entity = state.GetEntityById(entityId);
      if (entity != null) {
        var actionTimeComponent = entity.GetComponent<ActionTimeComponent>();
        actionTimeComponent.EndTurn(entity.GetComponent<SpeedComponent>());
      }
    }

    public static void ResolveActions(List<EncounterAction> actions, EncounterState state) {
      actions.ForEach((action) => ResolveAction(action, state));
      if (actions.Count > 0) {
        EndTurnIfStillExists(actions[0].ActorId, state);
      } else {
        throw new NotImplementedException("should never resolve zero actions");
      }
    }

    private static void ResolveAction(EncounterAction action, EncounterState state) {
      // If I had C# 8.0 I'd use the new, nifty switch! I'm using a dictionary because I *always* forget to break; out of a switch
      // and that usually causes an annoying bug that I spend way too long mucking around with. Instead here it will just EXPLODE!
      _actionMapping[action.ActionType].Invoke(action, state);
    }

    // TODO: If you autopilot to your current position, then autopilot somewhere else, it explodes with a could not pass time error
    private static void ResolveAutopilot(AutopilotAction action, EncounterState state) {
      var playerPosition = state.Player.GetComponent<PositionComponent>().EncounterPosition;
      EncounterZone zone = state.GetZoneById(action.ZoneId);

      var path = new EncounterPath(Pathfinder.AStarWithNewGrid(playerPosition, zone.Center, state, 600));
      state.Player.GetComponent<PlayerComponent>().LayInAutopilotPath(path);
    }

    private static void LogAttack(DefenderComponent defenderComponent, string message, EncounterState state) {
      if (defenderComponent.ShouldLogDamage) {
        state.LogMessage(message);
      }
    }

    // TODO: Implement XP and player level up
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

    private static void ResolveMove(MoveAction action, EncounterState state) {
      Entity actor = state.GetEntityById(action.ActorId);
      var positionComponent = state.GetEntityById(action.ActorId).GetComponent<PositionComponent>();

      if (positionComponent.EncounterPosition == action.TargetPosition) {
        GD.PrintErr(string.Format("Entity {0}:{1} tried to move to its current position {2}", actor.EntityName, actor.EntityId, action.TargetPosition));
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
      } else {
        state.TeleportEntity(actor, action.TargetPosition);
      }
    }

    private static void ResolveUse(UseAction action, EncounterState state) {
      // We assume that the used entity must be in the inventory of the user - this is pretty fragile and might change.
      var user = state.GetEntityById(action.ActorId);
      var usable = user.GetComponent<InventoryComponent>().StoredEntityById(action.UsableId);

      if (usable.GetComponent<UsableComponent>() == null) {
        throw new NotImplementedException("can't use non-usable thing TODO: Handle better!");
      }

      state.LogMessage(string.Format("{0} used {1}!", user.EntityName, usable.EntityName));

      // We keep this logic here instead of in the component itself because the component should have only state data. That said
      // we shouldn't keep it, like, *here* here, 'least not indefinitely.
      var useEffectHeal = usable.GetComponent<UseEffectHealComponent>();
      if (useEffectHeal != null) {
        var restored = user.GetComponent<DefenderComponent>().RestoreHP(useEffectHeal.Healpower);
        state.LogMessage(string.Format("{0} restored {1} HP to {2}!", usable.EntityName, restored, user.EntityName));
      }
    }

    private static void ResolveUseStairs(UseStairsAction action, EncounterState state) {
      var actorPosition = state.GetEntityById(action.ActorId).GetComponent<PositionComponent>().EncounterPosition;
      var stairs = state.EntitiesAtPosition(actorPosition.X, actorPosition.Y)
                        .FirstOrDefault(e => e.GetComponent<StairsComponent>() != null);
      if (stairs != null) {
        state.InitState(state.Player, state.DungeonLevel + 1);
      } else {
        GD.Print("TODO: Make this not eat your turn!");
      }
    }

    private static void ResolveWait(WaitAction action, EncounterState state) { }

    private static void ResolveFireProjectile(FireProjectileAction action, EncounterState state) {
      var actorPosition = state.GetEntityById(action.ActorId).GetComponent<PositionComponent>().EncounterPosition;
      Entity projectile = EntityBuilder.CreateProjectileEntity(action.ProjectileName, action.Power, action.PathFunction(actorPosition), action.Speed);
      state.PlaceEntity(projectile, actorPosition, true);
    }

    private static void ResolveGetItem(GetItemAction action, EncounterState state) {
      var actor = state.GetEntityById(action.ActorId);
      var actorPosition = actor.GetComponent<PositionComponent>().EncounterPosition;
      var item = state.EntitiesAtPosition(actorPosition.X, actorPosition.Y)
                      .FirstOrDefault(e => e.GetComponent<StorableComponent>() != null);

      if (item != null) {
        state.RemoveEntity(item);
        actor.GetComponent<InventoryComponent>().AddEntity(item);

        var logMessage = string.Format("[b]{0}[/b] has taken the [b]{1}[/b]", actor.EntityName, item.EntityName);
        state.LogMessage(logMessage);
      } else {
        GD.Print("TODO: Make this not eat your turn!");
      }
    }

    private static void ResolveSelfDestruct(SelfDestructAction action, EncounterState state) {
      Entity entity = state.GetEntityById(action.ActorId);
      state.RemoveEntity(entity);
    }
  }
}