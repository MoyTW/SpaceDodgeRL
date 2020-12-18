using System;
using System.Collections.Generic;
using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.resources.gamedata;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.components.use;

namespace SpaceDodgeRL.scenes.entities {

  public static class EntityBuilder {

    private static string _iPath = "res://resources/atlas_i.tres";
    private static string _JPath = "res://resources/atlas_J.tres";
    private static string _StarPath = "res://resources/atlas_Star.tres";
    

    private static string _texCarrierPath = "res://resources/sprites/carrier.png";
    private static string _texCruiserPath = "res://resources/sprites/cruiser.png";
    private static string _texDestroyerPath = "res://resources/sprites/destroyer.png";
    private static string _texEMPPath = "res://resources/tex_EMP.tres";
    private static string _texDiplomatPath = "res://resources/sprites/diplomat.png";
    private static string _texGunshipPath = "res://resources/sprites/gunship.png";
    private static string _texFighterPath = "res://resources/sprites/fighter.png";
    private static string _texFrigatePath = "res://resources/sprites/frigate.png";
    private static string _texItemBatteryPath = "res://resources/sprites/item/battery.png";
    private static string _texItemDuctTapePath = "res://resources/sprites/item/duct_tape.png";
    private static string _texItemEMPPath = "res://resources/sprites/item/emp.png";
    private static string _texItemRedPaintPath = "res://resources/sprites/item/red_paint.png";
    private static string _texPlayerPath = "res://resources/sprites/player.png";
    private static string _texSatellitePath = "res://resources/sprites/satellite.png";
    private static string _texScoutPath = "res://resources/sprites/scout.png";

    // Projectiles
    private static string _texCuttingLaserPath = "res://resources/sprites/projectile/cutting_laser.png";
    private static string _texSmallCannonPath = "res://resources/sprites/projectile/small_cannon.png";
    private static string _texSmallGatlingPath = "res://resources/sprites/projectile/small_gatling.png";
    private static string _texSmallShotgunPath = "res://resources/sprites/projectile/small_shotgun.png";
    private static string _texRailgunPath = "res://resources/sprites/projectile/railgun.png";
    private static string _texReverserPath = "res://resources/sprites/projectile/reverser.png";

    private class ProjectileDisplayData {
      public ProjectileType Type { get; }
      public string Name { get; }
      public string TexturePath { get; }

      public ProjectileDisplayData(ProjectileType type, string name, string texturePath) {
        this.Type = type;
        this.Name = name;
        this.TexturePath = texturePath;
      }
    }

    private static Dictionary<ProjectileType, ProjectileDisplayData> projectileTypeToProjectileDisplay = new Dictionary<ProjectileType, ProjectileDisplayData>() {
      { ProjectileType.CUTTING_LASER, new ProjectileDisplayData(ProjectileType.CUTTING_LASER, "cutting laser beam", _texCuttingLaserPath) },
      { ProjectileType.SMALL_CANNON, new ProjectileDisplayData(ProjectileType.SMALL_CANNON, "small cannon shell", _texSmallCannonPath) },
      { ProjectileType.SMALL_GATLING, new ProjectileDisplayData(ProjectileType.SMALL_GATLING, "small gatling shell", _texSmallGatlingPath) },
      { ProjectileType.SMALL_SHOTGUN, new ProjectileDisplayData(ProjectileType.SMALL_SHOTGUN, "small shotgun pellet", _texSmallShotgunPath) },
      { ProjectileType.RAILGUN, new ProjectileDisplayData(ProjectileType.RAILGUN, "railgun slug", _texRailgunPath) },
      { ProjectileType.REVERSER, new ProjectileDisplayData(ProjectileType.REVERSER, "reverser shot", _texReverserPath) }
    };

    private static Entity CreateEntity(string id, string name) {
      Entity newEntity = Entity.Create(id, name);
      return newEntity;
    }

    private static Entity CreateScoutEntity(string activationGroupId, int currentTick) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "scout");

      var statusEffectTrackerComponent = StatusEffectTrackerComponent.Create();

      e.AddComponent(new ScoutAIComponent(activationGroupId));

      e.AddComponent(ActionTimeComponent.Create(currentTick));
      e.AddComponent(CollisionComponent.CreateDefaultActor());
      e.AddComponent(DefenderComponent.Create(baseDefense: 0, maxHp: 10));
      e.AddComponent(DisplayComponent.Create(_texScoutPath, "A small scout craft, armed with a shotgun.", false));
      e.AddComponent(SpeedComponent.Create(baseSpeed: 75));
      e.AddComponent(statusEffectTrackerComponent);
      e.AddComponent(XPValueComponent.Create(xpValue: 30));

