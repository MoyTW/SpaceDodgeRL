namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public enum AutopilotEndReason {
    PLAYER_INPUT,
    ENEMY_DETECTED,
    DESTINATION_REACHED
  }

  public class AutopilotEndAction : EncounterAction {

    public AutopilotEndReason Reason { get; }

    public AutopilotEndAction(string actorId, AutopilotEndReason reason) : base(actorId, ActionType.AUTOPILOT_END) {
      this.Reason = reason;
    }
  }
}