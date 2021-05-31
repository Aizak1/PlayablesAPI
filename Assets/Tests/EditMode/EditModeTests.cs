using System.Collections;
using System.Collections.Generic;
using animator;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using vjp;

public class EditModeTests
{
    private const string INPUT_DATA = "InputData";
    private const string JSON_TEXT = "{\r\n \"InputData\":{\r\n   \"Commands\":[\r\n  {\r\n  " +
        "\"AddOutput\":{\r\n \"AnimationOutput\":{\r\n  \"Name\":\"main\"\r\n  }\r\n   }\r\n  " +
        " },{\r\n  \"AddInput\":{\r\n  \"AnimationInput\":{\r\n \"AnimationBrain\":{\r\n      " +
        "},\r\n \"Name\":\"brain\",\r\n \"Parent\":\"main\"\r\n }\r\n }\r\n  },{\r\n " +
        "\"AddController\":{\r\n \"ControllerInput\":{\r\n " +
        "\"ControllerType\": \"CloseCircle\",\r\n \"Name\": \"close\"\r\n }\r\n }\r\n },{\r\n " +
        " \"AddController\":{\r\n \"ControllerInput\":{\r\n\"ControllerType\":" +
        " \"OpenCircle\",\r\n \"Name\": \"open\"\r\n}\r\n}\r\n},{\r\n \"AddController\":{\r\n " +
        " \"ControllerInput\":{\r\n \"ControllerType\": \"Random\",\r\n  " +
        "\"Name\": \"random\",\r\n\"RandomWeights\": [10, 20, 30]\r\n}\r\n }\r\n},{\r\n " +
        "\"AddInput\":{\r\n \"AnimationInput\":{\r\n\"AnimationMixer\":{\r\n },\r\n    " +
        " \"Name\":\"mixer1\",\r\n  \"Parent\":\"brain\"\r\n }\r\n }\r\n },{\r\n " +
        " \"AddInput\":{\r\n \"AnimationInput\":{\r\n \"AnimationLayerMixer\":{\r\n },\r\n " +
        " \"Name\":\"layerMixer1\",\r\n  \"Parent\":\"mixer1\"\r\n }\r\n  }\r\n  },{\r\n    " +
        " \"AddInput\":{\r\n \"AnimationInput\":{\r\n \"AnimationJob\":{\r\n \"LookAtJob\":" +
        "{\r\n  }\r\n },\r\n \"Name\":\"lookJob\",\r\n \"Parent\":\"layerMixer1\"\r\n }\r\n " +
        " }\r\n},{\r\n\"AddInput\":{\r\n\"AnimationInput\":{\r\n\"AnimationJob\":{\r\n   " +
        "\"TwoBoneIKJob\":{\r\n}\r\n},\r\n\"Name\":\"twoBone\",\r\n\"Parent\":\"layerMixer1\"" +
        "\r\n}\r\n}\r\n},{\r\n\"AddInput\":{\r\n\"AnimationInput\":{\r\n\"AnimationJob\":{\r\n" +
        "\"DampingJob\":{\r\n}\r\n},\r\n\"Name\":\"damping\",\r\n\"Parent\":\"layerMixer1\"\r\n" +
        "}\r\n}\r\n},{\r\n\"AddInput\":{\r\n\"AnimationInput\":{\r\n\"AnimationMixer\":{\r\n " +
        "},\r\n\"Name\":\"mixer2\",\r\n\"Parent\":\"lookJob\"\r\n}\r\n}\r\n},{\r\n\"AddInput\"" +
        ":{\r\n\"AnimationInput\":{\r\n\"AnimationMixer\":{\r\n},\r\n\"Name\":\"mixer3\",\r\n  " +
        "\"Parent\":\"twoBone\"\r\n}\r\n}\r\n},{\r\n\"AddInput\":{\r\n\"AnimationInput\":{\r\n" +
        "\"AnimationMixer\":{\r\n},\r\n\"Name\":\"mixer4\",\r\n\"Parent\":\"damping\"\r\n" +
        "}\r\n}\r\n},{\r\n\"AddInput\":{\r\n\"AnimationInput\":{\r\n\"AnimationClip\":{\r\n" +
        "\"TransitionDuration\":0.5,\r\n\"ControllerName\": \"close\"\r\n},\r\n\"Name\"" +
        ":\"Clapping\",\r\n\"Parent\":\"mixer2\"\r\n}\r\n}\r\n},{\r\n\"AddInput\":{\r\n" +
        "\"AnimationInput\":{\r\n\"AnimationClip\":{\r\n\"TransitionDuration\":0.5,\r\n" +
        "\"ControllerName\": \"close\"\r\n},\r\n\"Name\":\"Flair\",\r\n\"Parent\":\"mixer2\"\r" +
        "\n}\r\n}\r\n},{\r\n\"AddInput\":{\r\n\"AnimationInput\":{\r\n\"AnimationClip\":{\r\n" +
        "\"TransitionDuration\":0.5,\r\n\"ControllerName\": \"open\"\r\n},\r\n\"Name\":" +
        "\"Silly Dancing\",\r\n\"Parent\":\"mixer3\"\r\n}\r\n}\r\n},{\r\n\"AddInput\":{\r\n" +
        "\"AnimationInput\":{\r\n\"AnimationClip\":{\r\n\"TransitionDuration\":" +
        "0.5,\r\n\"ControllerName\": \"open\"\r\n},\r\n\"Name\":\"Spin In Place\",\r\n" +
        "\"Parent\":\"mixer3\"\r\n}\r\n}\r\n},{\r\n\"AddInput\":{\r\n\"AnimationInput\":{\r\n" +
        "\"AnimationClip\":{\r\n\"TransitionDuration\":0.5,\r\n\"ControllerName\":" +
        " \"random\"\r\n},\r\n\"Name\":\"Silly Dancing\",\r\n\"Parent\":\"mixer4\"\r\n " +
        "}\r\n}\r\n},{\r\n\"AddInput\":{\r\n\"AnimationInput\":{\r\n\"AnimationClip\":{\r\n " +
        "\"TransitionDuration\":0.5,\r\n\"ControllerName\": \"random\"\r\n},\r\n\"Name\"" +
        ":\"Spin In Place\",\r\n\"Parent\":\"mixer4\"\r\n}\r\n}\r\n},{\r\n\"AddInput\":{\r\n" +
        "\"AnimationInput\":{\r\n\"AnimationClip\":{\r\n\"TransitionDuration\":0.5,\r\n" +
        "\"ControllerName\": \"random\"\r\n},\r\n\"Name\":\"Clapping\",\r\n\"Parent\":" +
        "\"mixer4\"\r\n}\r\n}\r\n}\r\n]\r\n}\r\n}";

