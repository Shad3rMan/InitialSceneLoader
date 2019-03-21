using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

    [InitializeOnLoad]
    internal static class InitialSceneLoader
    {
        private const string SHOULD_LOAD_INITIAL_SCENE_KEY = "ShouldLoadInitialScene";
        private const string SHOULD_LOAD_PREVIOUS_SCENE_KEY = "ShouldLoadPreviousScene";
        private const string INITIAL_SCENE_INDEX_KEY = "InitialSceneIndex";
        private const string PREVIOUS_SCENE_PATH_KEY = "PreviousScenePath";

        private static bool shouldLoadInitialScene = false;
        private static bool shouldLoadPreviousScene = false;
        private static int sceneToLoad = 0;

        static InitialSceneLoader()
        {
            shouldLoadInitialScene = EditorPrefs.GetBool(SHOULD_LOAD_INITIAL_SCENE_KEY, false);
            shouldLoadPreviousScene = EditorPrefs.GetBool(SHOULD_LOAD_PREVIOUS_SCENE_KEY, false);
            sceneToLoad = EditorPrefs.GetInt(INITIAL_SCENE_INDEX_KEY, 0);
            EditorApplication.playmodeStateChanged += EditorPlayModeChanged;
        }

        [PreferenceItem("Scenes")]
        private static void PreferencesGUI()
        {
            shouldLoadInitialScene = EditorGUILayout.Toggle("Should load initial scene", shouldLoadInitialScene);
            GUI.enabled = shouldLoadInitialScene;
            shouldLoadPreviousScene = EditorGUILayout.Toggle("Should load previous scene", shouldLoadPreviousScene);
            sceneToLoad = EditorGUILayout.Popup("Initial scene to load", sceneToLoad, GetScenes());

            if (GUI.changed)
            {
                EditorPrefs.SetBool(SHOULD_LOAD_INITIAL_SCENE_KEY, shouldLoadInitialScene);
                EditorPrefs.SetBool(SHOULD_LOAD_PREVIOUS_SCENE_KEY, shouldLoadPreviousScene);
                EditorPrefs.SetInt(INITIAL_SCENE_INDEX_KEY, sceneToLoad);
            }
        }

        private static string[] GetScenes()
        {
            string[] scenes = new string[EditorBuildSettings.scenes.Length];

            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);
            }

            return scenes;
        }

        private static void EditorPlayModeChanged()
        {
            if (shouldLoadInitialScene)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    if (!EditorApplication.isPlaying)
                    {
                        EditorPrefs.SetString(PREVIOUS_SCENE_PATH_KEY, SceneManager.GetActiveScene().path);
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[sceneToLoad].path);
                        }
                        else
                        {
                            EditorApplication.isPlaying = false;
                        }
                    }
                }
                else
                {
                    if (shouldLoadPreviousScene && !EditorApplication.isPlaying)
                    {
                        var lastScenePath = EditorPrefs.GetString(PREVIOUS_SCENE_PATH_KEY, string.Empty);
                        if (!string.IsNullOrEmpty(lastScenePath) && !lastScenePath.Equals(EditorSceneManager.GetActiveScene().path))
                        {
                            EditorSceneManager.OpenScene(lastScenePath);
                        }
                    }
                }
            }
        }
    }