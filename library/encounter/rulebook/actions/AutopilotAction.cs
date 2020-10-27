namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public class AutopilotAction : EncounterAction {

    public string ZoneId { get; }

    public AutopilotAction(string actorId, string zoneId) : base(actorId, ActionType.AUTOPILOT) {
      this.ZoneId = zoneId;
    }
  }
}