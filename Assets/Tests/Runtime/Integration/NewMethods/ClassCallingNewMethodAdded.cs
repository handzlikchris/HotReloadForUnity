using UnityEngine;

public class ClassCallingNewMethodAdded : MonoBehaviour
{
    private NewMethodAddedInClass NewMethodAddedInClass => _newMethodAddedInClass ?? (_newMethodAddedInClass = gameObject.AddComponent<NewMethodAddedInClass>());
    private NewMethodAddedInClass _newMethodAddedInClass;

    public void CallNewlyAddedHotReloadedClassMethod()
    {
        NewMethodAddedInClass.ExistingMethodBaselineCall();
        
        //<mock-runtime-code-change>// var result = NewMethodAddedInClass.NewMethod();    
        //<mock-runtime-code-change>// Debug.Log($"New Method returned {result}");
    }
}
