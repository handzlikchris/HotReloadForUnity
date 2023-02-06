using System.Collections.Generic;
using FastScriptReload.Editor;
using FastScriptReload.Editor.Compilation;
using UnityEditor;
using UnityEngine;

public class RecompileTest : MonoBehaviour
{
    [MenuItem("DEBUG/TestRecompile #F1")]
    static void TestRecompile_Remove()  //TODO: remove 
    {
        // FastScriptReloadManager.Instance.EnableExperimentalThisCallLimitationFix = true;
        var result = DynamicAssemblyCompiler.Compile(new List<string>()
        {
        //     @"E:\_src-unity\FastScriptReload\Assets\Scripts\CompilationTestClass.cs",
        //     @"E:\_src-unity\FastScriptReload\Assets\FastScriptReload\Examples\Scripts\FunctionLibrary.cs",
            @"E:\_src-unity\FastScriptReload\Assets\Scripts\NestedStructTest.cs",
        });
        
        Debug.Log(result.SourceCodeCombined);
    }
}
