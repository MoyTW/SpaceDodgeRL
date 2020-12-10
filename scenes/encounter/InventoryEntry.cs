using Godot;

public class InventoryEntry : HBoxContainer {
  [Export]
  public string EntityId { get; private set; }

  [Signal]
  public delegate void UseItemSelected();

  public void PopulateData(string entityId, string name, string description) {
    this.EntityId = entityId;
    this.GetNode<Label>("EntryText/EntryName").Text = name;
    this.GetNode<Label>("EntryText/EntryDescription").Text = description;
    this.GetNode<Button>("EntryUseButton").Connect("pressed", this, nameof(OnUseButtonPressed));
  }

  private void OnUseButtonPressed() {
    EmitSignal(nameof(UseItemSelected));
  }
}
