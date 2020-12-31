using Godot;
using SpaceDodgeRL.library.encounter;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes {

  public class InputHandler : Node {
    public class InputAction {
      public string Mapping { get; private set; }
      public InputAction(string mapping) {
        this.Mapping = mapping;
      }
    }

    public class ScanInputAction : InputAction {
      public int X { get; private set; }
      public int Y { get; private set; }
      public ScanInputAction(int x, int y) : base(ActionMapping.SCAN_POSITION) {
        this.X = x;
        this.Y = y;
      }
    }

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
      public static string AUTOEXPLORE = "encounter_autoexplore";
      public static string CHARACTER = "encounter_character_menu";
      public static string ESCAPE_MENU = "encounter_escape_menu";
      public static string INVENTORY = "encounter_inventory_menu";
      public static string USE_STAIRS = "encounter_use_stairs";
      public static string GET_ITEM = "encounter_get_item";
      // Currently not used after the InventoryMenu got added; may be used if we add interactables on the encounter map
      public static string USE_ITEM = "encounter_use_item";
      public static string ZOOM_IN = "encounter_camera_zoom_in";
      public static string ZOOM_OUT = "encounter_camera_zoom_out";
      public static string ZOOM_RESET = "encounter_camera_zoom_reset";
      // Special actions (mouse & menu mostly)
      public static string SCAN_POSITION = "scan_position";
      // PERF: make a set if perf bad?
      public static string[] AllMappings = new string[] { MOVE_N, MOVE_NE, MOVE_E, MOVE_SE, MOVE_S, MOVE_SW, MOVE_W, MOVE_NW,
        WAIT, AUTOPILOT, AUTOEXPLORE, CHARACTER, ESCAPE_MENU, INVENTORY, USE_STAIRS, GET_ITEM, USE_ITEM, ZOOM_IN, ZOOM_OUT,
        ZOOM_RESET
      };
    }

    private int _queueSize = 2;
    private List<InputAction> _playerMappingQueue = new List<InputAction>();

    public override void _UnhandledKeyInput(InputEventKey @event) {
      // I don't like iterating over every single possible action every time - surely there's a nicer way to do this?
      foreach (string mapping in ActionMapping.AllMappings) {
        if (@event.IsActionPressed(mapping, true) && _playerMappingQueue.Count < _queueSize) {
          _playerMappingQueue.Add(new InputAction(mapping));
          return;
        }
      }
    }

    public bool TryInsertInputAction(InputAction inputAction) {
      if (this._playerMappingQueue.Count < this._queueSize) {
        this._playerMappingQueue.Add(inputAction);
        return true;
      } else {
        return false;
      }

    }

    public InputAction PopQueue() {
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