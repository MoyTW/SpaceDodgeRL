using System;
using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.resources.gamedata;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.components.use;

namespace SpaceDodgeRL.scenes.entities {

  public static class EntityBuilder {
    // I assume these are all loaded at the same time as _Ready()?
    private static PackedScene _entityPrefab = GD.Load<PackedScene>("res://scenes/entities/Entity.tscn");

    private static string _bPath = "res://resources/atlas_b.tres";
    private static string _iPath = "res://resources/atlas_i.tres";
    private static string _tPath = "res://resources/atlas_t.tres";
    private static string _FPath = "res://resources/atlas_F.tres";
    private static string _JPath = "res://resources/atlas_J.tres";
    private static string _SPath = "res://resources/atlas_S.tres";
    private static string _AtSignPath = "res://resources/atlas_@.tres";
    private static string _StarPath = "res://resources/atlas_Star.tres";
    private static string _hashSignPath = "res://resources/atlas_HashSign.tres";

    private static Entity CreateEntity(string id, string name) {
      Entity newEntity = _entityPrefab.Instance() as Entity;
      newEntity.Init(id, name);
      return newEntity;
    }

    // TODO: Take the current tick in this function!
    private static Entity CreateScoutEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "scout");

      e.AddComponent(ScoutAIComponent.Create());

      e.AddComponent(ActionTimeComponent.Create(0));
      e.AddComponent(CollisionComponent.CreateDefaultActor());
      e.AddComponent(DefenderComponent.Create(baseDefense: 0, maxHp: 10));
      e.AddComponent(DisplayComponent.Create(_SPath, false));
      e.AddComponent(SpeedComponent.Create(baseSpeed: 75));

