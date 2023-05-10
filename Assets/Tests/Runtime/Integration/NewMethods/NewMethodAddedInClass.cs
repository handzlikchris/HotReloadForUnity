using UnityEngine;

public class NewMethodAddedInClass : MonoBehaviour
{
    //<mock-runtime-code-change>// public int NewMethod()
    //<mock-runtime-code-change>// {
    //<mock-runtime-code-change>//     Debug.Log("New Method Added at runtime called");
    //<mock-runtime-code-change>//     TestDetourConfirmation.Confirm(this.GetType(), nameof(NewMethod));
    //<mock-runtime-code-change>//     return 1;
    //<mock-runtime-code-change>// }

    public void ExistingMethodBaselineCall()
    {
        Debug.Log("Existing Method called"); 
    }
}
