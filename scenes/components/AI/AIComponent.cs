using System.Collections.Generic;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components.AI {

  interface AIComponent: Component {
    List<EncounterAction> DecideNextAction(EncounterState state, Entity parent);
  }

  public abstract class ActivatableAIComponent : AIComponent {
    public abstract string EntityGroup { get; }
    public string ActivationGroupId { get; protected set; }

    public ActivatableAIComponent(string activationGroupId) {
      this.ActivationGroupId = activationGroupId;
    }

    public abstract List<EncounterAction> _DecideNextAction(EncounterState state, Entity parent);

    public List<EncounterAction> DecideNextAction(EncounterState state, Entity parent) {
      if (!state.GroupActivated(this.ActivationGroupId)) {
        var position = parent.GetComponent<PositionComponent>().EncounterPosition;
        if (state.FoVCache.IsVisible(position.X, position.Y)) {
          state.ActivateGroup(this.ActivationGroupId);
        } else {
          return new List<EncounterAction>() { new WaitAction(parent.EntityId) };
        }
      }

      return _DecideNextAction(state, parent);
    }

    public abstract string Save();
    public abstract void NotifyAttached(Entity parent);
    public abstract void NotifyDetached(Entity parent);
  }
}