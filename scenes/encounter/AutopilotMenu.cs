using System.Collections.ObjectModel;
using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;

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
    var systemMap = this.GetNode<Control>("SystemMap");
    var sidebarContainer = this.GetNode<VBoxContainer>("SidebarContainer/ButtonConsole/DynamicButtonsContainer");

    // Clear all the old zone data
    foreach(Node child in systemMap.GetChildren()) {
      systemMap.RemoveChild(child);
      child.QueueFree();
    }
    foreach (Node child in sidebarContainer.GetChildren()) {
      sidebarContainer.RemoveChild(child);
      child.QueueFree();
    }

    foreach (EncounterZone zone in zones) {
      // Add the system
      var systemButton = new Button();
      systemButton.Text = zone.ZoneName;
      systemButton.AddToGroup(AutopilotMenu.ZONE_BUTTON_GROUP);
      systemButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { zone });
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
      var sidebarButton = new Button();
      sidebarButton.Text = zone.ZoneName;
      sidebarButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { zone });
      sidebarContainer.AddChild(sidebarButton);
    }
  }

  public void PrepMenu(EncounterState state) {
    bool isNewState = this._state != state;
    if (isNewState) {
      this._state = state;
      ResetZones(state.Zones, state.MapWidth, state.MapHeight);
    }
    
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

    // TODO: Put in a UI to display this onscreen
    if (state.Player.GetComponent<PlayerComponent>().KnowsIntel(state.DungeonLevel)) {
      GD.Print("##### INTEL READOUT FOR " + state.DungeonLevel + " #####");
      foreach (EncounterZone zone in state.Zones) {
        var items = "items=";
        foreach (Entity e in zone.ReadoutItems) { items += " " + e.EntityName; }
        var features = "features=";
        foreach (Entity e in zone.ReadoutFeatures) { features += " " + e.EntityName; }
        GD.Print(zone.ZoneName + " : " + zone.ReadoutEncounterName + " : " + items + " : " + features);
      }
    } else {
      GD.Print("##### PLAYER DOES NOT KNOW INTEL FOR LEVEL " + state.DungeonLevel + " #####");
    }
  }

  private void OnTreeEntered() {
    _closeButton.GrabFocus();
  }

  private void OnButtonPressed(EncounterZone zone) {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.CloseAutopilotMenu(zone);
  }
}
