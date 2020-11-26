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
    private EncounterRunner encounterRunner;
    // TODO: Encounter log sometimes stops updating - unsure why, I think it has to do with not cycling properly when you get to
    // max number of messages
    private RichTextLabel encounterLogLabel;

    public override void _Ready() {
      encounterViewport = GetNode<Viewport>("VBoxContainer/ViewportContainer/EncounterViewport");
      encounterLogLabel = GetNode<RichTextLabel>("VBoxContainer/HBoxContainer/EncounterLogLabel");

      encounterRunner = GetNode<EncounterRunner>("EncounterRunner");
      encounterRunner.inputHandlerRef = GetNode<InputHandler>("InputHandler");

      if (this.EncounterState == null) {
        throw new NotImplementedException("must call SetEncounterState before adding to tree");
      }
      this.encounterViewport.AddChild(this.EncounterState);
      this.encounterRunner.SetEncounterState(this.EncounterState);
      // Hook up the UI
      this.EncounterState.Connect("EncounterLogMessageAdded", this, "OnEncounterLogMessageAdded");
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