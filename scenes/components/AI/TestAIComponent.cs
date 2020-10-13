using Godot;
using System;

public class TestAIComponent : Node, AIComponent {
  // TODO: sight-lines & group activation
  public bool IsActive => true;

  public void DecideNextAction(EncounterState state) {
    GD.Print("Entity is waiting!");
  }
}
