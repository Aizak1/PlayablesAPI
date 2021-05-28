using System.Collections;
using System.Collections.Generic;
using animator;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using vjp;

public class FirstTest
{

    [Test]
    public void FirstTestSimplePasses()
    {
        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        string str = "{\r\n \"InputData\":{\r\n \"Commands\":" +
            "[\r\n {\r\n \"AddOutput\":{\r\n \"AnimationOutput\":" +
            "{\r\n \"Name\":\"main\"\r\n }\r\n \r\n }\r\n },{\r\n \"AddInput\":" +
            "{\r\n \"AnimationInput\":{\r\n \"AnimationBrain\":{\r\n },\r\n \"Name\":" +
            "\"brain\",\r\n \"Parent\":\"main\"\r\n }\r\n \r\n }\r\n },{\r\n \"AddController\":" +
            "{\r\n \"ControllerInput\":{\r\n \"ControllerType\": \"CloseCircle\",\r\n \"Name\":" +
            " \"close\"\r\n }\r\n }\r\n },{\r\n \"AddInput\":{\r\n \"AnimationInput\":{\r\n" +
            " \"AnimationJob\":{\r\n \"LookAtJob\":{\r\n\r\n }\r\n },\r\n \"Name\":\"job\",\r\n" +
            " \"Parent\":\"brain\"\r\n }\r\n \r\n }\r\n },{\r\n \"AddInput\":" +
            "{\r\n \"AnimationInput\":{\r\n \"AnimationMixer\":{\r\n },\r\n \"Name\":" +
            "\"mixer1\",\r\n \"Parent\":\"job\"\r\n }\r\n \r\n }\r\n },{\r\n \"AddInput\":" +
            "{\r\n \"AnimationInput\":{\r\n \"AnimationClip\":{\r\n\r\n \"TransitionDuration\":" +
            "0.5,\r\n \"ControllerName\": \"close\"\r\n },\r\n \"Name\":\"Clapping\"," +
            "\r\n \"Parent\":\"mixer1\"\r\n }\r\n \r\n }\r\n },{\r\n" +
            " \"AddInput\":{\r\n \"AnimationInput\":{\r\n \"AnimationClip\":{\r\n " +
            "\"TransitionDuration\":0.5,\r\n \"ControllerName\": \"close\"\r\n },\r\n " +
            "\"Name\":\"Flair\",\r\n \"Parent\":\"mixer1\"\r\n }" +
            "\r\n \r\n }\r\n }\r\n ]\r\n }\r\n}";

        Result<JSONType, JSONError> typeRes = VJP.Parse(str, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj["InputData"];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;

        Assert.AreEqual(7, commands.Count);
    }

}
