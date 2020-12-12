using System.Collections.ObjectModel;
using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.singletons;

namespace SpaceDodgeRL.scenes.encounter {
  public class AutopilotMenu : HBoxContainer {
    private static PackedScene _readoutPrefab = GD.Load<PackedScene>("res://scenes/encounter/AutopilotZoneReadout.tscn");
    private static string ZONE_BUTTON_GROUP = "ZONE_BUTTON_GROUP";

    private Button _closeButton;

    public override void _Ready() {
      _closeButton = GetNode<Button>("SidebarContainer/ButtonConsole/CloseButton");
      _closeButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { null });
      _closeButton.GrabFocus();
      _closeButton.Connect("tree_entered", this, nameof(OnTreeEntered));
    }

    private void ResetZones(EncounterState state) {
      var systemMap = GetNode<Control>("SystemMap");
      var sidebarButtons = GetNode<VBoxContainer>("SidebarContainer/ButtonConsole/DynamicButtonsContainer");

      // Clear all the old zone data
      foreach (Node child in systemMap.GetChildren()) {
        systemMap.RemoveChild(child);
        child.QueueFree();
      }
      foreach (Node child in sidebarButtons.GetChildren()) {
        sidebarButtons.RemoveChild(child);
        child.QueueFree();
      }

      bool hasIntel = state.Player.GetComponent<PlayerComponent>().KnowsIntel(state.DungeonLevel);


      foreach (EncounterZone zone in state.Zones) {
        // Add the system
        var systemButton = new Button();
        systemButton.Text = zone.ZoneName;
        systemButton.AddToGroup(ZONE_BUTTON_GROUP);
        systemButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { zone.ZoneId });
        // TODO: It doesn't scale if you resize the window
        // TODO: Make it look less dumb
        float x1Percentage = (zone.X1 + 1) / (float)state.MapWidth;
        float y1Percentage = (zone.Y1 + 1) / (float)state.MapHeight;
        float scaledX1 = systemMap.RectSize.x * x1Percentage;
        float scaledY1 = systemMap.RectSize.y * y1Percentage;

        systemButton.RectPosition = new Vector2(scaledX1, scaledY1);

        float widthPercentage = zone.Width / (float)state.MapWidth;
        float heightPercentage = zone.Height / (float)state.MapHeight;
        float scaledWidth = systemMap.RectSize.x * widthPercentage;
        float scaledHeight = systemMap.RectSize.y * heightPercentage;

        systemButton.RectSize = new Vector2(scaledWidth, scaledHeight);

        systemMap.AddChild(systemButton);

        // Add the sidebar
        var sidebarReadout = _readoutPrefab.Instance() as AutopilotZoneReadout;
        sidebarReadout.SetReadout(state, zone, hasIntel);
        sidebarReadout.AutopilotButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array() { zone.ZoneId });
        sidebarButtons.AddChild(sidebarReadout);
      }
    }

    public void PrepMenu(EncounterState state) {
      ResetZones(state);

      // TODO: Add You Are Here onto the starmap too!
      // You Are Here label
      var playerPosition = state.Player.GetComponent<PositionComponent>().EncounterPosition;
      EncounterZone closestZone = state.ClosestZone(playerPosition.X, playerPosition.Y);
      var youAreHereLabel = GetNode<RichTextLabel>("SidebarContainer/YouAreHereLabel");
      if (closestZone.Contains(playerPosition.X, playerPosition.Y)) {
        youAreHereLabel.BbcodeText = string.Format("You are in [b]Sector {0}[/b], [b]{1}[/b]", state.DungeonLevel, closestZone.ZoneName);
      } else {
        youAreHereLabel.BbcodeText = string.Format("You are in [b]Sector {0}[/b], near [b]{1}[/b]", state.DungeonLevel, closestZone.ZoneName);
      }
    }

    private void OnTreeEntered() {
      _closeButton.GrabFocus();
    }

    public override void _UnhandledKeyInput(InputEventKey @event) {
      if (@event.IsActionPressed("ui_cancel", true)) {
        OnButtonPressed(null);
        return;
      }
    }

    private void OnButtonPressed(string zoneId) {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.CloseAutopilotMenu(zoneId);
    }
  }
}