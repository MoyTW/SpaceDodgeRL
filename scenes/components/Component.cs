using Godot;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public interface Component {
    string EntityGroup { get; }
  }

  // TODO: After all Components implement Savable, delete Savable and move fns into Component
  public interface Savable {
    string Save();
    void NotifyAttached(Entity parent);
    void NotifyDetached(Entity parent);
  }
}