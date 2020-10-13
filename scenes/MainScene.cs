using Godot;
using System;

public class MainScene : Node2D {

  public override void _Ready() {
    var encounterState = GetNode<EncounterState>("EncounterState");
    GD.Print(GetNode<EntityBuilder>("EntityBuilder"));
    encounterState.entityBuilderRef = GetNode<EntityBuilder>("EntityBuilder");
    encounterState.inputHandlerRef = GetNode<InputHandler>("InputHandler");
  }
}
