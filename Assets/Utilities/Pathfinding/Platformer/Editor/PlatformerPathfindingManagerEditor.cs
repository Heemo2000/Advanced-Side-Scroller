using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace Utilities.Pathfinding.Platformer
{
#if UNITY_EDITOR
    [CustomEditor(typeof(PlatformerPathfindingManager))]
    public class PlatformerPathfindingManagerEditor : Editor
    {
        private const string TilemapPropertyName = "_tilemap";
        private const string NodesListPropertyName = "_nodesList";
        private const float MinOrthographicSize = 6.0f;

        private GUIStyle _labelStyle = null;
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.Add(new Label("<b>Pathfinding Settings:</b>"));
            SerializedProperty tilemapProperty = serializedObject.FindProperty(TilemapPropertyName);

            root.Add(new PropertyField(tilemapProperty));

            SerializedProperty nodesListProperty = serializedObject.FindProperty(NodesListPropertyName);

            root.Add(new PropertyField(nodesListProperty));

            Button createNodesAndPartiallyCreateConnections = new Button(()=>
            {
                
                PlatformerPathfindingManager pathfindingManager = (PlatformerPathfindingManager)target;

                Undo.RecordObject(pathfindingManager, "Generate Nodes");

                var previousNodes = FindObjectsByType<PlatformerGraphAstarNode>(FindObjectsSortMode.InstanceID);

                for (int i = 0; i < previousNodes.Length; i++)
                {
                    var node = previousNodes[i];
                    DestroyImmediate(node.gameObject);
                }

                List<PlatformerGraphAstarNode> requiredNodes = pathfindingManager.GenerateNodesAndPartiallyCreateConnections();

                

                

                pathfindingManager.NodesList = requiredNodes;

                EditorUtility.SetDirty(pathfindingManager);
                serializedObject.Update();
            });

            createNodesAndPartiallyCreateConnections.text = "Create Nodes and partially create connections";
            root.Add(createNodesAndPartiallyCreateConnections);
            
            root.Bind(serializedObject);
            return root;
        }

        private void OnSceneGUI()
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;

            if (sceneView == null)
                return;

            Camera sceneCamera = sceneView.camera;

            if(sceneCamera == null || sceneCamera.orthographicSize > MinOrthographicSize)
            {
                return;
            }

            PlatformerPathfindingManager pathfindingManager = (PlatformerPathfindingManager)target;
            if(pathfindingManager == null)
            {
                return;
            }

            if(pathfindingManager.NodesList == null || pathfindingManager.NodesList.Count == 0)
            {
                return;
            }

            if(_labelStyle == null)
            {
                _labelStyle = new GUIStyle(EditorStyles.helpBox);

                _labelStyle.normal.textColor = Color.white;
                _labelStyle.alignment = TextAnchor.MiddleCenter;
                _labelStyle.padding = new RectOffset(4, 4, 2, 2);
            }

            foreach(PlatformerGraphAstarNode node in pathfindingManager.NodesList)
            {
                if(node == null) continue;
                Vector3 position = node.transform.position;
                Handles.Label(position, node.transform.name, _labelStyle);
            }
        }
    }
#endif
}
