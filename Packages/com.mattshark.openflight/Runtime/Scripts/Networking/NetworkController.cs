using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Core;
using System.Threading.Tasks;
using UdonSharp;
using VRC.SDKBase;



#if UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor.Callbacks;
using VRC.SDKBase.Editor.Api;
using UdonSharpEditor;
#endif

// this will create enough network components to cover the maximum number of players on the world
namespace OpenFlightVRC.Net
{
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    public class NetworkControllerEditor : Editor, VRC.SDKBase.IEditorOnly
    {
        private static VRCWorld world;

        //TODO: get this running BEFORE client sim, otherwise it wont work
        //[PostProcessSceneAttribute]
        [MenuItem("VRC Packages/OpenFlight/Init Network Controller")]
        private static async void RunOnBuild()
        {
            await GetWorld();

            //find self
            NetworkController self = GameObject.FindObjectOfType<NetworkController>();

            //get the template
            GameObject template = self.PlayerStatusCollectorTemplate;

            //create enough copys for the maximum number of players
            int maxPlayers = world.Capacity;

            //get the template parent
            GameObject templateParent = template.transform.parent.gameObject;

            //temp child list
            List<Transform> children = new List<Transform>();

            //get all the children
            foreach (Transform child in self.transform)
            {
                children.Add(child);
            }

            //remove any existing copies
            foreach (Transform child in children)
            {
                //remove all but the template
                if (child.gameObject != template)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            //create enough copies
            for (int i = 1; i < maxPlayers; i++)
            {
                GameObject newObject = Instantiate(template);
                newObject.transform.SetParent(templateParent.transform);
                newObject.name = "PlayerStatusCollector_" + i;
            }
        }

        /*[InitializeOnLoadMethod]
        private static void Initialize()
        {
            RunOnBuild();
        }
        */

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
    }
#endif

    public class NetworkController : UdonSharpBehaviour
    {
        public GameObject PlayerStatusCollectorTemplate;
        PlayerStatusCollector[] collectors;

        // Start is called before the first frame update
        void Start()
        {
            Logger.Log("NetworkController started", this);

            //create a new array of collectors to hold the total number of children
            collectors = new PlayerStatusCollector[transform.childCount];

            //gather all the collectors under this gameobject
            for (int i = 0; i < collectors.Length; i++)
            {
                collectors[i] = transform.GetChild(i).GetComponent<PlayerStatusCollector>();
            }

            Logger.Log("Found " + collectors.Length + " collectors", this);
        }

        //on player join
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            Logger.Log("Player joined: " + GetWrappedID(player.playerId), this);

            //set the owner of the collector with the same ID as the player
            Networking.SetOwner(player, collectors[GetWrappedID(player.playerId) - 1].gameObject);

            //let the collector know it has an owner
            collectors[GetWrappedID(player.playerId) - 1].GainOwner(player);
        }

        //on player left
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            Logger.Log("Player left: " + GetWrappedID(player.playerId), this);

            //let the collector know it lost its owner
            collectors[GetWrappedID(player.playerId) - 1].LoseOwner();
        }

        private int GetWrappedID(int id)
        {
            return id % collectors.Length;
        }
    }
}
