using Godot;

public class EntityBuilder: Node {
  // I assume these are all loaded at the same time as _Ready()?
  private PackedScene _entityPrefab = GD.Load<PackedScene>("res://scenes/entities/Entity.tscn");
  // Component prefabs
  private PackedScene _positionComponentPrefab = GD.Load<PackedScene>("res://scenes/components/PositionComponent.tscn");
  private PackedScene _playerComponentPrefab = GD.Load<PackedScene>("res://scenes/components/PlayerComponent.tscn");
  private PackedScene _testAIComponentPrefab = GD.Load<PackedScene>("res://scenes/components/AI/TestAIComponent.tscn");

  private string _sPath = "res://resources/atlas_s.tres";
  private string _AtSignPath = "res://resources/atlas_@.tres";

  private Entity CreateEntity(string id, string name) {
    Entity newEntity = _entityPrefab.Instance() as Entity;
    newEntity.Init(id, name);
    return newEntity;
  }

  private Entity AddPositionComponent(Entity entity, GamePosition position, string texturePath) {
    var positionComponent = _positionComponentPrefab.Instance() as PositionComponent;
    positionComponent.Init(position, GD.Load<Texture>(texturePath));

    entity.AddChild(positionComponent);
    return entity;
  }

  public Entity CreatePlayerEntity(GamePosition pos) {
    var player = this.CreateEntity("uuid#1", "player");
    this.AddPositionComponent(player, pos, _AtSignPath);
    player.AddChild(this._playerComponentPrefab.Instance());
    player.AddToGroup("player");
    return player;
  }

  public Entity CreateScoutEntity(GamePosition pos) {
    var scout = this.CreateEntity("uuid#2", "scout");
    this.AddPositionComponent(scout, pos, _sPath);
    scout.AddChild(this._testAIComponentPrefab.Instance());
    return scout;
  }
}