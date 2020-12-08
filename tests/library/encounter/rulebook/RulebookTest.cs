using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.library.encounter.rulebook.actions;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.encounter;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class RulebookTest {
  private readonly ITestOutputHelper _output;

  public RulebookTest(ITestOutputHelper output) {
    _output = output;
  }

  /**
    * Crash via NPE on 3a6c655cd26edb289da3390d656770784c737837
    *
    * NPE on SpaceDodgeRL\library\encounter\rulebook\Rulebook.cs:124, occuring when:
    * 1: Entity A fires Projectile A
    * 2: Entity A is destroyed
    * 3: Projectile A destroys Entity B
    * The issue is that the code to assign XP to Entity A NPEs because the `sourceEntityId` in Projectile A refers to Entity A,
    * but Entity A has been removed from the state's dictionary lookup.
    */
  [Fact]
  public void Crash_NPE_3a6c655cd26edb289da3390d656770784c737837() {
    //Given
    EncounterState state = EncounterState.CreateWithoutSaving();
    state.SetStateForNewGame();

    var entityA = Entity.Create("a", "entity a");
    entityA.AddComponent(DisplayComponent.Create("res://resources/tex_test.tres", "", false));
    var entityAPosition = new EncounterPosition(10, 10);
    state.PlaceEntity(entityA, entityAPosition);

    var entityB = Entity.Create("b", "entity b");
    entityB.AddComponent(DisplayComponent.Create("res://resources/tex_test.tres", "", false));
    entityB.AddComponent(CollisionComponent.CreateDefaultActor());
    entityB.AddComponent(DefenderComponent.Create(baseDefense: 0, maxHp: 1));
    entityB.AddComponent(XPValueComponent.Create(xpValue: 1));
    var entityBPosition = new EncounterPosition(20, 20);
    state.PlaceEntity(entityB, entityBPosition);

    //When
    var fireAction = FireProjectileAction.CreateCuttingLaserAction(entityA.EntityId, 10, entityBPosition);
    Rulebook.ResolveAction(fireAction, state);
    var projectileId = state.EntitiesAtPosition(entityAPosition.X, entityAPosition.Y)
                            .Where(e => e.GetComponent<PathAIComponent>() != null)
                            .First()
                            .EntityId;
    var moveAction = new MoveAction(projectileId, entityBPosition);
    state.RemoveEntity(entityA);
    var exception = Record.Exception(() => Rulebook.ResolveAction(moveAction, state));

    //Then
    Assert.Null(exception);
  }
}
