using Godot;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class CollisionComponent : Component {
    public static readonly string ENTITY_GROUP = "COLLISION_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

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
      var component = new CollisionComponent();

      component.BlocksMovement = blocksMovement;
      component.BlocksVision = blocksVision;
      component.OnCollisionAttack = attackOnCollision;
      component.OnCollisionSelfDestruct = selfDestructOnCollision;

      return component;
    }

    public static CollisionComponent CreateDefaultActor() {
      return Create(blocksMovement: true, blocksVision: false);
    }
  }
}