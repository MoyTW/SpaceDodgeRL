using System.Collections.Generic;
using Godot;

namespace SpaceDodgeRL.scenes.encounter {

  abstract public class EntryButton : Button {

    public override void _Ready() {
      this.Connect("focus_entered", this, nameof(OnFocusEntered));
      this.Connect("focus_exited", this, nameof(OnFocusExited));
      this.Connect("mouse_entered", this, nameof(OnMouseEntered));
      this.Connect("mouse_exited", this, nameof(OnMouseExited));
    }

    private static List<Label> FindAllLabels(Node baseNode) {
      if (baseNode.GetChildCount() == 0) {
        if (baseNode is Label) {
          return new List<Label>() { (Label)baseNode };
        } else {
          return new List<Label>();
        }
      }

      var returnList = new List<Label>();
      foreach (Node child in baseNode.GetChildren()) {
        returnList.AddRange(FindAllLabels(child));
      }
      return returnList;
    }

    protected void OnFocusEntered() {
      Color backgroundColor = (Color)this.Theme.GetStylebox("normal", "Button").Get("bg_color");
      foreach (var label in FindAllLabels(this)) {
        label.AddColorOverride("font_color", backgroundColor);
      }
    }

    protected void OnFocusExited() {
      Color normalColor = (Color)this.Theme.GetColor("font_color", "Button");
      foreach (var label in FindAllLabels(this)) {
        label.AddColorOverride("font_color", normalColor);
      }
    }
    
    private void OnMouseEntered() {
      this.GrabFocus();
    }

    private void OnMouseExited() {
      this.ReleaseFocus();
    }
  }
}