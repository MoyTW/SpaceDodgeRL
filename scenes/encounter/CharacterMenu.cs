using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.singletons;
using System;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.encounter {

  // TODO: Make all this nice and pretty what with colors and stuff
  public class CharacterMenu : VBoxContainer {
    private static string _fontPath = "res://resources/fonts/Fira_Code_v5.2/ttf/FiraCode-Regular.ttf";

    private Button _closeButton;
    private List<Label> _intelLabels;

    public override void _Ready() {
      _closeButton = GetNode<Button>("Columns/CloseButton");
      _closeButton.Connect("pressed", this, nameof(OnCloseButtonPressed));
      _closeButton.Connect("tree_entered", this, nameof(OnTreeEntered));

      // Hook up the level up menu
      GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpHPSelection")
        .Connect("pressed", this, nameof(OnLevelUpSelectionPressed), new Godot.Collections.Array() { LevelUpBonus.MAX_HP });
      GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpAttackPowerSelection")
        .Connect("pressed", this, nameof(OnLevelUpSelectionPressed), new Godot.Collections.Array() { LevelUpBonus.ATTACK_POWER });
      GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpRepairSelection")
        .Connect("pressed", this, nameof(OnLevelUpSelectionPressed), new Godot.Collections.Array() { LevelUpBonus.REPAIR });
    }

    public void PrepMenu(EncounterState state) {
      PrepLevelColumn(state);
      PrepStatsColumn(state);
      PrepIntelColumn(state);
      PrepLevelUpMenu(state);
    }

    // TODO: Maybe show level history?
    private void PrepLevelColumn(EncounterState state) {
      var playerXPTracker = state.Player.GetComponent<XPTrackerComponent>();

      GetNode<Label>("Columns/LevelColumn/LevelLabel").Text = string.Format("Level: {0}", playerXPTracker.Level);
      GetNode<Label>("Columns/LevelColumn/ExperienceLabel").Text = string.Format("Experience: {0} / {1}", playerXPTracker.XP, playerXPTracker.NextLevelAtXP);
    }

    private void PrepLevelUpMenu(EncounterState state) {
      var playerXPTracker = state.Player.GetComponent<XPTrackerComponent>();

      if (playerXPTracker.UnusedLevelUps.Count == 0) {
        GetNode<VBoxContainer>("LevelUpMenu").Visible = false;
        _closeButton.Disabled = false;
        _closeButton.GrabFocus();
        return;
      } else {
        // Make appropriate elements visible
        GetNode<VBoxContainer>("LevelUpMenu").Visible = true;
        var playerDefender = state.Player.GetComponent<DefenderComponent>();
        if (playerDefender.MaxHp > playerDefender.CurrentHp) {
          GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpRepairSelection").Visible = true;
        } else {
          GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpRepairSelection").Visible = false;
        }

        // Disable exit
        GetNode<Button>("LevelUpMenu/LevelUpColumns/LevelUpHPSelection").GrabFocus();
        _closeButton.Disabled = true;
      }
    }

    private void PrepStatsColumn(EncounterState state) {
      var defenderComponent = state.Player.GetComponent<DefenderComponent>();
      GetNode<Label>("Columns/StatsColumn/StatsHPLabel").Text = string.Format("HP: {0}/{1}", defenderComponent.CurrentHp, defenderComponent.MaxHp);

      var playerComponent = state.Player.GetComponent<PlayerComponent>();
      GetNode<Label>("Columns/StatsColumn/StatsAttackPowerLabel").Text = string.Format("Cutting laser power: {0}", playerComponent.CuttingLaserPower);
      GetNode<Label>("Columns/StatsColumn/StatsAttackRangeLabel").Text = string.Format("Cutting laser range: {0}", playerComponent.CuttingLaserRange);

      var speed = state.Player.GetComponent<SpeedComponent>().Speed;
      GetNode<Label>("Columns/StatsColumn/StatsSpeedLabel").Text = string.Format("Speed: {0}", speed);
    }

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

          newLabel.Text = string.Format("Sector {0}: {1}", i, playerComponent.KnowsIntel(i) ? "KNOWN" : "UNKNOWN");
        }
      } else {
        for (int i = 1; i < state.LevelsInDungeon; i++) {
          _intelLabels[i - 1].Text = string.Format("Sector {0}: {1}", i, playerComponent.KnowsIntel(i) ? "KNOWN" : "UNKNOWN");
        }
      }
    }

    public override void _UnhandledKeyInput(InputEventKey @event) {
      if (@event.IsActionPressed("ui_cancel", true)) {
        OnCloseButtonPressed();
        return;
      }
    }

    private void OnTreeEntered() {
      _closeButton.GrabFocus();
    }

    private void OnCloseButtonPressed() {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ReturnToPreviousScene();
    }

    private void OnLevelUpSelectionPressed(string selection) {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.HandleLevelUpSelected(selection);
    }
  }
}