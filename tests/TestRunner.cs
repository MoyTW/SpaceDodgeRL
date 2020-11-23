using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests {

  public class TestRunnerOutput : ITestOutputHelper {
    public void WriteLine(string message) {
      GD.Print(message);
    }

    public void WriteLine(string format, params object[] args) {
      GD.Print(string.Format(format, args));
    }
  }

  public class TestRunner : Node {
    public override void _Ready() {
      GD.Print("Beginning test run!");

      var output = new TestRunnerOutput();
      var successfulTests = new List<MethodInfo>();
      var failedTests = new List<MethodInfo>();

      var testClasses = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.GetMethods().Any(m => m.GetCustomAttribute(typeof(FactAttribute)) != null))
        .ToList();
      foreach(var testClass in testClasses) {
        var classInstance = Activator.CreateInstance(testClass, output);

        var methods = testClass.GetMethods().Where(m => m.GetCustomAttribute(typeof(FactAttribute)) != null);
        foreach(var method in methods) {
          try {
            method.Invoke(classInstance, null);
            successfulTests.Add(method);
          } catch (Exception e) {
            failedTests.Add(method);
            output.WriteLine("Test failed for {0}.{1}!", testClass.Name, method.Name);
            output.WriteLine(e.GetBaseException().Message);
          }
        }
      }

      output.WriteLine("");
      output.WriteLine("Test run completed!");
      output.WriteLine("{0} tests ran: {1} successful, {2} failed.",
        successfulTests.Count + failedTests.Count, successfulTests.Count, failedTests.Count);
    }
  }
}