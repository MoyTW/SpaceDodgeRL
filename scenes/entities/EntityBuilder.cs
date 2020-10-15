using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;

namespace SpaceDodgeRL.scenes.entities {

  public class EntityBuilder : Node {
    // I assume these are all loaded at the same time as _Ready()?
    private PackedScene _entityPrefab = GD.Load<PackedScene>("res://scenes/entities/Entity.tscn");
    // Component prefabs
    private PackedScene _playerComponentPrefab = GD.Load<PackedScene>("res://scenes/components/PlayerComponent.tscn");
    private PackedScene _testAIComponentPrefab = GD.Load<PackedScene>("res://scenes/components/AI/TestAIComponent.tscn");
    private PackedScene _actionTimeComponentPrefab = GD.Load<PackedScene>("res://scenes/components/ActionTimeComponent.tscn");
    private PackedScene _speedComponentPrefab = GD.Load<PackedScene>("res://scenes/components/SpeedComponent.tscn");

    private string _sPath = "res://resources/atlas_s.tres";
    private string _AtSignPath = "res://resources/atlas_@.tres";

    private Entity CreateEntity(string id, string name) {
      Entity newEntity = _entityPrefab.Instance() as Entity;
      newEntity.Init(id, name);
      return newEntity;
    }

    private void AddActionTimeComponent(Entity entity, int ticksUntilTurn = 0) {
      var c = _actionTimeComponentPrefab.Instance() as ActionTimeComponent;
      c.Init(ticksUntilTurn);
      entity.AddChild(c);
    }

    private void AddPlayerComponent(Entity entity) {
      entity.AddChild(_playerComponentPrefab.Instance());
    }

    private void AddSpeedComponent(Entity entity, int baseSpeed) {
      var c = _speedComponentPrefab.Instance() as SpeedComponent;
      c.Init(baseSpeed);
      entity.AddChild(c);
    }

    public Entity CreatePlayerEntity() {
      var player = CreateEntity("uuid#1", "player");

      AddActionTimeComponent(player);
      AddPlayerComponent(player);
      player.AddChild(SpriteDataComponent.Create(_AtSignPath));
      AddSpeedComponent(player, 100);

      return player;
    }

    public Entity CreateScoutEntity() {
      var scout = CreateEntity("uuid#2", "scout");

      scout.AddChild(_testAIComponentPrefab.Instance());
      AddActionTimeComponent(scout);
      scout.AddChild(SpriteDataComponent.Create(_sPath));
      AddSpeedComponent(scout, 50);

      return scout;
    }
  }
}