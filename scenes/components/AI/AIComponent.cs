using System.Collections.Generic;

public interface AIComponent {
  bool IsActive { get; }
  // TODO: Return a list of actions
  List<EncounterAction> DecideNextAction(EncounterState state);
}
