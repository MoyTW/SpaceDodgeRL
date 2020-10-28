using System;
using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;

namespace SpaceDodgeRL.scenes.entities {

  public static class EntityBuilder {
    // I assume these are all loaded at the same time as _Ready()?
    private static PackedScene _entityPrefab = GD.Load<PackedScene>("res://scenes/entities/Entity.tscn");

    private static string _JPath = "res://resources/atlas_J.tres";
    private static string _sPath = "res://resources/atlas_s.tres";
    private static string _AtSignPath = "res://resources/atlas_@.tres";
    private static string _StarPath = "res://resources/atlas_Star.tres";
    private static string _hashSignPath = "res://resources/atlas_HashSign.tres";

    private static Entity CreateEntity(string id, string name) {
      Entity newEntity = _entityPrefab.Instance() as Entity;
      newEntity.Init(id, name);
      return newEntity;
    }

    public static Entity CreatePlayerEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "player");

      e.AddChild(ActionTimeComponent.Create(0));
      e.AddChild(CollisionComponent.Create(true, false));
      e.AddChild(DefenderComponent.Create(0, 100));
      e.AddChild(PlayerComponent.Create());
      e.AddChild(SpriteDataComponent.Create(_AtSignPath));
      e.AddChild(SpeedComponent.Create(100));

      return e;
    }

    public static Entity CreateScoutEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "scout");

      e.AddChild(ScoutAIComponent.Create());

      e.AddChild(ActionTimeComponent.Create(0));
      e.AddChild(CollisionComponent.Create(true, false));
      e.AddChild(DefenderComponent.Create(0, 3, isInvincible: true));
      e.AddChild(SpriteDataComponent.Create(_sPath));
      e.AddChild(SpeedComponent.Create(200));

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

      return e;
    }
  }
}