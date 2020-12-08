using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;
using System;

namespace SpaceDodgeRL.scenes {
  public class ViewportContainer : Godot.ViewportContainer {
    private void ResizeViewport() {
      var viewport = GetNode<Viewport>("EncounterViewport");
      viewport.Size = RectSize;
    }

    public override void _Ready() {
      ResizeViewport();
    }

    public override void _Input(InputEvent @event) {
      if (@event is InputEventMouseMotion eventMouseMotion) {
        var viewportRect = this.GetRect();
        var position = eventMouseMotion.Position;

        if (viewportRect.HasPoint(position)) {
          var player = GetTree().GetNodesInGroup(PlayerComponent.ENTITY_GROUP)[0] as Entity;
          var spritePos = player.GetComponent<PositionComponent>().GetNode<Sprite>("Sprite").Position;
          var dx = position.x - (viewportRect.Size.x / 2);
          var dy = position.y - (viewportRect.Size.y / 2);
          var selectedPosition = PositionComponent.VectorToIndex(dx + spritePos.x, dy + spritePos.y);
          GD.Print(string.Format("Player has moused over square {0}", selectedPosition));
        }
      }
    }

    private void OnViewportContainerResized() {
      ResizeViewport();
    }
  }
}