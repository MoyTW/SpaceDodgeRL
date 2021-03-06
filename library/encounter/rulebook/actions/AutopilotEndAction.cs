namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public enum AutopilotEndReason {
    PLAYER_INPUT,
    ENEMY_DETECTED,
    TASK_COMPLETED,
    INVENTORY_FULL,
    NO_PATH
  }

  public class AutopilotEndAction : EncounterAction {

    public AutopilotEndReason Reason { get; }

    public AutopilotEndAction(string actorId, AutopilotEndReason reason) : base(actorId, ActionType.AUTOPILOT_END) {
      this.Reason = reason;
    }
  }
}