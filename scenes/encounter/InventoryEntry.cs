using Godot;

public class InventoryEntry : CenterContainer {
  [Export]
  public string EntityId { get; private set; }

  [Signal]
  public delegate void UseItemSelected();

  public void PopulateData(string entityId, string name, string description) {
    this.EntityId = entityId;
    this.GetNode<Label>("HorizontalDivider/EntryText/EntryName").Text = name;
    this.GetNode<Label>("HorizontalDivider/EntryText/EntryDescription").Text = description;
    this.GetNode<Button>("HorizontalDivider/EntryUseButton").Connect("pressed", this, nameof(OnUseButtonPressed));
  }

  private void OnUseButtonPressed() {
    EmitSignal(nameof(UseItemSelected));
  }
}
