using System.Collections.Generic;
using SpaceDodgeRL.library.encounter.rulebook;

namespace SpaceDodgeRL.scenes.components.AI {

  public interface AIComponent {
    bool IsActive { get; }
    // TODO: Return a list of actions
    List<EncounterAction> DecideNextAction(EncounterState state);
  }
}