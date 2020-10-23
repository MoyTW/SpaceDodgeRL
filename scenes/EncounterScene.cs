using Godot;
using SpaceDodgeRL.scenes.encounter;

namespace SpaceDodgeRL.scenes {

  public class EncounterScene : Container {
    EncounterState encounterState;
    RichTextLabel encounterLogLabel;

    public override void _Ready() {
      encounterState = GetNode<EncounterState>("VBoxContainer/ViewportContainer/EncounterViewport/EncounterState");
      encounterLogLabel = GetNode<RichTextLabel>("VBoxContainer/HBoxContainer/EncounterLogLabel");

      var encounterRunner = GetNode<EncounterRunner>("EncounterRunner");
      encounterRunner.inputHandlerRef = GetNode<InputHandler>("InputHandler");
      encounterRunner.SetEncounterState(encounterState);

      // Hook up the UI
      encounterState.Connect("EncounterLogMessageAdded", this, "OnEncounterLogMessageAdded");

      // TODO: Proper state initialization & building & such!
      encounterState.InitState(20, 20);
    }

    // TODO: Decide if this is better placed directly onto the log label
    private void OnEncounterLogMessageAdded(string bbCodeMessage, int encounterLogSize) {
      if (encounterLogLabel.GetLineCount() > encounterLogSize) {
        encounterLogLabel.RemoveLine(0);
      }
      encounterLogLabel.AppendBbcode(bbCodeMessage + "\n");
    }
  }
}