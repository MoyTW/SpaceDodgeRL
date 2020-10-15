using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;
using System;

namespace SpaceDodgeRL.scenes.encounter {

  public class EncounterState : Node {

    // TODO: Save/Load/Proper New Game
    private bool hasCreated = false;

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

    // TODO: cache, maybe & also tag entities with groups per component
    public Godot.Collections.Array ActionEntities() {
      return GetTree().GetNodesInGroup(ActionTimeComponent.ENTITY_GROUP);
    }

    public Entity NextEntity() {
      int lowestTTL = int.MaxValue;
      Entity next = null;

      // TODO: We're doing a full scan every time right now; we should store these in a sorted list!
      // TODO: Also this code is really awful!
      var children = GetChildren();
      foreach (Node node in children) {
        if (node.IsInGroup(Entity.ENTITY_GROUP)) {
          var actionTimeComponent = (node as Entity).GetNode<ActionTimeComponent>("ActionTimeComponent");
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

    // TODO: Move into map gen & save/load
    public void InitState(EntityBuilder entityBuilder) {
      AddChild(entityBuilder.CreatePlayerEntity(new GamePosition(3, 5)));
      AddChild(entityBuilder.CreateScoutEntity(new GamePosition(5, 5)));

      // TODO: Attaching camera to the player like this is extremely jank! ALSO, it's causing a weird jumping behaviour where the
      // camera moves milliseconds after the player teleports to the next position!
      var camera = GetNode<Camera2D>("EncounterCamera");
      RemoveChild(camera);
      Player.GetNode<PositionComponent>("PositionComponent").AddChild(camera);

      hasCreated = true;
    }
  }
}