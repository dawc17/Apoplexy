using System;
using System.Collections.Generic;
using System.IO;
using Apoplexy.Levels;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Apoplexy.Editor
{
    public static class LegacyLevelImporter
    {
        private const string SceneDirectory =
            "Assets/Apoplexy/Scenes/Imported";

        [MenuItem("Assets/Apoplexy/Import Legacy Level")]
        private static void ImportSelectedLevel()
        {
            TextAsset source = Selection.activeObject as TextAsset;

            if (source == null)
            {
                return;
            }

            LegacyLevelData level;

            try
            {
                level = JsonConvert.DeserializeObject<LegacyLevelData>(
                    source.text);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "Level import failed",
                    "The selected JSON could not be parsed.",
                    "Close");
                return;
            }

            if (level == null)
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject root = new($"Level - {source.name}");

            Transform environment =
                CreateGroup("Environment", root.transform);

            Transform walls =
                CreateGroup("Walls", environment);

            Transform spawns =
                CreateGroup("Spawn Points", root.transform);

            Transform lights =
                CreateGroup("Lights", root.transform);

            Bounds levelBounds = ImportWalls(level.walls, walls);
            CreateFloor(levelBounds, environment);

            ImportSpawn(
                "Player Spawn",
                level.playerSpawn,
                SpawnPointType.Player,
                spawns);

            for (int index = 0; index < level.enemySpawns.Count; index++)
            {
                ImportSpawn(
                    $"Enemy Spawn {index + 1:00}",
                    level.enemySpawns[index],
                    SpawnPointType.Enemy,
                    spawns);
            }

            ImportLighting(level, lights);

            Directory.CreateDirectory(SceneDirectory);

            string scenePath =
                $"{SceneDirectory}/{source.name}.unity";

            EditorSceneManager.SaveScene(scene, scenePath);
            Selection.activeGameObject = root;

            Debug.Log(
                $"Imported {level.walls.Count} walls, " +
                $"{level.enemySpawns.Count} enemy spawns and " +
                $"{level.lights.Count} point lights into {scenePath}.");
        }

        [MenuItem(
            "Assets/Apoplexy/Import Legacy Level",
            true)]
        private static bool ValidateImportSelectedLevel()
        {
            if (Selection.activeObject is not TextAsset)
            {
                return false;
            }

            string path =
                AssetDatabase.GetAssetPath(Selection.activeObject);

            return path.EndsWith(
                ".json",
                StringComparison.OrdinalIgnoreCase);
        }

        private static Bounds ImportWalls(
            IReadOnlyList<LegacyWall> wallData,
            Transform parent)
        {
            bool hasBounds = false;
            Bounds combinedBounds = default;

            for (int index = 0; index < wallData.Count; index++)
            {
                LegacyWall data = wallData[index];

                Vector3 position = ToVector3(data.position);
                Vector3 size = ToVector3(data.size);

                GameObject wall =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);

                wall.name = $"Wall {index + 1:000}";
                wall.transform.SetParent(parent);
                wall.transform.position = position;
                wall.transform.localScale = size;

                GameObjectUtility.SetStaticEditorFlags(
                    wall,
                    StaticEditorFlags.BatchingStatic |
                    StaticEditorFlags.OccluderStatic |
                    StaticEditorFlags.OccludeeStatic |
                    StaticEditorFlags.ContributeGI |
                    StaticEditorFlags.NavigationStatic);

                Bounds wallBounds = new(position, size);

                if (!hasBounds)
                {
                    combinedBounds = wallBounds;
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(wallBounds);
                }
            }

            if (!hasBounds)
            {
                combinedBounds =
                    new Bounds(Vector3.zero, new Vector3(28f, 1f, 28f));
            }

            return combinedBounds;
        }

        private static void CreateFloor(
            Bounds levelBounds,
            Transform parent)
        {
            const float margin = 2f;
            const float thickness = 0.2f;

            GameObject floor =
                GameObject.CreatePrimitive(PrimitiveType.Cube);

            floor.name = "Floor";
            floor.transform.SetParent(parent);

            floor.transform.position = new Vector3(
                levelBounds.center.x,
                -thickness * 0.5f,
                levelBounds.center.z);

            floor.transform.localScale = new Vector3(
                Mathf.Max(levelBounds.size.x + margin * 2f, 28f),
                thickness,
                Mathf.Max(levelBounds.size.z + margin * 2f, 28f));

            GameObjectUtility.SetStaticEditorFlags(
                floor,
                StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.OccluderStatic |
                StaticEditorFlags.OccludeeStatic |
                StaticEditorFlags.ContributeGI |
                StaticEditorFlags.NavigationStatic);
        }

        private static void ImportSpawn(
            string name,
            float[] serializedPosition,
            SpawnPointType type,
            Transform parent)
        {
            GameObject spawn = new(name);

            spawn.transform.SetParent(parent);
            spawn.transform.position =
                ToVector3(serializedPosition);

            spawn.AddComponent<SpawnPoint>().Configure(type);
        }

        private static void ImportLighting(
            LegacyLevelData level,
            Transform parent)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;

            if (level.lighting != null)
            {
                RenderSettings.ambientLight =
                    ToColor(level.lighting.ambientColor) *
                    Mathf.Max(level.lighting.ambientIntensity, 0f);

                ImportSun(level.lighting.sun, parent);
            }

            for (int index = 0; index < level.lights.Count; index++)
            {
                LegacyPointLight data = level.lights[index];

                GameObject lightObject =
                    new($"Point Light {index + 1:00}");

                lightObject.transform.SetParent(parent);
                lightObject.transform.position =
                    ToVector3(data.position);

                Light light =
                    lightObject.AddComponent<Light>();

                light.type = LightType.Point;
                light.color = ToColor(data.color);
                light.intensity = Mathf.Max(data.intensity, 0f);
                light.range = Mathf.Max(data.radius, 0.01f);
                light.enabled = data.enabled;
                light.shadows = LightShadows.None;
            }
        }

        private static void ImportSun(
            LegacySun data,
            Transform parent)
        {
            if (data == null)
            {
                return;
            }

            Vector3 direction = ToVector3(data.direction);

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.down;
            }

            GameObject sunObject = new("Sun");
            sunObject.transform.SetParent(parent);

            sunObject.transform.rotation =
                Quaternion.LookRotation(-direction.normalized);

            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = ToColor(data.color);
            sun.intensity = Mathf.Max(data.intensity, 0f);
            sun.shadows = LightShadows.Soft;

            RenderSettings.sun = sun;
        }

        private static Transform CreateGroup(
            string name,
            Transform parent)
        {
            GameObject group = new(name);
            group.transform.SetParent(parent);
            return group.transform;
        }

        private static Vector3 ToVector3(float[] values)
        {
            if (values == null || values.Length < 3)
            {
                return Vector3.zero;
            }

            return new Vector3(values[0], values[1], values[2]);
        }

        private static Color ToColor(float[] values)
        {
            if (values == null || values.Length < 3)
            {
                return Color.white;
            }

            float alpha =
                values.Length >= 4 ? values[3] / 255f : 1f;

            return new Color(
                values[0] / 255f,
                values[1] / 255f,
                values[2] / 255f,
                alpha);
        }

        [Serializable]
        private sealed class LegacyLevelData
        {
            public float[] playerSpawn = Array.Empty<float>();
            public List<LegacyWall> walls = new();
            public List<LegacyDecal> wallDecals = new();
            public List<float[]> enemySpawns = new();
            public List<LegacyPointLight> lights = new();
            public LegacyLighting lighting;
        }

        [Serializable]
        private sealed class LegacyWall
        {
            public float[] position = Array.Empty<float>();
            public float[] size = Array.Empty<float>();
        }

        [Serializable]
        private sealed class LegacyDecal
        {
            public float[] position = Array.Empty<float>();
            public float[] normal = Array.Empty<float>();
            public float[] size = Array.Empty<float>();
            public string texture = string.Empty;
        }

        [Serializable]
        private sealed class LegacyPointLight
        {
            public float[] position = Array.Empty<float>();
            public float[] color = Array.Empty<float>();
            public float intensity = 1f;
            public float radius = 5f;
            public bool enabled = true;
        }

        [Serializable]
        private sealed class LegacyLighting
        {
            public float[] ambientColor = Array.Empty<float>();
            public float ambientIntensity = 1f;
            public LegacySun sun;
        }

        [Serializable]
        private sealed class LegacySun
        {
            public float[] direction = Array.Empty<float>();
            public float[] color = Array.Empty<float>();
            public float intensity = 1f;
        }
    }
}
