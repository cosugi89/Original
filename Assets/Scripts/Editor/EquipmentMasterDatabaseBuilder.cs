#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Core;
using Assets.Scripts.Data.MasterData;
using LayerLab.ArtMakerUnity;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    public static class EquipmentMasterDatabaseBuilder
    {
        private const string CharacterPrefabPath = "Assets/Layer Lab/2D Minimal-CharacterMaker/Prefabs/Character.prefab";
        private const string DatabaseAssetPath = "Assets/Resources/MasterData/EquipmentDatabase.asset";
        private const string EquipmentsRootFolder = "Assets/Resources/MasterData/Equipments";

        [MenuItem("Tools/Original/Rebuild Equipment Database")]
        public static void Rebuild()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(CharacterPrefabPath);
            try
            {
                var partsManager = prefabRoot != null
                    ? prefabRoot.GetComponentInChildren<PartsManager>(true)
                    : null;
                if (partsManager == null)
                {
                    Debug.LogError($"[EquipmentMasterDatabaseBuilder] PartsManager was not found in {CharacterPrefabPath}.");
                    return;
                }

                partsManager.Init();
                EnsureFolderExists(EquipmentsRootFolder);

                var generatedAssets = new List<EquipmentMasterData>();
                var authorableTypes = partsManager.GetAllPartsTypes()
                    .Where(IsAuthorableType)
                    .Distinct()
                    .OrderBy(type => type)
                    .ToArray();
                var totalCount = authorableTypes.Sum(type => Mathf.Max(0, partsManager.GetPartsCount(type)));
                var completed = 0;

                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (var partType in authorableTypes)
                    {
                        var count = Mathf.Max(0, partsManager.GetPartsCount(partType));
                        if (count == 0)
                        {
                            continue;
                        }

                        var partFolder = $"{EquipmentsRootFolder}/{partType}";
                        EnsureFolderExists(partFolder);

                        for (var index = 0; index < count; index++)
                        {
                            completed++;
                            EditorUtility.DisplayProgressBar(
                                "Rebuild Equipment Database",
                                $"{partType} {index + 1}/{count}",
                                totalCount <= 0 ? 1f : (float)completed / totalCount);

                            var assetPath = $"{partFolder}/{BuildAssetName(partType, index)}.asset";
                            var asset = LoadOrCreateEquipmentAsset(assetPath);
                            UpdateEquipmentAsset(asset, partType, index, partsManager.GetThumbnail(partType, index));
                            generatedAssets.Add(asset);
                        }
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                    EditorUtility.ClearProgressBar();
                }

                var orderedAssets = generatedAssets
                    .Where(asset => asset != null)
                    .OrderBy(asset => asset.PartType)
                    .ThenBy(asset => asset.SortOrder > 0 ? asset.SortOrder : asset.PartsIndex)
                    .ThenBy(asset => asset.PartsIndex)
                    .ToList();
                var database = LoadOrCreateDatabase();
                UpdateDatabase(database, orderedAssets);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = database;
                EditorGUIUtility.PingObject(database);
                Debug.Log($"[EquipmentMasterDatabaseBuilder] Rebuilt {orderedAssets.Count} equipment appearance assets.");
            }
            finally
            {
                if (prefabRoot != null)
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }

        private static bool IsAuthorableType(PartsType partType)
        {
            return partType != PartsType.Arrow &&
                   partType != PartsType.HelmetHair &&
                   partType != PartsType.Skin;
        }

        private static string BuildAssetName(PartsType partType, int index)
        {
            return $"{partType}_{index:D3}";
        }

        private static EquipmentMasterData LoadOrCreateEquipmentAsset(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<EquipmentMasterData>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<EquipmentMasterData>();
            asset.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static EquipmentMasterDatabase LoadOrCreateDatabase()
        {
            EnsureFolderExists(Path.GetDirectoryName(DatabaseAssetPath)?.Replace("\\", "/") ?? "Assets");
            var database = AssetDatabase.LoadAssetAtPath<EquipmentMasterDatabase>(DatabaseAssetPath);
            if (database != null)
            {
                return database;
            }

            database = ScriptableObject.CreateInstance<EquipmentMasterDatabase>();
            database.name = Path.GetFileNameWithoutExtension(DatabaseAssetPath);
            AssetDatabase.CreateAsset(database, DatabaseAssetPath);
            return database;
        }

        private static void UpdateEquipmentAsset(
            EquipmentMasterData asset,
            PartsType partType,
            int partsIndex,
            Sprite icon)
        {
            if (asset == null)
            {
                return;
            }

            asset.name = BuildAssetName(partType, partsIndex);
            var serializedObject = new SerializedObject(asset);
            var equipmentIdProp = serializedObject.FindProperty("equipmentId");
            if (equipmentIdProp.intValue <= 0)
            {
                equipmentIdProp.intValue = EquipmentIdUtility.Build(partType, partsIndex);
            }

            var displayNameProp = serializedObject.FindProperty("displayName");
            if (string.IsNullOrWhiteSpace(displayNameProp.stringValue))
            {
                displayNameProp.stringValue = $"{partType} {partsIndex:D3}";
            }

            serializedObject.FindProperty("partType").enumValueIndex = (int)partType;
            serializedObject.FindProperty("partsIndex").intValue = partsIndex;
            serializedObject.FindProperty("icon").objectReferenceValue = icon;
            var sortOrderProp = serializedObject.FindProperty("sortOrder");
            if (sortOrderProp.intValue <= 0)
            {
                sortOrderProp.intValue = partsIndex;
            }
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static void UpdateDatabase(EquipmentMasterDatabase database, IReadOnlyList<EquipmentMasterData> orderedAssets)
        {
            if (database == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(database);
            var equipmentsProp = serializedObject.FindProperty("equipments");
            equipmentsProp.arraySize = orderedAssets?.Count ?? 0;
            for (var i = 0; i < equipmentsProp.arraySize; i++)
            {
                equipmentsProp.GetArrayElementAtIndex(i).objectReferenceValue = orderedAssets[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        private static void EnsureFolderExists(string assetFolderPath)
        {
            if (string.IsNullOrWhiteSpace(assetFolderPath) || AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            var normalized = assetFolderPath.Replace("\\", "/");
            var segments = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                return;
            }

            var current = segments[0];
            for (var i = 1; i < segments.Length; i++)
            {
                var next = $"{current}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }

                current = next;
            }
        }
    }
}
#endif
