using SpaceDodgeRL.library.encounter.rulebook;

public class GetItemAction: EncounterAction {
  public GetItemAction(string actorId) : base(actorId, ActionType.GET_ITEM) { }
}