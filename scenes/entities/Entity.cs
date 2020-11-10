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
    private List<Component> _components;
    private Dictionary<Type, Component> _childTypeToComponent;
    private Dictionary<Component, List<Type>> _childComponentToTypes;

    public void Init(string id, string name) {
      _id = id;
      _name = name;
      _components = new List<Component>();
      _childTypeToComponent = new Dictionary<Type, Component>();
      _childComponentToTypes = new Dictionary<Component, List<Type>>();
      AddToGroup(ENTITY_GROUP);
    }

    public override void _Ready() {
      if (_id == null || _name == null) {
        throw new NotImplementedException("call Init() on your entities!");
      }
    }

    public new void AddChild(Node child, bool legibleUniqueName = false) {
      throw new NotImplementedException("Never directly add children!");
    }

    public new void RemoveChild(Node child) {
      throw new NotImplementedException("Never directly remove children!");
    }

    public void AddComponent(Component component, bool legibleUniqueName = false) {
      if (!(component is Component)) {
        throw new NotImplementedException();
      }
      _components.Add(component);
      _childTypeToComponent[component.GetType()] = component;
      _childComponentToTypes[component] = new List<Type>() { component.GetType() };
      AddToGroup((component as Component).EntityGroup);

      if (component is Node) {
        base.AddChild(component as Node);
      }
    }

    public void RemoveComponent(Component component) {
      _components.Remove(component);
      _childComponentToTypes[component].ForEach(t => _childTypeToComponent.Remove(t));
      _childComponentToTypes.Remove(component);
      RemoveFromGroup((component as Component).EntityGroup);

      if (component is Node) {
        base.RemoveChild(component as Node);
      }
    }

    public new T GetNode<T>(NodePath path) where T: class {
      throw new InvalidOperationException("Use GetComponent<T>() instead!");
    }

    /**
     * Returns the component matching the type. If there are multiple components matching the type, returns an arbitrary
     * matching component - though having multiple components match the type you're calling means you're trying to fetch an
     * inappropriate interface! Also it probably means you want a container component.
     */
    public T GetComponent<T>() where T: Component {
      Component foundComponent;
      _childTypeToComponent.TryGetValue(typeof(T), out foundComponent);
      if (foundComponent != null) {
        return (T)foundComponent;
      }

      // TODO: Optimize for misses too (failed check always crawls all components; set dirty on component addition/removal)
      foreach (Component child in this._components) {
        if (child is T) {
          var typeT = typeof(T);
          this._childTypeToComponent[typeT] = (Component)child;
          this._childComponentToTypes[(Component)child].Add(typeT);
          return (T)child;
        }
      }
      return default(T);
    }
  }
}