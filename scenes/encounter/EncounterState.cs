using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpaceDodgeRL.scenes.encounter {

  public class EncounterState : Control {

    public int EncounterLogSize = 50;
    private List<string> _encounterLog;
    public ReadOnlyCollection<string> EncounterLog { get => _encounterLog.AsReadOnly(); }
    [Signal]
    public delegate void EncounterLogMessageAdded(string message, int encounterLogSize);

    // ##########################################################################################################################
    #region Data Access
    // ##########################################################################################################################

    private Entity _player;
    public Entity Player {
      get {
        if (_player != null) {
          return _player;
        } else {
          _player = GetTree().GetNodesInGroup(PlayerComponent.ENTITY_GROUP)[0] as Entity;
          return _player;
        }
      }
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

    public bool ArePositionsAdjacent(EncounterPosition left, EncounterPosition right) {
      var dx = Math.Abs(left.X - right.X);
      var dy = Math.Abs(left.Y - right.Y);
      return dx < 2 && dy < 2 && (dx + dy != 0);
    }

    public List<EncounterPosition> AdjacentPositions(EncounterPosition position) {
      var adjacent = new List<EncounterPosition>();

      for (int x = position.X - 1; x <= position.X + 1; x++) {
        for (int y = position.Y - 1; y <= position.Y + 1; y++) {
          var newPosition = new EncounterPosition(x, y);
          if (newPosition != position) {
            adjacent.Add(newPosition);
          }
        }
      }

      return adjacent;
    }

    public bool IsPositionBlocked(EncounterPosition position) {
      return BlockingEntityAtPosition(position) != null;
    }

    // TODO: Maintain an internal representation
    public Entity BlockingEntityAtPosition(EncounterPosition position) {
      foreach (Entity entity in this.PositionEntities()) {
        if (position == entity.GetComponent<PositionComponent>().EncounterPosition &&
            entity.GetComponent<CollisionComponent>().BlocksMovement) {
          return entity;
        }
      }
      return null;
    }

    // ##########################################################################################################################
    #endregion
    // ##########################################################################################################################

    public void PlaceEntity(Entity entity, EncounterPosition targetPosition, bool ignoreCollision = false) {
      if (!ignoreCollision && IsPositionBlocked(targetPosition)) {
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

    public void UpdateDangerMap() {
      var dangerMap = GetNode<TileMap>("DangerMap");
      var pathEntities = GetTree().GetNodesInGroup(PathAIComponent.ENTITY_GROUP);
      var timeToNextPlayerMove = this.Player.GetComponent<SpeedComponent>().Speed;

      dangerMap.Clear();
      foreach (Entity pathEntity in pathEntities) {
        var pathEntitySpeed = pathEntity.GetComponent<SpeedComponent>().Speed;
        var path = pathEntity.GetComponent<PathAIComponent>().Path;
        var dangerPositions = path.Project(timeToNextPlayerMove / pathEntitySpeed);

        foreach (EncounterPosition dangerPosition in dangerPositions) {
          dangerMap.SetCell(dangerPosition.X, dangerPosition.Y, 0);
        }
      }
    }

    public void LogMessage(string bbCodeMessage) {
      // TODO: Emit the signal for the encounter log
      if (this._encounterLog.Count >= this.EncounterLogSize) {
        this._encounterLog.RemoveAt(0);
      }
      this._encounterLog.Add(bbCodeMessage);
      this.EmitSignal("EncounterLogMessageAdded", bbCodeMessage, this.EncounterLogSize);
    }

    // TODO: Move into map gen & save/load
    public void InitState() {
      PlaceEntity(EntityBuilder.CreatePlayerEntity(), new EncounterPosition(0, 0));
      PlaceEntity(EntityBuilder.CreateScoutEntity(), new EncounterPosition(10, 5));

      // TODO: Attaching camera to the player like this is extremely jank! Figure out a better way?
      var camera = GetNode<Camera2D>("EncounterCamera");
      RemoveChild(camera);
      // TODO: VERY DEFINITELY DON'T KEEP DOING THIS!!!
      Player.GetComponent<PositionComponent>().GetNode<Sprite>("Sprite").AddChild(camera);

      this._encounterLog = new List<string>();
      this.LogMessage("Encounter started!");
    }
  }
}