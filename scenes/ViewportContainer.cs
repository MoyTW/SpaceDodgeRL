using Godot;
using System;

public class ViewportContainer : Godot.ViewportContainer {
  private void ResizeViewport() {
    var viewport = this.GetNode<Viewport>("EncounterViewport");
    viewport.Size = this.RectSize;
  }

  public override void _Ready() {
    ResizeViewport();
  }

  private void OnViewportContainerResized() {
    ResizeViewport();
  }
}
