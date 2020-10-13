using Godot;
using System;

public class HelloWorld : Node2D {
  public override void _Ready() {
    // We're gonna build our entity on the fly here
    PackedScene entityPrefab = GD.Load<PackedScene>("res://scenes/entities/Entity.tscn");
    PackedScene positionComponentPrefab = GD.Load<PackedScene>("res://scenes/components/PositionComponent.tscn");
    AtlasTexture atlas = GD.Load<AtlasTexture>("res://resources/atlas_@.tres");

    GD.Print("Hello World!");

    // There's a lot of lines of code to build any entity of any size here!
    Entity newEntity = entityPrefab.Instance() as Entity;
    newEntity.Init("really should be a uuid", "the player!");

    var positionComponent = positionComponentPrefab.Instance() as PositionComponent;
    positionComponent.Init(new Position(3, 5));
    newEntity.AddChild(positionComponent);

    this.AddChild(newEntity);

    // Create an enemy
    newEntity = entityPrefab.Instance() as Entity;
    newEntity.Init("uuid#2", "the enemy scout");

    positionComponent = positionComponentPrefab.Instance() as PositionComponent;
    positionComponent.Init(new Position(5, 5));
    // This is real jankity!
    atlas.Region = new Rect2(new Vector2(3 * 24, 7 * 36), new Vector2(24, 36));
    positionComponent.Texture = atlas;

    newEntity.AddChild(positionComponent);

    this.AddChild(newEntity);
  }
}
