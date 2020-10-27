using System.Collections.ObjectModel;
using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.encounter;

public class AutopilotMenu : HBoxContainer {
  public class ActionMapping {
    public static string UI_ACCEPT = "ui_accept";
    public static string UI_CANCEL = "ui_cancel";
    public static string UI_UP = "ui_up";
    public static string UI_DOWN = "ui_down";
  }

  private ReadOnlyCollection<EncounterZone> _zones;
  public ReadOnlyCollection<EncounterZone> Zones {
    get => this._zones;
    set {
      var buttonContainer = this.GetNode<VBoxContainer>("SidebarContainer/DynamicButtonsContainer");

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

  private void OnButtonPressed(EncounterZone zone) {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.CloseAutopilotMenu(zone);
  }

  public override void _UnhandledKeyInput(InputEventKey @event) {
    if (@event.IsActionPressed(ActionMapping.UI_ACCEPT, true)) {
      GD.Print("User accepted");
    } else if (@event.IsActionPressed(ActionMapping.UI_CANCEL, true)) {
      GD.Print("lol");
    } else if (@event.IsActionPressed(ActionMapping.UI_UP, true)) {
      GD.Print("go up");
    } else if (@event.IsActionPressed(ActionMapping.UI_DOWN, true)) {
      GD.Print("go down");
    }
  }
}
