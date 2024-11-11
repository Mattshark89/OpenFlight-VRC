using UnityEditor;
using UnityEngine;

using System;
using System.Collections.Generic;

namespace OpenFlightVRC.Editor
{

    public class FontSetup : EditorWindow
    {
        private const string ContextMenuLocation = "Assets/Extract Unicode Character List";

        [MenuItem(ContextMenuLocation)]
        public static void OpenFontSetup()
        {
            //get the font
            Font font = Selection.activeObject as Font;

            CharacterInfo[] characters = font.characterInfo;

            string characterList = "";

            foreach (CharacterInfo character in characters)
            {
                characterList += (char)character.index + " ";
            }

            //save a text asset file next to the font
            string path = AssetDatabase.GetAssetPath(font);
            path = path.Substring(0, path.LastIndexOf('.')) + ".txt";
            System.IO.File.WriteAllText(path, characterList);

            AssetDatabase.Refresh();
        }

        [MenuItem(ContextMenuLocation, true)]
        public static bool OpenFontSetupValidation()
        {
            //return true when the selected object is a font
            return Selection.activeObject is Font;
        }
    }
}
