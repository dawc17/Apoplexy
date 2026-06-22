using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public static class RetroSnapAnchorBaker
    {
        private const float PositionQuantize = 10000f;

        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Bake Snap Anchors For Selection")]
        public static void BakeSelection()
        {
            RetroRenderToolkitInstaller.EnsureFolder(RetroRenderToolkitInstaller.SnapAnchorFolder);

            int baked = 0;
            foreach (Object selected in Selection.objects)
            {
                baked += BakeObject(selected);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Baked snap anchors for {baked} mesh asset(s).");
        }

        private static int BakeObject(Object selected)
        {
            if (selected == null)
            {
                return 0;
            }

            if (selected is Mesh mesh)
            {
                BakeMeshAsset(mesh, null, null);
                return 1;
            }

            if (selected is GameObject gameObject)
            {
                int count = 0;
                foreach (MeshFilter meshFilter in gameObject.GetComponentsInChildren<MeshFilter>())
                {
                    if (meshFilter.sharedMesh != null)
                    {
                        meshFilter.sharedMesh = BakeMeshAsset(meshFilter.sharedMesh, meshFilter.gameObject.name, meshFilter);
                        count++;
                    }
                }

                foreach (SkinnedMeshRenderer skinned in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    if (skinned.sharedMesh != null)
                    {
                        skinned.sharedMesh = BakeMeshAsset(skinned.sharedMesh, skinned.gameObject.name, skinned);
                        count++;
                    }
                }

                return count;
            }

            string path = AssetDatabase.GetAssetPath(selected);
            Mesh assetMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (assetMesh != null)
            {
                BakeMeshAsset(assetMesh, selected.name, null);
                return 1;
            }

            return 0;
        }

        private static Mesh BakeMeshAsset(Mesh source, string ownerName, Object undoTarget)
        {
            Mesh baked = Object.Instantiate(source);
            baked.name = $"{source.name}_SnapAnchors";
            WriteAnchors(baked);

            string safeOwner = string.IsNullOrEmpty(ownerName) ? source.name : ownerName;
            string path = AssetDatabase.GenerateUniqueAssetPath($"{RetroRenderToolkitInstaller.SnapAnchorFolder}/{safeOwner}_{source.name}_SnapAnchors.asset");
            AssetDatabase.CreateAsset(baked, path);

            if (undoTarget != null)
            {
                Undo.RecordObject(undoTarget, "Assign Snap Anchor Mesh");
                EditorUtility.SetDirty(undoTarget);
            }

            return baked;
        }

        private static void WriteAnchors(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            Dictionary<Vector3Int, AnchorBucket> buckets = new Dictionary<Vector3Int, AnchorBucket>();

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3Int key = Quantize(vertices[i]);
                if (!buckets.TryGetValue(key, out AnchorBucket bucket))
                {
                    bucket = new AnchorBucket();
                    buckets.Add(key, bucket);
                }

                bucket.sum += vertices[i];
                bucket.count++;
            }

            List<Vector4> uv4 = new List<Vector4>(vertices.Length);
            for (int i = 0; i < vertices.Length; i++)
            {
                AnchorBucket bucket = buckets[Quantize(vertices[i])];
                Vector3 anchor = bucket.sum / Mathf.Max(bucket.count, 1);
                uv4.Add(new Vector4(anchor.x, anchor.y, anchor.z, 1f));
            }

            mesh.SetUVs(3, uv4);
            mesh.UploadMeshData(false);
            EditorUtility.SetDirty(mesh);
        }

        private static Vector3Int Quantize(Vector3 position)
        {
            return new Vector3Int(
                Mathf.RoundToInt(position.x * PositionQuantize),
                Mathf.RoundToInt(position.y * PositionQuantize),
                Mathf.RoundToInt(position.z * PositionQuantize));
        }

        private sealed class AnchorBucket
        {
            public Vector3 sum;
            public int count;
        }
    }
}
