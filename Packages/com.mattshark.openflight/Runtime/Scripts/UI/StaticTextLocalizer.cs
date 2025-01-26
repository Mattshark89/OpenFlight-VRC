using System.Text.RegularExpressions;

using TMPro;

using UdonSharp;

using UnityEngine;

using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class StaticTextLocalizer : LoggableUdonSharpBehaviour
    {
        public TextAsset localeFile;
        private DataDictionary localeData;
        public string unlocalizedText;
        private TextMeshProUGUI textMesh;

        public override string _logCategory { get => UIBase.UICATEGORY; }


        void Start()
        {
            //grab the text mesh
            textMesh = GetComponent<TextMeshProUGUI>();

            //deserialize the locale file
            if (VRCJson.TryDeserializeFromJson(localeFile.text, out DataToken localeDataTemp))
            {
                localeData = localeDataTemp.DataDictionary;
            }

            //localize the text
            textMesh.text = AttemptLocalize(unlocalizedText);
        }

        void Update()
        {
            textMesh.text = AttemptLocalize(unlocalizedText);
        }

        private string AttemptLocalize(string unlocalizedText)
        {
            //find all sections in the unlocalized text that contain a localization identifier
            //localization identifiers are in the format %identifier_like_this%

            //run \%\w+\% to find all identifiers

            MatchCollection matches = Regex.Matches(unlocalizedText, @"\%\w+\%");
            for (int j = 0; j < matches.Count; j++)
            {
                //Logger.Log("Match found: " + matches[j].Groups[0].Value);
                Match match = matches[j];
                //remove the % from the start and end of the identifier
                string id = match.Groups[0].Value.Replace("%", "");

                //attempt to localize the identifier
                string localizedText = AttemptLocalizeIdentifier(id, VRCPlayerApi.GetCurrentLanguage());

                //replace the unlocalized text with the localized text
                unlocalizedText = unlocalizedText.Replace("%" + id + "%", localizedText);
            }

            return unlocalizedText;
        }

        private string AttemptLocalizeIdentifier(string identifier, string localeID)
        {
            //first see if the locale file contains the identifier
            if (localeData.TryGetValue(identifier, out DataToken identifierLocalizationData))
            {
                //now we want to find the correct locale
                if (identifierLocalizationData.DataDictionary.TryGetValue(localeID, out DataToken localeString))
                {
                    //return the localized text
                    return localeString.String;
                }
                else
                {
                    //if we don't have the correct locale, return the unlocalized text
                    Log(LogLevel.Error, "Failed to localize identifier: " + identifier + " - does not have a value for locale: " + localeID);
                    return identifier;
                }
            }
            else
            {
                //if not, return the identifier and print a error
                Log(LogLevel.Error, "Failed to localize identifier: " + identifier + " - does not exist in locale file");
                return identifier;
            }
        }
    }
}
