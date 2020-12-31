using Godot;
using SpaceDodgeRL.scenes.singletons;
using System;

namespace SpaceDodgeRL.scenes {

  public class SettingsMenu : VBoxContainer {

    private GameSettings _gameSettings;

    private HSlider _turnTimeSlider;
    private Label _turnTimeValueLabel;
    private Button _turnTimeDefaultButton;
    private Button _saveAndExitButton;

    public override void _Ready() {
      this._gameSettings = GetNode<GameSettings>("/root/GameSettings");

      this._turnTimeSlider = this.GetNode<HSlider>("TurnTimeContainer/TurnTimeSlider");
      this._turnTimeValueLabel = this.GetNode<Label>("TurnTimeContainer/TurnTimeValueLabel");
      this._turnTimeDefaultButton = this.GetNode<Button>("TurnTimeContainer/TurnTimeDefaultButton");
      this._saveAndExitButton = this.GetNode<Button>("SaveAndExitButton");

      // Set to current settings
      this._turnTimeSlider.Value = this._gameSettings.TurnTimeMs;
      this._turnTimeValueLabel.Text = string.Format("{0} ms", this._gameSettings.TurnTimeMs);

      // Connect controls
      this._turnTimeSlider.Connect("value_changed", this, nameof(OnTurnTimeSliderValueChanged));
      this._turnTimeDefaultButton.Connect("pressed", this, nameof(OnTurnTimeDefaultButtonPressed));
      // When we get another setting, have this control that too.
      this.GetNode<Button>("ResetAllToDefaultsButton").Connect("pressed", this, nameof(OnTurnTimeDefaultButtonPressed));
      this._saveAndExitButton.Connect("pressed", this, nameof(OnSaveAndExitButtonPressed));
    }

    public void SetFocus() {
      this._saveAndExitButton.GrabFocus();
    }

    private void OnTurnTimeSliderValueChanged(float value) {
      this._turnTimeValueLabel.Text = string.Format("{0} ms", (int)value);
      this._gameSettings.TurnTimeMs = (int)value;
    }

    private void OnTurnTimeDefaultButtonPressed() {
      this._turnTimeSlider.Value = this._gameSettings.DeafultTurnTimeMs;
    }

    private void OnSaveAndExitButtonPressed() {
      this._gameSettings.SaveToDisk();
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ExitToMainMenu(); // TODO: Figure out an easy way to have return to previous scene set focus properly
    }
  }
}