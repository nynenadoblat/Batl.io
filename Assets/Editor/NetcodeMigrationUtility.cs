using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace BatlIO.Editor
{
    public static class NetcodeMigrationUtility
    {
        [MenuItem("Netcode/Run Complete Migration")]
        public static void RunCompleteMigration()
        {
            Debug.Log("[NetcodeMigration] Starting Netcode migration process...");

            try
            {
                AssetDatabase.StartAssetEditing();

                MigratePrefabs();

                var prefabsList = CreateOrUpdateNetworkPrefabsList();

                ConfigureMenuScene(prefabsList);

                ConfigureGameScene();

                Debug.Log("[NetcodeMigration] Netcode migration completed successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetcodeMigration] Error during migration: {ex}");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static void MigratePrefabs()
        {
            string[] networkedPrefabPaths = new string[]
            {
                "Assets/Prefab/Resources/Player.prefab",
                "Assets/Prefab/Resources/AI Player.prefab",
                "Assets/Prefab/Resources/Creep.prefab",
                "Assets/Prefab/Resources/Rune.prefab",
                "Assets/Prefab/Resources/Xeroboro.prefab",
                "Assets/Prefab/Resources/Xeroboro 2.prefab",
                "Assets/Prefab/Resources/Xeroboro 3.prefab",
                "Assets/Prefab/Resources/Xeroboro 4.prefab",
                "Assets/Prefab/Resources/Xeroboro 5.prefab",
                "Assets/Prefab/Portal Variant.prefab"
            };

            foreach (var path in networkedPrefabPaths)
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[NetcodeMigration] Prefab not found: {path}");
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(path);
                if (root == null)
                    continue;

                RemovePhotonComponents(root);

                if (root.GetComponent<NetworkObject>() == null)
                {
                    root.AddComponent<NetworkObject>();
                    Debug.Log($"[NetcodeMigration] Added NetworkObject to {path}");
                }

                // Check NetworkTransform for movable prefabs
                if (path.Contains("Player") || path.Contains("Creep") || path.Contains("Xeroboro"))
                {
                    if (root.GetComponent<NetworkTransform>() == null)
                    {
                        root.AddComponent<NetworkTransform>();
                        Debug.Log($"[NetcodeMigration] Added NetworkTransform to {path}");
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }

            // Projectile prefabs (local visual effects, remove PhotonView)
            string[] projectilePaths = new string[]
            {
                "Assets/Prefab/Resources/Projectile 1.prefab",
                "Assets/Prefab/Resources/Projectile 2.prefab",
                "Assets/Prefab/Resources/Projectile 3.prefab"
            };

            foreach (var path in projectilePaths)
            {
                if (!File.Exists(path))
                    continue;

                var root = PrefabUtility.LoadPrefabContents(path);
                if (root == null)
                    continue;

                RemovePhotonComponents(root);

                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RemovePhotonComponents(GameObject root)
        {
            var components = root.GetComponentsInChildren<Component>(true);
            foreach (var c in components)
            {
                if (c == null)
                    continue;

                var type = c.GetType();
                if (type.FullName != null && (type.FullName.Contains("Photon") || type.Name.Contains("PhotonView")))
                {
                    UnityEngine.Object.DestroyImmediate(c, true);
                }
            }
        }

        private static NetworkPrefabsList CreateOrUpdateNetworkPrefabsList()
        {
            string listPath = "Assets/NetworkPrefabsList.asset";
            var listAsset = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(listPath);
            if (listAsset == null)
            {
                listAsset = ScriptableObject.CreateInstance<NetworkPrefabsList>();
                AssetDatabase.CreateAsset(listAsset, listPath);
                Debug.Log($"[NetcodeMigration] Created {listPath}");
            }

            string[] registeredPrefabs = new string[]
            {
                "Assets/Prefab/Resources/Player.prefab",
                "Assets/Prefab/Resources/AI Player.prefab",
                "Assets/Prefab/Resources/Creep.prefab",
                "Assets/Prefab/Resources/Rune.prefab",
                "Assets/Prefab/Resources/Xeroboro.prefab",
                "Assets/Prefab/Resources/Xeroboro 2.prefab",
                "Assets/Prefab/Resources/Xeroboro 3.prefab",
                "Assets/Prefab/Resources/Xeroboro 4.prefab",
                "Assets/Prefab/Resources/Xeroboro 5.prefab",
                "Assets/Prefab/Portal Variant.prefab"
            };

            var existingPrefabs = new HashSet<GameObject>();
            foreach (var item in listAsset.PrefabList)
            {
                if (item.Prefab != null)
                {
                    existingPrefabs.Add(item.Prefab);
                }
            }

            foreach (var path in registeredPrefabs)
            {
                var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabAsset != null && !existingPrefabs.Contains(prefabAsset))
                {
                    listAsset.Add(new NetworkPrefab { Prefab = prefabAsset });
                    existingPrefabs.Add(prefabAsset);
                    Debug.Log($"[NetcodeMigration] Registered {path} in NetworkPrefabsList");
                }
            }

            EditorUtility.SetDirty(listAsset);
            return listAsset;
        }

        private static void ConfigureMenuScene(NetworkPrefabsList prefabsList)
        {
            string menuPath = "Assets/Scenes/Menu.unity";
            if (!File.Exists(menuPath))
                return;

            var scene = EditorSceneManager.OpenScene(menuPath, OpenSceneMode.Single);

            var netManager = UnityEngine.Object.FindAnyObjectByType<NetworkManager>();
            if (netManager == null)
            {
                var go = new GameObject("NetworkManager");
                netManager = go.AddComponent<NetworkManager>();
                var utp = go.AddComponent<UnityTransport>();

                netManager.NetworkConfig = new NetworkConfig();
                netManager.NetworkConfig.NetworkTransport = utp;

                Debug.Log("[NetcodeMigration] Created NetworkManager in Menu.unity");
            }

            if (netManager.NetworkConfig == null)
            {
                netManager.NetworkConfig = new NetworkConfig();
            }

            if (netManager.GetComponent<UnityTransport>() == null)
            {
                var utp = netManager.gameObject.AddComponent<UnityTransport>();
                netManager.NetworkConfig.NetworkTransport = utp;
            }

            if (prefabsList != null && !netManager.NetworkConfig.Prefabs.NetworkPrefabsLists.Contains(prefabsList))
            {
                netManager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(prefabsList);
                Debug.Log("[NetcodeMigration] Added NetworkPrefabsList to NetworkManager in Menu.unity");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void ConfigureGameScene()
        {
            string gamePath = "Assets/Scenes/Game.unity";
            if (!File.Exists(gamePath))
                return;

            var scene = EditorSceneManager.OpenScene(gamePath, OpenSceneMode.Single);

            var mm = UnityEngine.Object.FindAnyObjectByType<MultiplayerManager>();
            if (mm != null)
            {
                if (mm.GetComponent<NetworkObject>() == null)
                {
                    mm.gameObject.AddComponent<NetworkObject>();
                    Debug.Log("[NetcodeMigration] Added NetworkObject to MultiplayerManager in Game.unity");
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            // Re-open Menu scene as default entry point
            EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity", OpenSceneMode.Single);
        }
    }
}