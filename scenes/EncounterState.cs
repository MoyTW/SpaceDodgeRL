using Godot;
using System;

public class EncounterState : Node {

  public InputHandler inputHandlerRef = null;

  // TODO: Move this out of EncounterState
  private PackedScene _entityPrefab = GD.Load<PackedScene>("res://scenes/entities/Entity.tscn");
  private PackedScene _positionComponentPrefab = GD.Load<PackedScene>("res://scenes/components/PositionComponent.tscn");
  private PackedScene _playerComponentPrefab = GD.Load<PackedScene>("res://scenes/components/PlayerComponent.tscn");
  private PackedScene _testAIComponentPrefab = GD.Load<PackedScene>("res://scenes/components/AI/TestAIComponent.tscn");
  // TODO: Bake the sub-textures into their own resources and load them instead of doing this Very Silly process
  private string _atlasPath = "res://resources/atlas_@.tres";
  private Rect2 _AtSignRect2 = new Rect2(new Vector2(0, 4 * 36), new Vector2(24, 36));

  // TODO: Move this out of EncounterState, create a better way of selecting the sprite than "hey an arbitrary rect"
  private Entity CreateEntity(string id, string name, GamePosition pos, Rect2 atlasRegion) {
    Entity newEntity = _entityPrefab.Instance() as Entity;
    newEntity.Init(id, name);

    var positionComponent = _positionComponentPrefab.Instance() as PositionComponent;
    AtlasTexture atlas = new AtlasTexture();
    atlas.Atlas = GD.Load<AtlasTexture>(_atlasPath);
    atlas.Region = atlasRegion;
    positionComponent.Init(pos, atlas);

    newEntity.AddChild(positionComponent);

    return newEntity;
  }

  public override void _Ready() {
    // ===== PLAYER HACK CREATION
    var player = this.CreateEntity("uuid#1", "player", new GamePosition(3, 5), _AtSignRect2);
    // TODO: Entity & Component hooks into add/remove, so we can use groupings properly!
    player.AddChild(this._playerComponentPrefab.Instance());
    player.AddToGroup("player");
    this.AddChild(player);

    // ===== AI SCOUT HACK CREATION
    var scout = this.CreateEntity("uuid#2", "scout", new GamePosition(5, 5), new Rect2(new Vector2(3 * 24, 7 * 36), new Vector2(24, 36)));
    scout.AddChild(this._testAIComponentPrefab.Instance());
    this.AddChild(scout);

    /*
    Entity testEntity = entityPrefab.Instance() as Entity;
    testEntity.Init("whoo", "blah");
    newEntity.AddChild(testEntity);

    var entities = GetTree().GetNodesInGroup(Entity.ENTITY_GROUP);
    for( int i = 0; i < entities.Count; i++ ) {
      var e = entities[i] as Entity;
      if (this == e.GetParent()) {
        GD.Print(entities[i] + " is a child of EncounterState");
      } else {
        GD.Print(entities[i] + " is a grandchild of EncounterState");
      }
    }
    */
  }

  public Entity Player {
    // TODO: player group in player component
    get => this.GetTree().GetNodesInGroup("player")[0] as Entity;
  }

  // TODO: Proper actions
  // TODO: Invert the Y-axis?
  private void TemporaryPlayerMoveFn(int dx, int dy) {
    var positionComponent = this.Player.GetNode<PositionComponent>("PositionComponent");
    var gamePosition = positionComponent.GamePosition;
    positionComponent.GamePosition = new GamePosition(gamePosition.X + dx, gamePosition.Y + dy);
  }

  public override void _Process(float delta) {
    var action = this.inputHandlerRef.PopQueue();
    // Super not a fan of the awkwardness of checking this twice! Switch string -> enum, maybe?
    if (action == InputHandler.ActionMapping.MOVE_N) {
      TemporaryPlayerMoveFn(0, -1);
    } else if (action == InputHandler.ActionMapping.MOVE_NE) {
      TemporaryPlayerMoveFn(1, -1);
    } else if (action == InputHandler.ActionMapping.MOVE_E) {
      TemporaryPlayerMoveFn(1, 0);
    } else if (action == InputHandler.ActionMapping.MOVE_SE) {
      TemporaryPlayerMoveFn(1, 1);
    } else if (action == InputHandler.ActionMapping.MOVE_S) {
      TemporaryPlayerMoveFn(0, 1);
    } else if (action == InputHandler.ActionMapping.MOVE_SW) {
      TemporaryPlayerMoveFn(-1, 1);
    } else if (action == InputHandler.ActionMapping.MOVE_W) {
      TemporaryPlayerMoveFn(-1, 0);
    } else if (action == InputHandler.ActionMapping.MOVE_NW) {
      TemporaryPlayerMoveFn(-1, -1);
    }
  }
}
