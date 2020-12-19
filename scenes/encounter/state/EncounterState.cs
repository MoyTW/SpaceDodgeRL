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
using System.Text.Json;
using System.Text.Json.Serialization;

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

      // It's a little silly to have this calculated on first invocation instead of on creation/on load; in practice computers
      // are fast and this is unnoticable; it might be annoying to track down six months from now if I'm still working on it
      // though...it'd definitely be annoying if somebody else was working on it!
      var closestZoneId = this._encounterTiles[x, y].ClosestZoneId;
      if (closestZoneId == null) {
        for (int tx = 0; tx < this.MapWidth; tx++) {
          for (int ty = 0; ty < this.MapHeight; ty++) {
            EncounterZone closestZone = null;
            float smallestDistance = float.MaxValue;
            foreach (EncounterZone zone in this.Zones) {
              var distance = zone.Center.DistanceTo(tx, ty);
              if (distance < smallestDistance) {
                smallestDistance = distance;
                closestZone = zone;
              }
            }
            this._encounterTiles[tx, ty].ClosestZoneId = closestZone.ZoneId;
          }
        }
        closestZoneId = this._encounterTiles[x, y].ClosestZoneId;
      }
      return this.GetZoneById(closestZoneId);
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
      
      var positionComponent = PositionComponent.Create(targetPosition, spriteData.TexturePath);
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
      positionComponent.EncounterPosition = targetPosition;
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

    /**
     * Displays the danger map on the "DangerMap" TileMap.
     *
     * "Danger" is kind of tricky, because it's not binary. We have several basic states:
     * 1: Safe - no projectile path crosses this tile at all
     * 2: Dangerous - a projectile path crosses this tile, and cannot be stopped from doing so (see below)
     * 3: Now safe, possibly dangerous - there is an obstruction in between the tile and the projectile, but the obstruction may
     *    move or be destroyed before the projectile hits it.
     * 4: Now dangerous, possibly safe - there is no obstruction, but there is the possibility of something moving between the
     *    projectile and the tile before the projectile finishes its movement.
     *
     * The current rule should be "You can never get hit on a safe square, but can sometimes avoid getting hit on a dangerous
     * square."
     *
     * With an infinite-speed projectile, cases 3 & 4 cease to exist; however all enemy projectiles have travel time. Therefore
     * you're going to have a lot of states 3 & 4! At the moment we should err on marking dangerous by default, but we should
     * come back to this and think about if we can have a different color/status to 3 & 4. Likewise, we currently do not attempt
     * to distinguish between "you will take a shotgun shell if you move here" and "you're gonna get pasted by 4 railgun shots",
     * which is another aspect that the danger map currently flattens.
     *
     * TODO: work out how to display blocking projectiles and danger magnitudes
     */
    public void UpdateDangerMap() {
      var dangerMap = GetNode<TileMap>("DangerMap");
      var pathEntities = GetTree().GetNodesInGroup(PathAIComponent.ENTITY_GROUP);
      var timeToNextPlayerMove = this.Player.GetComponent<SpeedComponent>().Speed;

      dangerMap.Clear();
      // TODO: We don't actually need to update every entity, every time, since we only need to set the cell when the projectile itself moves
      foreach (Entity pathEntity in pathEntities) {
        var pathEntitySpeed = pathEntity.GetComponent<SpeedComponent>().Speed;
        var path = pathEntity.GetComponent<PathAIComponent>().Path;

        int stepsToProject = pathEntitySpeed != 0 ? timeToNextPlayerMove / pathEntitySpeed : Int16.MaxValue;
        var dangerPositions = path.Project(stepsToProject);
        if (dangerPositions.Count > 0) {
          var pathEntityPositionComponent = pathEntity.GetComponent<PositionComponent>();
          var pathEntityPos = pathEntityPositionComponent.EncounterPosition;
          var lastDangerPosition = dangerPositions[dangerPositions.Count - 1];
          pathEntityPositionComponent.RotateSpriteTowards( lastDangerPosition.X - pathEntityPos.X, lastDangerPosition.Y - pathEntityPos.Y);
        }

        foreach (EncounterPosition dangerPosition in dangerPositions) {
          dangerMap.SetCell(dangerPosition.X, dangerPosition.Y, 0);

          // If we have a fully immobile, invincible entity at the position we stop the path - otherwise we still draw it.
          var blockingEntity = this.BlockingEntityAtPosition(dangerPosition.X, dangerPosition.Y);
          if (blockingEntity != null &&
              blockingEntity.GetComponent<ActionTimeComponent>() == null &&
              blockingEntity.GetComponent<DefenderComponent>() != null &&
              blockingEntity.GetComponent<DefenderComponent>().IsInvincible) {
            break;
          }
        }
      }
    }

    private void UpdateFoWForTile(TileMap overlaysMap, int x, int y) {
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
      // TODO: Appropriate vision radius
      // TODO: Hide sprites out of FoW and show sprites in FoW, according to their VisibleInFoW property
      this.FoVCache = FoVCache.ComputeFoV(this, this.Player.GetComponent<PositionComponent>().EncounterPosition, PLAYER_VISION_RADIUS);
      foreach (EncounterPosition position in this.FoVCache.VisibleCells) {
        this._encounterTiles[position.X, position.Y].Explored = true;
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

    public void LogMessage(string bbCodeMessage) {
      if (this._encounterLog.Count >= EncounterState.EncounterLogSize) {
        this._encounterLog.RemoveAt(0);
      }
      this._encounterLog.Add(bbCodeMessage);
      this.EmitSignal(nameof(EncounterLogMessageAdded), bbCodeMessage, EncounterState.EncounterLogSize);
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

      // Set the background image
      var background = GetNode<Sprite>("Background");
      var pixelsWidth = PositionComponent.STEP_X * this.MapWidth + PositionComponent.START_X;
      var pixelsHeight = PositionComponent.STEP_Y * this.MapWidth + PositionComponent.START_Y;
      background.Position = new Vector2(pixelsWidth / 2, pixelsHeight / 2);
      background.Scale = new Vector2(pixelsWidth, pixelsHeight) / background.Texture.GetSize();
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
      public EncounterTile.SaveData[][] EncounterTiles { get; set; }
      public List<EncounterZone> Zones { get; set; }
      public Dictionary<string, Entity> EntitiesById { get; set; }
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
      SaveData data = JsonSerializer.Deserialize<SaveData>(saveData);
      EncounterState state = _encounterPrefab.Instance() as EncounterState;

      state.SaveFilePath = data.SaveFilePath;
      // This populates the representation, but does not print - see EncounterScene._Ready() for the display code.
      state._encounterLog = data.EncounterLog;
      state.MapWidth = data.MapWidth;
      state.MapHeight = data.MapHeight;
      state._encounterTiles = new EncounterTile[data.MapHeight, data.MapHeight];
      for (int x = 0; x < data.MapWidth; x++) {
        for (int y = 0; y < data.MapHeight; y++) {
          state._encounterTiles[x, y] = EncounterTile.FromSaveData(data.EncounterTiles[x][y], data.EntitiesById);
        }
      }
      state._zones = data.Zones;
      state._entitiesById = data.EntitiesById;
      foreach (var entity in data.EntitiesById.Values) {
        state.AddChild(entity);
      }
      state._activationTracker = data.ActivationTracker;
      state.RunStatus = data.RunStatus != null ? data.RunStatus : EncounterState.RUN_STATUS_RUNNING;
      state._actionTimeline = ActionTimeline.FromSaveData(data.ActionTimeline, data.EntitiesById);
      state.Player = data.EntitiesById[data.PlayerId];
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

      return state;
    }

    public void WriteToFile() {
      Godot.File write = new Godot.File();
      write.Open(this.SaveFilePath, File.ModeFlags.Write);
      write.StoreString(this.ToSaveData());
      write.Close();
    }

    private string ToSaveData() {
      var data = new SaveData();

      data.SaveFilePath = this.SaveFilePath;
      data.EncounterLog = this._encounterLog;
      data.MapWidth = this.MapWidth;
      data.MapHeight = this.MapHeight;
      data.EncounterTiles = new EncounterTile.SaveData[data.MapWidth][];
      for (int x = 0; x < data.MapWidth; x++) {
        data.EncounterTiles[x] = new EncounterTile.SaveData[data.MapHeight];
        for (int y = 0; y < data.MapHeight; y++) {
          data.EncounterTiles[x][y] = this._encounterTiles[x, y].ToSaveData();
        }
      }
      data.Zones = this._zones;
      data.EntitiesById = this._entitiesById;
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