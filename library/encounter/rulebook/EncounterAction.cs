public class EncounterAction {
  protected string _actorId;
  protected ActionType _actionType;

  protected EncounterAction(string actorId, ActionType actionType) {
    this._actorId = actorId;
    this._actionType = actionType;
  }

  public string ActorId { get => _actorId; }
  public ActionType ActionType { get => _actionType; }
}