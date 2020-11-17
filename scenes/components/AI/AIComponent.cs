using System.Collections.Generic;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components.AI {

  interface AIComponent: Component {
    List<EncounterAction> DecideNextAction(EncounterState state, Entity parent);
  }
}