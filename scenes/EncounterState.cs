using Godot;
using System;

public class EncounterState : Node {

  // TODO: Move this out of EncounterState
  private PackedScene _entityPrefab = GD.Load<PackedScene>("res://scenes/entities/Entity.tscn");
  private PackedScene _positionComponentPrefab = GD.Load<PackedScene>("res://scenes/components/PositionComponent.tscn");
  // TODO: Bake the sub-textures into their own resources and load them instead of doing this Very Silly process
  private string _atlasPath = "res://resources/atlas_@.tres";
  private Rect2 _AtSignRect2 = new Rect2(new Vector2(0, 4 * 36), new Vector2(24, 36));

  // TODO: Move this out of EncounterState, create a better way of selecting the sprite than "hey an arbitrary rect"
  private Entity CreateEntity(string id, string name, Position pos, Rect2 atlasRegion) {
    Entity newEntity = _entityPrefab.Instance() as Entity;
    newEntity.Init(id, name);

    var positionComponent = _positionComponentPrefab.Instance() as PositionComponent;
    AtlasTexture atlas = new AtlasTexture();
    atlas.Atlas = GD.Load<AtlasTexture>(_atlasPath);
    atlas.Region = atlasRegion;
    positionComponent.Init(pos, atlas);

    newEntity.AddChild(positionComponent);

    return newEntity;
  }

  public override void _Ready() {
    this.AddChild(this.CreateEntity("uuid#1", "player", new Position(3, 5), _AtSignRect2));
    this.AddChild(this.CreateEntity("uuid#2", "scout", new Position(5, 5), new Rect2(new Vector2(3 * 24, 7 * 36), new Vector2(24, 36))));

    /*
    Entity testEntity = entityPrefab.Instance() as Entity;
    testEntity.Init("whoo", "blah");
    newEntity.AddChild(testEntity);

    var entities = GetTree().GetNodesInGroup(Entity.ENTITY_GROUP);
    for( int i = 0; i < entities.Count; i++ ) {
      var e = entities[i] as Entity;
      if (this == e.GetParent()) {
        GD.Print(entities[i] + " is a child of EncounterState");
      } else {
        GD.Print(entities[i] + " is a grandchild of EncounterState");
      }
    }
    */
  }
}
