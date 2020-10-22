using Godot;
using SpaceDodgeRL.library.encounter;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class PositionComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/PositionComponent.tscn");

    public static readonly string ENTITY_GROUP = "POSITION_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    // TODO: Don't put this here
    public const int START_X = 50;
    public const int START_Y = 54;
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

    public static PositionComponent Create(EncounterPosition position, Texture texture) {
      var component = _componentPrefab.Instance() as PositionComponent;

      component._encounterPosition = position;
      var sprite = component.GetNode<Sprite>("Sprite");
      sprite.Position = IndexToVector(position.X, position.Y);
      sprite.Texture = texture;

      return component;
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
  }
}