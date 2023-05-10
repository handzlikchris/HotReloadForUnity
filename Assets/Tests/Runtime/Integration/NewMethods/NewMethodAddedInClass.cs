using UnityEngine;

public class NewMethodAddedInClass : MonoBehaviour
{
    public bool WasExistingMethodBaselineCalled;
    public bool WasNewMethodCalled;
    
    //<mock-runtime-code-change>// public int NewMethod()
    //<mock-runtime-code-change>// {
    //<mock-runtime-code-change>//     Debug.Log("New Method Added at runtime called");
    //<mock-runtime-code-change>//     WasNewMethodCalled = true;
    //<mock-runtime-code-change>//     return 1;
    //<mock-runtime-code-change>// }

    public void ExistingMethodBaselineCall()
    {
        WasExistingMethodBaselineCalled = true;
        Debug.Log("Existing Method called");
    }
}
