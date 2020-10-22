using SpaceDodgeRL.library.encounter.rulebook;

public class WaitAction: EncounterAction {
  public WaitAction(string actorId) : base(actorId, ActionType.WAIT) { }
}