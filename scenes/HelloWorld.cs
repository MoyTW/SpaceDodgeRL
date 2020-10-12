using Godot;
using System;

public class HelloWorld : Node2D {
  public override void _Ready() {
    // We're gonna build our entity on the fly here
    PackedScene entityPrefab = GD.Load<PackedScene>("res://scenes/entities/Entity.tscn");
    PackedScene positionComponentPrefab = GD.Load<PackedScene>("res://scenes/components/PositionComponent.tscn");

    GD.Print("Hello World!");

    // There's a lot of lines of code to build any entity of any size here!
    Entity newEntity = entityPrefab.Instance() as Entity;
    newEntity.Init("really should be a uuid", "the player!");

    var positionComponent = positionComponentPrefab.Instance() as PositionComponent;
    positionComponent.Init(new Position(3, 5));
    newEntity.AddChild(positionComponent);

    this.AddChild(newEntity);
  }
}
