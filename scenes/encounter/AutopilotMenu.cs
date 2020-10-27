using Godot;

public class AutopilotMenu : HBoxContainer {
  public class ActionMapping {
    public static string UI_ACCEPT = "ui_accept";
    public static string UI_CANCEL = "ui_cancel";
    public static string UI_UP = "ui_up";
    public static string UI_DOWN = "ui_down";
  }

  public override void _UnhandledKeyInput(InputEventKey @event) {
    if (@event.IsActionPressed(ActionMapping.UI_ACCEPT, true)) {
      GD.Print("User accepted");
    } else if (@event.IsActionPressed(ActionMapping.UI_CANCEL, true)) {
      GD.Print("lol");
    } else if (@event.IsActionPressed(ActionMapping.UI_UP, true)) {
      GD.Print("go up");
    } else if (@event.IsActionPressed(ActionMapping.UI_DOWN, true)) {
      GD.Print("go down");
    }
  }
}
