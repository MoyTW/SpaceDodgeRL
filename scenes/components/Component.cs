using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  // Thanks to https://github.com/dlyz as per https://github.com/dotnet/runtime/issues/33112
  /**
   * For [reasons] (see above issue) you can't normally serialize/deserialize interfaces. This is accomplished by blocking the
   * attribute from being attached to the interface. In the case of Component, we're enforcing by convention a public static
   * Create(string saveData) {...} on all implementations of Component, so the below lets us proxy-paste it onto the interface.
   */
  [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
  public class JsonInterfaceConverterAttribute : JsonConverterAttribute {
    public JsonInterfaceConverterAttribute(Type converterType) : base(converterType) { }
  }

  // See also: https://github.com/dotnet/runtime/issues/30083
  public class ComponentConverter : JsonConverter<Component> {

    private Dictionary<string, KeyValuePair<Type, MethodInfo>> _entityGroupToCreateFn = new Dictionary<string, KeyValuePair<Type, MethodInfo>>();

    public ComponentConverter() {
      var componentTypes = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => typeof(Component).IsAssignableFrom(t))
        .Where(t => !t.IsAbstract && !t.IsInterface)
        .ToList();
      foreach (var componentType in componentTypes) {
        var entityGroupField = componentType.GetField("ENTITY_GROUP");
        if (entityGroupField == null) {
          throw new NotImplementedException("Either stick to ENTITY_GROUP or figure out how to bridge static -> non-static");
        }

        var createFn = componentType.GetMethod(
          "Create",
          BindingFlags.Public | BindingFlags.Static,
          null,
          CallingConventions.Standard,
          new Type[] { typeof(string) },
          null
        );
        if (createFn == null) {
          throw new NotImplementedException(String.Format("No function Create(string _) found for {0}", componentType.Name));
        }
        _entityGroupToCreateFn[(string)entityGroupField.GetValue(null)] = new KeyValuePair<Type, MethodInfo>(componentType, createFn);
      }
    }

    // See https://github.com/dotnet/runtime/issues/30083#issuecomment-544708557 for reference
    public override Component Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      using (var doc = JsonDocument.ParseValue(ref reader)) {
        string entityGroup = (string)doc.RootElement.GetProperty(@"EntityGroup").GetString();
        if (!this._entityGroupToCreateFn.ContainsKey(entityGroup)) {
          throw new NotImplementedException(String.Format("Can't load {0}, is an unknown Component type!", entityGroup));
        }

        var createFn = this._entityGroupToCreateFn[entityGroup].Value;
        // Does doc.RootElement.GetRawText() have bad perf? It 99% doesn't matter for me but curious.
        return (Component)createFn.Invoke(null, new object[] { doc.RootElement.GetRawText() });
      }
    }

    public override void Write(Utf8JsonWriter writer, Component value, JsonSerializerOptions options) {
      var componentType = this._entityGroupToCreateFn[value.EntityGroup].Key;
      JsonSerializer.Serialize(writer, Convert.ChangeType(value, componentType), options);
    }
  }

  [JsonInterfaceConverter(typeof(ComponentConverter))]
  public interface Component {
    string EntityGroup { get; }
  }

  // TODO: After all Components implement Savable, delete Savable and move fns into Component
  public interface Savable {
    string Save();
    void NotifyAttached(Entity parent);
    void NotifyDetached(Entity parent);
  }
}