using System;
using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes {

  public class EncounterScene : Container {
    public EncounterState EncounterState { get; private set; }
    private Viewport encounterViewport;
    public InputHandler inputHandler;
    private EncounterRunner encounterRunner;
    private RichTextLabel encounterLogLabel;

    public override void _Ready() {
      this.encounterViewport = GetNode<Viewport>("SceneFrame/EncounterViewportContainer/EncounterViewport");
      this.encounterLogLabel = GetNode<RichTextLabel>("SceneFrame/BottomUIContainer/EncounterLogLabel");

      this.inputHandler = GetNode<InputHandler>("InputHandler");

      this.encounterRunner = GetNode<EncounterRunner>("EncounterRunner");
      this.encounterRunner.inputHandlerRef = this.inputHandler;

      if (this.EncounterState == null) {
        throw new NotImplementedException("must call SetEncounterState before adding to tree");
      }
      this.encounterViewport.AddChild(this.EncounterState);
      this.encounterRunner.SetEncounterState(this.EncounterState);

      // Hook up the UI
      this.EncounterState.Connect(nameof(EncounterState.EncounterLogMessageAdded), this, nameof(OnEncounterLogMessageAdded));
      this.encounterRunner.Connect(nameof(EncounterRunner.TurnEnded), this, nameof(OnTurnEnded));
      // TODO: Add keyboard look via "s"
      // TODO: Add button to pick up item (for full mouse compatibility)
      // TODO: Add buttons to go to all the various screens (for full mouse compatibility)
      this.encounterRunner.Connect(nameof(EncounterRunner.PositionScanned), this, nameof(OnPositionScanned));
      var viewportContainer = GetNode<ViewportContainer>("SceneFrame/EncounterViewportContainer");
      viewportContainer.Connect(nameof(EncounterViewportContainer.MousedOverPosition), this, nameof(OnMousedOverPosition));
      viewportContainer.Connect(nameof(EncounterViewportContainer.ActionSelected), this, nameof(OnActionSelected));
      // Since we can't have the state broadcast its events before we connect, we instead pull log messages; this will be empty
      // on new game and populated on load.
      foreach (var logMessage in this.EncounterState.EncounterLog) {
        this.OnEncounterLogMessageAdded(logMessage, int.MaxValue);
      }

      OnTurnEnded();
    }

    /**
     * Must be called once and only once, before being added to the scene tree.
     */
    public void SetEncounterState(EncounterState state) {
      if (this.EncounterState != null) {
        throw new NotImplementedException("can't call SetEncounterState twice");
      }

      this.EncounterState = state;
    }

    // TODO: Decide if this is better placed directly onto the log label
    private void OnEncounterLogMessageAdded(string bbCodeMessage, int encounterLogSize) {
      if (encounterLogLabel.GetLineCount() > encounterLogSize) {
        encounterLogLabel.RemoveLine(0);
      }
      encounterLogLabel.AppendBbcode(bbCodeMessage + "\n");
      encounterLogLabel.Update();
    }

    private void OnTurnEnded() {
      var player = this.EncounterState.Player;

      // Left column
      var playerDefenderComponent = player.GetComponent<DefenderComponent>();
      var newHPText = String.Format("HP: {0}/{1}", playerDefenderComponent.CurrentHp, playerDefenderComponent.MaxHp);
      GetNode<Label>("SceneFrame/BottomUIContainer/StatsHBox/StatsLeftColumn/YourStatsBlock/HPLabel").Text = newHPText;

      var playerComponent = player.GetComponent<PlayerComponent>();
      var newAttackPowerText = String.Format("Cutting laser power: {0}", playerComponent.CuttingLaserPower);
      GetNode<Label>("SceneFrame/BottomUIContainer/StatsHBox/StatsLeftColumn/YourStatsBlock/AttackPowerLabel").Text = newAttackPowerText;

      var speedComponent = player.GetComponent<SpeedComponent>();
      var newSpeedText = String.Format("Speed: {0}", speedComponent.Speed);
      GetNode<Label>("SceneFrame/BottomUIContainer/StatsHBox/StatsLeftColumn/YourStatsBlock/SpeedLabel").Text = newSpeedText;

      var invComponent = player.GetComponent<InventoryComponent>();
      var newInvText = string.Format("Inventory Space: {0}/{1}", invComponent.InventoryUsed, invComponent.InventorySize);
      GetNode<Label>("SceneFrame/BottomUIContainer/StatsHBox/StatsLeftColumn/YourStatsBlock/InventoryLabel").Text = newInvText;

      // Right column
      var playerPos = player.GetComponent<PositionComponent>().EncounterPosition;

      var newSectorZoneText = string.Format("Sector: {0}", this.EncounterState.DungeonLevel);
      var closestZone = this.EncounterState.ClosestZone(playerPos.X, playerPos.Y);
      if (closestZone.Contains(playerPos)) {
        newSectorZoneText += string.Format(" {0}", closestZone.ZoneName);
      }
      GetNode<Label>("SceneFrame/BottomUIContainer/StatsHBox/StatsRightColumn/SectorZoneLabel").Text = newSectorZoneText;

      string newPositionZoneText = string.Format("Position: ({0}, {1})", playerPos.X, playerPos.Y);
      GetNode<Label>("SceneFrame/BottomUIContainer/StatsHBox/StatsRightColumn/PositionLabel").Text = newPositionZoneText;

      var xpComponent = player.GetComponent<XPTrackerComponent>();
      var newLevelText = string.Format("Level: {0}", xpComponent.Level);
      GetNode<Label>("SceneFrame/BottomUIContainer/StatsHBox/StatsRightColumn/LevelLabel").Text = newLevelText;
      var newXPText = string.Format("Experience: {0}/{1}", xpComponent.XP, xpComponent.NextLevelAtXP);
      GetNode<Label>("SceneFrame/BottomUIContainer/StatsHBox/StatsRightColumn/ExperienceLabel").Text = newXPText;

      var posComponent = player.GetComponent<PositionComponent>();
    }

    private void OnPositionScanned(int x, int y, Entity entity) {
      if (entity != null) {
        // I think this is a sign we should break the scene up.
        var scanTextureRect = GetNode<TextureRect>("SceneFrame/BottomUIContainer/StatsHBox/StatsLeftColumn/ScanBlock/ReadoutTextureRect");
        scanTextureRect.Texture = entity.GetComponent<PositionComponent>().SpriteTexture;
        var descriptionLabel = GetNode<RichTextLabel>("SceneFrame/BottomUIContainer/StatsHBox/StatsLeftColumn/ScanBlock/DescriptionLabel");
        descriptionLabel.BbcodeText = entity.GetComponent<DisplayComponent>().Description;
      }
    }

    private void OnMousedOverPosition(int x, int y) {
      this.inputHandler.TryInsertInputAction(new InputHandler.ScanInputAction(x, y));
    }

    private void OnActionSelected(string actionMapping) {
      this.inputHandler.TryInsertInputAction(new InputHandler.InputAction(actionMapping));
    }

    // This could probably be a signal.
    public void HandleAutopilotMenuClosed(string selectedZoneId) {
      if (selectedZoneId != null) {
        encounterRunner.HandleAutopilotSelection(selectedZoneId);
      }
    }

    // TODO: The many layers of indirection for these menus are vexing but feature-complete first
    public void HandleItemToUseSelected(string itemIdToUse) {
      encounterRunner.HandleUseItemSelection(itemIdToUse);
    }

    // This could probably be a signal.
    public void HandleLevelUpSelected(Entity entity, string levelUpSelection) {
      EncounterState.Player.GetComponent<XPTrackerComponent>().RegisterLevelUpChoice(entity, levelUpSelection);
    }
  }
}