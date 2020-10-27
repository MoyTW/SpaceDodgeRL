using Godot;
using SpaceDodgeRL.scenes.encounter;

namespace SpaceDodgeRL.scenes {

  public class EncounterScene : Container {
    EncounterState encounterState;
    EncounterRunner encounterRunner;
    RichTextLabel encounterLogLabel;

    public override void _Ready() {
      encounterState = GetNode<EncounterState>("VBoxContainer/ViewportContainer/EncounterViewport/EncounterState");
      encounterLogLabel = GetNode<RichTextLabel>("VBoxContainer/HBoxContainer/EncounterLogLabel");

      encounterRunner = GetNode<EncounterRunner>("EncounterRunner");
      encounterRunner.inputHandlerRef = GetNode<InputHandler>("InputHandler");
      encounterRunner.SetEncounterState(encounterState);

      // Hook up the UI
      encounterState.Connect("EncounterLogMessageAdded", this, "OnEncounterLogMessageAdded");

      // TODO: Proper state initialization & building & such!
      encounterState.InitState();
    }

    // TODO: Decide if this is better placed directly onto the log label
    private void OnEncounterLogMessageAdded(string bbCodeMessage, int encounterLogSize) {
      if (encounterLogLabel.GetLineCount() > encounterLogSize) {
        encounterLogLabel.RemoveLine(0);
      }
      encounterLogLabel.AppendBbcode(bbCodeMessage + "\n");
    }

    public void HandleAutopilotMenuClosed(EncounterZone selectedZone) {
      if (selectedZone != null) {
        encounterRunner.HandleAutopilotSelection(selectedZone);
      }
    }
  }
}