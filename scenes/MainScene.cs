using Godot;
using System;

public class MainScene : Node2D {
  public override void _Ready() {
    var encounterState = GetNode<EncounterState>("EncounterState");

    encounterState.entityBuilderRef = GetNode<EntityBuilder>("EntityBuilder");
    encounterState.inputHandlerRef = GetNode<InputHandler>("InputHandler");
  }
}
