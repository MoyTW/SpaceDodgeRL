using Godot;

public class SaveSlot : Button {
  public string SaveLocation { get; private set; }
  public bool HasSaveData { get; private set; }

  public override void _Ready() {
    this.SaveLocation = string.Format("user://{0}.dat", Name);

    Godot.File file = new Godot.File();
    file.Open(this.SaveLocation, File.ModeFlags.Read);
    if(file.IsOpen()) {
      this.HasSaveData = true;
    }
    file.Close();

    if (this.HasSaveData) {
      this.Text += "(data)";
    } else {
      this.Text += " (Empty)";
    }
  }
}
