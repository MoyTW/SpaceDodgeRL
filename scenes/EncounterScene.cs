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
    private RichTextLabel encounterLogLabel;

    public override void _Ready() {
      encounterViewport = GetNode<Viewport>("VBoxContainer/ViewportContainer/EncounterViewport");
      encounterLogLabel = GetNode<RichTextLabel>("VBoxContainer/HBoxContainer/EncounterLogLabel");

      this.EncounterState = EncounterState.Create();
      encounterViewport.AddChild(this.EncounterState);

      encounterRunner = GetNode<EncounterRunner>("EncounterRunner");
      encounterRunner.inputHandlerRef = GetNode<InputHandler>("InputHandler");
      encounterRunner.SetEncounterState(EncounterState);

      // Hook up the UI
      EncounterState.Connect("EncounterLogMessageAdded", this, "OnEncounterLogMessageAdded");

      var player = EntityBuilder.CreatePlayerEntity(0);
      this.EncounterState.ResetStateForNewLevel(player, 1);

      // TODO: Proper testing of saving/loading
      this.EncounterState.QueueFree();
      encounterViewport.RemoveChild(this.EncounterState);
      var asString = this.EncounterState.ToSaveData();
      this.EncounterState = EncounterState.FromSaveData(asString);
      this.encounterViewport.AddChild(this.EncounterState);
      GD.Print(GetTree().GetNodesInGroup("ENCOUNTER_CAMERA_GROUP"));
      this.encounterRunner.SetEncounterState(this.EncounterState);
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