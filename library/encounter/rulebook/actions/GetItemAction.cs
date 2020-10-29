namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public class GetItemAction : EncounterAction {
    public GetItemAction(string actorId) : base(actorId, ActionType.GET_ITEM) { }
  }
}