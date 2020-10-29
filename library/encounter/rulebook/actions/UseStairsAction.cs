namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  // TODO: Wrap this into the generic "use" action? as in, "use stairs" command could just map to "find, and then 'use' the
  // Usable stairs Entity on the square"
  public class UseStairsAction : EncounterAction {
    public UseStairsAction(string actorId) : base(actorId, ActionType.USE_STAIRS) { }
  }
}