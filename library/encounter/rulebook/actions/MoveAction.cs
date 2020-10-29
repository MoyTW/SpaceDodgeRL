namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public class MoveAction : EncounterAction {

    public EncounterPosition TargetPosition { get; private set; }

    public MoveAction(string actorId, EncounterPosition targetPosition) : base(actorId, ActionType.MOVE) {
      TargetPosition = targetPosition;
    }
  }
}