#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IKhom.TemplateInstaller;

namespace IKhom.TemplateInstaller.Editor
{
    /// <summary>
    /// Generates Unity scenes with proper structure for each template type
    /// </summary>
    public class SceneGenerator
    {
        private readonly TemplateType _templateType;

        public SceneGenerator(TemplateType templateType, string rootNamespace)
        {
            _templateType = templateType;
        }

        /// <summary>
        /// Create a scene from definition
        /// </summary>
        public string CreateScene(SceneDefinition definition)
        {
            string fullPath = Path.Combine(Application.dataPath, definition.ScenePath, $"{definition.SceneName}.unity");
            string scenePath = $"Assets/{definition.ScenePath}/{definition.SceneName}.unity";

            // Check if scene already exists
            if (File.Exists(fullPath))
            {
                Debug.Log($"[SceneGenerator] Scene '{definition.SceneName}' already exists. Skipping.");
                return scenePath;
            }

            // Ensure directory exists
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory) && directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Add template-specific game objects
            SetupSceneContent(newScene, definition);

            // Save scene
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"[SceneGenerator] Created scene: {scenePath}");

            return scenePath;
        }

        private void SetupSceneContent(Scene scene, SceneDefinition definition)
        {
            switch (_templateType)
            {
                case TemplateType.SingleScene:
                    SetupSingleSceneContent(scene, definition);
                    break;

                case TemplateType.Modular:
                    SetupModularSceneContent(scene, definition);
                    break;

                case TemplateType.CleanArchitecture:
                    SetupCleanArchitectureSceneContent(scene, definition);
                    break;
            }
        }

        private void SetupSingleSceneContent(Scene scene, SceneDefinition definition)
        {
            // Add a root game object for organization
            GameObject root = new GameObject("--- Scene Root ---");
            SceneManager.MoveGameObjectToScene(root, scene);

            if (definition.IsBootstrapScene)
            {
                // Add BootstrapInstaller placeholder (will be replaced by actual component later)
                GameObject bootstrap = new GameObject("BootstrapInstaller");
                bootstrap.transform.SetParent(root.transform);
                AddCommentComponent(bootstrap, "BootstrapInstaller will be added after code generation");
            }

            // Add common scene objects
            GameObject systems = new GameObject("--- Systems ---");
            SceneManager.MoveGameObjectToScene(systems, scene);
            
            GameObject gameplay = new GameObject("--- Gameplay ---");
            SceneManager.MoveGameObjectToScene(gameplay, scene);
            
            GameObject ui = new GameObject("--- UI ---");
            SceneManager.MoveGameObjectToScene(ui, scene);
        }

        private void SetupModularSceneContent(Scene scene, SceneDefinition definition)
        {
            GameObject root = new GameObject($"--- {definition.SceneName} ---");
            SceneManager.MoveGameObjectToScene(root, scene);

            if (definition.IsBootstrapScene)
            {
                GameObject projectContext = new GameObject("ProjectContext");
                projectContext.transform.SetParent(root.transform);
                AddCommentComponent(projectContext, "ProjectInstaller will be added after code generation");
            }
            else
            {
                // For Persistent/Shell/Gameplay scenes
                GameObject sceneContext = new GameObject("SceneContext");
                sceneContext.transform.SetParent(root.transform);
                AddCommentComponent(sceneContext, $"{definition.SceneName}Installer will be added after code generation");
            }
        }

        private void SetupCleanArchitectureSceneContent(Scene scene, SceneDefinition definition)
        {
            // Similar to Modular but with additional organization
            SetupModularSceneContent(scene, definition);

            // Add additional markers for Clean Architecture layers
            GameObject infrastructure = new GameObject("--- Infrastructure ---");
            SceneManager.MoveGameObjectToScene(infrastructure, scene);
            
            GameObject presentation = new GameObject("--- Presentation ---");
            SceneManager.MoveGameObjectToScene(presentation, scene);
        }

        private void AddCommentComponent(GameObject go, string comment)
        {
            // Add a simple component that serves as documentation
            var commentComponent = go.AddComponent<SceneComment>();
            #if UNITY_EDITOR
            SerializedObject so = new SerializedObject(commentComponent);
            so.FindProperty("comment").stringValue = comment;
            so.ApplyModifiedProperties();
            #endif
        }
    }

    /// <summary>
    /// Simple component to hold comments in scenes
    /// </summary>
    public class SceneComment : MonoBehaviour
    {
        [TextArea(3, 10)]
        public string comment;
    }
}
#endif
