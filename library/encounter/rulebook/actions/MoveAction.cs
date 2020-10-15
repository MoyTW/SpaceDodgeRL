using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;

public class MoveAction : EncounterAction {

  public GamePosition TargetPosition { get; private set; }

  public MoveAction(string actorId, GamePosition targetPosition) : base(actorId, ActionType.MOVE) {
    this.TargetPosition = targetPosition;
  }
}