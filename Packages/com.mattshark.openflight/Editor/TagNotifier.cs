/**
 * @ Maintainer: Happyrobot33
 */

using System;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Editor;
using VRC.SDKBase.Editor.Elements;
using System.Reflection;
using VRC.SDKBase.Editor.Api;
using VRC.Core;
using System.Threading.Tasks;

//this script literally is just here to notify the world creator at upload that we recommend adding the openflight tag to the world
namespace OpenFlightVRC.Editor
{
    /// <summary>
    /// A simple script to notify the user to add the OpenFlight tag to their world if it is not already there and there is space
    /// </summary>
    public class TagNotifier : EditorWindow
    {
        public const string ADD_TAG_DECISION_KEY = "OpenFlight_AddTagDecision";

        [InitializeOnLoadMethod]
        public static void RegisterSDKCallback()
        {
            VRCSdkControlPanel.OnSdkPanelEnable += AddBuildHook;
        }

        private static VRCWorld world;

        private static void AddBuildHook(object sender, EventArgs e)
        {
            if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkWorldBuilderApi>(out var builder))
            {
                builder.OnSdkBuildStart += OnBuildStarted;
            }
        }

        private static async void OnBuildStarted(object sender, object target)
        {
            await runAutoTag();
        }

        /// <summary>
        /// Returns a unique ID for this project
        /// </summary>
        private static string projectID
        {
            get
            {
                return Application.productName + Application.unityVersion;
            }
        }

        /// <summary>
        /// Returns true if the user has decided they dont want to be asked about adding the tag for this project
        /// </summary>
        /// <returns>True if the user has decided they dont want to be asked about adding the tag for this project</returns>
        private static bool getProjectTagDecision()
        {
            return EditorPrefs.GetBool(projectID + "_" + ADD_TAG_DECISION_KEY, false);
        }

        /// <summary>
        /// Sets the project tag decision
        /// </summary>
        /// <remarks>
        /// send TRUE to opt out of the auto tagging, send FALSE to opt in
        /// </remarks>
        /// <param name="decision">The decision to set</param>
        private static void setProjectTagDecision(bool decision)
        {
            EditorPrefs.SetBool(projectID + "_" + ADD_TAG_DECISION_KEY, decision);
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

        /// <summary>
        /// Adds a tag to the world, if it will fit
        /// </summary>
        /// <param name="Tag">The tag to add</param>
        /// <returns>True if the tag was added, false if it was not</returns>
        private static bool addTag(string Tag)
        {
            //check if the tag limit has been reached
            if (getTagCount() >= getTagLimit())
            {
                //if it has, return false
                return false;
            }

            //check to make sure the tag doesnt already exist
            if (world.Tags.Contains("author_tag_" + Tag))
            {
                //if it does, return false
                return false;
            }

            //make sure the tag has spaces replaced with underscores
            Tag = Tag.Replace(" ", "_");


            //ask the user if they want to add the tag
            if (EditorUtility.DisplayDialog("OpenFlight", "We have detected that your world does not currently have the OpenFlight tag. Would you like to add the OpenFlight tag to this world?", "Yes", "No, Dont ask again"))
            {
                //add the tag
                world.Tags.Add("author_tag_" + Tag);
                VRCApi.UpdateWorldInfo(world.ID, world);
            }
            else
            {
                //dont add the tag
                setProjectTagDecision(true);
            }

            //return true
            return true;
        }

        /// <summary>
        /// Returns the number of tags that can be added to the world
        /// </summary>
        /// <returns>The number of tags that can be added to the world</returns>
        private static int getTagLimit()
        {
            //use reflection to get the TagLimit variable without calling the constructor
            FieldInfo tagsFieldInfo = typeof(TagsField).GetField("TagLimit", BindingFlags.Public | BindingFlags.Instance);
            return (int)tagsFieldInfo.GetValue(new TagsField());
        }

        /// <summary>
        /// Returns the number of tags that are currently on the world
        /// </summary>
        /// <returns>The number of tags that are currently in the world</returns>
        private static int getTagCount()
        {
            //return the number of tags that start with author_tag_
            int count = 0;
            foreach (string tag in world.Tags)
            {
                if (tag.StartsWith("author_tag_"))
                {
                    count++;
                }
            }
            return count;
        }

        [MenuItem("VRC Packages/OpenFlight/Add OpenFlight Tag")]
        private static async Task runAutoTag()
        {
            try
            {
                //exit if they have opted out
                if (getProjectTagDecision())
                {
                    return;
                }

                await GetWorld();
                addTag("openflight");
            }
            catch (Exception e)
            {
                Debug.LogWarning("OpenFlight: Error automatically adding OpenFlight tag. This is not a critical error, but you may want to add the tag manually. Error: " + e.Message);
            }
        }

        //button to reset opt out decision
        [MenuItem("VRC Packages/OpenFlight/Reset OpenFlight Tag Opt Out")]
        private static void resetOptOut()
        {
            setProjectTagDecision(false);
        }
    }
}
