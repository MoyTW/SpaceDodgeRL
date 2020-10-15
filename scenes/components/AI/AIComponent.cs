using System.Collections.Generic;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.scenes.encounter;

namespace SpaceDodgeRL.scenes.components.AI {

  abstract public class AIComponent: Component {
    abstract public bool IsActive { get; }
    // TODO: Return a list of actions
    abstract public List<EncounterAction> DecideNextAction(EncounterState state);
  }
}