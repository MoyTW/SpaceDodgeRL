using Godot;

namespace SpaceDodgeRL.scenes.singletons {

  public class GameSettings : Node {

    public int TurnTimeMs { get; set; }

    public override void _Ready() {
      this.LoadFromDisk();
    }

    private void LoadFromDisk() {
      GD.Print("settings should be loaded now");
      this.TurnTimeMs = 100;
    }

    public void SaveToDisk() {
      GD.Print("here we save");
    }
  }
}