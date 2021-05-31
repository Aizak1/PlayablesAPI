using System.Collections;
using System.Collections.Generic;
using animator;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayModeTests {

    private List<Command> firstPreset = new List<Command>() {
         new Command() {
            AddOutput = new AddOutputCommand() {
                AnimationOutput = new AnimationOutput() {
                    Name = "output"
                }
            }
        },

       new Command() {
            AddInput = new AddInputCommand() {
                AnimationInput = new AnimationInput() {
                    AnimationMixer = new AnimationMixerInput(),
                    Name = "mixer1",
                    Parent = "output"
                }
            }
        }
    };

    private List<Command> secondPreset = new List<Command>() {
        new Command() {
            AddOutput = new AddOutputCommand() {
                AnimationOutput = new AnimationOutput() {
                    Name = "output"
                }
            }
        },

        new Command() {
            AddInput = new AddInputCommand() {
                AnimationInput = new AnimationInput() {
                    AnimationBrain = new AnimationBrainInput(),
                    Name = "brain",
                    Parent = "output"
                }
            }
        },

        new Command() {
            AddContoller = new AddControllerCommand() {
                ControllerInput = new AnimationControllerInput() {
                    ControllerType = "CloseCircle",
                    Name = "close1"
                }
            }
        },

        new Command() {
            AddContoller = new AddControllerCommand() {
                ControllerInput = new AnimationControllerInput() {
                    ControllerType = "CloseCircle",
                    Name = "close2"
                }
            }
        }
    };

    private List<Command> thirdPreset = new List<Command>() {

        new Command() {
            AddInput = new AddInputCommand() {
                AnimationInput = new AnimationInput() {
                    AnimationBrain = new AnimationBrainInput(),
                    Name = "brain",
                    Parent = "output"
                }
            }
        },

        new Command() {
            AddContoller = new AddControllerCommand() {
                ControllerInput = new AnimationControllerInput() {
                    ControllerType = "CloseCircle",
                    Name = "close1"
                }
            }
        },

        new Command() {
            AddContoller = new AddControllerCommand() {
                ControllerInput = new AnimationControllerInput() {
                    ControllerType = "CloseCircle",
                    Name = "close2"
                }
            }
        }

    };

    [UnityTest]
    public IEnumerator GraphSetUpTest() {
        var animatiorObject = new GameObject();
        animatiorObject.AddComponent<PlayablesAnimator>();
        var animator = animatiorObject.GetComponent<PlayablesAnimator>();


        animator.Setup(firstPreset);

        yield return null;
        var graph = animator.GraphPeel();
        var outputs = graph.GetOutputCount();
        var inputs = graph.GetPlayableCount();
        Assert.AreEqual(2, outputs + inputs);
    }

    [UnityTest]
    public IEnumerator GraphAddTest() {
        var animatiorObject = new GameObject();
        animatiorObject.AddComponent<PlayablesAnimator>();
        var animator = animatiorObject.GetComponent<PlayablesAnimator>();

        animator.Setup(firstPreset);
        animator.AddNewCommands(thirdPreset);

        yield return null;
        var graph = animator.GraphPeel();
        var outputs = graph.GetOutputCount();
        var inputs = graph.GetPlayableCount();
        Assert.AreEqual(3, outputs + inputs);
    }

    [UnityTest]
    public IEnumerator ControllerListTest() {
        var animatorObject = new GameObject();
        animatorObject.AddComponent<PlayablesAnimator>();
        var animator = animatorObject.GetComponent<PlayablesAnimator>();



        animator.Setup(secondPreset);

        yield return null;

        int count = animator.Brain.AnimControllers.Count;

        Assert.AreEqual(2, count);
    }


    [UnityTest]
    public IEnumerator ControllerListActivateTest() {
        var animatorObject = new GameObject();
        animatorObject.AddComponent<PlayablesAnimator>();
        var animator = animatorObject.GetComponent<PlayablesAnimator>();

        animator.Setup(secondPreset);

        yield return null;

        string[] names = new string[animator.Brain.ControllerNames.Count];

        for (int i = 0; i < names.Length; i++) {
            names[i] = animator.Brain.ControllerNames[i];
        }

        animator.Brain.ActivateControllers(names);

        bool isAllEnabled = true;

        foreach (var item in animator.Brain.ControllerNames) {
            if (!animator.Brain.AnimControllers[item].IsEnable) {
                isAllEnabled = false;
            }
        }

        Assert.AreEqual(true, isAllEnabled);
    }

    [UnityTest]
    public IEnumerator ControllerListDeactivateTest() {
        var animatorObject = new GameObject();
        animatorObject.AddComponent<PlayablesAnimator>();
        var animator = animatorObject.GetComponent<PlayablesAnimator>();


        animator.Setup(secondPreset);

        yield return null;

        animator.Brain.DeactivateAllControllers();

        bool isAllEnabled = false;

        foreach (var item in animator.Brain.ControllerNames) {
            if (animator.Brain.AnimControllers[item].IsEnable) {
                isAllEnabled = true;
            }
        }

        Assert.AreEqual(false, isAllEnabled);
    }
}
