using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Core;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
using VRC.SDKBase.Editor.Api;
#endif

// this will create enough network components to cover the maximum number of players on the world
namespace OpenFlightVRC.Net
{
    internal class NetworkComponentPopulator : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
#if UNITY_EDITOR
        public GameObject PlayerStatusCollectorTemplate;
        private static VRCWorld world;

        //TODO: get this running BEFORE client sim, otherwise it wont work
        //[PostProcessSceneAttribute]
        public static async void OnPostProcessScene()
        {
            await GetWorld();

            //find self
            NetworkComponentPopulator self = GameObject.FindObjectOfType<NetworkComponentPopulator>();
            //get the template
            GameObject template = self.PlayerStatusCollectorTemplate;

            //create enough copys for the maximum number of players
            int maxPlayers = world.Capacity;

            //get the template parent
            GameObject templateParent = template.transform.parent.gameObject;

            //create enough copies
            for (int i = 1; i < maxPlayers; i++)
            {
                GameObject newObject = Instantiate(template);
                newObject.transform.SetParent(templateParent.transform);
                newObject.name = "PlayerStatusCollector_" + i;
                newObject.GetComponent<PlayerStatusCollector>().ID = i;
            }
        }

        private static async Task GetWorld()
        {
            //get the pipeline manager by finding the PipelineManager component in the current scene
            PipelineManager pipelineManager = GameObject.FindObjectOfType<PipelineManager>();
            var worldId = pipelineManager.blueprintId;

            //if its a new world (no id), skip
            if (worldId != null)
            {
                //get the world from the api
                world = await VRCApi.GetWorld(worldId, true);
            }
        }
#endif
    }
}
