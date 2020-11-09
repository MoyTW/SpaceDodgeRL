using Godot;
using SpaceDodgeRL.scenes.components;
using System;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.entities {

  public class Entity : Node {

    public static readonly string ENTITY_GROUP = "groupEntity";

    private string _id, _name;
    public string EntityId { get => _id; }
    public string EntityName { get => _name; }

    // Currently we make the assumption that an Entity can have 1 and only 1 of each node of any particular inheritance tree. So,
    // a node can only have 1 AIComponent, for example. This might change if we model, I don't know, a status effect as a node -
    // but if you can have 4 status effects on you and a caller is asking for "the" status effect (via StatusEffect class) that's
    // logically nonsensical. So, the assumption at least now isn't that terrible.
    //
    // I will admit that the implementation is imperfect.
    private Dictionary<Type, Node> _childTypeToComponent;
    private Dictionary<Node, List<Type>> _childComponentToTypes;

    public void Init(string id, string name) {
      _id = id;
      _name = name;
      _childTypeToComponent = new Dictionary<Type, Node>();
      _childComponentToTypes = new Dictionary<Node, List<Type>>();
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
      _childTypeToComponent[node.GetType()] = node;
      _childComponentToTypes[node] = new List<Type>() { node.GetType() };
      AddToGroup((node as Component).EntityGroup);
    }

    public new void RemoveChild(Node node) {
      base.RemoveChild(node);
      _childComponentToTypes[node].ForEach(t => _childTypeToComponent.Remove(t));
      _childComponentToTypes.Remove(node);
      RemoveFromGroup((node as Component).EntityGroup);
    }

    public new T GetNode<T>(NodePath path) where T: class {
      throw new InvalidOperationException("Use GetComponent<T>() instead!");
    }

    /**
     * Returns the component matching the type. If there are multiple components matching the type, returns an arbitrary
     * matching component - though having multiple components match the type you're calling means you're trying to fetch an
     * inappropriate interface! Also it probably means you want a container component.
     */
    public T GetComponent<T>() where T: Node {
      Node foundNode;
      _childTypeToComponent.TryGetValue(typeof(T), out foundNode);
      if (foundNode != null) {
        return foundNode as T;
      }

      foreach (Node child in this.GetChildren()) {
        if (child is T) {
          var typeT = typeof(T);
          this._childTypeToComponent[typeT] = child as T;
          this._childComponentToTypes[child].Add(typeT);
          return child as T;
        }
      }
      return default(T);
    }
  }
}