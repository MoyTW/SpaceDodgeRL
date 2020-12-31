using System.Text.Json;
using Godot;

namespace SpaceDodgeRL.scenes.singletons {

  public class GameSettings : Node {

    private string _saveLocation = "user://settings.json";

    private SaveData _saveData;
    public int DeafultTurnTimeMs { get => this._saveData.DeafultTurnTimeMs; }
    public int TurnTimeMs { get => this._saveData.TurnTimeMs; set => this._saveData.TurnTimeMs = value; }

    public override void _Ready() {
      this.LoadFromDisk();
    }

    private void LoadFromDisk() {
      bool firstRun = false;
      Godot.File file = new Godot.File();
      file.Open(this._saveLocation, File.ModeFlags.Read);
      if (file.IsOpen()) {
        var saveDataJson = file.GetAsText();
        this._saveData = JsonSerializer.Deserialize<SaveData>(saveDataJson);
      } else {
        this._saveData = new SaveData();
        firstRun = true;
      }
      file.Close();

      if (firstRun) {
        this.SaveToDisk();
      }
    }

    public void SaveToDisk() {
      Godot.File write = new Godot.File();
      write.Open(this._saveLocation, File.ModeFlags.Write);
      write.StoreString(JsonSerializer.Serialize(this._saveData));
      write.Close();
    }

    private class SaveData {
      public int DeafultTurnTimeMs { get; } = 100;
      public int TurnTimeMs { get; set; }

      public SaveData() {
        this.TurnTimeMs = this.DeafultTurnTimeMs;
      }
    }
  }
}