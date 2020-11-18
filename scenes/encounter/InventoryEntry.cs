using Godot;

public class InventoryEntry : CenterContainer {
  [Export]
  public string EntityId { get; private set; }

  public void PopulateData(string entityId, string name, string description) {
    this.EntityId = entityId;
    this.GetNode<Label>("HorizontalDivider/EntryText/EntryName").Text = name;
    this.GetNode<Label>("HorizontalDivider/EntryText/EntryDescription").Text = description;
  }
}
