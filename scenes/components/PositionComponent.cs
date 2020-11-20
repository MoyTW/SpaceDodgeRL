using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceDodgeRL.scenes.components {

  [JsonConverter(typeof(PositionComponentConverter))]
  public class PositionComponent : Node, Component, Savable {
    private static PackedScene _scenePrefab = GD.Load<PackedScene>("res://scenes/components/PositionComponent.tscn");

    public static readonly string ENTITY_GROUP = "POSITION_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    // TODO: Don't put this here
    public const int START_X = 12;
    public const int START_Y = 18;
    public const int STEP_X = 24;
    public const int STEP_Y = 36;

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
      return JsonSerializer.Deserialize<PositionComponent>(saveData);
    }

    private void Tween(Vector2 newPosition) {
      var tween = GetNode<Tween>("Tween");
      var sprite = GetNode<Sprite>("Sprite");
      tween.InterpolateProperty(sprite, "position", sprite.Position, newPosition, 0.1f);
      tween.Start();
    }

    private static Vector2 IndexToVector(int x, int y, int xOffset = 0, int yOffset = 0) {
      return new Vector2(START_X + STEP_X * x, START_Y + STEP_Y * y);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }

  public class PositionComponentConverter : JsonConverter<PositionComponent> {
    private class SaveData {
      public string EntityGroup { get; }
      public EncounterPosition EncounterPosition { get; }
      public string TexturePath { get; }

      public SaveData(PositionComponent component) {
        this.EntityGroup = PositionComponent.ENTITY_GROUP;
        this.EncounterPosition = component.EncounterPosition;
        this.TexturePath = component.GetNode<Sprite>("Sprite").Texture.ResourcePath;
      }
    }

    public override PositionComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      var saveData = JsonSerializer.Deserialize<SaveData>(reader.GetString(), options);
      return PositionComponent.Create(saveData.EncounterPosition, saveData.TexturePath);
    }

    public override void Write(Utf8JsonWriter writer, PositionComponent value, JsonSerializerOptions options) {
      JsonSerializer.Serialize<SaveData>(writer, new SaveData(value), options);
    }
  }
}