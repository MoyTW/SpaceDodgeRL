using Godot;
using System;

public class MainScene : Node2D {

  public override void _Ready() {
    GetNode<EncounterState>("EncounterState").inputHandlerRef = GetNode<InputHandler>("InputHandler");
  }
}
