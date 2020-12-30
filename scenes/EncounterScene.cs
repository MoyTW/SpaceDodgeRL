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

    public override void _Ready() {
      this.encounterViewport = GetNode<Viewport>("SceneFrame/SceneVBox/EncounterViewportContainer/EncounterViewport");

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
      var viewportContainer = GetNode<ViewportContainer>("SceneFrame/SceneVBox/EncounterViewportContainer");
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
      this.GetNode<MenuButtonBar>("SceneFrame/SceneVBox/MenuButtonBar").SetState(state);
    }

    private void OnEncounterLogMessageAdded(string bbCodeMessage, int encounterLogSize) {
      this.GetNode<SidebarDisplay>("SceneFrame/SidebarDisplay").AddEncounterLogMessage(bbCodeMessage, encounterLogSize);
    }

    private void OnTurnEnded() {
      this.GetNode<SidebarDisplay>("SceneFrame/SidebarDisplay").RefreshStats(this.EncounterState);
    }

    private void OnPositionScanned(int x, int y, Entity entity) {
      this.GetNode<SidebarDisplay>("SceneFrame/SidebarDisplay").DisplayScannedEntity(x, y, entity);
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