      return e;
    }

    private static Entity CreateFighterEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "fighter");

      e.AddComponent(FighterAIComponent.Create());

      e.AddComponent(ActionTimeComponent.Create(0));
      e.AddComponent(CollisionComponent.CreateDefaultActor());
      e.AddComponent(DefenderComponent.Create(baseDefense: 0, maxHp: 30));
      e.AddComponent(DisplayComponent.Create(_FPath, false));
      e.AddComponent(SpeedComponent.Create(baseSpeed: 125));

      return e;
    }

    private static Entity CreateExtraBatteryEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "extra battery");

      e.AddComponent(DisplayComponent.Create(_bPath, true));
      e.AddComponent(StorableComponent.Create());
      e.AddComponent(UsableComponent.Create());
      e.AddComponent(UseEffectBoostPowerComponent.Create(boostPower: 20, duration: 450));

      return e;
    }

    private static Entity CreateDuctTapeEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "duct tape");

      e.AddComponent(DisplayComponent.Create(_tPath, true));
      e.AddComponent(StorableComponent.Create());
      e.AddComponent(UsableComponent.Create());
      e.AddComponent(UseEffectHealComponent.Create(healpower: 10));

      return e;
    }

    private static Entity CreateRedPaintEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "red paint");

      e.AddComponent(DisplayComponent.Create(_bPath, true));
      e.AddComponent(StorableComponent.Create());
      e.AddComponent(UsableComponent.Create());
      e.AddComponent(UseEffectBoostSpeedComponent.Create(boostPower: 75, duration: 300));

      return e;
    }

    public static Entity CreateEntityByEntityDefId(string entityDefId) {
      if (entityDefId == EntityDefId.SCOUT) {
        return EntityBuilder.CreateScoutEntity();
      } else if (entityDefId == EntityDefId.FIGHTER) {
        return EntityBuilder.CreateFighterEntity();
      } else if (entityDefId == EntityDefId.GUNSHIP) {
        GD.Print("TODO: No implementation yet for ID ", entityDefId);
        return EntityBuilder.CreateScoutEntity();
      } else if (entityDefId == EntityDefId.FRIGATE) {
        GD.Print("TODO: No implementation yet for ID ", entityDefId);
        return EntityBuilder.CreateScoutEntity();
      } else if (entityDefId == EntityDefId.DESTROYER) {
        GD.Print("TODO: No implementation yet for ID ", entityDefId);
        return EntityBuilder.CreateScoutEntity();
      } else if (entityDefId == EntityDefId.CRUISER) {
        GD.Print("TODO: No implementation yet for ID ", entityDefId);
        return EntityBuilder.CreateScoutEntity();
      } else if (entityDefId == EntityDefId.CARRIER) {
        GD.Print("TODO: No implementation yet for ID ", entityDefId);
        return EntityBuilder.CreateScoutEntity();
      } else if (entityDefId == EntityDefId.ITEM_DUCT_TAPE) {
        return EntityBuilder.CreateDuctTapeEntity();
      } else if (entityDefId == EntityDefId.ITEM_EXTRA_BATTERY) {
        return EntityBuilder.CreateExtraBatteryEntity();
      } else if (entityDefId == EntityDefId.ITEM_RED_PAINT) {
        return EntityBuilder.CreateRedPaintEntity();
      } else if (entityDefId == EntityDefId.ITEM_EMP) {
        GD.Print("TODO: No implementation yet for ID ", entityDefId);
        return EntityBuilder.CreateDuctTapeEntity();
      } else {
        throw new NotImplementedException("No mapping defined for " + entityDefId);
      }
    }

    public static Entity CreatePlayerEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "player");

      e.AddComponent(ActionTimeComponent.Create(0));
      e.AddComponent(CollisionComponent.Create(true, false));
      e.AddComponent(DefenderComponent.Create(0, 100));
      e.AddComponent(DisplayComponent.Create(_AtSignPath, false));
      e.AddComponent(InventoryComponent.Create(inventorySize: 26));
      e.AddComponent(PlayerComponent.Create());      
      e.AddComponent(SpeedComponent.Create(100));

      return e;
    }

    public static Entity CreateProjectileEntity(string projectileName, int power, EncounterPath path, int speed) {
      var e = CreateEntity(Guid.NewGuid().ToString(), projectileName);

      e.AddComponent(PathAIComponent.Create(path));

      e.AddComponent(ActionTimeComponent.Create(0)); // Should it go instantly or should it wait for its turn...?
      e.AddComponent(AttackerComponent.Create(power));
      e.AddComponent(CollisionComponent.Create(false, false, true, true));
      e.AddComponent(DisplayComponent.Create(_StarPath, false));
      e.AddComponent(SpeedComponent.Create(speed));

      return e;
    }

    public static Entity CreateMapWallEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "map wall");

      e.AddComponent(CollisionComponent.Create(true, false));
      e.AddComponent(DefenderComponent.Create(0, 100, logDamage: false, isInvincible: true));
      e.AddComponent(DisplayComponent.Create(_StarPath, true));

      return e;
    }

    public static Entity CreateSatelliteEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "satellite");

      e.AddComponent(CollisionComponent.Create(blocksMovement: true, blocksVision: true));
      e.AddComponent(DefenderComponent.Create(baseDefense: int.MaxValue, maxHp: int.MaxValue, logDamage: false));
      e.AddComponent(DisplayComponent.Create(_hashSignPath, true));

      return e;
    }

    public static Entity CreateStairsEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "jump point");

      e.AddComponent(DisplayComponent.Create(_JPath, true));
      e.AddComponent(StairsComponent.Create());

      return e;
    }

    public static Entity CreateIntelEntity(int targetDungeonLevel) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "intel for sector " + targetDungeonLevel);

      e.AddComponent(DisplayComponent.Create(_iPath, true));
      e.AddComponent(StorableComponent.Create());
      e.AddComponent(UsableComponent.Create());
      e.AddComponent(UseEffectAddIntelComponent.Create(targetDungeonLevel));

      return e;
    }
  }
}