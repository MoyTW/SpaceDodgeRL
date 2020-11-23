using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests {

  public class TestRunnerOutput : ITestOutputHelper {

    private string _filepath;
    private List<string> _output;

    public TestRunnerOutput(string filepath) {
      this._filepath = filepath;
      this._output = new List<string>();
    }

    public void WriteLine(string message) {
      this._output.Add(message);
    }

    public void WriteLine(string format, params object[] args) {
      this._output.Add(string.Format(format, args));
    }

    public void Flush() {
      using (StreamWriter file = new StreamWriter(this._filepath)) {
        foreach (string message in this._output) {
          file.WriteLine(message);
        }
      }
    }
  }

  /**
   * This is a hack to allow us to run tests in the context of Godot. Since any code using Godot objects can't be run independent
   * of the engine, this scene manually finds and runs appropriately annotated classes. The true preference would be for tests to
   * be properly integrated but this is better than nothing.
   */
  public class TestRunner : Node {
    public override void _Ready() {
      var output = new TestRunnerOutput("test_results.log");
      var successfulTests = new List<MethodInfo>();
      var failedTests = new List<MethodInfo>();

      output.WriteLine("Beginning tests!");

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
            output.WriteLine("");
            output.WriteLine("Test failed for {0}.{1}!", testClass.Name, method.Name);
            output.WriteLine(e.GetBaseException().Message);
            output.WriteLine(e.GetBaseException().StackTrace);
          }
        }
      }

      output.WriteLine("");
      output.WriteLine("Test run completed!");
      output.WriteLine("{0} tests ran: {1} successful, {2} failed.",
        successfulTests.Count + failedTests.Count, successfulTests.Count, failedTests.Count);

      output.Flush();

      GD.Print("Test run completed!");
      GD.Print(String.Format("{0} tests ran: {1} successful, {2} failed.",
        successfulTests.Count + failedTests.Count, successfulTests.Count, failedTests.Count));

      GetTree().Quit();
    }
  }
}