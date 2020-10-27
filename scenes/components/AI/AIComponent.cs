using System.Collections.Generic;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.scenes.encounter.state;

namespace SpaceDodgeRL.scenes.components.AI {

  abstract public class AIComponent: Component {
    abstract public bool IsActive { get; }
    abstract public List<EncounterAction> DecideNextAction(EncounterState state);
  }
}