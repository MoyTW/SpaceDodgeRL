using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes {

  public class EncounterScene : Container {
    public EncounterState EncounterState { get; private set; }
    private EncounterRunner encounterRunner;
    private RichTextLabel encounterLogLabel;

    public override void _Ready() {
      EncounterState = GetNode<EncounterState>("VBoxContainer/ViewportContainer/EncounterViewport/EncounterState");
      encounterLogLabel = GetNode<RichTextLabel>("VBoxContainer/HBoxContainer/EncounterLogLabel");

      encounterRunner = GetNode<EncounterRunner>("EncounterRunner");
      encounterRunner.inputHandlerRef = GetNode<InputHandler>("InputHandler");
      encounterRunner.SetEncounterState(EncounterState);

      // Hook up the UI
      EncounterState.Connect("EncounterLogMessageAdded", this, "OnEncounterLogMessageAdded");

      // TODO: Proper state initialization & building & such!
      var player = EntityBuilder.CreatePlayerEntity(0);
      EncounterState.InitState(player, 1);
    }

    // TODO: Decide if this is better placed directly onto the log label
    private void OnEncounterLogMessageAdded(string bbCodeMessage, int encounterLogSize) {
      if (encounterLogLabel.GetLineCount() > encounterLogSize) {
        encounterLogLabel.RemoveLine(0);
      }
      encounterLogLabel.AppendBbcode(bbCodeMessage + "\n");
    }

    // This could probably be a signal.
    public void HandleAutopilotMenuClosed(EncounterZone selectedZone) {
      if (selectedZone != null) {
        encounterRunner.HandleAutopilotSelection(selectedZone);
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