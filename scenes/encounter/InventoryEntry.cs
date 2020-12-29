using Godot;

namespace SpaceDodgeRL.scenes.encounter {
  public class InventoryEntry : EntryButton {
    public string EntityId { get; private set; }

    [Signal]
    public delegate void UseItemSelected();

    private Label _nameLabel;
    private Label _descriptionLabel;

    public override void _Ready() {
      base._Ready();
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

    private void OnUseButtonPressed() {
      EmitSignal(nameof(UseItemSelected));
    }
  }
}