using Godot;
using SpaceDodgeRL.scenes.encounter;

namespace SpaceDodgeRL.scenes {

  public class EncounterScene : Container {

    public override void _Ready() {
      // TODO: Proper state initialization & building & such!
      var encounterState = GetNode<EncounterState>("VBoxContainer/ViewportContainer/EncounterViewport/EncounterState");
      encounterState.InitState();

      var encounterRunner = GetNode<EncounterRunner>("EncounterRunner");
      encounterRunner.inputHandlerRef = GetNode<InputHandler>("InputHandler");
      encounterRunner.SetEncounterState(encounterState);
    }
  }
}