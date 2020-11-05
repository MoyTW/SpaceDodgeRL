using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.resources.gamedata;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpaceDodgeRL.scenes.encounter.state {

  public class EncounterState : Control {

    // TODO: put this somewhere proper!
    public static int PLAYER_VISION_RADIUS = 10;
    public static int ZONE_MIN_SIZE = 20;
    public static int ZONE_MAX_SIZE = 40;

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
    private List<EncounterZone> _zones;
    public ReadOnlyCollection<EncounterZone> Zones { get => _zones.AsReadOnly(); }
    private Dictionary<string, Entity> _entitiesById;

    // ##########################################################################################################################
    #region Data Access
    // ##########################################################################################################################

    public Entity Player { get; private set; }
    public int DungeonLevel { get; private set; }
    public Entity NextEntity { get; private set; }

    /**
     * Only returns direct child entities.
     */
    public Entity GetEntityById(string entityId) {
      if (_entitiesById.ContainsKey(entityId)) {
        return _entitiesById[entityId];
      } else {
        return null;
      }
    }

    public EncounterZone GetZoneById(string zoneId) {
      // Yes we're iterating them all at one time but there's only, like...10 zones, so...computers are fast right?
      foreach (EncounterZone zone in _zones) {
        if (zone.ZoneId == zoneId) {
          return zone;
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

    public bool IsPositionBlocked(int x, int y) {
      return BlockingEntityAtPosition(x, y) != null;
    }

    public bool IsPositionBlocked(EncounterPosition position) {
      return IsPositionBlocked(position.X, position.Y);
    }

    public ReadOnlyCollection<Entity> EntitiesAtPosition(int x, int y) {
      if (!IsInBounds(x, y)) {
        throw new NotImplementedException("out of bounds");
      }

      return this._encounterTiles[x, y].Entities;
    }

    public Entity BlockingEntityAtPosition(int x, int y) {
      if (!IsInBounds(x, y)) {
        throw new NotImplementedException("out of bounds");
      }

      return this._encounterTiles[x, y].Entities.FirstOrDefault<Entity>(e => {
        var collisionComponent = e.GetComponent<CollisionComponent>();
        return collisionComponent != null && collisionComponent.BlocksMovement;
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
      this._entitiesById[entity.EntityId] = entity;
    }

    public void RemoveEntity(Entity entity) {
      var positionComponent = entity.GetComponent<PositionComponent>();
      entity.RemoveChild(positionComponent);
      positionComponent.QueueFree();

      var entityPosition = positionComponent.EncounterPosition;
      RemoveChild(entity);
      this._encounterTiles[entityPosition.X, entityPosition.Y].RemoveEntity(entity);
      this._entitiesById.Remove(entity.EntityId);
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

    private void InitFoWOverlay() {
      var overlaysMap = GetNode<TileMap>("FoWOverlay");

      for (int x = 0; x < this.MapWidth; x++) {
        for (int y = 0; y < this.MapWidth; y++) {
          overlaysMap.SetCell(x, y, 2);
        }
      }
    }

    private void UpdateFoWOverlay() {
      var overlaysMap = GetNode<TileMap>("FoWOverlay");
      var playerPos = this.Player.GetComponent<PositionComponent>().EncounterPosition;

      // TODO: When you move sometimes long vertical lines appear, there was something about that in a tutorial - hunt that down
      for (int x = playerPos.X - PLAYER_VISION_RADIUS - 1; x <= playerPos.X + PLAYER_VISION_RADIUS + 1; x++) {
        for (int y = playerPos.Y - PLAYER_VISION_RADIUS - 1; y <= playerPos.Y + PLAYER_VISION_RADIUS + 1; y++) {
          if (!this.IsInBounds(x, y)) {
            // If you're out of bounds no-op
          } else if (this.FoVCache.Contains(x, y)) {
            overlaysMap.SetCell(x, y, -1);
          } else if (this._encounterTiles[x, y].Explored) {
            overlaysMap.SetCell(x, y, 1);
          } else {
            overlaysMap.SetCell(x, y, 2);
          }
        }
      }
    }

    // Those are very similar but the same, but anywhere you'd want to update your FoV you'd want to update your FoW;
    // contemplating just eliding one of the two in names?
    public void UpdateFoVAndFoW() {
      // TODO: Appropriate vision radius
      this.FoVCache = FoVCache.ComputeFoV(this, this.Player.GetComponent<PositionComponent>().EncounterPosition, PLAYER_VISION_RADIUS);
      foreach (EncounterPosition position in this.FoVCache.VisibleCells) {
        this._encounterTiles[position.X, position.Y].Explored = true;
      }
      this.UpdateFoWOverlay();
    }

    // TODO: I should roll all this into one singular "Update for end turn" function taking (player=false)
    public void CalculateNextEntity() {
      int lowestTTL = int.MaxValue;
      Entity next = null;

      // TODO: This is slow because we can all ActionTimeComponents every time, whereas we should maintain an internal representation
      // TODO: Also this code is really awful!
      foreach (Node node in GetTree().GetNodesInGroup(ActionTimeComponent.ENTITY_GROUP)) {
        if (node.GetParent() == this) {
          var ticksUntilTurn = (node as Entity).GetComponent<ActionTimeComponent>().TicksUntilTurn;
          if (ticksUntilTurn < lowestTTL) {
            lowestTTL = ticksUntilTurn;
            next = node as Entity;
          }
        }
      }

      if (lowestTTL == int.MaxValue) {
        throw new NotImplementedException();
      }

      this.NextEntity = next;
    }

    public void UpdatePlayerOverlays() {
      var overlaysMap = GetNode<TileMap>("PlayerOverlays");
      overlaysMap.Clear();

      // Update the range indicator
      var laserRange = this.Player.GetComponent<PlayerComponent>().CuttingLaserRange;
      var playerPos = this.Player.GetComponent<PositionComponent>().EncounterPosition;
      for (int x = playerPos.X - laserRange; x <= playerPos.X + laserRange; x++) {
        for (int y = playerPos.Y - laserRange; y <= playerPos.Y + laserRange; y++) {
          var distance = playerPos.DistanceTo(x, y);
          if (distance <= laserRange && distance > laserRange - 1 && IsInBounds(x, y)) {
            overlaysMap.SetCell(x, y, 0);
          }
        }
      }
    }

    public void LogMessage(string bbCodeMessage) {
      if (this._encounterLog.Count >= this.EncounterLogSize) {
        this._encounterLog.RemoveAt(0);
      }
      this._encounterLog.Add(bbCodeMessage);
      this.EmitSignal("EncounterLogMessageAdded", bbCodeMessage, this.EncounterLogSize);
    }

    private static void PopulateZone(EncounterZone zone, int dungeonLevel, Random seededRand, EncounterState state, bool safe=false) {
      // Add satellites
      int numSatellites = LevelData.GetNumberOfSatellites(dungeonLevel);
      for (int i = 0; i < numSatellites; i++) {
        var unblockedPosition = zone.RandomUnblockedPosition(seededRand, state);
        var satellite = EntityBuilder.CreateSatelliteEntity();
        state.PlaceEntity(satellite, unblockedPosition);
      }

      EncounterDef encounterDef;
      if (safe) {
        encounterDef = LevelData.GetEncounterDefById(EncounterDefId.EMPTY_ENCOUNTER);
      } else {
        encounterDef = LevelData.ChooseEncounter(dungeonLevel, seededRand);
      }
      zone.ReadoutEncounterName = encounterDef.Name;

      foreach (string entityDefId in encounterDef.EntityDefIds) {
        var unblockedPosition = zone.RandomUnblockedPosition(seededRand, state);
        var newEntity = EntityBuilder.CreateEntityByEntityDefId(entityDefId);
        state.PlaceEntity(newEntity, unblockedPosition);
      }

      var chosenItemDefs = LevelData.ChooseItemDefs(dungeonLevel, seededRand);
      foreach(string chosenItemDefId in chosenItemDefs) {
        var unblockedPosition = zone.RandomUnblockedPosition(seededRand, state);
        var newEntity = EntityBuilder.CreateEntityByEntityDefId(chosenItemDefId);
        state.PlaceEntity(newEntity, unblockedPosition);
        zone.AddItemToReadout(newEntity);
      }
    }

    public static void DoTempMapGen(Entity player, int dungeonLevel, EncounterState state, Random seededRand,
        int width = 300, int height = 300, int maxZones = 10, int maxZoneGenAttempts = 100) {
      // Initialize the map with empty tiles
      state.MapWidth = width;
      state.MapHeight = height;
      state._encounterTiles = new EncounterTile[width, height];
      for (int x = 0; x < width; x++) {
        for (int y = 0; y < width; y++) {
          state._encounterTiles[x, y] = new EncounterTile();
        }
      }

      // Create border walls to prevent objects running off the map
      for (int x = 0; x < width; x++) {
        for (int y = 0; y < height; y++) {
          if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
            state.PlaceEntity(EntityBuilder.CreateMapWallEntity(), new EncounterPosition(x, y));
          }
        }
      }

      // Place each empty zone onto the map
      int zoneGenAttemps = 0;
      List<EncounterZone> zones = new List<EncounterZone>();
      while (zoneGenAttemps < maxZoneGenAttempts && zones.Count < maxZones) {
        int zoneWidth = seededRand.Next(ZONE_MIN_SIZE, ZONE_MAX_SIZE + 1);
        int zoneHeight = seededRand.Next(ZONE_MIN_SIZE, ZONE_MAX_SIZE + 1);
        int zoneX = seededRand.Next(1, state.MapWidth - zoneWidth);
        int zoneY = seededRand.Next(1, state.MapHeight - zoneHeight);

        var newZone = new EncounterZone(Guid.NewGuid().ToString(), new EncounterPosition(zoneX, zoneY), zoneWidth, zoneHeight,
          "Zone " + zones.Count.ToString());

        bool overlaps = zones.Any(existing => existing.Intersects(newZone));
        if (!overlaps) {
          zones.Add(newZone);
        }
      }
      state._zones = zones;

      // Add the player to the map
      var playerZoneIdx = seededRand.Next(0, zones.Count);
      state.PlaceEntity(player, zones[playerZoneIdx].Center);
      // TODO: delete the following test item
      var nextToPlayer = new EncounterPosition(zones[playerZoneIdx].Center.X + 1, zones[playerZoneIdx].Center.Y + 1);
      state.PlaceEntity(EntityBuilder.CreateEntityByEntityDefId(EntityDefId.ITEM_DUCT_TAPE), nextToPlayer);

      // Add all the various zone features to the map
      // TODO: Handle last level & add diplomat

      // Generate the stairs (maybe we should refer interally as something more themetically appropriate?)
      // You can get stairs in your starting zone, but you probably shouldn't take them...
      var stairsZone = zones[playerZoneIdx]; // TODO: not this
      //var stairsZone = zones[seededRand.Next(0, zones.Count)];
      var stairsPosition = stairsZone.RandomUnblockedPosition(seededRand, state);
      var stairs = EntityBuilder.CreateStairsEntity();
      state.PlaceEntity(stairs, stairsPosition);
      stairsZone.AddFeatureToReadout(stairs);

      // Generate intel
      var intelZone = zones[playerZoneIdx]; // TODO: not this
      //var intelZone = zones[seededRand.Next(0, zones.Count)];
      var intelPosition = intelZone.RandomUnblockedPosition(seededRand, state);
      var intel = EntityBuilder.CreateIntelEntity(dungeonLevel + 1);
      state.PlaceEntity(intel, intelPosition);
      intelZone.AddFeatureToReadout(intel);

      // Populate each zone with an encounter
      foreach (EncounterZone zone in zones) {
        if (zone == zones[playerZoneIdx]) {
          PopulateZone(zone, dungeonLevel, seededRand, state, safe: true);
        } else {
          PopulateZone(zone, dungeonLevel, seededRand, state);
        }
      }
    }

    // TODO: Move into map gen & save/load
    public void InitState(Entity player, int dungeonLevel) {
      string ENCOUNTER_CAMERA_GROUP = "ENCOUNTER_CAMERA_GROUP";
      // TODO: Rather than re-using a state when we switch levels, I'd rather sub in a new one, but I think I need to think
      // about how that'd work in Godot, since we'd need to do some rewiring and the state has the camera, which is ugh.
      foreach (Entity e in GetTree().GetNodesInGroup(Entity.ENTITY_GROUP)) {
        if (e.GetParent() == this) {
          this.RemoveEntity(e);
          if (e != player) {
            e.QueueFree();
          }
        }
      }

      this.Player = player;
      this.DungeonLevel = dungeonLevel;

      // This class is kinda becoming a monster WRT "here's a cached thing for perf/getting around Godot reasons!"
      this._encounterLog = new List<string>();
      this._entitiesById = new Dictionary<string, Entity>();

      // TODO: Map gen seed properly
      DoTempMapGen(player, dungeonLevel, this, new Random(1));

      // TODO: Attaching camera to the player like this is extremely jank! Figure out a better way?
      if (GetTree().GetNodesInGroup(ENCOUNTER_CAMERA_GROUP).Count == 0) {
        var camera = new Camera2D();
        camera.AddToGroup(ENCOUNTER_CAMERA_GROUP);
        camera.Current = true;
        Player.GetComponent<PositionComponent>().GetNode<Sprite>("Sprite").AddChild(camera);
      }

      // Populate all our initial caches
      this.LogMessage(string.Format("Level {0} started!", dungeonLevel));
      this.CalculateNextEntity();
      // Init FoW overlay as all back
      this.InitFoWOverlay();
      this.UpdateFoVAndFoW();
      this.UpdatePlayerOverlays();
    }
  }
}