using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpaceDodgeRL.scenes.encounter {

  public class EncounterTile {
    public bool BlocksMovement { get {
      return this._entities.Any(e => {
        var component = e.GetComponent<CollisionComponent>();
        return component != null && component.BlocksMovement;
      });
    } }
    public bool BlocksVision { get {
      return this._entities.Any(e => {
        var component = e.GetComponent<CollisionComponent>();
        return component != null && component.BlocksVision;
      });
    } }
    public bool Explored { get; set; } = false;

    private List<Entity> _entities = new List<Entity>();
    public ReadOnlyCollection<Entity> Entities { get => _entities.AsReadOnly(); }

    public void AddEntity(Entity entity) {
      this._entities.Add(entity);
    }
    public void RemoveEntity(Entity entity) {
      this._entities.Remove(entity);
    }
  }

  public class FoVCache {
    private List<EncounterPosition> _visibleCells;
    public ReadOnlyCollection<EncounterPosition> VisibleCells { get => this._visibleCells.AsReadOnly(); }

    public bool Contains(int x, int y) {
      return this._visibleCells.Contains(new EncounterPosition(x, y));
    }

    protected FoVCache(List<EncounterPosition> visibleCells) {
      this._visibleCells = visibleCells;
    }

    public static FoVCache ComputeFoV(EncounterState state, EncounterPosition center, int radius) {
      List<EncounterPosition> visibleCells = new List<EncounterPosition>();

      // TODO: Implement actual FoV calculations
      for (int x = center.X - radius; x < center.X + radius; x++) {
        for (int y = center.Y - radius; y < center.Y + radius; y++) {
          if (center.DistanceTo(x, y) <= radius && state.IsInBounds(x, y)) {
            visibleCells.Add(new EncounterPosition(x, y));
          }
        }
      }

      return new FoVCache(visibleCells);
    }
  }

  public class EncounterState : Control {

    // Encounter Log
    public int EncounterLogSize = 50;
    private List<string> _encounterLog;
    public ReadOnlyCollection<string> EncounterLog { get => _encounterLog.AsReadOnly(); }
    [Signal]
    public delegate void EncounterLogMessageAdded(string message, int encounterLogSize);

    // Encounter Map
    public int MapWidth { get; private set; }
    public int MapHeight { get; private set; }
    EncounterTile[,] _encounterTiles;
    public FoVCache FoVCache { get; private set; }

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

    public bool IsInBounds(int x, int y) {
      return x >= 0 && x < this.MapWidth && y >= 0 && y < this.MapHeight;
    }

    public bool IsInBounds(EncounterPosition position) {
      return IsInBounds(position.X, position.Y);
    }

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

    public Entity BlockingEntityAtPosition(EncounterPosition position) {
      if (!IsInBounds(position)) {
        throw new NotImplementedException("out of bounds");
      }

      return this._encounterTiles[position.X, position.Y].Entities.FirstOrDefault<Entity>(e => {
        return e.GetComponent<CollisionComponent>().BlocksMovement;
      });
    }

    // ##########################################################################################################################
    #endregion
    // ##########################################################################################################################

    public void PlaceEntity(Entity entity, EncounterPosition targetPosition, bool ignoreCollision = false) {
      if (!IsInBounds(targetPosition)) {
        throw new NotImplementedException("out of bounds");
      }
      if (!ignoreCollision && IsPositionBlocked(targetPosition)) {
        throw new NotImplementedException("probably handle this more gracefully than exploding");
      }

      var spriteData = entity.GetComponent<SpriteDataComponent>();
      
      var positionComponent = PositionComponent.Create(targetPosition, spriteData.Texture);
      entity.AddChild(positionComponent);

      var entityPosition = positionComponent.EncounterPosition;
      AddChild(entity);
      this._encounterTiles[entityPosition.X, entityPosition.Y].AddEntity(entity);
    }

    public void RemoveEntity(Entity entity) {
      var positionComponent = entity.GetComponent<PositionComponent>();
      entity.RemoveChild(positionComponent);

      var entityPosition = positionComponent.EncounterPosition;
      RemoveChild(entity);
      this._encounterTiles[entityPosition.X, entityPosition.Y].RemoveEntity(entity);
    }

    /**
    * Disregards intervening terrain; only checks for collisions at the target position.
    */
    public void TeleportEntity(Entity entity, EncounterPosition targetPosition) {
      if (!IsInBounds(targetPosition)) {
        throw new NotImplementedException("out of bounds");
      }
      if (IsPositionBlocked(targetPosition)) {
        throw new NotImplementedException("probably handle this more gracefully than exploding");
      }
      var positionComponent = entity.GetComponent<PositionComponent>();
      var oldPosition = positionComponent.EncounterPosition;

      this._encounterTiles[oldPosition.X, oldPosition.Y].RemoveEntity(entity);
      positionComponent.EncounterPosition = targetPosition;
      this._encounterTiles[targetPosition.X, targetPosition.Y].AddEntity(entity);
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

    // Those are very similar but the same, but anywhere you'd want to update your FoV you'd want to update your FoW;
    // contemplating just eliding one of the two in names?
    public void UpdateFoVAndFoW() {
      // TODO: Appropriate vision radius
      this.FoVCache = FoVCache.ComputeFoV(this, this.Player.GetComponent<PositionComponent>().EncounterPosition, 10);
      foreach (EncounterPosition position in this.FoVCache.VisibleCells) {
        this._encounterTiles[position.X, position.Y].Explored = true;
      }
    }

    public void UpdatePlayerOverlays() {
      var overlaysMap = GetNode<TileMap>("PlayerOverlays");
      overlaysMap.Clear();

      var playerPos = this.Player.GetComponent<PositionComponent>().EncounterPosition;

      // Range indicator
      // TODO: Have it respect FoV restrictions (? or not, it seems...fine?)
      var laserRange = this.Player.GetComponent<PlayerComponent>().CuttingLaserRange;
      for (int x = playerPos.X - laserRange; x <= playerPos.X + laserRange; x++) {
        for (int y = playerPos.Y - laserRange; y <= playerPos.Y + laserRange; y++) {
          var distance = playerPos.DistanceTo(x, y);
          if (distance <= laserRange && distance > laserRange - 1 && IsInBounds(x, y)) {
            overlaysMap.SetCell(x, y, 0);
          }
        }
      }

      // TODO: Fog of War overlay marks explored and doesn't just have black tiles
      // TODO: When you move sometimes long vertical lines appear, there was something about that in a tutorial - hunt that down
      // If we have perf issues we could change only the parts of the overlay that were changed in the FoV recalc
      for (int x = 0; x < this.MapWidth; x++) {
        for (int y = 0; y < this.MapHeight; y++) {
          if (this.FoVCache.Contains(x, y)) {
            // The space is visible and we do not mask it
          } else if (this._encounterTiles[x, y].Explored) {
            overlaysMap.SetCell(x, y, 1);
          } else {
            overlaysMap.SetCell(x, y, 2);
          }
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

    public static void DoTempMapGen(EncounterState state, int width = 300, int height = 300, int maxZones = 10, int maxZoneGenAttempts = 100) {
      // Initialize the map with empty tiles
      state.MapWidth = width;
      state.MapHeight = height;
      state._encounterTiles = new EncounterTile[width, height];
      for (int x = 0; x < width; x++) {
        for (int y = 0; y < width; y++) {
          state._encounterTiles[x, y] = new EncounterTile();
        }
      }

      state.PlaceEntity(EntityBuilder.CreatePlayerEntity(), new EncounterPosition(1, 1));
      state.PlaceEntity(EntityBuilder.CreateScoutEntity(), new EncounterPosition(10, 5));

      // Create border walls to prevent objects running off the map
      for (int x = 0; x < width; x++) {
        for (int y = 0; y < height; y++) {
          if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
            state.PlaceEntity(EntityBuilder.CreateMapWallEntity(), new EncounterPosition(x, y));
          }
        }
      }
    }

    // TODO: Move into map gen & save/load
    public void InitState() {
      DoTempMapGen(this);

      // TODO: Attaching camera to the player like this is extremely jank! Figure out a better way?
      var camera = GetNode<Camera2D>("EncounterCamera");
      RemoveChild(camera);
      // TODO: VERY DEFINITELY DON'T KEEP DOING THIS!!!
      Player.GetComponent<PositionComponent>().GetNode<Sprite>("Sprite").AddChild(camera);

      // Populate all our initial caches
      this._encounterLog = new List<string>();
      this.LogMessage("Encounter started!");
      this.UpdateFoVAndFoW();
      this.UpdatePlayerOverlays();
    }
  }
}