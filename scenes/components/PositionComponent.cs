using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.entities;
using SpaceDodgeRL.scenes.singletons;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceDodgeRL.scenes.components {

  [JsonConverter(typeof(PositionConverter))]
  public class PositionComponent : Node, Component {
    private static PackedScene _scenePrefab = GD.Load<PackedScene>("res://scenes/components/PositionComponent.tscn");

    public static readonly string ENTITY_GROUP = "POSITION_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    // TODO: Don't put this here
    public const int START_X = 12;
    public const int START_Y = 18;
    public const int STEP_X = 24;
    public const int STEP_Y = 36;

    public Texture SpriteTexture { get => GetNode<Sprite>("Sprite").Texture; }
    private GameSettings _gameSettings;
    private GameSettings GameSettings { get {
      if (this._gameSettings == null) {
        this._gameSettings = this.GetNode<GameSettings>("/root/GameSettings");
      }
      return this._gameSettings;
    } }

    private EncounterPosition _encounterPosition = new EncounterPosition(int.MinValue, int.MinValue);
    public EncounterPosition EncounterPosition {
      get => _encounterPosition;
      set {
        _encounterPosition = value;
        Tween(IndexToVector(value.X, value.Y));
      }
    }

    public static PositionComponent Create(EncounterPosition position, string texturePath) {
      var component = _scenePrefab.Instance() as PositionComponent;

      component._encounterPosition = position;
      var sprite = component.GetNode<Sprite>("Sprite");
      sprite.Position = IndexToVector(position.X, position.Y);
      sprite.Texture = GD.Load<Texture>(texturePath);

      return component;
    }

    public static PositionComponent Create(string saveData) {
      var loaded = JsonSerializer.Deserialize<SaveData>(saveData);
      return PositionComponent.Create(loaded.EncounterPosition, loaded.TexturePath);
    }

    private void Tween(Vector2 newPosition) {
      var tween = GetNode<Tween>("Tween");
      var sprite = GetNode<Sprite>("Sprite");
      tween.InterpolateProperty(sprite, "position", sprite.Position, newPosition, this.GameSettings.TurnTimeMs / 1000f);
      tween.Start();
    }

    public static EncounterPosition VectorToIndex(float x, float y) {
      return new EncounterPosition((int)(x / STEP_X), (int)(y / STEP_Y));
    }

    private static Vector2 IndexToVector(int x, int y) {
      return new Vector2(START_X + STEP_X * x, START_Y + STEP_Y * y);
    }

    public class SaveData {
      public string EntityGroup { get; set; }
      public EncounterPosition EncounterPosition { get; set; }
      public string TexturePath { get; set; }

      public SaveData() { }

      public SaveData(PositionComponent component) {
        this.EntityGroup = PositionComponent.ENTITY_GROUP;
        this.EncounterPosition = component.EncounterPosition;
        this.TexturePath = component.GetNode<Sprite>("Sprite").Texture.ResourcePath;
      }
    }

    public string Save() {
      return JsonSerializer.Serialize(new SaveData(this));
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }

  public class PositionConverter : JsonConverter<PositionComponent> {
    public override PositionComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      using (var doc = JsonDocument.ParseValue(ref reader)) {
        return PositionComponent.Create(doc.RootElement.GetRawText());
      }
    }

    public override void Write(Utf8JsonWriter writer, PositionComponent value, JsonSerializerOptions options) {
      JsonSerializer.Serialize(writer, new PositionComponent.SaveData(value), options);
    }
  }
}