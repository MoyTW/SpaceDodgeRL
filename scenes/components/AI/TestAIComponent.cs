using Godot;
using System;

public class TestAIComponent : Node, Component, AIComponent {
  public static string ENTITY_GROUP = "TEST_AI_COMPONENT_GROUP";
  public string EntityGroup => ENTITY_GROUP;

  // TODO: sight-lines & group activation
  public bool IsActive => true;

  public void DecideNextAction(EncounterState state) {
    GD.Print("Entity is waiting!");
  }
}
