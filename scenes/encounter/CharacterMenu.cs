using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter;
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
    _closeButton.Connect("tree_entered", this, nameof(OnTreeEntered));

    // Hook up the level up menu
    GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpHPSelection")
      .Connect("pressed", this, nameof(OnLevelUpSelectionPressed),  new Godot.Collections.Array() { LevelUpBonus.MAX_HP });
    GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpAttackPowerSelection")
      .Connect("pressed", this, nameof(OnLevelUpSelectionPressed),  new Godot.Collections.Array() { LevelUpBonus.ATTACK_POWER });
    GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpRepairSelection")
      .Connect("pressed", this, nameof(OnLevelUpSelectionPressed),  new Godot.Collections.Array() { LevelUpBonus.REPAIR });
  }

  public void PrepMenu(EncounterState state) {
    PrepLevelColumn(state);
    PrepStatsColumn(state);
    PrepIntelColumn(state);
    PrepLevelUpMenu(state);
  }

  private void PrepLevelColumn(EncounterState state) {
    var playerXPTracker = state.Player.GetComponent<XPTrackerComponent>();

    GetNode<Label>("Columns/LevelColumn/LevelLabel").Text = String.Format("Level: {0}", playerXPTracker.Level);
    GetNode<Label>("Columns/LevelColumn/ExperienceLabel").Text = String.Format("Experience: {0} / {1}", playerXPTracker.XP, playerXPTracker.NextLevelAtXP);
  }

  private void PrepLevelUpMenu(EncounterState state) {
    var playerXPTracker = state.Player.GetComponent<XPTrackerComponent>();

    if (playerXPTracker.UnusedLevelUps.Count == 0) {
      GetNode<VBoxContainer>("LevelUpMenu").Visible = false;
      _closeButton.Disabled = false;
      _closeButton.GrabFocus();
      return;
    }

    // Make appropriate elements visible
    GetNode<VBoxContainer>("LevelUpMenu").Visible = true;
    var playerDefender = state.Player.GetComponent<DefenderComponent>();
    if (playerDefender.MaxHp > playerDefender.CurrentHp) {
      GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpRepairSelection").Visible = true;
    } else {
      GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpRepairSelection").Visible = false;
    }

    // TODO: You get a !insideTree error here and if you enter level-up BEFORE first entering the screen it errors - reason's
    // PrepMenu is called BEFORE mounting it. ok. well we'll patch that up.
    // Disable exit
    GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpHPSelection").GrabFocus();
    _closeButton.Disabled = true;
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

  private void OnLevelUpSelectionPressed(string selection) {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.HandleLevelUpSelected(selection);
  }
}
