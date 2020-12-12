using Godot;
using SpaceDodgeRL.scenes.singletons;
using System;

namespace SpaceDodgeRL.scenes {

  public class SettingsMenu : VBoxContainer {

    private GameSettings _gameSettings;

    public HSlider TurnTimeSlider;
    public Label TurnTimeValueLabel;
    public Button TurnTimeDefaultButton;

    public override void _Ready() {
      this._gameSettings = GetNode<GameSettings>("/root/GameSettings");

      this.TurnTimeSlider = this.GetNode<HSlider>("TurnTimeContainer/TurnTimeSlider");
      this.TurnTimeValueLabel = this.GetNode<Label>("TurnTimeContainer/TurnTimeValueLabel");
      this.TurnTimeDefaultButton = this.GetNode<Button>("TurnTimeContainer/TurnTimeDefaultButton");

      // Set to current settings
      this.TurnTimeSlider.Value = this._gameSettings.TurnTimeMs;
      this.TurnTimeValueLabel.Text = string.Format("{0} ms", this._gameSettings.TurnTimeMs);

      // Connect controls
      this.TurnTimeSlider.Connect("value_changed", this, nameof(OnTurnTimeSliderValueChanged));
      this.TurnTimeDefaultButton.Connect("pressed", this, nameof(OnTurnTimeDefaultButtonPressed));
      // When we get another setting, have this control that too.
      this.GetNode<Button>("ResetAllToDefaultsButton").Connect("pressed", this, nameof(OnTurnTimeDefaultButtonPressed));
      this.GetNode<Button>("SaveAndExitButton").Connect("pressed", this, nameof(OnSaveAndExitButtonPressed));
    }

    private void OnTurnTimeSliderValueChanged(float value) {
      this.TurnTimeValueLabel.Text = string.Format("{0} ms", (int)value);
      this._gameSettings.TurnTimeMs = (int)value;
    }

    private void OnTurnTimeDefaultButtonPressed() {
      this.TurnTimeSlider.Value = this._gameSettings.DeafultTurnTimeMs;
    }

    private void OnSaveAndExitButtonPressed() {
      this._gameSettings.SaveToDisk();
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ReturnToPreviousScene();
    }
  }
}