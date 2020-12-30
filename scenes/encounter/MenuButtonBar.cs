using Godot;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.singletons;

namespace SpaceDodgeRL.scenes.encounter {

  public class MenuButtonBar : TextureRect {
    public SceneManager _sceneManager { get => (SceneManager)GetNode("/root/SceneManager"); }
    public EncounterState _state;

    public void SetState(EncounterState state) {
      this._state = state;
    }

    public override void _Ready() {
      this.GetNode<Button>("ButtonBarContainer/AutopilotButton").Connect("pressed", this, nameof(OnAutopilotButtonPressed));
      this.GetNode<Button>("ButtonBarContainer/CharacterButton").Connect("pressed", this, nameof(OnCharacterButtonPressed));
      this.GetNode<Button>("ButtonBarContainer/InventoryButton").Connect("pressed", this, nameof(OnInventoryButtonPressed));
      this.GetNode<Button>("ButtonBarContainer/EscapeButton").Connect("pressed", this, nameof(OnEscapeButtonPressed));
    }

    private void OnAutopilotButtonPressed() {
      this._sceneManager.ShowAutopilotMenu(this._state);
    }

    private void OnCharacterButtonPressed() {
      this._sceneManager.ShowCharacterMenu(this._state);
    }

    private void OnInventoryButtonPressed() {
      this._sceneManager.ShowInventoryMenu(this._state);
    }

    private void OnEscapeButtonPressed() {
      this._sceneManager.ShowEscapeMenu(this._state);
    }
  }
}