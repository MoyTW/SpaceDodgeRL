using Godot;
using SpaceDodgeRL.library;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SpaceDodgeRL.scenes.encounter.state {

  public class EncounterState : Control {
    private static PackedScene _encounterPrefab = GD.Load<PackedScene>("res://scenes/encounter/state/EncounterState.tscn");

    // TODO: put this somewhere proper!
    // Original 7DRL had vision radius = 20, but I dropped it for screen space reasons. If we bump up the screen/downsize the
    // tiles maybe I will add it back.
    public static int PLAYER_VISION_RADIUS = 12;
    public static int EncounterLogSize = 50;
    public static string RUN_STATUS_RUNNING = "ENCOUNTER_RUN_STATUS_RUNNING";
    public static string RUN_STATUS_PLAYER_VICTORY = "ENCOUNTER_RUN_STATUS_PLAYER_VICTORY";
    public static string RUN_STATUS_PLAYER_DEFEAT = "ENCOUNTER_RUN_STATUS_PLAYER_DEFEAT";

    public string SaveFilePath { get; private set; }

    // Encounter Log
    private List<string> _encounterLog;
    public ReadOnlyCollection<string> EncounterLog { get => _encounterLog.AsReadOnly(); }
    [Signal]
    public delegate void EncounterLogMessageAdded(string message, int encounterLogSize);

    // Encounter Map
    public int MapWidth { get; set; }
    public int MapHeight { get; set; }
    // TODO: Come back to the builder & access levels
    public EncounterTile[,] _encounterTiles;
    public List<EncounterZone> _zones;
    public ReadOnlyCollection<EncounterZone> Zones { get => _zones.AsReadOnly(); }
    public int LevelsInDungeon { get => 10; } // TODO: Properly pass this in!
    public int DungeonLevel { get; private set; }

    // Entity tracking
    private Dictionary<string, bool> _activationTracker;
    private Dictionary<string, Entity> _entitiesById;

    // Time & runner state
    public string RunStatus { get; private set; }
    private ActionTimeline _actionTimeline;
    public int CurrentTick { get => _actionTimeline.CurrentTick; }
    public Entity NextEntity { get => _actionTimeline.NextEntity; }
    public Entity Player { get; private set; }

    // Transitory data
    public FoVCache FoVCache { get; private set; }
    public Random EncounterRand { get; private set; }
    private List<PositionComponent> _animatingSprites = new List<PositionComponent>();
    private DynamicFont _damageFont;
    private List<Label> _damageLabels = new List<Label>();
    public bool HasAnimatingSprites { get {
      foreach (Entity e in this.PositionEntities()) {
        if (e.GetComponent<PositionComponent>().IsAnimating) {
          return true;
        }
      }
      return this._animatingSprites.Count > 0;
    } }

    public override void _Process(float delta) {
      foreach(var c in this._animatingSprites) {
        if (!c.IsAnimating) {
          this.RemoveChild(c);
          c.QueueFree();
        }
      }
      this._animatingSprites.RemoveAll(c => !c.IsAnimating);
    }

    // ##########################################################################################################################
    #region Data Access

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
      // This should never be hit, but it sometimes does get hit, apparently!
      throw new NotImplementedException(
        String.Format("Attempting to fetch zone id {0} but could not. Contents of zones: {1}", zoneId, JsonSerializer.Serialize(this._zones))
      );
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

    public bool IsExplored(EncounterPosition position) {
      return this._encounterTiles[position.X, position.Y].Explored;
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

    public bool IsPositionVisible(int x, int y) {
      if (!IsInBounds(x, y)) {
        return false;
      }

      return !this._encounterTiles[x, y].Entities.Any<Entity>(e => {
        var collisionComponent = e.GetComponent<CollisionComponent>();
        return collisionComponent != null && collisionComponent.BlocksVision;
      });
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

    public EncounterZone ClosestZone(int x, int y) {
      if (!IsInBounds(x, y)) {
        throw new NotImplementedException("out of bounds");
      }

      EncounterZone closestZone = null;
      float smallestDistance = float.MaxValue;
      foreach (EncounterZone zone in this.Zones) {
        var distance = zone.Center.DistanceTo(x, y);
        if (distance < smallestDistance) {
          smallestDistance = distance;
          closestZone = zone;
        }
      }
      return closestZone;
    }

    /**
     * Returns the zone containing the position. Returns null if the position is not inside a zone.
     */
    public EncounterZone ContainingZone(int x, int y) {
      var closest = this.ClosestZone(x, y);
      if (closest.Contains(x, y)) {
        return closest;
      } else {
        return null;
      }
    }

    #endregion
    // ##########################################################################################################################

    // ##########################################################################################################################
    #region Entity Management

    public void EntityHasEndedTurn(Entity entity) {
      _actionTimeline.EntityHasEndedTurn(entity);
    }

    public void PlaceEntity(Entity entity, EncounterPosition targetPosition, bool ignoreCollision = false) {
      if (!IsInBounds(targetPosition)) {
        throw new NotImplementedException("out of bounds");
      }
      if (!ignoreCollision && IsPositionBlocked(targetPosition)) {
        throw new NotImplementedException("probably handle this more gracefully than exploding");
      }

      // Add the position component
      var spriteData = entity.GetComponent<DisplayComponent>();
      var positionComponent = PositionComponent.Create(targetPosition, spriteData.TexturePath, spriteData.ZIndex);
      entity.AddComponent(positionComponent);

      var entityPosition = positionComponent.EncounterPosition;
      AddChild(entity);
      this._encounterTiles[entityPosition.X, entityPosition.Y].AddEntity(entity);
      this._entitiesById[entity.EntityId] = entity;

      // If it's an action entity, add it into the timeline. Anything with speed 0 is set to the front so it will instantly resolve.
      if (entity.GetComponent<ActionTimeComponent>() != null) {
        this._actionTimeline.AddEntityToTimeline(entity as Entity, entity.GetComponent<SpeedComponent>().Speed == 0);
      }
    }

    public void RemoveEntity(Entity entity) {
      // Remove the position component from both
      var positionComponent = entity.GetComponent<PositionComponent>();
      // This is absurdly awkward; it turns out removing the PositionComponent from the entity clears the Tween (because it
      // removes it from the tree, I believe?) which means we need to manually restart the tween. It also causes a shadow effect
      // where the projectile now overshoots the target instead of undershooting it.
      if (positionComponent.IsAnimating) {
        entity.RemoveComponent(positionComponent);
        this.AddChild(positionComponent);
        positionComponent.RestartTween();
        this._animatingSprites.Add(positionComponent);
      } else {
        entity.RemoveComponent(positionComponent);
      }

      var entityPosition = positionComponent.EncounterPosition;
      RemoveChild(entity);
      this._encounterTiles[entityPosition.X, entityPosition.Y].RemoveEntity(entity);
      this._entitiesById.Remove(entity.EntityId);

      // If it's an action entity, remove the action entity ONLY from EncounterState
      if (entity.GetComponent<ActionTimeComponent>() != null) {
        this._actionTimeline.RemoveEntityFromTimeline(entity as Entity);
      }
    }

    /**
    * Disregards intervening terrain; only checks for collisions at the target position.
    */
    public void TeleportEntity(Entity entity, EncounterPosition targetPosition, bool ignoreCollision) {
      if (!IsInBounds(targetPosition)) {
        throw new NotImplementedException("out of bounds");
      }
      if (!ignoreCollision && IsPositionBlocked(targetPosition)) {
        throw new NotImplementedException("probably handle this more gracefully than exploding");
      }
      var positionComponent = entity.GetComponent<PositionComponent>();
      var oldPosition = positionComponent.EncounterPosition;

      this._encounterTiles[oldPosition.X, oldPosition.Y].RemoveEntity(entity);
      bool shouldBeVisible = entity.GetComponent<DisplayComponent>().VisibleInFoW || this.FoVCache.IsVisible(targetPosition);
      positionComponent.SetEncounterPosition(targetPosition, shouldBeVisible);
      this._encounterTiles[targetPosition.X, targetPosition.Y].AddEntity(entity);
    }

    public bool GroupActivated(string activationGroupId) {
      if (!this._activationTracker.ContainsKey(activationGroupId)) {
        this._activationTracker[activationGroupId] = false;
      }
      return this._activationTracker[activationGroupId];
    }

    public void ActivateGroup(string activationGroupId) {
      this._activationTracker[activationGroupId] = true;
    }

    #endregion
    // ##########################################################################################################################
    #region Display caches

    public void UpdateDangerMap() {
      var dangerMap = GetNode<DangerMap>("CanvasLayer/DangerMap");
      dangerMap.UpdateAllTiles(this);
    }

    private void UpdateFoWForTile(TileMap overlaysMap, int x, int y) {
      if (!this.IsInBounds(x, y)) {
        // If you're out of bounds no-op
      } else if (this.FoVCache.IsVisible(x, y)) {
        overlaysMap.SetCell(x, y, -1);
      } else if (this._encounterTiles[x, y].Explored) {
        overlaysMap.SetCell(x, y, 1);
      } else {
        overlaysMap.SetCell(x, y, 2);
      }
    }

    private void InitFoWOverlay() {
      var overlaysMap = GetNode<TileMap>("FoWOverlay");

      for (int x = 0; x < this.MapWidth; x++) {
        for (int y = 0; y < this.MapWidth; y++) {
          this.UpdateFoWForTile(overlaysMap, x, y);
        }
      }
    }

    private void UpdateFoWOverlay() {
      var overlaysMap = GetNode<TileMap>("FoWOverlay");
      var playerPos = this.Player.GetComponent<PositionComponent>().EncounterPosition;

      // TODO: When you move sometimes long vertical lines appear, there was something about that in a tutorial - hunt that down
      for (int x = playerPos.X - PLAYER_VISION_RADIUS - 1; x <= playerPos.X + PLAYER_VISION_RADIUS + 1; x++) {
        for (int y = playerPos.Y - PLAYER_VISION_RADIUS - 1; y <= playerPos.Y + PLAYER_VISION_RADIUS + 1; y++) {
          this.UpdateFoWForTile(overlaysMap, x, y);
        }
      }
    }

    // Those are very similar but the same, but anywhere you'd want to update your FoV you'd want to update your FoW;
    // contemplating just eliding one of the two in names?
    public void UpdateFoVAndFoW() {
      HashSet<EncounterPosition> oldVisibleCells = this.FoVCache != null ? this.FoVCache.VisibleCells : new HashSet<EncounterPosition>();
      this.FoVCache = FoVCache.ComputeFoV(this, this.Player.GetComponent<PositionComponent>().EncounterPosition, PLAYER_VISION_RADIUS);
      var newVisibleCells = FoVCache.VisibleCells;

      // When we recalculate FoW we forcefully show/hide entities in the new FoV
      foreach (EncounterPosition position in newVisibleCells) {
        this._encounterTiles[position.X, position.Y].Explored = true;
        foreach (Entity entity in this._encounterTiles[position.X, position.Y].Entities) {
          entity.GetComponent<PositionComponent>().Show();
        }
      }
      foreach (EncounterPosition position in oldVisibleCells.Except(newVisibleCells)) {
        foreach (Entity entity in this._encounterTiles[position.X, position.Y].Entities) {
          if (!entity.GetComponent<DisplayComponent>().VisibleInFoW) {
            entity.GetComponent<PositionComponent>().Hide();
          }
        }
      }

      this.UpdateFoWOverlay();
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

    #endregion
    // ##########################################################################################################################

    public void LogMessage(string bbCodeMessage, bool failed=false) {
      if (failed) {
        bbCodeMessage = string.Format("[b]Failed:[/b] {0}", bbCodeMessage);
      }

      if (this._encounterLog.Count >= EncounterState.EncounterLogSize) {
        this._encounterLog.RemoveAt(0);
      }
      this._encounterLog.Add(bbCodeMessage);
      this.EmitSignal(nameof(EncounterLogMessageAdded), bbCodeMessage, EncounterState.EncounterLogSize);
    }

    public void ZoomIn() {
      Camera2D cam = (Camera2D)GetTree().GetNodesInGroup("ENCOUNTER_CAMERA_GROUP")[0];
      var oldZoom = cam.Zoom;
      // If you zoom past 0 it appears to invert, but we should stop it before it gets there.
      if (oldZoom.x > .4f) {
        cam.Zoom = new Vector2(oldZoom.x - .2f, oldZoom.y - .2f);
      }
    }

    public void ZoomOut() {
      Camera2D cam = (Camera2D)GetTree().GetNodesInGroup("ENCOUNTER_CAMERA_GROUP")[0];
      var oldZoom = cam.Zoom;
      cam.Zoom = new Vector2(oldZoom.x + .2f, oldZoom.y + .2f);
    }

    public void ZoomReset() {
      Camera2D cam = (Camera2D)GetTree().GetNodesInGroup("ENCOUNTER_CAMERA_GROUP")[0];
      cam.Zoom = new Vector2(1f, 1f);
    }

    public static EncounterState Create(string saveFilePath) {
      var state = _encounterPrefab.Instance() as EncounterState;
      state.SaveFilePath = saveFilePath;
      return state;
    }

    // Should be for testing purposes only!
    public static EncounterState CreateWithoutSaving() {
      return _encounterPrefab.Instance() as EncounterState;
    }

    public void SetStateForNewGame() {
      this.ResetStateForNewLevel(EntityBuilder.CreatePlayerEntity(0), 1);
    }

    // TODO: Think harder about initialization & such & how it integrates into Godot
    public override void _Ready() {
      if (GetTree().GetNodesInGroup("ENCOUNTER_CAMERA_GROUP").Count == 0) {
        var camera = new Camera2D();
        camera.AddToGroup("ENCOUNTER_CAMERA_GROUP");
        camera.Current = true;
        this.Player.GetComponent<PositionComponent>().GetNode<Sprite>("Sprite").AddChild(camera);
      }
      this.UpdateDangerMap();

      // Set the background image - stretch it out so that it covers visible OOB areas too.
      var background = GetNode<Sprite>("Background");
      var pixelsWidth = PositionComponent.STEP_X * this.MapWidth + PositionComponent.START_X;
      var pixelsHeight = PositionComponent.STEP_Y * this.MapWidth + PositionComponent.START_Y;
      background.Position = new Vector2(pixelsWidth / 2, pixelsHeight / 2);
      background.RegionEnabled = true;
      background.RegionRect = new Rect2(new Vector2(0, 0), pixelsWidth * 2, pixelsHeight * 2);
    }

    // TODO: Move into map gen & save/load
    public void ResetStateForNewLevel(Entity player, int dungeonLevel) {

      string ENCOUNTER_CAMERA_GROUP = "ENCOUNTER_CAMERA_GROUP";
      // TODO: Rather than re-using a state when we switch levels, I'd rather sub in a new one, but I think I need to think
      // about how that'd work in Godot, since we'd need to do some rewiring and the state has the camera, which is ugh.
      if (this.IsInsideTree()) {
        foreach (Entity e in GetTree().GetNodesInGroup(Entity.ENTITY_GROUP)) {
          if (e.GetParent() == this) {
            this.RemoveEntity(e);
            if (e != player) {
              e.QueueFree();
            }
          }
        }
      }

      this.Player = player;
      this.DungeonLevel = dungeonLevel;

      // This class is kinda becoming a monster WRT "here's a cached thing for perf/getting around Godot reasons!"
      this._encounterLog = new List<string>();
      this._entitiesById = new Dictionary<string, Entity>();
      this._activationTracker = new Dictionary<string, bool>();
      this.RunStatus = EncounterState.RUN_STATUS_RUNNING;
      this._actionTimeline = new ActionTimeline(0);
      // We also need to reset the player's action time
      player.GetComponent<ActionTimeComponent>().SetNextTurnAtTo(0);

      // TODO: Map gen seed properly
      var seed = new Random().Next();
      GD.Print("Seed:", seed);
      EncounterStateBuilder.PopulateStateForLevel(player, dungeonLevel, this, new Random(seed));

      // TODO: save/load the state of rand for reproducibility?
      this.EncounterRand = new Random(1);

      // TODO: Attaching camera to the player like this is extremely jank! Figure out a better way?
      if (this.IsInsideTree() && GetTree().GetNodesInGroup(ENCOUNTER_CAMERA_GROUP).Count == 0) {
        var camera = new Camera2D();
        camera.AddToGroup(ENCOUNTER_CAMERA_GROUP);
        camera.Current = true;
        Player.GetComponent<PositionComponent>().GetNode<Sprite>("Sprite").AddChild(camera);
      }

      // Populate all our initial caches
      this.LogMessage(string.Format("Level {0} started!", dungeonLevel));
      this.UpdateFoVAndFoW();
      this.InitFoWOverlay();
      this.UpdatePlayerOverlays();
      if (this.IsInsideTree()) {
        this.UpdateDangerMap();
      }
    }

    public void NotifyPlayerVictory() {
      this.RunStatus = EncounterState.RUN_STATUS_PLAYER_VICTORY;
    }

    public void NotifyPlayerDefeat() {
      this.RunStatus = EncounterState.RUN_STATUS_PLAYER_DEFEAT;
    }

    public class SaveData {
      // TODO: Don't store the path in the save file itself (you can't move/rename save files which is real annoying)
      public string SaveFilePath { get; set; }
      public List<string> EncounterLog { get; set; }
      public int MapWidth { get; set; }
      public int MapHeight { get; set; }
      public List<Entity> Entities { get; set; }
      public string EncounterTileExploration { get; set; }
      public List<EncounterZone> Zones { get; set; }
      public Dictionary<string, bool> ActivationTracker { get; set; }
      public string RunStatus { get; set; }
      public ActionTimeline.SaveData ActionTimeline { get; set; }
      public string PlayerId { get; set; }
      public int LevelsInDungeon { get; set; }
      public int DungeonLevel { get; set; }
      // For now we're just gonna...not deal with the rand; that's a whole OTHER issue. Probably solution is re-seed every
      // invocation and store the re-seed though. Or we just say "eh we don't care, it can go be random however".
      // public Random EncounterRand { get; private set; }
    }

    public static EncounterState FromSaveData(string saveData) {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();

      Stopwatch deserializer = new Stopwatch();
      deserializer.Start();
      SaveData data = JsonSerializer.Deserialize<SaveData>(saveData);
      deserializer.Stop();
      GD.Print("Deserialization ms: ", deserializer.ElapsedMilliseconds);
      EncounterState state = _encounterPrefab.Instance() as EncounterState;

      state.SaveFilePath = data.SaveFilePath;
      // This populates the representation, but does not print - see EncounterScene._Ready() for the display code.
      state._encounterLog = data.EncounterLog;
      state.MapWidth = data.MapWidth;
      state.MapHeight = data.MapHeight;

      Dictionary<EncounterPosition, List<Entity>> entitiesToPositions = new Dictionary<EncounterPosition, List<Entity>>();
      Dictionary<string, Entity> entitiesById = new Dictionary<string, Entity>();
      foreach (var entity in data.Entities) {
        entitiesById.Add(entity.EntityId, entity);
        var positionComponent = entity.GetComponent<PositionComponent>();
        if (positionComponent != null) {
          if (!entitiesToPositions.ContainsKey(positionComponent.EncounterPosition)) {
            entitiesToPositions[positionComponent.EncounterPosition] = new List<Entity>();
          }
          entitiesToPositions[positionComponent.EncounterPosition].Add(entity);
        }
      }

      state._encounterTiles = new EncounterTile[data.MapHeight, data.MapHeight];
      for (int x = 0; x < data.MapWidth; x++) {
        for (int y = 0; y < data.MapHeight; y++) {
          var pos = new EncounterPosition(x, y);
          bool explored = data.EncounterTileExploration[x * data.MapWidth + y] == 'x';
          List<Entity> entities = entitiesToPositions.ContainsKey(pos) ? entitiesToPositions[pos] : null;
          state._encounterTiles[x, y] = EncounterTile.FromSaveData(explored, entities);
        }
      }

      state._zones = data.Zones;
      state._entitiesById = entitiesById;
      foreach (var entity in data.Entities) {
        state.AddChild(entity);
      }
      state._activationTracker = data.ActivationTracker;
      state.RunStatus = data.RunStatus != null ? data.RunStatus : EncounterState.RUN_STATUS_RUNNING;
      state._actionTimeline = ActionTimeline.FromSaveData(data.ActionTimeline, entitiesById);
      state.Player = entitiesById[data.PlayerId];
      // TODO: Dungeon height
      state.DungeonLevel = data.DungeonLevel;

      // TODO: save rand
      state.EncounterRand = new Random(1);

      string ENCOUNTER_CAMERA_GROUP = "ENCOUNTER_CAMERA_GROUP";
      var camera = new Camera2D();
      camera.AddToGroup(ENCOUNTER_CAMERA_GROUP);
      camera.Current = true;
      state.Player.GetComponent<PositionComponent>().GetNode<Sprite>("Sprite").AddChild(camera);

      state.UpdateFoVAndFoW();
      state.InitFoWOverlay();
      state.UpdatePlayerOverlays();

      stopwatch.Stop();
      GD.Print("EncounterState saveData -> EncounterState completed, elapsed time: ", stopwatch.ElapsedMilliseconds);

      return state;
    }

    public void WriteToFile() {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();

      Godot.File write = new Godot.File();
      write.Open(this.SaveFilePath, File.ModeFlags.Write);
      string saveString = this.ToSaveData();
      saveString = StringCompression.CompressString(saveString);
      write.StoreString(saveString);
      write.Close();

      GD.Print("EncounterState file writed completed, elapsed time: ", stopwatch.ElapsedMilliseconds);
    }

    private string ToSaveData() {
      var data = new SaveData();

      data.SaveFilePath = this.SaveFilePath;
      data.EncounterLog = this._encounterLog;
      data.MapWidth = this.MapWidth;
      data.MapHeight = this.MapHeight;

      StringBuilder builder = new StringBuilder();
      for (int x = 0; x < data.MapWidth; x++) {
        for (int y = 0; y < data.MapHeight; y++) {
          if (this._encounterTiles[x, y].Explored) {
            builder.Append('x');
          } else {
            builder.Append('o');
          }
        }
      }
      data.EncounterTileExploration = builder.ToString();

      data.Zones = this._zones;
      data.Entities = this._entitiesById.Values.ToList();
      data.ActivationTracker = this._activationTracker;
      data.RunStatus = this.RunStatus;
      data.ActionTimeline = this._actionTimeline.ToSaveData();
      data.PlayerId = this.Player.EntityId;
      data.LevelsInDungeon = this.LevelsInDungeon;
      data.DungeonLevel = this.DungeonLevel;

      return JsonSerializer.Serialize(data);
    }
  }
}