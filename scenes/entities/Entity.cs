using Godot;
using SpaceDodgeRL.scenes.components;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceDodgeRL.scenes.entities {

  [JsonConverter(typeof(EntityConverter))]
  public class Entity : Node {

    public static readonly string ENTITY_GROUP = "groupEntity";

    [JsonInclude] public string EntityId { get; private set; }
    [JsonInclude] public string EntityName { get; private set; }

    // Currently we make the assumption that an Entity can have 1 and only 1 of each node of any particular inheritance tree. So,
    // a node can only have 1 AIComponent, for example. This might change if we model, I don't know, a status effect as a node -
    // but if you can have 4 status effects on you and a caller is asking for "the" status effect (via StatusEffect class) that's
    // logically nonsensical. So, the assumption at least now isn't that terrible.
    //
    // I will admit that the implementation is imperfect.
    public List<Component> _Components { get; private set; }
    private Dictionary<Type, Component> _childTypeToComponent;
    private Dictionary<Component, List<Type>> _childComponentToTypes;

    private Entity Init(string entityId, string entityName) {
      this.EntityId = entityId;
      this.EntityName = entityName;
      this._Components = new List<Component>();
      this._childTypeToComponent = new Dictionary<Type, Component>();
      this._childComponentToTypes = new Dictionary<Component, List<Type>>();
      this.AddToGroup(ENTITY_GROUP);
      return this;
    }

    public static Entity Create(string entityId, string entityName) {
      return new Entity().Init(entityId, entityName);
    }

    public static Entity Create(string saveData) {
      var loaded = JsonSerializer.Deserialize<SaveData>(saveData);

      var entity = new Entity().Init(entityId: loaded.EntityId, entityName: loaded.EntityName);
      foreach(var component in loaded.Components) {
        entity.AddComponent(component);
      }

      return entity;
    }

    public override void _Ready() {
      if (this.EntityId == null || this.EntityName == null) {
        throw new NotImplementedException("call Init() on your entities!");
      }
    }

    public new void AddChild(Node child, bool legibleUniqueName = false) {
      throw new NotImplementedException("Never directly add children!");
    }

    public new void RemoveChild(Node child) {
      throw new NotImplementedException("Never directly remove children!");
    }

    public void AddComponent(Component component, bool legibleUniqueName = false) {
      if (!(component is Component)) {
        throw new NotImplementedException();
      }
      _Components.Add(component);
      _childTypeToComponent[component.GetType()] = component;
      _childComponentToTypes[component] = new List<Type>() { component.GetType() };
      AddToGroup((component as Component).EntityGroup);
      component.NotifyAttached(this);

      if (component is Node) {
        base.AddChild(component as Node);
      }
    }

    public void RemoveComponent(Component component) {
      _Components.Remove(component);
      _childComponentToTypes[component].ForEach(t => _childTypeToComponent.Remove(t));
      _childComponentToTypes.Remove(component);
      RemoveFromGroup((component as Component).EntityGroup);
      component.NotifyDetached(this);

      if (component is Node) {
        base.RemoveChild(component as Node);
      }
    }

    public new T GetNode<T>(NodePath path) where T: class {
      throw new InvalidOperationException("Use GetComponent<T>() instead!");
    }

    /**
     * Returns the component matching the type. If there are multiple components matching the type, returns an arbitrary
     * matching component - though having multiple components match the type you're calling means you're trying to fetch an
     * inappropriate interface! Also it probably means you want a container component.
     */
    public T GetComponent<T>() where T: Component {
      Component foundComponent;
      _childTypeToComponent.TryGetValue(typeof(T), out foundComponent);
      if (foundComponent != null) {
        return (T)foundComponent;
      }

      // TODO: Optimize for misses too (failed check always crawls all components; set dirty on component addition/removal)
      foreach (Component child in this._Components) {
        if (child is T) {
          var typeT = typeof(T);
          this._childTypeToComponent[typeT] = (Component)child;
          this._childComponentToTypes[(Component)child].Add(typeT);
          return (T)child;
        }
      }
      return default(T);
    }

    public class SaveData {
      public string EntityId { get; set; }
      public string EntityName { get; set; }
      public List<Component> Components { get; set; }

      public SaveData() { }

      public SaveData(Entity entity) {
        this.EntityId = entity.EntityId;
        this.EntityName = entity.EntityName;
        this.Components = entity._Components;
      }
    }
  }

  public class EntityConverter : JsonConverter<Entity> {
    public override Entity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      using (var doc = JsonDocument.ParseValue(ref reader)) {
        return Entity.Create(doc.RootElement.GetRawText());
      }
    }

    public override void Write(Utf8JsonWriter writer, Entity value, JsonSerializerOptions options) {
      JsonSerializer.Serialize(writer, new Entity.SaveData(value), options);
    }
  }
}