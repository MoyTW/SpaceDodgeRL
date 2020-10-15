using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;
using System;

namespace SpaceDodgeRL.scenes.encounter {

  public class EncounterState : Node {

    // ##########################################################################################################################
    #region Data Access
    // ##########################################################################################################################

    public Entity Player {
      get => GetTree().GetNodesInGroup(PlayerComponent.ENTITY_GROUP)[0] as Entity;
    }

    public Entity GetEntityById(string entityId) {
      var entities = GetTree().GetNodesInGroup(Entity.ENTITY_GROUP);
      // It kinda chafes that Godot arrays don't have all the fancy utility functions C# collections do.
      foreach (Entity entity in entities) {
        if (entity.EntityId == entityId) {
          return entity;
        }
      }
      return null;
    }

    /**
    * Note that this will fetch sub-entities as well (for example, if somebody implements an Inventory component which stores
    * the inventory elements via AddChild(...)) - this might be desirable (you want time to tick for items in the inventory) or
    * it might not! Who knows?
    */
    public Godot.Collections.Array ActionEntities() {
      return GetTree().GetNodesInGroup(ActionTimeComponent.ENTITY_GROUP);
    }

    /**
    * Here we assume that it's impossible for there to be nested child nodes with Position entities.
    */
    public Godot.Collections.Array PositionEntities() {
      return GetTree().GetNodesInGroup(PositionComponent.ENTITY_GROUP);
    }

    public Entity NextEntity() {
      int lowestTTL = int.MaxValue;
      Entity next = null;

      // TODO: We're doing a full scan every time right now; we should store these in a sorted list!
      // TODO: Also this code is really awful!
      var children = GetChildren();
      foreach (Node node in children) {
        if (node.IsInGroup(Entity.ENTITY_GROUP)) {
          var actionTimeComponent = (node as Entity).GetComponent<ActionTimeComponent>();
          if (actionTimeComponent != null && actionTimeComponent.TicksUntilTurn < lowestTTL) {
            lowestTTL = actionTimeComponent.TicksUntilTurn;
            next = node as Entity;
          }
        }
      }

      if (lowestTTL == int.MaxValue) {
        throw new NotImplementedException();
      }

      return next;
    }

    // Positional Queries

    // TODO: Maintain a internal representation
    // TODO: Just, like...definte a couple of extension functions on Godot.Collections.Array or something maybe?
    public bool IsPositionBlocked(EncounterPosition position) {
      // TODO: Distinguish between blocking/non-blocking entities
      foreach (Entity entity in this.PositionEntities()) {
        if (position == entity.GetComponent<PositionComponent>().EncounterPosition) {
          return true;
        }
      }
      return false;
    }
 
    // ##########################################################################################################################
    #endregion
    // ##########################################################################################################################

    public void PlaceEntity(Entity entity, EncounterPosition targetPosition) {
      if (IsPositionBlocked(targetPosition)) {
        throw new NotImplementedException("probably handle this more gracefully than exploding");
      }

      var spriteData = entity.GetComponent<SpriteDataComponent>();
      
      var positionComponent = PositionComponent.Create(targetPosition, spriteData.Texture);
      entity.AddChild(positionComponent);

      AddChild(entity);
    }

    public void RemoveEntity(Entity entity) {
      RemoveChild(entity);
    }

    /**
    * Disregards intervening terrain; only checks for collisions at the target position.
    */
    public void TeleportEntity(Entity entity, EncounterPosition targetPosition) {
      if (IsPositionBlocked(targetPosition)) {
        throw new NotImplementedException("probably handle this more gracefully than exploding");
      }
      entity.GetComponent<PositionComponent>().EncounterPosition = targetPosition;
    }

    // TODO: Move into map gen & save/load
    public void InitState(EntityBuilder entityBuilder) {
      PlaceEntity(entityBuilder.CreatePlayerEntity(), new EncounterPosition(3, 5));
      PlaceEntity(entityBuilder.CreateScoutEntity(), new EncounterPosition(5, 5));

      // TODO: Attaching camera to the player like this is extremely jank! Figure out a better way?
      var camera = GetNode<Camera2D>("EncounterCamera");
      RemoveChild(camera);
      // TODO: VERY DEFINITELY DON'T KEEP DOING THIS!!!
      Player.GetComponent<PositionComponent>().GetNode<Sprite>("Sprite").AddChild(camera);
    }
  }
}