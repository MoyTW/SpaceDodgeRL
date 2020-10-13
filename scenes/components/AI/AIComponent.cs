using Godot;
using System;

public interface AIComponent {
  bool IsActive { get; }
  // TODO: Return a list of actions
  void DecideNextAction(EncounterState state);
}
