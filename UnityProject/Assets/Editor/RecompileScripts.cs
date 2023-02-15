 using UnityEditor;
 #if UNITY_2019_3_OR_NEWER
 using UnityEditor.Compilation;
 #elif UNITY_2017_1_OR_NEWER
 using System.Reflection;
 #endif
 using UnityEngine;
 
     public class RecompileScripts : EditorWindow
     {
         [MenuItem("Window/Pump Editor/Compilation")]
         private static void ShowWindow()
         {
             var window = EditorWindow.GetWindow<RecompileScripts>();
             window.titleContent = new GUIContent("Compilation");
             window.Show();
         }
 
         private void OnGUI()
         {
             if (GUILayout.Button("Request Script Compilation"))
             {
 #if UNITY_2019_3_OR_NEWER
                 CompilationPipeline.RequestScriptCompilation();
 #elif UNITY_2017_1_OR_NEWER
                 var editorAssembly = Assembly.GetAssembly(typeof(Editor));
                 var editorCompilationInterfaceType = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
                 var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod("DirtyAllScripts", BindingFlags.Static | BindingFlags.Public);
                 dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);
 #endif
             }
         }
     }




