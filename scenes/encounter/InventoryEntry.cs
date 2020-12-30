using Godot;

namespace SpaceDodgeRL.scenes.encounter {
  public class InventoryEntry : EntryButton {
    public string EntityId { get; private set; }

    [Signal]
    public delegate void UseItemSelected();

    private Label _nameLabel;
    private Label _descriptionLabel;
    private TextureRect _textureRect;

    public override void _Ready() {
      base._Ready();
      Connect("pressed", this, nameof(OnUseButtonPressed));
    }

    public void SetData(string entityId, string name, string description, string texturePath) {
      EntityId = entityId;

      if (_nameLabel == null) {
        _nameLabel = GetNode<Label>("HBoxContainer/VBoxContainer/EntryName");
        _descriptionLabel = GetNode<Label>("HBoxContainer/EntryDescription");
        _textureRect = GetNode<TextureRect>("HBoxContainer/VBoxContainer/CenterContainer/TextureRect");
      }

      _nameLabel.Text = name;
      _descriptionLabel.Text = description;
      _textureRect.Texture = GD.Load<Texture>(texturePath);
      OnFocusExited();
    }

    private void OnUseButtonPressed() {
      EmitSignal(nameof(UseItemSelected));
    }
  }
}