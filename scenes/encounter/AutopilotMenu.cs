using System.Collections.ObjectModel;
using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter.state;

public class AutopilotMenu : HBoxContainer {

  private static string ZONE_BUTTON_GROUP = "ZONE_BUTTON_GROUP";

  private EncounterState _state;
  private Button _closeButton;

  public override void _Ready() {
    _closeButton = this.GetNode<Button>("SidebarContainer/ButtonConsole/CloseButton");
    _closeButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { null });
    _closeButton.GrabFocus();
    _closeButton.Connect("tree_entered", this, nameof(OnTreeEntered));
  }

  private void ResetZones(ReadOnlyCollection<EncounterZone> zones, int mapWidth, int mapHeight) {
    // TODO: Put a You Are Here in!
    // For the system map
    var systemMap = this.GetNode<Control>("SystemMap");

    foreach(Node child in systemMap.GetChildren()) {
      systemMap.RemoveChild(child);
      child.QueueFree();
    }

    foreach (EncounterZone zone in zones) {
      var button = new Button();
      button.AddToGroup(AutopilotMenu.ZONE_BUTTON_GROUP);
      button.Text = zone.Name;
      button.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { zone });
      // TODO: Also scale the width/height of the zone
      // TODO: It doesn't scale if you resize the window
      // TODO: Make it look less dumb
      float x1Percentage = (zone.X1 + 1) / (float)mapWidth;
      float y1Percentage = (zone.Y1 + 1) / (float)mapHeight;
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
    foreach (EncounterZone zone in zones) {
      var button = new Button();
      button.Text = zone.Name;
      button.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { zone });
      buttonContainer.AddChild(button);
    }
  }

  public void PrepMenu(EncounterState state) {
    bool isNewState = this._state != state;
    if (isNewState) {
      this._state = state;
      ResetZones(state.Zones, state.MapWidth, state.MapHeight);
    }
    
    // You Are Here label
    var playerPosition = state.Player.GetComponent<PositionComponent>().EncounterPosition;
    EncounterZone closestZone = null;
    float smallestDistance = float.MaxValue;
    foreach (EncounterZone zone in state.Zones) {
      var distance = zone.Center.DistanceTo(playerPosition);
      if (distance < smallestDistance) {
        smallestDistance = distance;
        closestZone = zone;
      }
    }
    var youAreHereLabel = this.GetNode<RichTextLabel>("SidebarContainer/YouAreHereLabel");
    youAreHereLabel.BbcodeText = string.Format("You are currently near [b]{0}[/b]", closestZone.Name);
  }

  private void OnTreeEntered() {
    _closeButton.GrabFocus();
  }

  private void OnButtonPressed(EncounterZone zone) {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.CloseAutopilotMenu(zone);
  }
}
