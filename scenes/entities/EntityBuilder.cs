using System;
using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;

namespace SpaceDodgeRL.scenes.entities {

  public static class EntityBuilder {
    // I assume these are all loaded at the same time as _Ready()?
    private static PackedScene _entityPrefab = GD.Load<PackedScene>("res://scenes/entities/Entity.tscn");

    private static string _sPath = "res://resources/atlas_s.tres";
    private static string _AtSignPath = "res://resources/atlas_@.tres";
    private static string _StarPath = "res://resources/atlas_*.tres";

    private static Entity CreateEntity(string id, string name) {
      Entity newEntity = _entityPrefab.Instance() as Entity;
      newEntity.Init(id, name);
      return newEntity;
    }

    public static Entity CreatePlayerEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "player");

      e.AddChild(ActionTimeComponent.Create(0));
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
      e.AddChild(DefenderComponent.Create(0, 100));
      e.AddChild(SpriteDataComponent.Create(_sPath));
      e.AddChild(SpeedComponent.Create(250));

      return e;
    }

    public static Entity CreateProjectileEntity(int power, EncounterPath path, int speed) {
      var e = CreateEntity(Guid.NewGuid().ToString(), "TODO: nicer names for projectiles");

      e.AddChild(PathAIComponent.Create(path));

      e.AddChild(ActionTimeComponent.Create(0)); // Should it go instantly or should it wait for its turn...?
      e.AddChild(AttackerComponent.Create(power));
      e.AddChild(SpriteDataComponent.Create(_StarPath));
      e.AddChild(SpeedComponent.Create(speed));

      return e;
    }

    public static Entity CreateTestProjectileEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "test projectile");

      var arbitraryPath = EncounterPathBuilder.BuildStraightLinePath(new EncounterPosition(6,8), new EncounterPosition(13, 42), 5);
      e.AddChild(PathAIComponent.Create(arbitraryPath));

      e.AddChild(ActionTimeComponent.Create(0));
      e.AddChild(SpriteDataComponent.Create(_StarPath));
      e.AddChild(SpeedComponent.Create(50));

      return e;
    }
  }
}