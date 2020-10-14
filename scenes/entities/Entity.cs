using Godot;
using SpaceDodgeRL.scenes.components;
using System;

namespace SpaceDodgeRL.scenes.entities {

  public class Entity : Node {
    public static string ENTITY_GROUP = "groupEntity";

    private string _id, _name;
    public string EntityId { get => _id; }
    public string EntityName { get => _name; }

    public void Init(string id, string name) {
      _id = id;
      _name = name;
      AddToGroup(ENTITY_GROUP);
    }

    public override void _Ready() {
      if (_id == null || _name == null) {
        throw new NotImplementedException("call Init() on your entities!");
      }
    }

    public new void AddChild(Node node, bool legibleUniqueName = false) {
      if (!(node is Component)) {
        throw new NotImplementedException();
      }
      base.AddChild(node, legibleUniqueName);
      AddToGroup((node as Component).EntityGroup);
    }

    public new void RemoveChild(Node node) {
      base.RemoveChild(node);
      RemoveFromGroup((node as Component).EntityGroup);
    }
  }
}