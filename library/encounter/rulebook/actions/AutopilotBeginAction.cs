namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  public class AutopilotBeginAction : EncounterAction {

    public string ZoneId { get; }

    public AutopilotBeginAction(string actorId, string zoneId) : base(actorId, ActionType.AUTOPILOT_BEGIN) {
      this.ZoneId = zoneId;
    }
  }
}