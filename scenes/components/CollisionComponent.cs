using Godot;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceDodgeRL.scenes.components {

  public class CollisionComponent : Component {
    public static readonly string ENTITY_GROUP = "COLLISION_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public bool BlocksMovement { get; private set; }
    [JsonInclude] public bool BlocksVision { get; private set; }
    [JsonInclude] public bool OnCollisionAttack { get; private set; }
    [JsonInclude] public bool OnCollisionSelfDestruct { get; private set; }

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

    public static CollisionComponent Create(string saveData) {
      return JsonSerializer.Deserialize<CollisionComponent>(saveData);
    }

    public static CollisionComponent CreateDefaultActor() {
      return Create(blocksMovement: true, blocksVision: false);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}