namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public class DestroyAction : EncounterAction {
    public DestroyAction(string actorId) : base(actorId, ActionType.DESTROY) { }
  }
}