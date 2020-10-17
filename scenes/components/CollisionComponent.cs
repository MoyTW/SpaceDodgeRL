using Godot;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class CollisionComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/CollisionComponent.tscn");

    public static readonly string ENTITY_GROUP = "COLLISION_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public bool BlocksMovement { get; private set; }
    public bool BlocksVision { get; private set; }
    public bool OnCollisionAttack { get; private set; }
    public bool OnCollisionSelfDestruct { get; private set; }

    public static CollisionComponent Create(
      bool blocksMovement,
      bool blocksVision,
      bool attackOnCollision = false,
      bool selfDestructOnCollision = false
    ) {
      var component = _componentPrefab.Instance() as CollisionComponent;

      component.BlocksMovement = blocksMovement;
      component.BlocksVision = blocksVision;
      component.OnCollisionAttack = attackOnCollision;
      component.OnCollisionSelfDestruct = selfDestructOnCollision;

      return component;
    }
  }
}