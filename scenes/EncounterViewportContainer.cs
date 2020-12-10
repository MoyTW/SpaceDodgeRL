using System.Collections.Generic;
using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes {
  public class EncounterViewportContainer : Godot.ViewportContainer {
    private Resource moveNCursor = ResourceLoader.Load("res://resources/cursors/move_n_24x24.png");
    private Resource moveNECursor = ResourceLoader.Load("res://resources/cursors/move_ne_24x24.png");
    private Resource moveECursor = ResourceLoader.Load("res://resources/cursors/move_e_24x24.png");
    private Resource moveSECursor = ResourceLoader.Load("res://resources/cursors/move_se_24x24.png");
    private Resource moveSCursor = ResourceLoader.Load("res://resources/cursors/move_s_24x24.png");
    private Resource moveSWCursor = ResourceLoader.Load("res://resources/cursors/move_sw_24x24.png");
    private Resource moveWCursor = ResourceLoader.Load("res://resources/cursors/move_w_24x24.png");
    private Resource moveNWCursor = ResourceLoader.Load("res://resources/cursors/move_nw_24x24.png");

    [Signal] public delegate void MousedOverPosition(int x, int y);

    private void ResizeViewport() {
      var viewport = GetNode<Viewport>("EncounterViewport");
      viewport.Size = RectSize;
    }

    public override void _Ready() {
      ResizeViewport();
    }

    public override void _Input(InputEvent @event) {
      if (@event is InputEventMouseMotion eventMouseMotion) {
        Input.SetMouseMode(Input.MouseMode.Visible);

        var viewportRect = this.GetRect();
        var position = eventMouseMotion.Position;

        if (viewportRect.HasPoint(position)) {
          var player = GetTree().GetNodesInGroup(PlayerComponent.ENTITY_GROUP)[0] as Entity;
          var spritePos = player.GetComponent<PositionComponent>().GetNode<Sprite>("Sprite").Position;

          var dx = position.x - (viewportRect.Size.x / 2);
          var dy = position.y - (viewportRect.Size.y / 2);

          // Determine which octant the player's mouse moved over and set the cursor appropriately
          // TODO: The cursor's position is the upper-left corner of the image, which is perfectly sensible with the default
          // cursor but also not really what I'd like!
          var unitVectorToMouse = new Vector2(dx, dy).Normalized();
          var unitYVec = new Vector2(0, -1);
          var deg = Mathf.Rad2Deg(unitYVec.AngleTo(unitVectorToMouse));
          if (deg >= -22.5f && deg <= 22.5f) {
            Input.SetCustomMouseCursor(moveNCursor);
          } else if (deg >= 22.5f && deg <= 67.5f) {
            Input.SetCustomMouseCursor(moveNECursor);
          } else if (deg >= 67.5f && deg <= 112.5f) {
            Input.SetCustomMouseCursor(moveECursor);
          } else if (deg >= 112.5f && deg <= 157.5f) {
            Input.SetCustomMouseCursor(moveSECursor);
          } else if (deg >= 157.5f || deg <= -157.5f) {
            Input.SetCustomMouseCursor(moveSCursor);
          } else if (deg >= -157.5f && deg <= -112.5f) {
            Input.SetCustomMouseCursor(moveSWCursor);
          } else if (deg >= -112.5f && deg <= -67.5f) {
            Input.SetCustomMouseCursor(moveWCursor);
          } else if (deg >= -67.5f && deg <= -22.5f) {
            Input.SetCustomMouseCursor(moveNWCursor);
          }
          GD.Print(unitVectorToMouse, deg);
          // GD.Print("X: ", unitXVec.Dot(unitVectorToMouse), " Y: ", unitYVec.Dot(unitVectorToMouse));

          var selectedPosition = PositionComponent.VectorToIndex(dx + spritePos.x, dy + spritePos.y);
          EmitSignal(nameof(MousedOverPosition), selectedPosition.X, selectedPosition.Y);
        } else {
          Input.SetCustomMouseCursor(null);
        }
      } else if (@event is InputEventKey) {
        Input.SetMouseMode(Input.MouseMode.Hidden);
      }
    }

    private void OnEncounterViewportContainerResized() {
      ResizeViewport();
    }
  }
}