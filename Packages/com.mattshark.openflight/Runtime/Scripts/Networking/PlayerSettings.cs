/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using VRC.Udon.Common;
using VRC.SDK3.Components;
using System;
using VRC.SDK3.Persistence;

namespace OpenFlightVRC.Net
{
    /// <summary>
    /// The callback types for the <see cref="PlayerSettings"/>
    /// </summary>
    public enum PlayerSettingsCallback
    {
        /// <summary>
        /// Called when player data is ready to use
        /// </summary>
        OnDataReady,
        /// <summary>
        /// Called when local data is ready
        /// </summary>
        OnLocalDataReady,
        /// <summary>
        /// Called when player data is changed for any reason
        /// </summary>
        OnDataChanged,
        /// <summary>
        /// Called when the world defaults setting is changed
        /// </summary>
        useWorldDefaultsWhenLoadingChanged,
        /// <summary>
        /// Called when the slot to load by default is changed
        /// </summary>
        slotToLoadByDefaultChanged,
        /// <summary>
        /// Called when remote differences are detected
        /// </summary>
        OnRemoteDifferencesDetected,
        /// <summary>
        /// Called when remote differences are resolved or not detected
        /// </summary>
        OnRemoteDifferencesResolved,
        /// <summary>
        /// Called when the object is finished starting
        /// </summary>
        OnStartFinished,
        /// <summary>
        /// Called when we detect that storage is full
        /// </summary>
        OnStorageFull,
        /// <summary>
        /// Called when we detect that storage is free
        /// </summary>
        OnStorageFree
    }

