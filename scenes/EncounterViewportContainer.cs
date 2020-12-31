using System.Collections.Generic;
using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes {
  public class EncounterViewportContainer : Godot.ViewportContainer {
    private Texture moveNCursor = ResourceLoader.Load<Texture>("res://resources/cursors/move_n_24x24.png");
    private Texture moveNECursor = ResourceLoader.Load<Texture>("res://resources/cursors/move_ne_24x24.png");
    private Texture moveECursor = ResourceLoader.Load<Texture>("res://resources/cursors/move_e_24x24.png");
    private Texture moveSECursor = ResourceLoader.Load<Texture>("res://resources/cursors/move_se_24x24.png");
    private Texture moveSCursor = ResourceLoader.Load<Texture>("res://resources/cursors/move_s_24x24.png");
    private Texture moveSWCursor = ResourceLoader.Load<Texture>("res://resources/cursors/move_sw_24x24.png");
    private Texture moveWCursor = ResourceLoader.Load<Texture>("res://resources/cursors/move_w_24x24.png");
    private Texture moveNWCursor = ResourceLoader.Load<Texture>("res://resources/cursors/move_nw_24x24.png");

    private int CursorWidth { get => this.moveNCursor.GetWidth(); }
    private int CursorHeight { get => this.moveNCursor.GetHeight(); }

    [Signal] public delegate void ActionSelected(string actionMapping);
    [Signal] public delegate void MousedOverPosition(int x, int y);

    private void ResizeViewport() {
      var viewport = GetNode<Viewport>("EncounterViewport");
      viewport.Size = RectSize;
    }

    public override void _Ready() {
      ResizeViewport();
    }

    public override void _Input(InputEvent @event) {
      if (@event is InputEventMouseButton eventMouseButton) {
        Input.SetMouseMode(Input.MouseMode.Visible);

        // We only want to move on release of left click (think Tangledeep but with mouse) (probably will change this later)
        // Possibly we can add a hold but that runs the risk of you flying right into the middle of an encounter, since movement
        // is so insanely fast.
        if (eventMouseButton.Pressed || eventMouseButton.ButtonIndex != 1) {
          return;
        }

        var viewportRect = this.GetRect();
        var position = eventMouseButton.Position;
        var movementIndicator = GetNode<Sprite>("MovementIndicator");

        if (viewportRect.HasPoint(position)) {
          movementIndicator.Show();
          var dx = position.x - (viewportRect.Size.x / 2);
          var dy = position.y - (viewportRect.Size.y / 2);

          var unitVectorToMouse = new Vector2(dx + CursorWidth / 2, dy + CursorHeight / 2).Normalized();
          var unitYVec = new Vector2(0, -1);
          var deg = Mathf.Rad2Deg(unitYVec.AngleTo(unitVectorToMouse));
          if (deg >= -22.5f && deg <= 22.5f) {
            EmitSignal(nameof(ActionSelected), InputHandler.ActionMapping.MOVE_N);
          } else if (deg >= 22.5f && deg <= 67.5f) {
            EmitSignal(nameof(ActionSelected), InputHandler.ActionMapping.MOVE_NE);
          } else if (deg >= 67.5f && deg <= 112.5f) {
            EmitSignal(nameof(ActionSelected), InputHandler.ActionMapping.MOVE_E);
          } else if (deg >= 112.5f && deg <= 157.5f) {
            EmitSignal(nameof(ActionSelected), InputHandler.ActionMapping.MOVE_SE);
          } else if (deg >= 157.5f || deg <= -157.5f) {
            EmitSignal(nameof(ActionSelected), InputHandler.ActionMapping.MOVE_S);
          } else if (deg >= -157.5f && deg <= -112.5f) {
            EmitSignal(nameof(ActionSelected), InputHandler.ActionMapping.MOVE_SW);
          } else if (deg >= -112.5f && deg <= -67.5f) {
            EmitSignal(nameof(ActionSelected), InputHandler.ActionMapping.MOVE_W);
          } else if (deg >= -67.5f && deg <= -22.5f) {
            EmitSignal(nameof(ActionSelected), InputHandler.ActionMapping.MOVE_NW);
          }
        } else {
          movementIndicator.Hide();
        }
      } else if (@event is InputEventMouseMotion eventMouseMotion) {
        Input.SetMouseMode(Input.MouseMode.Visible);

        var viewportRect = this.GetRect();
        var position = eventMouseMotion.Position;
        var movementIndicator = GetNode<Sprite>("MovementIndicator");

        if (viewportRect.HasPoint(position)) {
          movementIndicator.Show();
          var player = GetTree().GetNodesInGroup(PlayerComponent.ENTITY_GROUP)[0] as Entity;
          var spritePos = player.GetComponent<PositionComponent>().GetNode<Sprite>("Sprite").Position;

          var dx = position.x - (viewportRect.Size.x / 2);
          var dy = position.y - (viewportRect.Size.y / 2);

          // Determine which octant the player's mouse moved over and set the cursor appropriately
          var unitVectorToMouse = new Vector2(dx + CursorWidth / 2, dy + CursorHeight / 2).Normalized();
          var unitYVec = new Vector2(0, -1);
          var deg = Mathf.Rad2Deg(unitYVec.AngleTo(unitVectorToMouse));

          if (deg >= -22.5f && deg <= 22.5f) {
            movementIndicator.Texture = moveNCursor;
            movementIndicator.Position = new Vector2(viewportRect.Size.x / 2, viewportRect.Size.y / 2 - 28);
          } else if (deg >= 22.5f && deg <= 67.5f) {
            movementIndicator.Texture = moveNECursor;
            movementIndicator.Position = new Vector2(viewportRect.Size.x / 2 + 28, viewportRect.Size.y / 2 - 28);
          } else if (deg >= 67.5f && deg <= 112.5f) {
            movementIndicator.Texture = moveECursor;
            movementIndicator.Position = new Vector2(viewportRect.Size.x / 2 + 28, viewportRect.Size.y / 2);
          } else if (deg >= 112.5f && deg <= 157.5f) {
            movementIndicator.Texture = moveSECursor;
            movementIndicator.Position = new Vector2(viewportRect.Size.x / 2 + 28, viewportRect.Size.y / 2 + 28);
          } else if (deg >= 157.5f || deg <= -157.5f) {
            movementIndicator.Texture = moveSCursor;
            movementIndicator.Position = new Vector2(viewportRect.Size.x / 2, viewportRect.Size.y / 2 + 28);
          } else if (deg >= -157.5f && deg <= -112.5f) {
            movementIndicator.Texture = moveSWCursor;
            movementIndicator.Position = new Vector2(viewportRect.Size.x / 2 - 28, viewportRect.Size.y / 2 + 28);
          } else if (deg >= -112.5f && deg <= -67.5f) {
            movementIndicator.Texture = moveWCursor;
            movementIndicator.Position = new Vector2(viewportRect.Size.x / 2 - 28, viewportRect.Size.y / 2);
          } else if (deg >= -67.5f && deg <= -22.5f) {
            movementIndicator.Texture = moveNWCursor;
            movementIndicator.Position = new Vector2(viewportRect.Size.x / 2 - 28, viewportRect.Size.y / 2 - 28);
          }

          var selectedPosition = PositionComponent.VectorToIndex(dx + spritePos.x, dy + spritePos.y);
          EmitSignal(nameof(MousedOverPosition), selectedPosition.X, selectedPosition.Y);
        } else {
          movementIndicator.Hide();
        }
      } else if (@event is InputEventKey) {
        GetNode<Sprite>("MovementIndicator").Hide();
        Input.SetMouseMode(Input.MouseMode.Hidden);
      }
    }

    private void OnEncounterViewportContainerResized() {
      ResizeViewport();
    }
  }
}