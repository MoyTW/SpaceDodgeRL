using SpaceDodgeRL.library.encounter.rulebook;

public class EndTurnAction: EncounterAction {
  public EndTurnAction(string actorId) : base(actorId, ActionType.END_TURN) { }
}