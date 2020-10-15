using Godot;

namespace SpaceDodgeRL.scenes.components {

  abstract public class Component: Node {
    public virtual string EntityGroup { get; }
  }
}