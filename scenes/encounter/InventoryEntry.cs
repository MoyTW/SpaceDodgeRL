using Godot;

public class InventoryEntry : Button {
  public string EntityId { get; private set; }

  [Signal]
  public delegate void UseItemSelected();

  private Label _nameLabel;
  private Label _descriptionLabel;

  public override void _Ready() {
    this.Connect("focus_entered", this, nameof(OnFocusEntered));
    this.Connect("focus_exited", this, nameof(OnFocusExited));
    this.Connect("mouse_entered", this, nameof(OnMouseEntered));
    this.Connect("mouse_exited", this, nameof(OnMouseExited));
    this.Connect("pressed", this, nameof(OnUseButtonPressed));
  }

  public void SetData(string entityId, string name, string description) {
    this.EntityId = entityId;

    if (this._nameLabel == null) {
      this._nameLabel = this.GetNode<Label>("EntryText/EntryName");
      this._descriptionLabel =this.GetNode<Label>("EntryText/EntryDescription");
    }

    this._nameLabel.Text = name;
    this._descriptionLabel.Text = description;
    this.OnFocusExited();
  }

  // Because we're painting labels on top of a button, the font doesn't switch automatically on hover, so we must do it here.
  private void OnFocusEntered() {
    Color backgroundColor = (Color)this.Theme.GetStylebox("normal", "Button").Get("bg_color");
    this._nameLabel.AddColorOverride("font_color", backgroundColor);
    this._descriptionLabel.AddColorOverride("font_color", backgroundColor);
  }

  private void OnFocusExited() {
    Color normalColor = (Color)this.Theme.GetColor("font_color", "Button");
    this._nameLabel.AddColorOverride("font_color", normalColor);
    this._descriptionLabel.AddColorOverride("font_color", normalColor);
  }

  // Focus != hover, but we'd like if it were - hence we synchronize the mouse to keyboard.
  private void OnMouseEntered() {
    this.GrabFocus();
  }

  private void OnMouseExited() {
    this.ReleaseFocus();
  }

  private void OnUseButtonPressed() {
    EmitSignal(nameof(UseItemSelected));
  }
}
