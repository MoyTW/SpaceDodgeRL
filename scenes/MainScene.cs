using Godot;
using System;

public class MainScene : Node2D {

  public override void _Ready() {
    var encounterState = GetNode<EncounterState>("EncounterState");
    encounterState.inputHandlerRef = GetNode<InputHandler>("InputHandler");

    // TODO: Attaching camera to the player like this is extremely jank! ALSO, it's causing a weird jumping behaviour where the
    // camera moves milliseconds after the player teleports to the next position!
    var camera = this.GetNode<Camera2D>("MainCamera");
    this.RemoveChild(camera);
    encounterState.Player.GetNode<PositionComponent>("PositionComponent").AddChild(camera);
  }
}
