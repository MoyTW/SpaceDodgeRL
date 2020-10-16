namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public class SelfDestructAction : EncounterAction {
    public SelfDestructAction(string actorId) : base(actorId, ActionType.SELF_DESTRUCT) { }
  }
}