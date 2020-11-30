using Godot;
using SpaceDodgeRL.scenes.encounter.state;
using System;

public class AutopilotZoneReadout : HBoxContainer {

  public Button AutopilotButton { get => this.GetNode<Button>("AutopilotButton"); }

  public void SetReadout(EncounterZone zone) {
    this.GetNode<Label>("ReadoutContainer/NameEncounterBar/ZoneNameLabel").Text = zone.ZoneName;
    this.GetNode<Label>("ReadoutContainer/NameEncounterBar/ZoneEncounterLabel").Text = zone.ReadoutEncounterName;
  }
}
