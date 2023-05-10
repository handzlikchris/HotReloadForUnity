using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FastScriptReload.Tests.Editor.Integration.CodePatterns;
using FastScriptReload.Tests.Runtime.Integration.CodePatterns;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class NewMethodAddedTests : CompileWithRedirectTestBase
{
    [UnityTest]
    public IEnumerator AssignNestedEnumToField_ValueAssignedDirectly_CorrectValueOnOriginalInstance()
    {
        var instance = new GameObject("instance").AddComponent<ClassCallingNewMethodAdded>();

        Assert.AreEqual(instance.WasExistingMethodBaselineCalled, false, "not called yet");
        instance.CallNewlyAddedHotReloadedClassMethod();
        Assert.AreEqual(instance.WasExistingMethodBaselineCalled, true, "baseline called");

        TestCompileAndDetour(ResolveFullTestFilePath(@"Runtime\Integration\NewMethods\NewMethodAddedInClass.cs"), (compilationResult) =>
        {
            var addedMethod = compilationResult.CompiledAssembly.GetTypes().FirstOrDefault(t => t.Name == nameof(NewMethodAddedInClass) + FastScriptReload.Runtime.AssemblyChangesLoader.ClassnamePatchedPostfix)
                .GetMethod("NewMethod", BindingFlags.Instance | BindingFlags.Public);
            
            Assert.IsNotNull(addedMethod, "method should be added to new type");
        });

        TestCompileAndDetour(ResolveFullTestFilePath(@"Runtime\Integration\NewMethods\ClassCallingNewMethodAdded.cs"), (compilationResult) =>
        {
            var err = compilationResult.IsError;
        });
        
        Assert.AreEqual(instance.WasNewMethodCalled, false, "not called yet");
        instance.CallNewlyAddedHotReloadedClassMethod();
        Assert.AreEqual(instance.WasNewMethodCalled, true, "newly added method called from hotreloaded class");
        
        yield return null;
    }
}
