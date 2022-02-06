# BehaviourTree
Simple node based Behaviour Tree implementation for AI programming in Unity
Made with Graph View

This is not a fully-supported project, I'm working on it in my free time.

---
How to use:
## Write your own tasks: 

Write simple task with fields
```csharp
using Recstazy.BehaviourTree;

[TaskOut] // This creates execution output so you can connect next task. 
[TaskMenu("My Awesome Tasks/Hello World")]
public class HelloWorld : BehaviourTask
{
    #region Fields

    // Fields just like you normally do
    [SerializeField]
    private string _logString = "Hello World";

    #endregion

    #region Properties

    #endregion

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
    #region Fields

    // Input connection pin with a bool value
    [SerializeField]
    private InputValue<bool> _bool;

    #endregion

    #region Properties

    #endregion

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
