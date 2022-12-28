﻿using System;
using System.Collections.Generic;
using System.Linq;
using FastScriptReload.Editor.Compilation;
using FastScriptReload.Runtime;
using ImmersiveVRTools.Editor.Common.Utilities;
using ImmersiveVRTools.Editor.Common.WelcomeScreen;
using ImmersiveVRTools.Editor.Common.WelcomeScreen.GuiElements;
using ImmersiveVRTools.Editor.Common.WelcomeScreen.PreferenceDefinition;
using ImmersiveVRTools.Editor.Common.WelcomeScreen.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FastScriptReload.Editor
{
    public class FastScriptReloadWelcomeScreen : ProductWelcomeScreenBase
    {
        public static string BaseUrl = "https://immersivevrtools.com";
        public static string GenerateGetUpdatesUrl(string userId, string versionId)
        {
            return $"{BaseUrl}/updates/fast-script-reload/{userId}?CurrentVersion={versionId}";
        }
        public static string VersionId = "1.0";
        private static readonly string ProjectIconName = "ProductIcon64";
        public static readonly string ProjectName = "fast-script-reload";

        private static Vector2 _WindowSizePx = new Vector2(650, 500);
        private static string _WindowTitle = "Fast Script Reload";

        public static ChangeMainViewButton ExclusionsSection { get; private set; }

        public void OpenExclusionsSection()
        {
            ExclusionsSection.OnClick(this);
        }

        private static readonly List<GuiSection> LeftSections = CreateLeftSections(new List<ChangeMainViewButton>
            {
                new ChangeMainViewButton("On-Device\r\nHot-Reload",  
                    (screen) =>
                    {
                        EditorGUILayout.LabelField("Live Script Reload", screen.BoldTextStyle); 
                        
                        GUILayout.Space(10);
                        EditorGUILayout.LabelField(@"There's an extension to this asset that'll allow you to include Hot-Reload capability in builds (standalone / Android), please click the button below to learn more.", screen.TextStyle);

                        GUILayout.Space(20);
                        if (GUILayout.Button("View Live Script Reload on Asset Store"))
                        {
                            Application.OpenURL($"{RedirectBaseUrl}/live-script-reload-extension");
                        }
                    }
                )
            }, 
            new LaunchSceneButton("Basic Example", (s) => GetScenePath("ExampleScene"), (screen) =>
            {
                GUILayout.Label(
                    $@"Asset is very simple to use:

1) Hit play to start.
2) Go to 'FunctionLibrary.cs' ({@"Assets/FastScriptReload/Examples/Scripts/"})", screen.TextStyle);
                
                CreateOpenFunctionLibraryOnRippleMethodButton();

                
                GUILayout.Label(
                    $@"3) Change 'Ripple' method (eg change line before return statement to 'p.z = v * 10'
4) Save file
5) See change immediately",
                    screen.TextStyle
                );
                
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("There are some limitations to what can be Hot-Reloaded, documentation lists them under 'limitations' section.", MessageType.Warning);
            }));

        protected static List<GuiSection> CreateLeftSections(List<ChangeMainViewButton> additionalSections, LaunchSceneButton launchSceneButton)
        {
            return new List<GuiSection>() {
                new GuiSection("", new List<ClickableElement>
                {
                    new LastUpdateButton("New Update!", (screen) => LastUpdateUpdateScrollViewSection.RenderMainScrollViewSection(screen)),
                    new ChangeMainViewButton("Welcome", (screen) => MainScrollViewSection.RenderMainScrollViewSection(screen)),
                }),
                new GuiSection("Options", new List<ClickableElement>
                {
                    new ChangeMainViewButton("Reload", (screen) =>
                    {
                        const int sectionBreakHeight = 15;
                        GUILayout.Label(
                            @"Asset watches all script files and automatically hot-reloads on change, you can disable that behaviour and reload on demand.",
                            screen.TextStyle
                        );
                
                        using (LayoutHelper.LabelWidth(300))
                        {
                            ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.EnableAutoReloadForChangedFiles);
                        }
                        GUILayout.Space(sectionBreakHeight);
                
                        EditorGUILayout.HelpBox("On demand reload:\r\nvia Window -> Fast Script Reload -> Force Reload, \r\nor by calling 'FastScriptIterationManager.Instance.TriggerReloadForChangedFiles()'", MessageType.Info);
                        GUILayout.Space(sectionBreakHeight);
                
                        GUILayout.Label(
                            @"For performance reasons script changes are batched are reloaded every N seconds",
                            screen.TextStyle
                        );

                        using (LayoutHelper.LabelWidth(300))
                        {
                            ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.BatchScriptChangesAndReloadEveryNSeconds);
                        }

                        GUILayout.Space(sectionBreakHeight);
                    
                        using (LayoutHelper.LabelWidth(350))
                        {
                            ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.EnableExperimentalThisCallLimitationFix);
                        }
                        EditorGUILayout.HelpBox("Method calls utilizing 'this' will trigger compiler exception, if enabled tool will rewrite the calls to have proper type after adjustments." +
                                                "\r\n\r\nIn case you're seeing compile errors relating to 'this' keyword please let me know via support page. Also turning this setting off will prevent rewrite.", MessageType.Info);
                        
                        GUILayout.Space(sectionBreakHeight);
                        using (LayoutHelper.LabelWidth(350))
                        {
                            ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.IsDidFieldsOrPropertyCountChangedCheckDisabled);
                        }
                        EditorGUILayout.HelpBox("By default if you add / remove fields, tool will not redirect method calls for recompiled class." +
                                                "\r\nThis is to ensure there are no issues as that is generally not supported." +
                                                "\r\n\r\nSome assets however will use IL weaving to adjust your classes (eg Mirror) as a post compile step. In that case it's quite likely hot-reload will still work. " +
                                                "\r\n\r\nTick this box for tool to try and reload changes when that happens."
                            
                            , MessageType.Info);

                    }),
                    (ExclusionsSection = new ChangeMainViewButton("Exclusions", (screen) => 
                    {
                        EditorGUILayout.HelpBox("Those are easiest to manage from Project window by right clicking on script file and selecting: " +
                                                "\r\nFast Script Reload -> Add Hot-Reload Exclusion " +
                                                "\r\nFast Script Reload -> Remove Hot-Reload Exclusion", MessageType.Info);
                        GUILayout.Space(10);
                
                        ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.FilesExcludedFromHotReload);
                    })),
                    new ChangeMainViewButton("Logging", (screen) => 
                    {
                        using (LayoutHelper.LabelWidth(350))
                        {
                            ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.LogHowToFixMessageOnCompilationError);
                            ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.StopShowingAutoReloadEnabledDialogBox);
                        }
                    }),
                    new ChangeMainViewButton("File Watcher\r\n(Advanced Setup)", (screen) => 
                    {
                        EditorGUILayout.HelpBox(
$@"Asset watches .cs files for changes. Unfortunately Unity's FileWatcher 
implementation has some performance issues.

By default all project directories can be watched, you can adjust that here.

path - which directory to watch
filter - narrow down files to match filter, eg all *.cs files (*.cs)
includeSubdirectories - whether child directories should be watched as well

{FastScriptReloadManager.FileWatcherReplacementTokenForApplicationDataPath} - you can use that token and it'll be replaced with your /Assets folder"
, MessageType.Info);
                        
                        EditorGUILayout.HelpBox("Recompile after making changes for file watchers to re-load.", MessageType.Warning);
                        
                        ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.FileWatcherSetupEntries);
                    })
                }.Concat(additionalSections).ToList()),
                new GuiSection("Launch Demo", new List<ClickableElement>
                {
                    launchSceneButton
                })
            };
        }

        private static readonly string RedirectBaseUrl = "https://immersivevrtools.com/redirect/fast-script-reload"; 
        private static readonly GuiSection TopSection = new GuiSection("Support", new List<ClickableElement>
            {
                new OpenUrlButton("Documentation", $"{RedirectBaseUrl}/documentation"),
                new OpenUrlButton("Discord", $"{RedirectBaseUrl}/discord"),
                new OpenUrlButton("Unity Forum", $"{RedirectBaseUrl}/unity-forum"),
                new OpenUrlButton("Contact", $"{RedirectBaseUrl}/contact")
            }
        );

        private static readonly GuiSection BottomSection = new GuiSection(
            "I want to make this tool better. And I need your help!",
            $"It'd be great if you could share your feedback (good and bad) with me. I'm very keen to make this tool better and that can only happen with your help. Please use:",
            new List<ClickableElement>
            {
                new OpenUrlButton(" Unity Forum", $"{RedirectBaseUrl}/unity-forum"),
                new OpenUrlButton(" or Write a Short Review", $"{RedirectBaseUrl}/asset-store-review"),
            }
        );

        private static readonly ScrollViewGuiSection MainScrollViewSection = new ScrollViewGuiSection(
            "", (screen) =>
            {
                GenerateCommonWelcomeText(FastScriptReloadPreference.ProductName, screen);

                GUILayout.Label("Quick adjustments:", screen.LabelStyle);
                using (LayoutHelper.LabelWidth(350))
                {
                    ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.BatchScriptChangesAndReloadEveryNSeconds);
                    ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.EnableAutoReloadForChangedFiles);
                    ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.EnableExperimentalThisCallLimitationFix);
                    ProductPreferenceBase.RenderGuiAndPersistInput(FastScriptReloadPreference.LogHowToFixMessageOnCompilationError);
                }
            }
        );

        private static readonly ScrollViewGuiSection LastUpdateUpdateScrollViewSection = new ScrollViewGuiSection(
            "New Update", (screen) =>
            {
                GUILayout.Label(screen.LastUpdateText, screen.BoldTextStyle, GUILayout.ExpandHeight(true));
            }
        );

        public override string WindowTitle { get; } = _WindowTitle;
        public override Vector2 WindowSizePx { get; } = _WindowSizePx;

