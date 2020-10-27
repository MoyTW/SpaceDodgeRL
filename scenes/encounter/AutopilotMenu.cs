using System.Collections.ObjectModel;
using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.encounter;

public class AutopilotMenu : HBoxContainer {

  private ReadOnlyCollection<EncounterZone> _zones;
  public ReadOnlyCollection<EncounterZone> Zones {
    get => this._zones;
    // TODO: It's unintuitive to have essentially "reset the state of the scene" in the setter.
    set {
      // TODO: Put a You Are Here in!
      // For the system map
      var systemMap = this.GetNode<Control>("SystemMap");

      foreach(Node child in systemMap.GetChildren()) {
        systemMap.RemoveChild(child);
        child.QueueFree();
      }

      foreach (EncounterZone zone in value) {
        var button = new Button();
        button.Text = zone.Name;
        button.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { zone });
        // TODO: Also scale the width/height of the zone
        // TODO: Make it look less dumb
        // TODO: Don't hardcode width/height
        float x1Percentage = (zone.X1 + 1) / 300f;
        float y1Percentage = (zone.Y1 + 1) / 300f;
        float scaledX1 = systemMap.RectSize.x * x1Percentage;
        float scaledY1 = systemMap.RectSize.y * y1Percentage;

        button.RectPosition = new Vector2(scaledX1, scaledY1);

        systemMap.AddChild(button);
      }


      // For the sidebar buttons
      var buttonContainer = this.GetNode<VBoxContainer>("SidebarContainer/ButtonConsole/DynamicButtonsContainer");

      // Remove old nodes
      foreach (Node child in buttonContainer.GetChildren()) {
        buttonContainer.RemoveChild(child);
        child.QueueFree();
      }

      // Add new nodes
      foreach (EncounterZone zone in value) {
        var button = new Button();
        button.Text = zone.Name;
        button.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { zone });
        buttonContainer.AddChild(button);
      }

      this._zones = value;
    }
  }

  private Button _closeButton;

  public override void _Ready() {
    _closeButton = this.GetNode<Button>("SidebarContainer/ButtonConsole/CloseButton");
    _closeButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { null });
    _closeButton.GrabFocus();
    _closeButton.Connect("tree_entered", this, nameof(OnTreeEntered));
  }

  private void OnTreeEntered() {
    _closeButton.GrabFocus();
  }

  private void OnButtonPressed(EncounterZone zone) {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.CloseAutopilotMenu(zone);
  }
}
