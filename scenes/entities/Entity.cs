using Godot;
using System;

public class Entity : Node {
  private string _id, _name;
  public string EntityId { get => _id; }
  public string EntityName { get => _name; }

  public void Init(string id, string name) {
    this._id = id;
    this._name = name;
  }

  public override void _Ready() {
    GD.Print("Entity is ready!");
  }
}
