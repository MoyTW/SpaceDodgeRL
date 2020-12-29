using Godot;

namespace SpaceDodgeRL.scenes.encounter {
  public class InventoryEntry : Button {
    public string EntityId { get; private set; }

    [Signal]
    public delegate void UseItemSelected();

    private Label _nameLabel;
    private Label _descriptionLabel;

    public override void _Ready() {
      Connect("focus_entered", this, nameof(OnFocusEntered));
      Connect("focus_exited", this, nameof(OnFocusExited));
      Connect("mouse_entered", this, nameof(OnMouseEntered));
      Connect("mouse_exited", this, nameof(OnMouseExited));
      Connect("pressed", this, nameof(OnUseButtonPressed));
    }

    public void SetData(string entityId, string name, string description) {
      EntityId = entityId;

      if (_nameLabel == null) {
        _nameLabel = GetNode<Label>("EntryText/EntryName");
        _descriptionLabel = GetNode<Label>("EntryText/EntryDescription");
      }

      _nameLabel.Text = name;
      _descriptionLabel.Text = description;
      OnFocusExited();
    }

    // Because we're painting labels on top of a button, the font doesn't switch automatically on hover, so we must do it here.
    private void OnFocusEntered() {
      Color backgroundColor = (Color)Theme.GetStylebox("normal", "Button").Get("bg_color");
      _nameLabel.AddColorOverride("font_color", backgroundColor);
      _descriptionLabel.AddColorOverride("font_color", backgroundColor);
    }

    private void OnFocusExited() {
      Color normalColor = Theme.GetColor("font_color", "Button");
      _nameLabel.AddColorOverride("font_color", normalColor);
      _descriptionLabel.AddColorOverride("font_color", normalColor);
    }

    // Focus != hover, but we'd like if it were - hence we synchronize the mouse to keyboard.
    private void OnMouseEntered() {
      GrabFocus();
    }

    private void OnMouseExited() {
      ReleaseFocus();
    }

    private void OnUseButtonPressed() {
      EmitSignal(nameof(UseItemSelected));
    }
  }
}