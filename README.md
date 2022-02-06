# BehaviourTree
Simple node based Behaviour Tree implementation for AI programming in Unity
Made with Graph View

This is not a fully-supported project, I'm working on it in my free time.

![image](https://user-images.githubusercontent.com/30838103/152697189-e2e57eba-7e64-4567-bef2-b822c5cbe7eb.png)

---

## Watch the execution
* Use top toolbar to choose a target and see what happens in runtime

* Use "Window/Blackboard Watcher" to see or change values in the runtime blackboard instanced fot a target

## Write a blackboard

```csharp
using Recstazy.BehaviourTree;

[CreateAssetMenu(menuName = "Behaviour Tree/MyBlackboard")]
public class MyBlackboard : Blackboard
{
    // Use serialized fields to monitor values or assign them in editor
    [SerializeField]
    private bool _booleanValue;

    [SerializeField]
    private ScriptableObject _someScriptable;

    // Properties are what exposed to the behaviour tree editor window
    public bool BooleanValue { get => _booleanValue; set => _booleanValue = value; }

    // Complex things are supported too
    public ScriptableObject SomeScriptable { get => _someScriptable; set => _someScriptable = value; }

    // You don't technically need the serialized field
    public float FloatValue { get; set; }

    // Access modifiers also work
    public float OnlyGetFloat { get; private set; } // You won't be able to change this
    public float OnlySetFloat { private get; set; } // Or get this

    [HideInTree] // Don't expose this property to the behaviour tree window
    public float HiddenFloat { get; set; }

    // Why properties?
    // Because they work through delegates instead of FieldInfo.GetValue(obj)
    // 1200ms(field) vs 80ms(prop) for 1 million operations
}
```

## Write your own tasks: 

Write simple task with fields
```csharp
using Recstazy.BehaviourTree;

[TaskOut] // This creates executable output. 
[TaskMenu("My Awesome Tasks/Hello World")]
public class HelloWorld : BehaviourTask
{
    // Fields just like you normally do
    [SerializeField]
    private string _logString = "Hello World";

    // What we do in this task
    protected override IEnumerator TaskRoutine()
    {
        Debug.Log("Hello World");
        yield return new WaitForSeconds(1f);

        // You can fail task
        // Execution will stop here if Succeed == false
        // True by default

        // Some nodes like Selector and Sequencer
        // will use this result to make decisions
        Succeed = true;
    }
}
    
```

Write more complex task with inputs and different outputs
```csharp
using Recstazy.BehaviourTree;

[TaskMenu("Branch")]
[TaskOut("True"), TaskOut("False")] // Name your outputs
public class Branch : BehaviourTask
{
    // Input connection pin with a bool value
    [SerializeField]
    private InputValue<bool> _bool;

    protected override int GetNextOutIndex()
    {
        // Return out[0] (we named it "True") or out[1] ("False") depending on value
        // Execution will continue from this out
        return _bool.Value ? 0 : 1;
    }

    // This is not neccessary
    // default behavior is to wait one frame and Succeed
    protected override IEnumerator TaskRoutine()
    {
        yield break;
    }
}
```

Multiout task will generate outputs while you're connecting them. You can define concrete outputs as usual but there will be always one more pin to connect something

```csharp
/// <summary>
/// Executes random connection
/// </summary>
[TaskMenu("Tasks/Run Random")]
public class RunRandom : MultioutTask
{
    protected override int GetNextOutIndex()
    {
        return Random.Range(0, Connections.Count);
    }
}
```
