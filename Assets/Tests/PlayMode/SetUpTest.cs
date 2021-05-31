using System.Collections;
using System.Collections.Generic;
using animator;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SetUpTest
{
    [UnityTest]
    public IEnumerator SetUpTestWithEnumeratorPasses()
    {
        var animatiorObject = new GameObject();
        animatiorObject.AddComponent<PlayablesAnimator>();
        var animator = animatiorObject.GetComponent<PlayablesAnimator>();

        Command command1 = new Command() {
            AddOutput = new AddOutputCommand() {
                AnimationOutput = new AnimationOutput() {
                    Name = "output"
                }
            }
        };

        Command command2 = new Command() {
            AddInput = new AddInputCommand() {
                AnimationInput = new AnimationInput() {
                    AnimationMixer = new AnimationMixerInput(),
                    Name = "mixer1",
                    Parent = "output"
                }
            }
        };

        List<Command> commands = new List<Command>() { command1, command2 };

        animator.Setup(commands);

        yield return null;
        var graph = animator.GraphPeel();
        var outputs = graph.GetOutputCount();
        var inputs = graph.GetPlayableCount();
        Assert.AreEqual(2, outputs + inputs);
    }
}