    [Test]
    public void CountCommandsTest()
    {
        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;

        Assert.AreEqual(20, commands.Count);
    }
    [Test]
    public void AddOutPutTest() {

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;

        Assert.AreEqual(true, commands[0].AddOutput.HasValue);

    }


    [Test]
    public void AddBrainTest() {

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;

        Assert.AreEqual(true, commands[1].AddInput.Value.AnimationInput.AnimationBrain.HasValue);

    }

    [Test]
    public void AddCloseControllerTest() {

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;

        var controllerType = commands[2].AddContoller.Value.ControllerInput.ControllerType;

        Assert.AreEqual(ControllerType.CloseCircle.ToString(),controllerType);

    }

    [Test]
    public void AddOpenControllerTest() {

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;

        var controllerType = commands[3].AddContoller.Value.ControllerInput.ControllerType;

        Assert.AreEqual(ControllerType.OpenCircle.ToString(), controllerType);

    }

    [Test]
    public void AddRandomControllerTest() {

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;

        var controllerType = commands[4].AddContoller.Value.ControllerInput.ControllerType;

        Assert.AreEqual(ControllerType.Random.ToString(), controllerType);


    }

    [Test]
    public void AddMixerTest() {

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;

        Assert.AreEqual(true, commands[5].AddInput.Value.AnimationInput.AnimationMixer.HasValue);

    }

    [Test]
    public void AddLayerMixerTest() {

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;
        var isLayerMixer = commands[6].AddInput.Value.AnimationInput.AnimationLayerMixer.HasValue;
        Assert.AreEqual(true,isLayerMixer);

    }

    [Test]
    public void AddLookAtJobTest() {

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;
        var isJob =
            commands[7].AddInput.Value.AnimationInput.AnimationJob.Value.LookAtJob.HasValue;
        Assert.AreEqual(true,isJob);

    }

    [Test]
    public void AddTwoBoneIKTest() {

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;

        var isJob =
           commands[8].AddInput.Value.AnimationInput.AnimationJob.Value.TwoBoneIKJob.HasValue;
        Assert.AreEqual(true, isJob);

    }

    [Test]
    public void AddDampingJobTest() {

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;

        var isJob =
          commands[9].AddInput.Value.AnimationInput.AnimationJob.Value.DampingJob.HasValue;
        Assert.AreEqual(true, isJob);

    }

    [Test]
    public void AddAnimationClipTest() {

        GameObject gameObject = new GameObject();
        gameObject.AddComponent<AnimatorDataParser>();

        Result<JSONType, JSONError> typeRes = VJP.Parse(JSON_TEXT, 1024);
        JSONType type = typeRes.AsOk();
        var obj = type.Obj.Peel();
        JSONType json = obj[INPUT_DATA];
        Option<InputData> optionInputData =
            gameObject.GetComponent<AnimatorDataParser>().LoadInputDataFromJSON(json);

        var inputData = optionInputData.Peel();
        var commands = inputData.commands;

        var isClip = commands[13].AddInput.Value.AnimationInput.AnimationClip.HasValue;
        Assert.AreEqual(true, isClip);

    }

}