#if !LiveScriptReload_Enabled
        [MenuItem("Window/Fast Script Reload/Start Screen", false, 1999)]
#endif
        public static FastScriptReloadWelcomeScreen Init()
        {
            return OpenWindow<FastScriptReloadWelcomeScreen>(_WindowTitle, _WindowSizePx);
        }
    
#if !LiveScriptReload_Enabled
        [MenuItem("Window/Fast Script Reload/Force Reload", true, 1999)]
#endif
        public static bool ForceReloadValidate()
        {
            return EditorApplication.isPlaying;
        }
    
#if !LiveScriptReload_Enabled
        [MenuItem("Window/Fast Script Reload/Force Reload", false, 1999)]
#endif
        public static void ForceReload()
        {
            FastScriptReloadManager.Instance.TriggerReloadForChangedFiles();
        }

        public void OnEnable()
        {
            OnEnableCommon(ProjectIconName);
        }

        public void OnGUI()
        {
            RenderGUI(LeftSections, TopSection, BottomSection, MainScrollViewSection);
        }
        
        protected static void CreateOpenFunctionLibraryOnRippleMethodButton()
        {
            if (GUILayout.Button("Open 'FunctionLibrary.cs'"))
            {
                var codeComponent = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"t:Script FunctionLibrary")[0]));
                CodeEditorManager.GotoScript(codeComponent, "Ripple");
            }
        }
    }

    public class FastScriptReloadPreference : ProductPreferenceBase
    {
        public const string ProductName = "Fast Script Reload";
        private static string[] ProductKeywords = new[] { "productivity", "tools" };

        public static readonly IntProjectEditorPreferenceDefinition BatchScriptChangesAndReloadEveryNSeconds = new IntProjectEditorPreferenceDefinition(
            "Batch script changes and reload every N seconds", "BatchScriptChangesAndReloadEveryNSeconds", 3);

        public static readonly ToggleProjectEditorPreferenceDefinition EnableAutoReloadForChangedFiles = new ToggleProjectEditorPreferenceDefinition(
            "Enable auto Hot-Reload for changed files", "EnableAutoReloadForChangedFiles", true);
        
        public static readonly ToggleProjectEditorPreferenceDefinition EnableExperimentalThisCallLimitationFix = new ToggleProjectEditorPreferenceDefinition(
            "(Experimental) Enable method calls with 'this' as argument fix", "EnableExperimentalThisCallLimitationFix", true, (object newValue, object oldValue) =>
            {
                DynamicCompilationBase.EnableExperimentalThisCallLimitationFix = (bool)newValue;
            },
            (value) =>
            {
                DynamicCompilationBase.EnableExperimentalThisCallLimitationFix = (bool)value;
            });
    
        public static readonly StringListProjectEditorPreferenceDefinition FilesExcludedFromHotReload = new StringListProjectEditorPreferenceDefinition(
            "Files excluded from Hot-Reload", "FilesExcludedFromHotReload", new List<string> {}, isReadonly: true);
        
        public static readonly ToggleProjectEditorPreferenceDefinition LogHowToFixMessageOnCompilationError = new ToggleProjectEditorPreferenceDefinition(
            "Log how to fix message on compilation error", "LogHowToFixMessageOnCompilationError", true, (object newValue, object oldValue) =>
            {
                DynamicCompilationBase.LogHowToFixMessageOnCompilationError = (bool)newValue;
            },
            (value) =>
            {
                DynamicCompilationBase.LogHowToFixMessageOnCompilationError = (bool)value;
            }
        );
        
        public static readonly ToggleProjectEditorPreferenceDefinition StopShowingAutoReloadEnabledDialogBox = new ToggleProjectEditorPreferenceDefinition(
            "Stop showing assets/script auto-reload enabled warning", "StopShowingAutoReloadEnabledDialogBox", false);
        
        public static readonly ToggleProjectEditorPreferenceDefinition IsDidFieldsOrPropertyCountChangedCheckDisabled = new ToggleProjectEditorPreferenceDefinition(
            "Disable added/removed fields check", "IsDidFieldsOrPropertyCountChangedCheckDisabled", false,
            (object newValue, object oldValue) =>
            {
                AssemblyChangesLoader.IsDidFieldsOrPropertyCountChangedCheckDisabled = (bool)newValue;
            },
            (value) =>
            {
                AssemblyChangesLoader.IsDidFieldsOrPropertyCountChangedCheckDisabled = (bool)value;
            }
        );
        
        public static readonly JsonObjectListProjectEditorPreferenceDefinition<FileWatcherSetupEntry> FileWatcherSetupEntries = new JsonObjectListProjectEditorPreferenceDefinition<FileWatcherSetupEntry>(
            "File Watchers Setup", "FileWatcherSetupEntries", new List<string>
            {
                JsonUtility.ToJson(new FileWatcherSetupEntry("<Application.dataPath>", "*.cs", true))
            }, 
            () => new FileWatcherSetupEntry("<Application.dataPath>", "*.cs", true)
        );
        
        public static void SetCommonMaterialsShader(ShadersMode newShaderModeValue)
        {
            var rootToolFolder = AssetPathResolver.GetAssetFolderPathRelativeToScript(ScriptableObject.CreateInstance(typeof(FastScriptReloadWelcomeScreen)), 1);
            if (rootToolFolder.Contains("/Scripts"))
            {
                rootToolFolder = rootToolFolder.Replace("/Scripts", ""); //if nested remove that and go higher level
            }
            var assets = AssetDatabase.FindAssets("t:Material Point", new[] { rootToolFolder });

            try
            {
                Shader shaderToUse = null;
                switch (newShaderModeValue)
                {
                    case ShadersMode.HDRP: shaderToUse = Shader.Find("Shader Graphs/Point URP"); break;
                    case ShadersMode.URP: shaderToUse = Shader.Find("Shader Graphs/Point URP"); break;
                    case ShadersMode.Surface: shaderToUse = Shader.Find("Graph/Point Surface"); break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                foreach (var guid in assets)
                {
                    var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                    if (material.shader.name != shaderToUse.name)
                    {
                        material.shader = shaderToUse;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Shader does not exist: {ex.Message}");
            }
        }

        public static List<ProjectEditorPreferenceDefinitionBase> PreferenceDefinitions = new List<ProjectEditorPreferenceDefinitionBase>()
        {
            CreateDefaultShowOptionPreferenceDefinition(),
            BatchScriptChangesAndReloadEveryNSeconds,
            EnableAutoReloadForChangedFiles,
            EnableExperimentalThisCallLimitationFix,
            LogHowToFixMessageOnCompilationError,
            StopShowingAutoReloadEnabledDialogBox,
            IsDidFieldsOrPropertyCountChangedCheckDisabled,
            FileWatcherSetupEntries
        };

        private static bool PrefsLoaded = false;


#if !LiveScriptReload_Enabled
    #if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider ImpostorsSettings()
        {
            return GenerateProvider(ProductName, ProductKeywords, PreferencesGUI);
        }

    #else
	[PreferenceItem(ProductName)]
    #endif
#endif
        public static void PreferencesGUI()
        {
            if (!PrefsLoaded)
            {
                LoadDefaults(PreferenceDefinitions);
                PrefsLoaded = true;
            }

            RenderGuiCommon(PreferenceDefinitions);
        }

        public enum ShadersMode
        {
            HDRP,
            URP,
            Surface
        }
    }

#if !LiveScriptReload_Enabled
    [InitializeOnLoad]
#endif
    public class FastScriptReloadWelcomeScreenInitializer : WelcomeScreenInitializerBase
    {
#if !LiveScriptReload_Enabled
        static FastScriptReloadWelcomeScreenInitializer()
        {
            var userId = ProductPreferenceBase.CreateDefaultUserIdDefinition(FastScriptReloadWelcomeScreen.ProjectName).GetEditorPersistedValueOrDefault().ToString();

            HandleUnityStartup(
                () => FastScriptReloadWelcomeScreen.Init(),
                FastScriptReloadWelcomeScreen.GenerateGetUpdatesUrl(userId, FastScriptReloadWelcomeScreen.VersionId),
                new List<ProjectEditorPreferenceDefinitionBase>(),
                (isFirstRun) =>
                {
                    AutoDetectAndSetShaderMode();
                }
            );
            
            InitCommon();
        }
#endif
        
        protected static void InitCommon()
        {
            DisplayMessageIfLastDetourPotentiallyCrashedEditor();
            EnsureUserAwareOfAutoRefresh();

            DynamicCompilationBase.LogHowToFixMessageOnCompilationError = (bool)FastScriptReloadPreference.LogHowToFixMessageOnCompilationError.GetEditorPersistedValueOrDefault();
            AssemblyChangesLoader.IsDidFieldsOrPropertyCountChangedCheckDisabled = (bool)FastScriptReloadPreference.IsDidFieldsOrPropertyCountChangedCheckDisabled.GetEditorPersistedValueOrDefault();
        }

        private static void EnsureUserAwareOfAutoRefresh()
        {
            var autoRefreshMode = (AssetPipelineAutoRefreshMode)EditorPrefs.GetInt("kAutoRefreshMode", EditorPrefs.GetBool("kAutoRefresh") ? 1 : 0);
            if (autoRefreshMode == AssetPipelineAutoRefreshMode.Enabled)
            {
                Debug.LogWarning("Fast Script Reload - asset auto refresh enabled - full reload will be triggered unless editor preference adjusted - see documentation for more details.");

                if (!(bool)FastScriptReloadPreference.StopShowingAutoReloadEnabledDialogBox.GetEditorPersistedValueOrDefault())
                {
                    var chosenOption = EditorUtility.DisplayDialogComplex("Fast Script Reload - Warning",
                        "Auto reload for assets/scripts is enabled." +
                        $"\n\nThis means any change made in playmode will likely trigger full recompile." +
                        $"\r\n\r\nIt's an editor setting and can be adjusted at any time via Edit -> Preferences -> Asset Pipeline -> Auto Refresh" +
                        $"\r\n\r\nI can also adjust that for you now - that means you'll need to manually load changes (outside of playmode) via Assets -> Refresh (CTRL + R)." +
                        $"\r\n\r\nIn some editor versions you can also set script compilation to happen only outside of playmode. " +
                        $"\r\n\r\nDepending on version you'll find it via: " +
                        $"\r\n1) Edit -> Preferences -> General -> Script Changes While Playing -> Recompile After Finished Playing." +
                        $"\r\n2) Edit -> Preferences -> Asset Pipeline -> Auto Refresh -> Enabled Outside Playmode",
                        "Ok, disable asset auto refresh (I'll refresh manually when needed)",
                        "No, don't change (stop showing this message)",
                        "No, don't change"
                    );

                    switch (chosenOption)
                    {
                        // change.
                        case 0:
                            EditorPrefs.SetInt("kAutoRefreshMode", (int)AssetPipelineAutoRefreshMode.Disabled);
                            EditorPrefs.SetInt("kAutoRefresh", 0); //older unity versions
                            break;

                        // don't change and stop showing message.
                        case 1:
                            FastScriptReloadPreference.StopShowingAutoReloadEnabledDialogBox.SetEditorPersistedValue(true);

                            break;

                        // don't change
                        case 2:

                            break;

                        default:
                            Debug.LogError("Unrecognized option.");
                            break;
                    }
                }
            }
        }

        //copied from internal UnityEditor.AssetPipelineAutoRefreshMode
        internal enum AssetPipelineAutoRefreshMode
        {
            Disabled,
            Enabled,
            EnabledOutsidePlaymode,
        }

        private static void DisplayMessageIfLastDetourPotentiallyCrashedEditor()
        {
            const string firstInitSessionKey = "FastScriptReloadWelcomeScreenInitializer_FirstInitDone";
            if (!SessionState.GetBool(firstInitSessionKey, false))
            {
                SessionState.SetBool(firstInitSessionKey, true);

                var lastDetour = DetourCrashHandler.RetrieveLastDetour();
                if (!string.IsNullOrEmpty(lastDetour))
                {
                    EditorUtility.DisplayDialog("Fast Script Reload",
                        $@"That's embarrassing!

It seems like I've crashed your editor, sorry!

Last detoured method was: '{lastDetour}'

If this happens again, please reach out via support and we'll sort it out.

In the meantime, you can exclude any file from Hot-Reload by 
1) right-clicking on .cs file in Project menu
2) Fast Script Reload 
3) Add Hot-Reload Exclusion
", "Ok");
                    DetourCrashHandler.ClearDetourLog();
                }
            }
        }

        protected static void AutoDetectAndSetShaderMode()
        {
            var usedShaderMode = FastScriptReloadPreference.ShadersMode.Surface;
            var renderPipelineAsset = GraphicsSettings.renderPipelineAsset;
            if (renderPipelineAsset == null)
            {
                usedShaderMode = FastScriptReloadPreference.ShadersMode.Surface;
            }
            else if (renderPipelineAsset.GetType().Name.Contains("HDRenderPipelineAsset"))
            {
                usedShaderMode = FastScriptReloadPreference.ShadersMode.HDRP;
            }
            else if (renderPipelineAsset.GetType().Name.Contains("UniversalRenderPipelineAsset"))
            {
                usedShaderMode = FastScriptReloadPreference.ShadersMode.URP;
            }
        
            FastScriptReloadPreference.SetCommonMaterialsShader(usedShaderMode);
        }
    }
}