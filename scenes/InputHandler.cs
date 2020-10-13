using Godot;
using System;
using System.Collections.Generic;

public class InputHandler : Node {
  public class ActionMapping {
    public static string MOVE_N = "move_n";
    public static string MOVE_NE = "move_ne";
    public static string MOVE_E = "move_e";
    public static string MOVE_SE = "move_se";
    public static string MOVE_S = "move_s";
    public static string MOVE_SW = "move_sw";
    public static string MOVE_W = "move_w";
    public static string MOVE_NW = "move_nw";

    public static string[] MoveActionMappings = new string[] { MOVE_N, MOVE_NE, MOVE_E, MOVE_SE, MOVE_S, MOVE_SW, MOVE_W, MOVE_NW };
  }

  private int _queueSize = 2;
  private List<string> _playerMappingQueue = new List<string>();

  public override void _Ready() {
      
  }

  public override void _UnhandledKeyInput(InputEventKey @event) {
    // I don't like iterating over every single possible action every time - surely there's a nicer way to do this?
    foreach (string mapping in ActionMapping.MoveActionMappings) {
      if (@event.IsActionPressed(mapping, true) && _playerMappingQueue.Count < _queueSize) {
        _playerMappingQueue.Add(mapping);
        return;
      }
    }
  }

  public string PopQueue() {
    if (this._playerMappingQueue.Count > 0) {
      var mapping = this._playerMappingQueue[0];
      this._playerMappingQueue.RemoveAt(0);
      return mapping;
    } else {
      return null;
    }
  }
}
