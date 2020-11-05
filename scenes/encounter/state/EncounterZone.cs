using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.encounter.state {
  /**
   * An EncounterZone indicates a region of the map which is analagous to a room in a dungeon. It is essentially a POI and
   * contains zero or more of enemies, items, interactables, or doodads. They are currently all recntangular but that's an
   * artifact of development not a design decision. Zones also serve as autopilot points, and have a string name and readout
   * data. Note that the entitites in the readout are not removed if they are destroyed/picked up!
   */
  public class EncounterZone: Godot.Object {
    public static int MAX_UNBLOCKED_POSITION_ATTEMPTS = 250;

    public string ZoneId { get; }
    public EncounterPosition Position { get; }
    public int Width { get; }
    public int Height { get; }
    // Readout information
    public string ZoneName { get; }
    public string ReadoutEncounterName { get; set; } // TODO: Consider making a builder for this
    // Note that these do not get removed from the zone when picked up, though they are removed from the EncounterState!
    private List<Entity> _readoutItems;
    public ReadOnlyCollection<Entity> ReadoutItems { get => _readoutItems.AsReadOnly(); }
    private List<Entity> _readoutFeatures;
    public ReadOnlyCollection<Entity> ReadoutFeatures { get => _readoutFeatures.AsReadOnly(); }

    public int X1 { get => Position.X; }
    public int X2 { get => Position.X + Width; }
    public int Y1 { get => Position.Y; }
    public int Y2 { get => Position.Y + Height; }
    public EncounterPosition Center { get; private set; }

    // This is required for Godot.Object
    public EncounterZone() { }

    public EncounterZone(string zoneId, EncounterPosition position, int width, int height, string zoneName) {
      this.ZoneId = zoneId;
      this.Position = position;
      this.Width = width;
      this.Height = height;
      this.ZoneName = zoneName;
      this._readoutItems = new List<Entity>();
      this._readoutFeatures = new List<Entity>();

      this.Center = new EncounterPosition((this.X1 + this.X2) / 2, (this.Y1 + this.Y2) / 2);
    }

    public EncounterPosition RandomUnblockedPosition(Random seededRand, EncounterState state) {
      int attempts = 0;
      while (attempts < MAX_UNBLOCKED_POSITION_ATTEMPTS) {
        int x = seededRand.Next(this.Width);
        int y = seededRand.Next(this.Height);

        if (!state.IsPositionBlocked(this.X1 + x, this.Y1 + y)) {
          return new EncounterPosition(this.X1 + x, this.Y1 + y);
        } else {
          attempts++;
        }
      }
      throw new NotImplementedException("ok really we should probably handle this sanely but 250 attemps it a lotta attempts!");
    }

    public bool Intersects(EncounterZone other) {
      return this.X1 <= other.X2 && this.X2 >= other.X1 && this.Y1 <= other.Y2 && this.Y2 >= other.Y1;
    }

    public void AddItemToReadout(Entity item) {
      this._readoutItems.Add(item);
    }

    public void AddFeatureToReadout(Entity feature) {
      this._readoutFeatures.Add(feature);
    }

    public override string ToString() {
      return string.Format("[{0} - [({1},{2}), ({3},{4})]]", this.ZoneName, this.X1, this.Y1, this.X2, this.Y2);
    }
  }
}