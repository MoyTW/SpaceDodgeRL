using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes {
  public class EncounterViewportContainer : Godot.ViewportContainer {
    private Resource scanningCursor = ResourceLoader.Load("res://resources/scanning_18x18.png");

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
          Input.SetCustomMouseCursor(scanningCursor);

          var player = GetTree().GetNodesInGroup(PlayerComponent.ENTITY_GROUP)[0] as Entity;
          var spritePos = player.GetComponent<PositionComponent>().GetNode<Sprite>("Sprite").Position;
          var dx = position.x - (viewportRect.Size.x / 2);
          var dy = position.y - (viewportRect.Size.y / 2);
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