      return e;
    }

    private static Entity CreateFighterEntity(string activationGroupId, int currentTick) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "fighter");

      var statusEffectTrackerComponent = StatusEffectTrackerComponent.Create();

      e.AddComponent(new FighterAIComponent(activationGroupId));

      e.AddComponent(ActionTimeComponent.Create(currentTick));
      e.AddComponent(CollisionComponent.CreateDefaultActor());
      e.AddComponent(DefenderComponent.Create(baseDefense: 0, maxHp: 30));
      e.AddComponent(DisplayComponent.Create(_texFighterPath, "An interceptor craft armed with a rapid-fire cannon.", false));
      e.AddComponent(SpeedComponent.Create(baseSpeed: 125));
      e.AddComponent(statusEffectTrackerComponent);
      e.AddComponent(XPValueComponent.Create(xpValue: 50));

      return e;
    }

    private static Entity CreateGunshipEntity(string activationGroupId, int currentTick) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "gunship");

      var statusEffectTrackerComponent = StatusEffectTrackerComponent.Create();

      e.AddComponent(new GunshipAIComponent(activationGroupId));

      e.AddComponent(ActionTimeComponent.Create(currentTick));
      e.AddComponent(CollisionComponent.CreateDefaultActor());
      e.AddComponent(DefenderComponent.Create(baseDefense: 4, maxHp: 50));
      e.AddComponent(DisplayComponent.Create(_texGunshipPath, "A sturdy gunship, armed with anti-fighter flak and a cannon.", false));
      e.AddComponent(SpeedComponent.Create(baseSpeed: 100));
      e.AddComponent(statusEffectTrackerComponent);
      e.AddComponent(XPValueComponent.Create(xpValue: 100));

      return e;
    }

    private static Entity CreateFrigateEntity(string activationGroupId, int currentTick) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "frigate");

      var statusEffectTrackerComponent = StatusEffectTrackerComponent.Create();

      e.AddComponent(new FrigateAIComponent(activationGroupId));

      e.AddComponent(ActionTimeComponent.Create(currentTick));
      e.AddComponent(CollisionComponent.CreateDefaultActor());
      e.AddComponent(DefenderComponent.Create(baseDefense: 10, maxHp: 150));
      e.AddComponent(DisplayComponent.Create(_texFrigatePath, "An escort ship sporting a reverser gun, as well as secondary batteries.", false));
      e.AddComponent(SpeedComponent.Create(baseSpeed: 250));
      e.AddComponent(statusEffectTrackerComponent);
      e.AddComponent(XPValueComponent.Create(xpValue: 200));

      return e;
    }

    private static Entity CreateDestroyerEntity(string activationGroupId, int currentTick) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "destroyer");

      var statusEffectTrackerComponent = StatusEffectTrackerComponent.Create();

      e.AddComponent(new DestroyerAIComponent(activationGroupId));

      e.AddComponent(ActionTimeComponent.Create(currentTick));
      e.AddComponent(CollisionComponent.CreateDefaultActor());
      e.AddComponent(DefenderComponent.Create(baseDefense: 15, maxHp: 200));
      e.AddComponent(DisplayComponent.Create(_texDestroyerPath, "A larger anti-fighter craft with a ferocious flak barrage.", false));
      e.AddComponent(SpeedComponent.Create(baseSpeed: 300));
      e.AddComponent(statusEffectTrackerComponent);
      e.AddComponent(XPValueComponent.Create(xpValue: 500));

      return e;
    }

    private static Entity CreateCruiserEntity(string activationGroupId, int currentTick) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "cruiser");

      var statusEffectTrackerComponent = StatusEffectTrackerComponent.Create();

      e.AddComponent(new CruiserAIComponent(activationGroupId));

      e.AddComponent(ActionTimeComponent.Create(currentTick));
      e.AddComponent(CollisionComponent.CreateDefaultActor());
      e.AddComponent(DefenderComponent.Create(baseDefense: 10, maxHp: 300));
      e.AddComponent(DisplayComponent.Create(_texCruiserPath, "A heavily armed and armored behemoth with a ferocious railgun.", false));
      e.AddComponent(SpeedComponent.Create(baseSpeed: 400));
      e.AddComponent(statusEffectTrackerComponent);
      e.AddComponent(XPValueComponent.Create(xpValue: 1000));

      return e;
    }

    private static Entity CreateCarrierEntity(string activationGroupId, int currentTick) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "carrier");

      var statusEffectTrackerComponent = StatusEffectTrackerComponent.Create();

      e.AddComponent(new CarrierAIComponent(activationGroupId));

      e.AddComponent(ActionTimeComponent.Create(currentTick));
      e.AddComponent(CollisionComponent.CreateDefaultActor());
      e.AddComponent(DefenderComponent.Create(baseDefense: 0, maxHp: 500));
      e.AddComponent(DisplayComponent.Create(_texCarrierPath, "An extremely slow carrier, which launches fighters or scouts every action.", false));
      e.AddComponent(SpeedComponent.Create(baseSpeed: 200));
      e.AddComponent(statusEffectTrackerComponent);
      e.AddComponent(XPValueComponent.Create(xpValue: 2000));

      return e;
    }

    private static Entity CreateDiplomatEntity(string activationGroupId, int currentTick) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "diplomat");

      var statusEffectTrackerComponent = StatusEffectTrackerComponent.Create();

      e.AddComponent(new DiplomatAIComponent(activationGroupId));

      e.AddComponent(ActionTimeComponent.Create(currentTick));
      e.AddComponent(CollisionComponent.CreateDefaultActor());
      e.AddComponent(DefenderComponent.Create(baseDefense: 0, maxHp: 100));
      e.AddComponent(DisplayComponent.Create(_texDiplomatPath, "Your target, the diplomat!", false));
      e.AddComponent(OnDeathComponent.Create(new List<string>() { OnDeathEffectType.PLAYER_VICTORY }));
      e.AddComponent(SpeedComponent.Create(baseSpeed: 100));
      e.AddComponent(statusEffectTrackerComponent);
      e.AddComponent(XPValueComponent.Create(xpValue: 0));

      return e;
    }

    private static Entity CreateExtraBatteryEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "extra battery");

      e.AddComponent(DisplayComponent.Create(_texItemBatteryPath, "An extra battery for your weapons. Gives 20 power for 450 ticks.", true));
      e.AddComponent(StorableComponent.Create());
      e.AddComponent(UsableComponent.Create(useOnGet: false));
      e.AddComponent(UseEffectBoostPowerComponent.Create(boostPower: 20, duration: 450));

      return e;
    }

    private static Entity CreateDuctTapeEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "duct tape");

      e.AddComponent(DisplayComponent.Create(_texItemDuctTapePath, "Some duct tape. Heals 10 HP.", true));
      e.AddComponent(StorableComponent.Create());
      e.AddComponent(UsableComponent.Create(useOnGet: false));
      e.AddComponent(UseEffectHealComponent.Create(healpower: 10));

      return e;
    }

    private static Entity CreateRedPaintEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "red paint");

      // TODO: I put it down as 'r', but that's the same as the reverser shot. Well we'll replace all the sprites anwyays
      // if I do make it pretty.
      e.AddComponent(DisplayComponent.Create(_texItemRedPaintPath, "Red paint makes you go faster! Reduces turn time by 75 for 300 ticks (minimum time is 1).", true));
      e.AddComponent(StorableComponent.Create());
      e.AddComponent(UsableComponent.Create(useOnGet: false));
      e.AddComponent(UseEffectBoostSpeedComponent.Create(boostPower: 75, duration: 300));

      return e;
    }

    private static Entity CreateEMPEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "EMP");

      e.AddComponent(DisplayComponent.Create(_texItemEMPPath, "An EMP burst. Disables enemies for 10 turns in radius 20.", true));
      e.AddComponent(StorableComponent.Create());
      e.AddComponent(UsableComponent.Create(useOnGet: false));
      // I seriously put 20 radius 10 turns? That's enough time to mop up an entire encounter!
      e.AddComponent(UseEffectEMPComponent.Create(radius: 20, disableTurns: 10));

      return e;
    }

    public static Entity CreateEnemyByEntityDefId(string enemyDefId, string activationGroupId, int currentTick) {
      if (enemyDefId == EntityDefId.SCOUT) {
        return EntityBuilder.CreateScoutEntity(activationGroupId, currentTick);
      } else if (enemyDefId == EntityDefId.FIGHTER) {
        return EntityBuilder.CreateFighterEntity(activationGroupId, currentTick);
      } else if (enemyDefId == EntityDefId.GUNSHIP) {
         return EntityBuilder.CreateGunshipEntity(activationGroupId, currentTick);
      } else if (enemyDefId == EntityDefId.FRIGATE) {
        return EntityBuilder.CreateFrigateEntity(activationGroupId, currentTick);
      } else if (enemyDefId == EntityDefId.DESTROYER) {
        return EntityBuilder.CreateDestroyerEntity(activationGroupId, currentTick);
      } else if (enemyDefId == EntityDefId.CRUISER) {
        return EntityBuilder.CreateCruiserEntity(activationGroupId, currentTick);
      } else if (enemyDefId == EntityDefId.CARRIER) {
        return EntityBuilder.CreateCarrierEntity(activationGroupId, currentTick);
      } else if (enemyDefId == EntityDefId.DIPLOMAT) {
        return EntityBuilder.CreateDiplomatEntity(activationGroupId, currentTick);
      } else {
        throw new NotImplementedException("No mapping defined for " + enemyDefId);
      }
    }

    public static Entity CreateItemByEntityDefId(string itemDefId) {
      if (itemDefId == EntityDefId.ITEM_DUCT_TAPE) {
        return EntityBuilder.CreateDuctTapeEntity();
      } else if (itemDefId == EntityDefId.ITEM_EXTRA_BATTERY) {
        return EntityBuilder.CreateExtraBatteryEntity();
      } else if (itemDefId == EntityDefId.ITEM_RED_PAINT) {
        return EntityBuilder.CreateRedPaintEntity();
      } else if (itemDefId == EntityDefId.ITEM_EMP) {
        return EntityBuilder.CreateEMPEntity();
      } else {
        throw new NotImplementedException("No mapping defined for " + itemDefId);
      }
    }

    public static Entity CreatePlayerEntity(int currentTick) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "player");

      var statusEffectTrackerComponent = StatusEffectTrackerComponent.Create();

      e.AddComponent(ActionTimeComponent.Create(currentTick));
      e.AddComponent(CollisionComponent.Create(blocksMovement: true, blocksVision: false));
      e.AddComponent(DefenderComponent.Create(baseDefense: 0, maxHp: 100, isInvincible: false));
      e.AddComponent(DisplayComponent.Create(_texPlayerPath, "It's you!", false));
      e.AddComponent(InventoryComponent.Create(inventorySize: 26));
      e.AddComponent(OnDeathComponent.Create(new List<string>() { OnDeathEffectType.PLAYER_DEFEAT }));
      e.AddComponent(PlayerComponent.Create());
      e.AddComponent(SpeedComponent.Create(baseSpeed: 100));
      e.AddComponent(statusEffectTrackerComponent);
      e.AddComponent(XPTrackerComponent.Create(levelUpBase: 200, levelUpFactor: 150));

      return e;
    }

    public static Entity CreateProjectileEntity(Entity source, ProjectileType type, int power, EncounterPath path, int speed, int currentTick) {
      var displayData = projectileTypeToProjectileDisplay[type];

      var e = CreateEntity(Guid.NewGuid().ToString(), displayData.Name);

      e.AddComponent(PathAIComponent.Create(path));

      e.AddComponent(ActionTimeComponent.Create(currentTick)); // Should it go instantly or should it wait for its turn...?
      e.AddComponent(AttackerComponent.Create(source.EntityId, power));
      e.AddComponent(CollisionComponent.Create(false, false, true, true));
      e.AddComponent(DisplayComponent.Create(displayData.TexturePath, "A projectile.", false));
      e.AddComponent(SpeedComponent.Create(speed));

      return e;
    }

    public static Entity CreateMapWallEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "map wall");

      e.AddComponent(CollisionComponent.Create(true, false));
      e.AddComponent(DefenderComponent.Create(0, 100, logDamage: false, isInvincible: true));
      e.AddComponent(DisplayComponent.Create(_StarPath, "TODO: think of something to say for the map edge blockers", true));

      return e;
    }

    public static Entity CreateSatelliteEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "satellite");

      e.AddComponent(CollisionComponent.Create(blocksMovement: true, blocksVision: true));
      e.AddComponent(DefenderComponent.Create(baseDefense: int.MaxValue, maxHp: int.MaxValue, isInvincible: true, logDamage: false));
      e.AddComponent(DisplayComponent.Create(_texSatellitePath, "Space junk. Blocks movement and projectiles. Cannot be destroyed.", true));

      return e;
    }

    public static Entity CreateStairsEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "jump point");

      e.AddComponent(DisplayComponent.Create(_JPath, "The jump point to the next sector.", true));
      e.AddComponent(StairsComponent.Create());

      return e;
    }

    public static Entity CreateIntelEntity(int targetDungeonLevel) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "intel for sector " + targetDungeonLevel);

      e.AddComponent(DisplayComponent.Create(_iPath, "Intel! Gives you zone information for the next sector. You want this.", true));
      e.AddComponent(StorableComponent.Create());
      e.AddComponent(UsableComponent.Create(useOnGet: true));
      e.AddComponent(UseEffectAddIntelComponent.Create(targetDungeonLevel));

      return e;
    }
  }
}