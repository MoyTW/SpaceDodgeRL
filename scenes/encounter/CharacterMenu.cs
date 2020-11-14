using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;

public class CharacterMenu : VBoxContainer {
  private static string _fontPath = "res://resources/fonts/Fira_Code_v5.2/ttf/FiraCode-Regular.ttf";

  private Button _closeButton;
  private List<Label> _intelLabels;

  public override void _Ready() {
    _closeButton = this.GetNode<Button>("Columns/CloseButton");
    _closeButton.Connect("pressed", this, nameof(OnButtonPressed));
    _closeButton.GrabFocus();
    _closeButton.Connect("tree_entered", this, nameof(OnTreeEntered));
  }

  public void PrepMenu(EncounterState state) {
    PrepLevelColumn(state);
    PrepStatsColumn(state);
    PrepIntelColumn(state);


  }

  private void PrepLevelColumn(EncounterState state) {
    var playerXPTracker = state.Player.GetComponent<XPTrackerComponent>();

    GetNode<Label>("Columns/LevelColumn/LevelLabel").Text = String.Format("Level: {0}", playerXPTracker.Level);
    GetNode<Label>("Columns/LevelColumn/ExperienceLabel").Text = String.Format("Experience: {0} / {1}", playerXPTracker.XP, playerXPTracker.NextLevelAtXP);
  }

  // TODO: Fill this out
  private void PrepStatsColumn(EncounterState state) {
    GD.Print("TODO: Make the stats column cool");
  }

  // TODO: Make this not hideous, lol - add in the font
  private void PrepIntelColumn(EncounterState state) {
    var playerComponent = state.Player.GetComponent<PlayerComponent>();
    var intelColumn = GetNode<VBoxContainer>("Columns/IntelColumn");

    if (_intelLabels == null) {
      _intelLabels = new List<Label>();
      for (int i = 1; i < state.LevelsInDungeon; i++) {
        var font = new DynamicFont();
        font.FontData = (DynamicFontData)GD.Load(_fontPath);

        var newLabel = new Label();
        newLabel.AddFontOverride("font", font);
        intelColumn.AddChild(newLabel);
        _intelLabels.Add(newLabel);

        newLabel.Text = String.Format("Sector {0}: {1}", i, playerComponent.KnowsIntel(i) ? "KNOWN" : "UNKNOWN");
      }
    } else {
      for (int i = 1; i < state.LevelsInDungeon; i++) {
        _intelLabels[i - 1].Text = String.Format("Sector {0}: {1}", i, playerComponent.KnowsIntel(i) ? "KNOWN" : "UNKNOWN");
      }
    }
  }

  private void OnTreeEntered() {
    _closeButton.GrabFocus();
  }

  private void OnButtonPressed() {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.CloseCharacterMenu();
  }
}
