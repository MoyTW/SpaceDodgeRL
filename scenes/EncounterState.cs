using Godot;
using System;

public class EncounterState : Node {

  public InputHandler inputHandlerRef = null;
  public EntityBuilder entityBuilderRef = null;

  // TODO: Save/Load/Proper New Game
  private bool hasCreated = false;

  public override void _Ready() { }

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
    if (!this.hasCreated) {
      this.AddChild(entityBuilderRef.CreatePlayerEntity(new GamePosition(3, 5)));
      this.AddChild(entityBuilderRef.CreateScoutEntity(new GamePosition(5, 5)));

      // TODO: Attaching camera to the player like this is extremely jank! ALSO, it's causing a weird jumping behaviour where the
      // camera moves milliseconds after the player teleports to the next position!
      var camera = this.GetNode<Camera2D>("EncounterCamera");
      this.RemoveChild(camera);
      this.Player.GetNode<PositionComponent>("PositionComponent").AddChild(camera);

      this.hasCreated = true;
    }

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
