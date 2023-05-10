using UnityEngine;

public class ClassCallingNewMethodAdded : MonoBehaviour
{
    private NewMethodAddedInClass//<mock-runtime-code-change>//__Patched_
        NewMethodAddedInClass => _newMethodAddedInClass ??= gameObject.AddComponent<NewMethodAddedInClass//<mock-runtime-code-change>//__Patched_
    >();
    private NewMethodAddedInClass//<mock-runtime-code-change>//__Patched_
        _newMethodAddedInClass;
    
    public bool WasExistingMethodBaselineCalled => NewMethodAddedInClass.WasExistingMethodBaselineCalled;
    public bool WasNewMethodCalled => NewMethodAddedInClass.WasNewMethodCalled;

    public void CallNewlyAddedHotReloadedClassMethod()
    {
        NewMethodAddedInClass.ExistingMethodBaselineCall();
        
        //<mock-runtime-code-change>// var result = NewMethodAddedInClass.NewMethod();    
        //<mock-runtime-code-change>// Debug.Log($"New Method returned {result}");
    }
}
