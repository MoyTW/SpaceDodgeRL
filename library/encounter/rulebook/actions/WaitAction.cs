namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public class WaitAction : EncounterAction {
    public WaitAction(string actorId) : base(actorId, ActionType.WAIT) { }
  }
}