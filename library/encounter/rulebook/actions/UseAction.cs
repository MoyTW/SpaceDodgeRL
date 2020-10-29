namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public class UseAction : EncounterAction {

    public string UsableId { get; }

    public UseAction(string actorId, string usableId) : base(actorId, ActionType.USE) {
      this.UsableId = usableId;
    }
  }
}