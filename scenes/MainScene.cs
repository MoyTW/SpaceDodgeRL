using Godot;
using SpaceDodgeRL.scenes.encounter;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes {

  public class MainScene : Node2D {
    public override void _Ready() {
      // TODO: Proper state initialization & building & such!
      var encounterState = GetNode<EncounterState>("EncounterState");
      encounterState.InitState(GetNode<EntityBuilder>("EntityBuilder"));

      var encounterRunner = GetNode<EncounterRunner>("EncounterRunner");
      encounterRunner.inputHandlerRef = GetNode<InputHandler>("InputHandler");
      encounterRunner.SetEncounterState(encounterState);
    }
  }
}