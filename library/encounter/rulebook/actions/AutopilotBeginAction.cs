namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public class AutopilotBeginAction : EncounterAction {

    public string ZoneId { get; }
    public string Mode { get; }

    public AutopilotBeginAction(string actorId, string zoneId, string mode) : base(actorId, ActionType.AUTOPILOT_BEGIN) {
      this.ZoneId = zoneId;
      this.Mode = mode;
    }
  }
}