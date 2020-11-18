using Godot;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes {

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
      public static string WAIT = "encounter_wait";
      public static string AUTOPILOT = "encounter_autopilot_menu";
      public static string CHARACTER = "encounter_character_menu";
      public static string INVENTORY = "encounter_inventory_menu";
      public static string USE_STAIRS = "encounter_use_stairs";
      public static string GET_ITEM = "encounter_get_item";
      // Currently not used after the InventoryMenu got added; may be used if we add interactables on the encounter map
      public static string USE_ITEM = "encounter_use_item";
      // TODO: make a set if perf bad?
      public static string[] AllMappings = new string[] { MOVE_N, MOVE_NE, MOVE_E, MOVE_SE, MOVE_S, MOVE_SW, MOVE_W, MOVE_NW,
        WAIT, AUTOPILOT, CHARACTER, INVENTORY, USE_STAIRS, GET_ITEM, USE_ITEM
      };
    }

    private int _queueSize = 2;
    private List<string> _playerMappingQueue = new List<string>();

    public override void _UnhandledKeyInput(InputEventKey @event) {
      // I don't like iterating over every single possible action every time - surely there's a nicer way to do this?
      foreach (string mapping in ActionMapping.AllMappings) {
        if (@event.IsActionPressed(mapping, true) && _playerMappingQueue.Count < _queueSize) {
          _playerMappingQueue.Add(mapping);
          return;
        }
      }
    }

    public string PopQueue() {
      if (_playerMappingQueue.Count > 0) {
        var mapping = _playerMappingQueue[0];
        _playerMappingQueue.RemoveAt(0);
        return mapping;
      } else {
        return null;
      }
    }
  }
}