    /// <summary>
    /// This class is used to store player settings
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(VRCEnablePersistence))]
    public class PlayerSettings : CallbackUdonSharpBehaviour
    {
        #region Object References
        public WingFlightPlusGlide wingFlightPlusGlide;
        #endregion

        /// <summary>
        /// Temporary variable until bug is fixed, contains the owner of the object
        /// </summary>
        public VRCPlayerApi TEMPOWNERDOREMOVEWHENFIXED;

        /// <summary>
        /// The actual settings data that goes to the VRC servers / other players
        /// </summary>
        [UdonSynced]
        [ReadOnlyInspector]
        public string synchedSettings = "";

        /// <summary>
        /// Maximum ammount of bytes that can be saved before VRC wont save any more
        /// TODO: Change to a method call once VRC adds a method to get this value directly
        /// </summary>
        public const int MAXSAVEBYTES = 100000;

        /// <summary>
        /// The searchable settings data
        /// </summary>
        /// <value>
        /// "Global Settings":
        /// {
        /// "Setting Name": "Setting Value"
        /// ...
        /// },
        /// "Slots":
        /// "Slot Name":
        /// {
        ///    "Setting Name": "Setting Value"
        ///    ...
        /// },
        /// ...
        /// }
        /// </value>
        private DataDictionary m_RemoteSettings = new DataDictionary();

        /// <summary>
        /// The settings that have changed since the last sync
        /// </summary>
        private DataDictionary m_LocalSettings = new DataDictionary();

        #region Global Settings Management
        #region Key name constants
        /// <summary>
        /// Upon joining the world, should the player use the world defaults for their settings, or load their own settings
        /// </summary>
        public const string useWorldDefaultsWhenLoadingKey = "useWorldDefaultsWhenLoading";
        /// <summary>
        /// The slot to load by default upon joining the world, if <see cref="useWorldDefaultsWhenLoadingKey"/> is false
        /// </summary>
        public const string slotToLoadByDefaultKey = "slotToLoadByDefault";

        /// <summary>
        /// The key for the last updated date time
        /// </summary>
        public const string updatedDateTimeKey = "lastUpdatedDateTime";

        /// <summary>
        /// The key for the revision number, increments every time the settings are changed
        /// </summary>
        public const string revisionKey = "revision";


        /// <summary>
        /// The player data folder key
        /// </summary>
        public const string playerDataFolderKey = "OpenFlightVRC/";

        /// <summary>
        /// The player data key for the backup of the settings
        /// </summary>
        public const string DBBackupKey = playerDataFolderKey + "DBBackup";
        #endregion


        public int _SpaceUsed(DataDictionary settings)
        {
            if (VRCJson.TrySerializeToJson(settings, JsonExportType.Minify, out DataToken _settings_token))
            {
                return _settings_token.ToString().Length;
            }
            else
            {
                return 0;
            }
        }

        public int _SpaceFree(DataDictionary settings)
        {
            return MAXSAVEBYTES - _SpaceUsed(settings);
        }

        public bool _IsSpaceAvailable(DataDictionary settings, int bytes)
        {
            return _SpaceFree(settings) >= bytes;
        }

        public string _GetStorageInfo()
        {
            return string.Format(@"Local: Used: {0} bytes, Free: {1} bytes, Total: {2} bytes
Remote: Used: {0} bytes, Free: {1} bytes, Total: {2} bytes",
                                 _SpaceUsed(m_LocalSettings),
                                 _SpaceFree(m_LocalSettings),
                                 MAXSAVEBYTES,
                                 _SpaceUsed(m_RemoteSettings),
                                 _SpaceFree(m_RemoteSettings),
                                 MAXSAVEBYTES);
        }

        public string _GetDBInfo()
        {
            //get from remote
            _GetGlobalSetting(m_RemoteSettings, revisionKey, out DataToken revision);
            _GetGlobalSetting(m_RemoteSettings, updatedDateTimeKey, out DataToken updatedDateTime);
            return string.Format("Revision: {0}, Last Updated: {1}", revision.ToString(), updatedDateTime.ToString());
        }

        /// <summary>
        /// Set a global setting. Only applies to the local player
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns> True if the setting was set, false if it failed </returns>
        public bool _SetGlobalSetting(string key, DataToken value)
        {
            DataDictionary globalSettings = GetGlobalSettingsDictionary(m_LocalSettings);

            globalSettings.SetValue(new DataToken(key), value);

            switch (key)
            {
                case useWorldDefaultsWhenLoadingKey:
                    RunCallback(PlayerSettingsCallback.useWorldDefaultsWhenLoadingChanged);
                    break;
                case slotToLoadByDefaultKey:
                    RunCallback(PlayerSettingsCallback.slotToLoadByDefaultChanged);
                    break;
                    //default:
                    //    Logger.LogWarning(string.Format("No callback for global setting {0}! This should be fixed", key), this);
                    //    break;
            }

            CheckForDifferences();

            return true;
        }

        /// <summary>
        /// Get a global setting
        /// </summary>
        /// <param name="settingsDictionary"> The dictionary to get the setting from </param>
        /// <param name="key"> The key of the setting to retrieve </param>
        /// <param name="returnToken"> The token that was retrieved </param>
        /// <returns> True if the setting was retrieved, false if it failed </returns>
        public bool _GetGlobalSetting(DataDictionary settingsDictionary, string key, out DataToken returnToken)
        {
            DataDictionary globalSettings = GetGlobalSettingsDictionary(settingsDictionary);
            if (globalSettings.Count > 0)
            {
                if (globalSettings.TryGetValue(key, out DataToken token))
                {
                    returnToken = token;
                    return true;
                }
                else
                {
                    Logger.LogWarning(string.Format("Failed to get global setting {0}! Setting not found", key), this);
                    returnToken = default;
                    return false;
                }
            }
            else
            {
                Logger.LogWarning("Failed to get global setting! Global settings not found", this);
                returnToken = default;
                return false;
            }
        }

        /// <inheritdoc cref="_GetGlobalSetting(DataDictionary, string, out DataToken)"/>
        public bool _GetGlobalSetting(string key, out DataToken returnToken)
        {
            return _GetGlobalSetting(m_LocalSettings, key, out returnToken);
        }
        #endregion

        #region Slot Saving
        /// <inheritdoc cref="_SaveSlot(DataDictionary, string, out string)"/>
        public bool _SaveSlot(string slotName, out string returnSlotName)
        {
            return _SaveSlot(GatherSlotSettings(), slotName, out returnSlotName);
        }

        /// <summary>
        /// Saves the current settings to a slot
        /// </summary>
        /// <param name="slotData"> The settings to save </param>
        /// <param name="slotName"> The name of the slot to save to </param>
        /// <param name="returnSlotName"> The name of the slot that was saved </param>
        /// <returns> True if the slot was saved, false if it failed </returns>
        public bool _SaveSlot(DataDictionary slotData, string slotName, out string returnSlotName)
        {
            //check if we can edit the settings
            if (!CanEdit)
            {
                Logger.LogWarning("Failed to save settings! Must be the owner of the object to save settings", this);

                returnSlotName = "";
                return false;
            }

            Logger.Log("Saving current settings to slot " + slotName, this);

            //get the local slots
            DataDictionary localSlots = GetSlots(m_LocalSettings);

            //overwrite the slot data at that slot index
            DataList keys = localSlots.GetKeys();
            //make sure the slot name is valid. if it is not, we will get a unique name
            if (!keys.Contains(new DataToken(slotName)))
            {
                slotName = _GetUniqueSlotName(slotName);
            }

            //get the pre-existing slot data if it exists
            DataDictionary prevSlotData = new DataDictionary();
            if (localSlots.TryGetValue(new DataToken(slotName), TokenType.DataDictionary, out DataToken slotDataToken))
            {
                prevSlotData = slotDataToken.DataDictionary;

                //Load in the metadata
                slotData.SetValue(new DataToken(revisionKey), new DataToken(_GetSlotRevision(slotName)));
                slotData.SetValue(new DataToken(updatedDateTimeKey), new DataToken(_GetSlotUpdatedDateTime(slotName)));
            }

            //only edit these if there is differences
            if (_GetDictionaryDifferences(slotData, prevSlotData))
            {
                slotData.SetValue(new DataToken(revisionKey), new DataToken(_GetSlotRevision(slotName) + 1));
                slotData.SetValue(new DataToken(updatedDateTimeKey), new DataToken(System.DateTime.Now.ToString()));
            }

            //slot name is assumed valid past here
            localSlots.SetValue(new DataToken(slotName), new DataToken(slotData));

            //check for differences
            CheckForDifferences();

            //invoke the callback
            RunCallback(PlayerSettingsCallback.OnDataChanged);

            returnSlotName = slotName;
            return true;
        }
        #endregion




        /// <summary>
        /// Uploads the settings to the VRC servers, along with updating our local unchanged copy
        /// </summary>
        public void _UploadSettings()
        {
            //get the revision number
            if (_GetGlobalSetting(revisionKey, out DataToken revision))
            {
                _SetGlobalSetting(revisionKey, new DataToken((long)revision.Double + 1));
            }
            else
            {
                Logger.Log("Revision key not found, initializing it", this);
                //initialize it
                _SetGlobalSetting(revisionKey, new DataToken(0));
            }

            //check if we have space
            if (!_IsSpaceAvailable(m_LocalSettings, _SpaceUsed(m_LocalSettings)))
            {
                Logger.LogWarning("Failed to upload settings! Not enough space to save settings", this);
                RunCallback(PlayerSettingsCallback.OnStorageFull);
                return;
            }
            else
            {
                RunCallback(PlayerSettingsCallback.OnStorageFree);
            }

            //update the date
            _SetGlobalSetting(updatedDateTimeKey, new DataToken(System.DateTime.Now.ToString()));

            if (VRCJson.TrySerializeToJson(m_LocalSettings, JsonExportType.Minify, out DataToken _settings_token))
            {
                synchedSettings = _settings_token.ToString();
                RequestSerialization();
                //update our live copy of the remote settings
                m_RemoteSettings = m_LocalSettings.DeepClone();
                Logger.Log("Settings uploaded successfully", this);
                //invoke the resolve callback, assuming there is no differences as we just uploaded
                RunCallback(PlayerSettingsCallback.OnRemoteDifferencesResolved);
            }
            else
            {
                Logger.LogWarning(string.Format("Failed to upload settings! Your settings have not been uploaded to prevent corruption. Error reason: {0}", _settings_token.Error.ToString()), this);
            }
        }

        /// <summary>
        /// Reverts the local settings to the remote settings
        /// </summary>
        public void _RevertSettings()
        {
            //revert the local settings
            m_LocalSettings = m_RemoteSettings.DeepClone();

            //if on revert we have no settings, run initial setup
            _InitializeDatabase();

            //invoke the resolve callback, assuming there is no differences as we just reverted
            RunCallback(PlayerSettingsCallback.OnRemoteDifferencesResolved);
            RunCallback(PlayerSettingsCallback.OnDataChanged);
        }

        /// <summary>
        /// Creates a new slot
        /// </summary>
        /// <param name="returnSlotName"> The name of the new slot </param>
        /// <inheritdoc cref="_SaveSlot(string, out string)" path="/returns"/>
        public bool _NewSlot(out string returnSlotName)
        {
            return _SaveSlot("", out returnSlotName);
        }

        #region Slot Duplication
        /// <summary>
        /// Duplicates the input slot
        /// </summary>
        /// <param name="slotName"> The name of the slot to duplicate </param>
        /// <param name="returnSlotName"> The name of the duplicated slot </param>
        /// <returns> True if the slot was duplicated, false if it failed </returns>
        public bool _DuplicateSlot(string slotName, out string returnSlotName)
        {
            _LoadSlot(slotName, out DataDictionary slotData, false);
            string uniqueName = _GetUniqueSlotName(slotName);

            return _SaveSlot(slotData, uniqueName, out returnSlotName);
        }
        #endregion

        #region Slot Renaming

        /// <summary>
        /// Renames a slot
        /// </summary>
        /// <param name="slot"> The name of the slot to rename </param>
        /// <param name="newName"> The new name of the slot </param>
        /// <returns> True if the slot was renamed, false if it failed </returns>
        public bool _RenameSlot(string slot, string newName)
        {
            //check if we can edit the settings
            if (!CanEdit)
            {
                Logger.LogWarning("Failed to rename slot! Must be the owner of the object to rename a slot", this);
                return false;
            }

            Logger.Log("Attempting to rename slot " + slot + " to " + newName, this);

            //get the local slots
            DataDictionary localSlots = GetSlots(m_LocalSettings);

            //check if the new name doesnt already exist
            if (localSlots.ContainsKey(new DataToken(newName)))
            {
                Logger.LogWarning("Failed to rename slot! New slot name already exists!", this);
                return false;
            }

            DataList keys = localSlots.GetKeys();
            if (keys.Contains(new DataToken(slot)))
            {
                DataToken slotName = new DataToken(slot);
                DataToken slotData = localSlots[slotName];
                localSlots.Remove(slotName);
                localSlots.Add(new DataToken(newName), slotData);
                //update the slot to load by default if it was the slot being renamed
                _GetGlobalSetting(slotToLoadByDefaultKey, out DataToken slotToLoadByDefault);
                if (slotToLoadByDefault.ToString() == slotName)
                {
                    //slotToLoadByDefault = newName;
                    _SetGlobalSetting(slotToLoadByDefaultKey, newName);
                }

                Logger.Log("Slot renamed successfully", this);

                CheckForDifferences();
                RunCallback(PlayerSettingsCallback.OnDataChanged);

                return true;
            }
            else
            {
                Logger.LogWarning("Failed to rename slot! Slot does not exist", this);
                return false;
            }
        }
        #endregion

        #region Slot Loading
        /// <summary>
        /// Loads the settings from a slot
        /// </summary>
        /// <param name="slotName"> The name of the slot to load </param>
        /// <param name="outDict"> The resulting dictionary from the slot </param>
        /// <param name="spread"> If the settings should be spread to the objects upon loading </param>
        /// <returns> True if the slot was loaded, false if it failed </returns>
        public bool _LoadSlot(string slotName, out DataDictionary outDict, bool spread = true)
        {
            //Logger.Log("Loading settings from slot " + slot, this);

            //get the local slots
            DataDictionary localSlots = GetSlots(m_LocalSettings);

            //get all keys
            DataList keys = localSlots.GetKeys();

            if (localSlots.TryGetValue(slotName, TokenType.DataDictionary, out DataToken slotData))
            {
                //spread the settings
                if (spread)
                {
                    SpreadSettings(slotData.DataDictionary);
                }
                //Logger.Log(string.Format("Settings loaded successfully from slot {0} (Slot ID {1})", slotName, slot), this);
                outDict = slotData.DataDictionary;
                return true;
            }
            else
            {
                Logger.LogWarning(string.Format("Failed to load settings from slot {0}. Keeping current settings. Error reason: {1}", slotName, slotData.Error.ToString()), this);
                outDict = new DataDictionary();
                return false;
            }
        }
        #endregion

        #region Slot Deletion
        /// <summary>
        /// Deletes a slot
        /// </summary>
        /// <param name="slot"> The name of the slot to delete </param>
        /// <returns> True if the slot was deleted, false if it failed </returns>
        public bool _DeleteSlot(string slot)
        {
            //check if we can edit the settings
            if (!CanEdit)
            {
                Logger.LogWarning("Failed to delete slot! Must be the owner of the object to delete a slot", this);
                return false;
            }

            //get the slots
            DataDictionary localSlots = GetSlots(m_LocalSettings);

            //only allow deletion if there is more than one slot
            if (localSlots.GetKeys().Count <= 1)
            {
                Logger.LogWarning("Failed to delete slot! Must have at least one slot", this);
                return false;
            }

            DataList keys = localSlots.GetKeys();
            if (keys.Contains(new DataToken(slot)))
            {
                DataToken slotName = new DataToken(slot);
                localSlots.Remove(slotName);
                //update the slot to load by default if it was the slot being deleted
                //string slotToLoadByDefault = _GetGlobalSetting(slotToLoadByDefaultKey).ToString();
                _GetGlobalSetting(slotToLoadByDefaultKey, out DataToken slotToLoadByDefault);
                if (slotToLoadByDefault.ToString() == slotName)
                {
                    //slotToLoadByDefault = GetSlotName(0);
                    _SetGlobalSetting(slotToLoadByDefaultKey, _GetSlotName(0));
                    //use world defaults in this case, to avoid confusion
                    _SetGlobalSetting(useWorldDefaultsWhenLoadingKey, true);
                }

                Logger.Log("Slot deleted successfully", this);

                CheckForDifferences();

                RunCallback(PlayerSettingsCallback.OnDataChanged);

                return true;
            }
            else
            {
                Logger.LogWarning("Failed to delete slot! Slot does not exist", this);
                return false;
            }
        }
        #endregion

        #region  Import / Export
        /// <summary>
        /// Exports a slot to a JSON string
        /// </summary>
        /// <param name="slot"> The name of the slot to export </param>
        /// <param name="json"> The resulting JSON string </param>
        /// <returns> The JSON string of the slot. Empty string if failed </returns>
        public bool _GetSlotExport(string slot, out string json)
        {
            //DataDictionary slotData = _LoadSlotDictionary(slot, false);
            if (_LoadSlot(slot, out DataDictionary slotData, false))
            {
                //nest the slot data so the name is included
                DataDictionary slotDataNested = new DataDictionary();
                slotDataNested.Add(new DataToken(slot), new DataToken(slotData));
                if (VRCJson.TrySerializeToJson(slotDataNested, JsonExportType.Minify, out DataToken _settings_token))
                {
                    json = _settings_token.ToString();
                    return true;
                }
                else
                {
                    Logger.LogWarning(string.Format("Failed to export slot {0}! Error reason: {1}", slot, _settings_token.Error.ToString()), this);
                    json = "";
                    return false;
                }
            }
            else
            {
                Logger.LogWarning("Failed to export slot! Slot does not exist", this);
                json = "";
                return false;
            }
        }

        /// <summary>
        /// Exports the settings to a JSON string
        /// </summary>
        /// <param name="json"> The resulting JSON string </param>
        /// <returns> True if the settings were exported, false if it failed </returns>
        public bool _GetDBExport(out string json)
        {
            if (VRCJson.TrySerializeToJson(m_RemoteSettings, JsonExportType.Minify, out DataToken _settings_token))
            {
                json = _settings_token.ToString();
                return true;
            }
            else
            {
                Logger.LogWarning(string.Format("Failed to export settings! Error reason: {0}", _settings_token.Error.ToString()), this);
                json = "";
                return false;
            }
        }

        /// <summary>
        /// Imports a slot from a JSON string
        /// </summary>
        /// <param name="json"> The JSON string to import </param>
        /// <param name="returnSlotName"> The name of the slot that was imported </param>
        /// <returns> True if the slot was imported, false if it failed </returns>
        public bool _ImportSlot(string json, out string returnSlotName)
        {
            DataToken token;
            if (VRCJson.TryDeserializeFromJson(json, out token))
            {
                DataDictionary slotData = token.DataDictionary;
                //top level key is the slot name, the value is all the settings
                string slotName = slotData.GetKeys()[0].ToString();
                _SaveSlot(slotData[slotName].DataDictionary, slotName, out returnSlotName);
                returnSlotName = slotName;
                CheckForDifferences();

                RunCallback(PlayerSettingsCallback.OnDataChanged);
                return true;
            }
            else
            {
                Logger.LogWarning(string.Format("Failed to import slot! Error reason: {0}", token.Error.ToString()), this);
                returnSlotName = "";
                return false;
            }
        }

        /// <summary>
        /// Imports the settings from a JSON string. Completely overwrites the current settings
        /// </summary>
        /// <param name="json"> The JSON string to import </param>
        /// <returns> True if the settings were imported, false if it failed </returns>
        public bool _ImportDB(string json)
        {
            DataToken token;
            if (VRCJson.TryDeserializeFromJson(json, out token))
            {
                m_LocalSettings = token.DataDictionary;
                Logger.Log("Settings imported successfully", this);
                CheckForDifferences();
                RunCallback(PlayerSettingsCallback.OnDataChanged);
                return true;
            }
            else
            {
                Logger.LogWarning(string.Format("Failed to import settings! Error reason: {0}", token.Error.ToString()), this);
                return false;
            }
        }
        #endregion

        #region Slot Utilities


        /// <summary>
        /// Checks if the player settings store is initialized with data, and has atleast one slot
        /// </summary>
        public bool IsInitialized => m_LocalSettings.Count > 0 && _GetSlotCount() > 0;

        /// <summary>
        /// Makes a unique name for a slot
        /// </summary>
        /// <param name="name"> The name to make unique </param>
        /// <returns> A unique slot name, may or may not be the same as the input </returns>
        public string _GetUniqueSlotName(string name)
        {
            const string defaultName = "New Slot ";

            //get the local slots
            DataDictionary localSlots = GetSlots(m_LocalSettings);

            //if the string is empty, use the default name
            if (string.IsNullOrEmpty(name))
            {
                int i = 0;
                while (localSlots.ContainsKey(new DataToken(defaultName + i)))
                {
                    i++;
                }
                return defaultName + i;
            }
            else
            {
                //add clone to the end
                while (localSlots.ContainsKey(new DataToken(name)))
                {
                    name += " Clone";
                }

                return name;
            }
        }

        /// <summary>
        /// Checks if we can edit the settings
        /// </summary>
        public bool CanEdit => Networking.IsOwner(gameObject);

        /// <summary>
        /// Gets the name of a slot based on a slot index
        /// </summary>
        /// <param name="slot"> The index of the slot </param>
        /// <returns> The name of the slot </returns>
        public string _GetSlotName(int slot)
        {
            //get the local slots
            DataDictionary localSlots = GetSlots(m_LocalSettings);

            DataList keys = localSlots.GetKeys();
            if (keys.Count > slot)
            {
                return keys[slot].ToString();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Gets all the slot names in order
        /// </summary>
        /// <returns></returns>
        public string[] _GetSlotNames()
        {
            //get the local slots
            DataDictionary localSlots = GetSlots(m_LocalSettings);

            DataList keys = localSlots.GetKeys();
            string[] names = new string[keys.Count];
            for (int i = 0; i < keys.Count; i++)
            {
                names[i] = keys[i].ToString();
            }
            return names;
        }

        /// <summary>
        /// Gets the slot index based on a slot name
        /// </summary>
        /// <param name="slotName"> The name of the slot </param>
        /// <returns> The index of the slot, or -1 if it does not exist </returns>
        public int _GetSlotIndex(string slotName)
        {
            //get the local slots
            DataDictionary localSlots = GetSlots(m_LocalSettings);

            DataList keys = localSlots.GetKeys();
            return keys.IndexOf(new DataToken(slotName));
        }

        /// <summary>
        /// Gets the count of slots
        /// </summary>
        /// <returns> The count of slots </returns>
        public int _GetSlotCount()
        {
            //get the local slots
            DataDictionary localSlots = GetSlots(m_LocalSettings);
            return localSlots.GetKeys().Count;
        }

        /// <summary>
        /// Validates a slot index, returning a valid slot index
        /// </summary>
        /// <param name="slot"> The slot index to validate </param>
        /// <returns> A valid slot index </returns>
        public int _ValidateSlot(int slot)
        {
            if (slot < 0)
            {
                return 0;
            }
            else if (slot >= _GetSlotCount())
            {
                return _GetSlotCount() - 1;
            }
            else
            {
                return slot;
            }
        }

        /// <summary>
        /// Validates a slot name, returning a valid slot name
        /// </summary>
        /// <param name="slotName"> The slot name to validate </param>
        /// <returns> A valid slot name, or the first slot if the input is invalid </returns>
        public string _ValidateSlot(string slotName)
        {
            //get the local slots
            DataDictionary localSlots = GetSlots(m_LocalSettings);
            if (localSlots.ContainsKey(new DataToken(slotName)))
            {
                return slotName;
            }
            else
            {
                return _GetSlotName(0);
            }
        }

        /// <summary>
        /// Gets the default slot to load
        /// </summary>
        /// <returns> The default slot to load, or the first slot if the default is invalid </returns>
        public string _GetDefaultSlot()
        {
            _GetGlobalSetting(slotToLoadByDefaultKey, out DataToken slotToLoadByDefault);
            return _ValidateSlot(slotToLoadByDefault.ToString());
        }

        /// <summary>
        /// Checks if a slot is valid
        /// </summary>
        /// <param name="slot"> The slot to check </param>
        /// <returns> True if the slot is valid, false if it is not </returns>
        public bool _IsSlotValid(string slot)
        {
            //return slot >= 0 && slot < GetSlotCount();
            //get the local slots
            DataDictionary localSlots = GetSlots(m_LocalSettings);
            return localSlots.ContainsKey(new DataToken(slot));
        }

        /// <summary>
        /// Gets the revision of a slot
        /// </summary>
        /// <param name="slot"></param>
        /// <returns>-1 if invalid</returns>
        public long _GetSlotRevision(string slot)
        {
            if (_IsSlotValid(slot))
            {
                //get the revision in the slot
                DataDictionary remoteSlots = GetSlots(m_RemoteSettings);
                if (remoteSlots.TryGetValue(slot, TokenType.DataDictionary, out DataToken slotDataToken))
                {
                    if (slotDataToken.DataDictionary.TryGetValue(revisionKey, out DataToken revision))
                    {
                        return (long)revision.Double;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the last updated date time of a slot
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public string _GetSlotUpdatedDateTime(string slot)
        {
            if (_IsSlotValid(slot))
            {
                //get the date in the slot
                DataDictionary remoteSlots = GetSlots(m_RemoteSettings);
                if (remoteSlots.TryGetValue(slot, TokenType.DataDictionary, out DataToken slotDataToken))
                {
                    if (slotDataToken.DataDictionary.TryGetValue(updatedDateTimeKey, out DataToken updatedDateTime))
                    {
                        return updatedDateTime.ToString();
                    }
                }
            }
            return "Unknown";
        }

        public string _GetSlotInfo(string slot)
        {
            return string.Format("Slot Revision: {0}, Last Updated: {1}", _GetSlotRevision(slot), _GetSlotUpdatedDateTime(slot));
        }
        #endregion

        #region Remote Differences
        /// <summary>
        /// Gets the differences between two dictionaries
        /// </summary>
        /// <param name="dict1"> The first dictionary </param>
        /// <param name="dict2"> The second dictionary </param>
        /// <returns> True if there are differences, false if there are not </returns>
        public bool _GetDictionaryDifferences(DataDictionary dict1, DataDictionary dict2)
        {
            //this is the lazy way right now but fuck it, the recursive system is really confusing my brain and semi unnecesary
            //TODO: Make this actually work properly and the way it should again. The ordering *shouldnt* matter since they are dictionarys
            //convert both to strings and use compare on them
            string dict1str = ConvertDictToString(dict1);
            string dict2str = ConvertDictToString(dict2);
            return !dict1str.Equals(dict2str);
            //differences = _GetDictionaryDifferencesRecursive(dict1, dict2);
            //return differences.Count > 0;
        }
        /* 
                /// <inheritdoc cref="_GetDictionaryDifferences(DataDictionary, DataDictionary, out DataDictionary)"/>
                [RecursiveMethod]
                public DataDictionary _GetDictionaryDifferencesRecursive(DataDictionary dict1, DataDictionary dict2)
                {
                    //TODO: possible early exit strategy, if the dictionarys are exactly the same

                    DataDictionary differences = new DataDictionary();
                    DataList dict1keys = dict1.GetKeys();
                    DataList dict2keys = dict2.GetKeys();
                    foreach (DataToken key in dict1keys.ToArray())
                    {
                        if (dict2keys.Contains(key))
                        {
                            //dictionary 2 has the same key, but we need to check its contents to see if they are the same or not
                            if (dict1[key].TokenType == TokenType.DataDictionary && dict2[key].TokenType == TokenType.DataDictionary)
                            {
                                //recursively check the dictionaries
                                DataDictionary subDifferences = _GetDictionaryDifferencesRecursive(dict1[key].DataDictionary, dict2[key].DataDictionary);
                                //only add the key if subDifferences contains anything
                                if (subDifferences.Count > 0)
                                {
                                    differences.Add(key, new DataToken(subDifferences));
                                }
                            }
                            else
                            {
                                //the values are normal, so we can compare them directly
                                if (!dict1[key].Equals(dict2[key]))
                                {
                                    differences.Add(key, dict1[key]);
                                }
                            }
                        }
                        else
                        {
                            //dictionary 2 does not have the key, so we add it to the differences entirely
                            differences.Add(key, dict1[key]);
                        }
                    }

                    //do the same but in the opposite direction, being just as vigilant
                    foreach (DataToken key in dict2keys.ToArray())
                    {
                        if (!dict1keys.Contains(key))
                        {
                            differences.Add(key, dict2[key]);
                        }
                    }

                    return differences;
                } */

        /// <summary>
        /// Checks if there are differences between the local and remote settings
        /// </summary>
        public bool _HasRemoteDifferences()
        {
            //do a dummy early check and see if their counts are different
            if (m_LocalSettings.Count != m_RemoteSettings.Count)
            {
                return true;
            }

            return _GetDictionaryDifferences(m_LocalSettings, m_RemoteSettings);
        }


        /// <summary>
        /// Checks for differences between the local and remote settings, and runs the callback if differences are found
        /// </summary>
        private void CheckForDifferences()
        {
            if (_HasRemoteDifferences())
            {
                RunCallback(PlayerSettingsCallback.OnRemoteDifferencesDetected);
            }
            else
            {
                RunCallback(PlayerSettingsCallback.OnRemoteDifferencesResolved);
            }
        }

        /// <summary>
        /// Converts a dictionary to a string for testing purposes
        /// </summary>
        /// <param name="inputDictionary"> The dictionary to convert </param>
        /// <returns> The dictionary as a string </returns>
        static string ConvertDictToString(DataDictionary inputDictionary)
        {
            VRCJson.TrySerializeToJson(inputDictionary, JsonExportType.Minify, out DataToken token);
            return token.ToString();
        }

        /// <summary>
        /// Get the slots dictionary from the settings
        /// </summary>
        /// <param name="settings"> The settings to get the slots from </param>
        /// <returns> The slots dictionary, or an empty dictionary if it does not exist </returns>
        private DataDictionary GetSlots(DataDictionary settings)
        {
            //check if we have the slots key
            if (settings.TryGetValue("Slots", TokenType.DataDictionary, out DataToken slotsToken))
            {
                return slotsToken.DataDictionary;
            }
            else
            {
                //if we dont, create it and then return it
                DataDictionary slots = new DataDictionary();
                settings.Add(new DataToken("Slots"), new DataToken(slots));
                return slots;
            }
        }

        /// <summary>
        /// Get the global settings dictionary from the settings
        /// </summary>
        /// <param name="settings"> The settings to get the global settings from </param>
        /// <returns> The global settings dictionary, or an empty dictionary if it does not exist </returns>
        private DataDictionary GetGlobalSettingsDictionary(DataDictionary settings)
        {
            //check if we have the global settings key
            if (settings.TryGetValue("Global Settings", TokenType.DataDictionary, out DataToken globalSettingsToken))
            {
                return globalSettingsToken.DataDictionary;
            }
            else
            {
                //if we dont, create it and then return it
                DataDictionary globalSettings = new DataDictionary();
                settings.Add(new DataToken("Global Settings"), new DataToken(globalSettings));
                return globalSettings;
            }
        }
        #endregion

        #region Initialization and Data retrieval
        void Start()
        {
            //setup our object name
            VRCPlayerApi Owner = Networking.GetOwner(gameObject);
            gameObject.name = Owner.displayName + "'s OF Settings";

            TEMPOWNERDOREMOVEWHENFIXED = Owner;

            RunCallback(PlayerSettingsCallback.OnStartFinished);
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            SendCustomEventDelayedFrames(nameof(_InitializeDatabase), UI.OpenFlightTablet.fadeTimeoutStart);
        }

        /// <summary>
        /// Loads the settings from the backup stored in playerdata. This is a one way operation, and should only be used if the settings are lost
        /// </summary>
        public void _LoadFromBackup()
        {
            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            //set the synched settings to the backup
            if (PlayerData.TryGetString(owner, DBBackupKey, out string backup))
            {
                synchedSettings = backup;
                if (VRCJson.TryDeserializeFromJson(synchedSettings, out DataToken _settings_token))
                {
                    m_RemoteSettings = _settings_token.DataDictionary;
                    //if is the local player, we want a full clone of the data so we can manipulate it. Otherwise, we just want a reference
                    if (Networking.IsOwner(gameObject))
                    {
                        m_LocalSettings = m_RemoteSettings.DeepClone();
                    }
                    else
                    {
                        m_LocalSettings = m_RemoteSettings;
                    }
                    Logger.Log("Settings database recovered successfully", this);
                    //invoke the callback
                    RunCallback(PlayerSettingsCallback.OnDataReady);
                    RunCallback(PlayerSettingsCallback.OnDataChanged);

                    //immediately serialize since our original data is just lost
                    _UploadSettings();
                }
                else
                {
                    string tokenError = _settings_token.Error.ToString();
                    Logger.LogWarning(string.Format("Failed to recover settings for player {0}. Error Reason: {1}.", owner.displayName, tokenError), this);
                }
            }
        }

        /// <summary>
        /// Saves the settings to the backup stored in playerdata. This should be used sparingly, as it can be a potentially heavy operation based on what other data is in player data
        /// </summary>
        public void _SaveBackup()
        {
            PlayerData.SetString(DBBackupKey, synchedSettings);
        }

        public void _InitializeDatabase()
        {
            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            Logger.Log(string.Format("Checking for settings database on player {0}", owner.displayName), this);
            if (VRCJson.TryDeserializeFromJson(synchedSettings, out DataToken _settings_token))
            {
                m_RemoteSettings = _settings_token.DataDictionary;
                //if is the local player, we want a full clone of the data so we can manipulate it. Otherwise, we just want a reference
                if (Networking.IsOwner(gameObject))
                {
                    m_LocalSettings = m_RemoteSettings.DeepClone();
                }
                else
                {
                    m_LocalSettings = m_RemoteSettings;
                }
                Logger.Log("Settings database deserialized successfully", this);
                //invoke the callback
                RunCallback(PlayerSettingsCallback.OnDataReady);
                RunCallback(PlayerSettingsCallback.OnDataChanged);
            }
            else
            {
                string tokenError = _settings_token.Error.ToString();
                Logger.LogWarning(string.Format("Failed to deserialize settings for player {0}. Error Reason: {1}. Attempting to load from backup instead", owner.displayName, tokenError), this);
                _LoadFromBackup();
            }

            //skip everything past here if its not the local player
            if (!Networking.IsOwner(gameObject))
            {
                return;
            }

            //check if this player has any settings that were loaded
            if (!IsInitialized)
            {
                Logger.Log("No slots found for player " + Networking.LocalPlayer.displayName + ". Creating a default slot", this);
                const string defaultSlotName = "Default";
                //initialize the settings
                _SaveSlot(defaultSlotName, out var DISCARD);

                //make sure the slot to load by default is set
                _SetGlobalSetting(slotToLoadByDefaultKey, defaultSlotName);
                _SetGlobalSetting(useWorldDefaultsWhenLoadingKey, false);
            }
            else
            {
                //we should save the backup
                _SaveBackup();
            }

            //else, we have data already

            //check if we need to load the settings on join
            if (_GetGlobalSetting(useWorldDefaultsWhenLoadingKey, out DataToken useWorldDefaultsWhenLoading))
            {
                if (!useWorldDefaultsWhenLoading.Boolean)
                {
                    if (_GetGlobalSetting(slotToLoadByDefaultKey, out DataToken settingValue))
                    {
                        if (_IsSlotValid(settingValue.ToString()))
                        {
                            _LoadSlot(_ValidateSlot(settingValue.ToString()), out var DISCARD, false);
                        }
                        else
                        {
                            Logger.LogWarning("Slot to load by default is invalid! Setting to first slot", this);
                            _SetGlobalSetting(slotToLoadByDefaultKey, _GetSlotName(0));
                        }
                    }
                }
            }
            else
            {
                Logger.Log("Not loading local user settings on join", this);
            }

            RunCallback(PlayerSettingsCallback.OnLocalDataReady);
            CheckForDifferences();
        }

        /// <summary>
        /// Called when we can ensure that the data in <see cref="synchedSettings"/> is valid
        /// </summary>
        public override void OnDeserialization(DeserializationResult result)
        {
            //ignore if from storage, as we will handle it in OnPlayerRestored
            if (result.isFromStorage)
            {
                return;
            }

            _InitializeDatabase();
        }
        #endregion

        #region Settings Application/Retrieval
        /// <summary>
        /// Gathers the settings from the various objects
        /// </summary>
        /// <returns> The settings gathered </returns>
        private DataDictionary GatherSlotSettings()
        {
            DataDictionary slotSettings = new DataDictionary();
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.flapStrengthBase)), wingFlightPlusGlide.flapStrengthBase);
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.flightGravityBase)), wingFlightPlusGlide.flightGravityBase);
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.requireJump)), wingFlightPlusGlide.requireJump);
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.allowLoco)), wingFlightPlusGlide.allowLoco);
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.useAvatarModifiers)), wingFlightPlusGlide.useAvatarModifiers);
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.canGlide)), wingFlightPlusGlide.canGlide);
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.fallToGlide)), wingFlightPlusGlide.fallToGlide);
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.glideControl)), wingFlightPlusGlide.glideControl);
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.airFriction)), wingFlightPlusGlide.airFriction);
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.bankingTurns)), wingFlightPlusGlide.bankingTurns);
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.glideAngleOffset)), wingFlightPlusGlide.glideAngleOffset);
            slotSettings.Add(new DataToken(nameof(wingFlightPlusGlide.useAvatarScale)), wingFlightPlusGlide.useAvatarScale);
            return slotSettings;
        }

        /// <summary>
        /// Spreads the settings to the various objects
        /// </summary>
        /// <param name="settingsLocation"> The settings to spread </param>
        private void SpreadSettings(DataDictionary settingsLocation)
        {
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.flapStrengthBase), ref wingFlightPlusGlide.flapStrengthBase);
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.flightGravityBase), ref wingFlightPlusGlide.flightGravityBase);
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.requireJump), ref wingFlightPlusGlide.requireJump);
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.allowLoco), ref wingFlightPlusGlide.allowLoco);
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.useAvatarModifiers), ref wingFlightPlusGlide.useAvatarModifiers);
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.canGlide), ref wingFlightPlusGlide.canGlide);
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.fallToGlide), ref wingFlightPlusGlide.fallToGlide);
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.glideControl), ref wingFlightPlusGlide.glideControl);
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.airFriction), ref wingFlightPlusGlide.airFriction);
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.bankingTurns), ref wingFlightPlusGlide.bankingTurns);
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.glideAngleOffset), ref wingFlightPlusGlide.glideAngleOffset);
            TryApplySetting(settingsLocation, nameof(wingFlightPlusGlide.useAvatarScale), ref wingFlightPlusGlide.useAvatarScale);
        }

        /// <summary>
        /// Gets a setting from the settings dictionary
        /// </summary>
        /// <param name="settingsLocation"> The location of the settings </param>
        /// <param name="key"> The key of the setting </param>
        /// <param name="token"> The resulting token of the setting </param>
        /// <returns> True if the setting was retrieved, false if it failed </returns>
        public static bool GetSetting(DataDictionary settingsLocation, string key, out DataToken token)
        {
            if (settingsLocation.TryGetValue(key, out token))
            {
                return true;
            }
            else
            {
                Logger.LogWarning(string.Format("Failed to get setting {0} from settings. Keeping current setting. Error reason: {1}", key, token.Error.ToString()), null);
                return false;
            }
        }

        /// <summary>
        /// Tries to apply a setting to a variable reference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingsLocation">Where the settings are stored</param>
        /// <param name="keyName">The key of the setting</param>
        /// <param name="variableReference">The variable to apply the setting to</param>
        /// <returns>True if the setting was applied, false if it failed</returns>
        public static bool TryApplySetting<T>(DataDictionary settingsLocation, string keyName, ref T variableReference)
        {
            bool ableToGetSetting = GetSetting(settingsLocation, keyName, out DataToken token);
            if (ableToGetSetting)
            {
                /*
				just for reference
				nameof(System.Boolean) = "Boolean"
				typeof(T).Name = "Boolean"
				
				a single in this case actually means float
				*/

                //enforce the type of the setting
                switch (typeof(T).Name)
                {
                    case nameof(System.Boolean):
                        variableReference = (T)(object)token.Boolean;
                        break;
                    case nameof(System.Single):
                        variableReference = (T)(object)token.Number;
                        break;
                    case nameof(System.Int32):
                        variableReference = (T)(object)token.Number;
                        break;
                    default:
                        Logger.LogError(string.Format("Failed to apply setting {0}. Unsupported type {1}", keyName, token.TokenType.ToString()), null);
                        return false;
                }
                return true;
            }

            return false;
        }
        #endregion
    }
}
