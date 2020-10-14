namespace SpaceDodgeRL.library.encounter.rulebook {

  public class EncounterAction {
    protected string _actorId;
    protected ActionType _actionType;

    protected EncounterAction(string actorId, ActionType actionType) {
      _actorId = actorId;
      _actionType = actionType;
    }

    public string ActorId { get => _actorId; }
    public ActionType ActionType { get => _actionType; }
  }
}