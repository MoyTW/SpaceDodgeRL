using Godot;

public class InventoryEntry : Button {
  [Export]
  public string EntityId { get; private set; }

  [Signal]
  public delegate void UseItemSelected();

  public void PopulateData(string entityId, string name, string description) {
    this.EntityId = entityId;
    this.GetNode<Label>("EntryText/EntryName").Text = name;
    this.GetNode<Label>("EntryText/EntryDescription").Text = description;
    this.Connect("pressed", this, nameof(OnUseButtonPressed));
  }

  private void OnUseButtonPressed() {
    EmitSignal(nameof(UseItemSelected));
  }
}
