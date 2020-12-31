namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public class UseAction : EncounterAction {

    public string UsableId { get; }
    public bool FromInventory { get; }

    public UseAction(string actorId, string usableId, bool fromInventory) : base(actorId, ActionType.USE) {
      this.UsableId = usableId;
      this.FromInventory = fromInventory;
    }
  }
}