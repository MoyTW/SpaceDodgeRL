using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;

public class MoveAction : EncounterAction {

  public EncounterPosition TargetPosition { get; private set; }

  public MoveAction(string actorId, EncounterPosition targetPosition) : base(actorId, ActionType.MOVE) {
    this.TargetPosition = targetPosition;
  }
}