using System;
using Godot;
using SpaceDodgeRL.scenes.components;

namespace SpaceDodgeRL.scenes.entities {

  public class EntityBuilder : Node {
    // I assume these are all loaded at the same time as _Ready()?
    private PackedScene _entityPrefab = GD.Load<PackedScene>("res://scenes/entities/Entity.tscn");
    // Component prefabs
    private PackedScene _testAIComponentPrefab = GD.Load<PackedScene>("res://scenes/components/AI/TestAIComponent.tscn");

    private string _sPath = "res://resources/atlas_s.tres";
    private string _AtSignPath = "res://resources/atlas_@.tres";

    private Entity CreateEntity(string id, string name) {
      Entity newEntity = _entityPrefab.Instance() as Entity;
      newEntity.Init(id, name);
      return newEntity;
    }

    public Entity CreatePlayerEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "player");

      e.AddChild(ActionTimeComponent.Create(0));
      e.AddChild(DefenderComponent.Create(0, 100));
      e.AddChild(PlayerComponent.Create());
      e.AddChild(SpriteDataComponent.Create(_AtSignPath));
      e.AddChild(SpeedComponent.Create(100));

      return e;
    }

    public Entity CreateScoutEntity() {
      var e = CreateEntity(Guid.NewGuid().ToString(), "scout");

      e.AddChild(_testAIComponentPrefab.Instance()); // we'll delete it in a bit anyways
      e.AddChild(ActionTimeComponent.Create(0));
      e.AddChild(DefenderComponent.Create(0, 100));
      e.AddChild(SpriteDataComponent.Create(_sPath));
      e.AddChild(SpeedComponent.Create(50));

      return e;
    }
  }
}