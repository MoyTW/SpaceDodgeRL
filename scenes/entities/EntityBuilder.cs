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

    private static Entity CreateScoutEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "scout");

      e.AddChild(ScoutAIComponent.Create());

      e.AddChild(ActionTimeComponent.Create(0));
      e.AddChild(CollisionComponent.CreateDefaultActor());
      e.AddChild(DefenderComponent.Create(baseDefense: 0, maxHp: 10));
      e.AddChild(SpriteDataComponent.Create(_SPath));
      e.AddChild(SpeedComponent.Create(baseSpeed: 75));

      return e;
    }

    private static Entity CreateFighterEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "fighter");

      e.AddChild(FighterAIComponent.Create());

      e.AddChild(ActionTimeComponent.Create(0));
      e.AddChild(CollisionComponent.CreateDefaultActor());
      e.AddChild(DefenderComponent.Create(baseDefense: 0, maxHp: 30));
      e.AddChild(SpriteDataComponent.Create(_FPath));
      e.AddChild(SpeedComponent.Create(baseSpeed: 125));

      return e;
    }

    // TODO: Add Use ability
    // TODO: Add "always true even in FoW" tag
    private static Entity CreateDuctTapeEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "duct tape");

      e.AddChild(SpriteDataComponent.Create(_tPath));
      e.AddChild(StorableComponent.Create());
      e.AddChild(UsableComponent.Create());
      e.AddChild(UseEffectHealComponent.Create(healpower: 10));

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
        GD.Print("TODO: No implementation yet for ID ", entityDefId);
        return EntityBuilder.CreateDuctTapeEntity();
      } else if (entityDefId == EntityDefId.ITEM_RED_PAINT) {
        GD.Print("TODO: No implementation yet for ID ", entityDefId);
        return EntityBuilder.CreateDuctTapeEntity();
      } else if (entityDefId == EntityDefId.ITEM_EMP) {
        GD.Print("TODO: No implementation yet for ID ", entityDefId);
        return EntityBuilder.CreateDuctTapeEntity();
      } else {
        throw new NotImplementedException("No mapping defined for " + entityDefId);
      }
    }

    public static Entity CreatePlayerEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "player");

      e.AddChild(ActionTimeComponent.Create(0));
      e.AddChild(CollisionComponent.Create(true, false));
      e.AddChild(DefenderComponent.Create(0, 100));
      e.AddChild(InventoryComponent.Create(inventorySize: 26));
      e.AddChild(PlayerComponent.Create());
      e.AddChild(SpriteDataComponent.Create(_AtSignPath));
      e.AddChild(SpeedComponent.Create(100));

      return e;
    }

    public static Entity CreateProjectileEntity(string projectileName, int power, EncounterPath path, int speed) {
      var e = CreateEntity(Guid.NewGuid().ToString(), projectileName);

      e.AddChild(PathAIComponent.Create(path));

      e.AddChild(ActionTimeComponent.Create(0)); // Should it go instantly or should it wait for its turn...?
      e.AddChild(AttackerComponent.Create(power));
      e.AddChild(CollisionComponent.Create(false, false, true, true));
      e.AddChild(SpriteDataComponent.Create(_StarPath));
      e.AddChild(SpeedComponent.Create(speed));

      return e;
    }

    public static Entity CreateMapWallEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "map wall");

      e.AddChild(CollisionComponent.Create(true, false));
      e.AddChild(DefenderComponent.Create(0, 100, logDamage: false, isInvincible: true));
      e.AddChild(SpriteDataComponent.Create(_StarPath));

      return e;
    }

    public static Entity CreateSatelliteEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "satellite");

      e.AddChild(CollisionComponent.Create(blocksMovement: true, blocksVision: true));
      e.AddChild(DefenderComponent.Create(baseDefense: int.MaxValue, maxHp: int.MaxValue, logDamage: false));
      e.AddChild(SpriteDataComponent.Create(_hashSignPath));

      return e;
    }

    public static Entity CreateStairsEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "jump point");

      e.AddChild(SpriteDataComponent.Create(_JPath));
      e.AddChild(StairsComponent.Create());

      return e;
    }
  }
}