using System.Collections.ObjectModel;
using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;

public class AutopilotMenu : HBoxContainer {
  private static PackedScene _readoutPrefab = GD.Load<PackedScene>("res://scenes/encounter/AutopilotZoneReadout.tscn");
  private static string ZONE_BUTTON_GROUP = "ZONE_BUTTON_GROUP";

  private Button _closeButton;

  public override void _Ready() {
    _closeButton = this.GetNode<Button>("SidebarContainer/ButtonConsole/CloseButton");
    _closeButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { null });
    _closeButton.GrabFocus();
    _closeButton.Connect("tree_entered", this, nameof(OnTreeEntered));
  }

  private void ResetZones(ReadOnlyCollection<EncounterZone> zones, int mapWidth, int mapHeight, bool hasIntel) {
    var systemMap = this.GetNode<Control>("SystemMap");
    var sidebarButtons = this.GetNode<VBoxContainer>("SidebarContainer/ButtonConsole/DynamicButtonsContainer");

    // Clear all the old zone data
    foreach(Node child in systemMap.GetChildren()) {
      systemMap.RemoveChild(child);
      child.QueueFree();
    }
    foreach (Node child in sidebarButtons.GetChildren()) {
      sidebarButtons.RemoveChild(child);
      child.QueueFree();
    }

    foreach (EncounterZone zone in zones) {
      // Add the system
      var systemButton = new Button();
      systemButton.Text = zone.ZoneName;
      systemButton.AddToGroup(AutopilotMenu.ZONE_BUTTON_GROUP);
      systemButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { zone.ZoneId });
      // TODO: It doesn't scale if you resize the window
      // TODO: Make it look less dumb
      float x1Percentage = (zone.X1 + 1) / (float)mapWidth;
      float y1Percentage = (zone.Y1 + 1) / (float)mapHeight;
      float scaledX1 = systemMap.RectSize.x * x1Percentage;
      float scaledY1 = systemMap.RectSize.y * y1Percentage;

      systemButton.RectPosition = new Vector2(scaledX1, scaledY1);

      float widthPercentage = (zone.Width) / (float)mapWidth;
      float heightPercentage = (zone.Height) / (float)mapHeight;
      float scaledWidth = systemMap.RectSize.x * widthPercentage;
      float scaledHeight = systemMap.RectSize.y * heightPercentage;

      systemButton.RectSize = new Vector2(scaledWidth, scaledHeight);

      systemMap.AddChild(systemButton);

      // Add the sidebar
      var sidebarReadout = _readoutPrefab.Instance() as AutopilotZoneReadout;
      sidebarReadout.SetReadout(zone, hasIntel);
      sidebarReadout.AutopilotButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { zone.ZoneId });
      sidebarButtons.AddChild(sidebarReadout);
    }
  }

  public void PrepMenu(EncounterState state) {
    bool hasIntel = state.Player.GetComponent<PlayerComponent>().KnowsIntel(state.DungeonLevel);
    ResetZones(state.Zones, state.MapWidth, state.MapHeight, hasIntel);
    
    // TODO: Add You Are Here onto the starmap too!
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
    youAreHereLabel.BbcodeText = string.Format("You are currently near [b]{0}[/b]", closestZone.ZoneName);
  }

  private void OnTreeEntered() {
    _closeButton.GrabFocus();
  }

  private void OnButtonPressed(string zoneId) {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.CloseAutopilotMenu(zoneId);
  }
}
