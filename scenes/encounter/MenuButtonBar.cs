using Godot;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.singletons;

namespace SpaceDodgeRL.scenes.encounter {

  public class MenuButtonBar : TextureRect {
    public SceneManager _sceneManager { get => (SceneManager)GetNode("/root/SceneManager"); }
    private EncounterState _state;
    private InputHandler _inputHandler;


    public void SetState(EncounterState state, InputHandler inputHandler) {
      this._state = state;
      this._inputHandler = inputHandler;
    }

    public override void _Ready() {
      this.GetNode<Button>("VBoxContainer/TopButtonBar/GetButton").Connect("pressed", this, nameof(OnGetButtonPressed));
      this.GetNode<Button>("VBoxContainer/TopButtonBar/JumpButton").Connect("pressed", this, nameof(OnJumpButtonPressed));
      this.GetNode<Button>("VBoxContainer/TopButtonBar/ExploreZoneButton").Connect("pressed", this, nameof(OnExploreZoneButtonPressed));
      this.GetNode<Button>("VBoxContainer/TopButtonBar/HelpButton").Connect("pressed", this, nameof(OnHelpButtonPressed));
      this.GetNode<Button>("VBoxContainer/BottomButtonBar/AutopilotButton").Connect("pressed", this, nameof(OnAutopilotButtonPressed));
      this.GetNode<Button>("VBoxContainer/BottomButtonBar/CharacterButton").Connect("pressed", this, nameof(OnCharacterButtonPressed));
      this.GetNode<Button>("VBoxContainer/BottomButtonBar/InventoryButton").Connect("pressed", this, nameof(OnInventoryButtonPressed));
      this.GetNode<Button>("VBoxContainer/BottomButtonBar/EscapeButton").Connect("pressed", this, nameof(OnEscapeButtonPressed));
    }

    private void OnGetButtonPressed() {
      this._inputHandler.TryInsertInputAction(new InputHandler.InputAction(InputHandler.ActionMapping.GET_ITEM));
    }

    private void OnJumpButtonPressed() {
      this._inputHandler.TryInsertInputAction(new InputHandler.InputAction(InputHandler.ActionMapping.USE_STAIRS));
    }

    private void OnExploreZoneButtonPressed() {
      this._inputHandler.TryInsertInputAction(new InputHandler.InputAction(InputHandler.ActionMapping.AUTOEXPLORE));
    }

    private void OnHelpButtonPressed() {
      GD.Print("Help button pressed: TODO actually implement a help screen");
      //this._inputHandler.TryInsertInputAction(new InputHandler.InputAction(InputHandler.ActionMapping.HELP));
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