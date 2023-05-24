using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

public class TestRunnerScript
{
    [RuntimeInitializeOnLoadMethod]
    public static void RunTests()
    {
        var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();

        var filter = new Filter
        {
            testMode = TestMode.EditMode
        };

        // var callback = new Callback
        // {
        //     runStarted = (testAdaptor) => Debug.Log("Run started"),
        //     runFinished = (testResultAdaptor) => Debug.Log("Run finished"),
        //     testStarted = (testAdaptor) => Debug.Log("Test started: " + testAdaptor.Name),
        //     testFinished = (testResultAdaptor) => Debug.Log("Test finished: " + testResultAdaptor.Name)
        // };

        var settings = new ExecutionSettings(filter)
        {
            runSynchronously = true,
        };

        testRunnerApi.Execute(settings);
    }
}
