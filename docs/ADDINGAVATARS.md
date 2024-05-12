!> This documentation is incomplete! Help us improve it by contributing to the [OpenFlight Repository](https://github.com/Mattshark89/OpenFlight-VRC/tree/main/docs) or by joining the [OpenFlight Discord](https://discord.gg/XrJsUfwqkf) and letting us know what you want to see here!

# Adding Avatars to OpenFlight
OpenFlight is designed to be as simple as possible to add new avatars to, both from a world creator standpoint and as a contributer. This guide will walk you through the process of adding a new avatar to the system, so all OpenFlight worlds can detect it. For more information on how we know what avatar you are in, see the [Avatar Detection](TODO) page.

## Collecting the required information
<!-- panels:start -->
To begin, its recommended to collect all of the information you will need to add the avatar to the system in a notepad or other text editor. This will make it easier to copy and paste the information into the system later on, and provide the relevant information in your subsequent pull request. Dont worry about sanitizing any fields, as standard UTF-8 characters are supported.
- Avatar Base Name
  - This is the name of the base itself. Some examples of this are Kitavali, Da'vali, etc.
- Avatar Creator
  - This is the name of the person who created the avatar base. If the base is an edit of another creators base, include the original creators name as well, seperated by a `/`. For example, `Happyrobot33 / Mattshark89`
- Introducer
  - This is you! This is the name that will be displayed in the system as the person who added the avatar to the systems list.

<!-- div:left-panel -->
- Hash
  - This is the hash of the avatar. You can find this by going to a world with the tablet system installed, navigating to the debug tab and copying down the hash listed. If the world has a OpenFlight version older than 1.6.0, there will be 2 hashes listed. The mandatory hash you *must* include is the hash with `v2` on the end. The `v1` hash is deprecated and should not be submitted.

<!-- div:right-panel -->
<br>

<!-- div:left-panel -->
- WingTipOffset
  - This can be determined by turning on the `Show Gizmos` toggle in the tablet and adjusting the slider labelled `WingtipOffset` until the transparent ball attached to your right hand is roughly at the tip of your wing. If your wings dont extend past your hand or stop before your hand, you can leave this at 0.

<!-- div:right-panel -->
<br>

!> Ensure that you do not extend your VR controller to the point where your avatars hand cannot reach it, as we use the position of the VR controller to determine the position of the wingtip gizmo, not your avatars hand.

<!-- div:middle-panel -->
- Link to the avatar base / photo of the avatar base
  - This is the link to the avatar base itself, or a photo of the avatar base. This is used to verify that the avatar base actually meets the criteria for being added to the system.
- Weight
  - This is the weight of the avatar. The way of determining this value hasnt actually been decided yet, so you can ignore it for now and just set it to 1.
<!-- panels:end -->

## Formatting the information
<!-- panels:start -->
<!-- div:left-panel -->
Some of the information you collected now needs to be formatted into JSON. Copy this template and then fill in each field accordingly. For the hash field, make sure you format it in an array, even if you only have one entry. A example is provided incase you need help.
<!-- div:right-panel -->
<!-- tabs:start -->
#### **Template**
```json
"?": {
  "Original": {
    "Name": "?",
    "Creator": "?",
    "Introducer": "?",
    "Hash": ["?", "?"],
    "Weight": ?,
    "WingtipOffset": ?
  }
}
```
#### **Example**
```json
"Kitavali": {
  "Original": {
    "Name": "Kitavali",
    "Creator": "Rai Kitamatsu",
    "Introducer": "Happyrobot33",
    "Hash": ["234133961v2"],
    "Weight": 1,
    "WingtipOffset": 6.6
  }
}
```
<!-- tabs:end -->
<!-- panels:end -->

### Advanced Formatting
In some cases, an avatar base may have different versions as the creator has updated it. If you are either adding a new version of an existing avatar base, or adding a new avatar base that has multiple versions, there is a specific format to follow. Here is an example of what a properly formatted JSON entry for an avatar base with multiple versions should look like:
```json
"Reverse Avali": {
  "Original": {
    "Name": "Reverse Avali",
    "Creator": "VictonRoy",
    "Introducer": "Happyrobot33",
    "Hash": ["-1678847866v2"],
    "Weight": 1,
    "WingtipOffset": 1.3
  },
  "Modified": {
    "Name": "Reverse Avali",
    "Creator": "VictonRoy / farfelu",
    "Introducer": "Heather May",
    "Hash": ["-1731617419v2"],
    "Weight": 1,
    "WingtipOffset": 2.28
  },
  "Webserfer Alteration": {
    "Name": "Reverse Avali",
    "Creator": "VictonRoy / Webserfer",
    "Introducer": "Webserfer",
    "Hash": ["-597043440v2"],
    "Weight": 1,
    "WingtipOffset": 4.5
  }
}
```
?> If you need more help with this, dont be afraid to reach out to us on the [Discord](https://discord.gg/XrJsUfwqkf)!

## Editing the JSON file and submitting a pull request
!> This section needs to be completed!

To finalize this process, we now need to edit the actual file in the repository. To do this, follow [this link](https://github.dev/Mattshark89/OpenFlight-VRC/blob/main/Packages/com.mattshark.openflight/Runtime/data.json) to be taken to a web-based editor for the file. You may need to login to GitHub first. Once the editor and file has loaded, scroll to the bottom of the file where the last sets of `}` are.
```json
      }
    } <-- Add a comma here
    Your JSON entry here
  }
}
```
You will also need to make sure you increment the version number at the top of the file, and update the date to the current date.

```json
{
  "JSON Version": "1.1.5", <-- Increment this
  "JSON Date": "2023-04-26", <-- Update this
  "Bases": {
    ...
  }
}
```

?> We use [Semantic Versioning](https://semver.org/) for our version numbers, so make sure you follow the rules for that. For our purposes, avatar additions are considered a minor version update, so increment that field, and set the patch field to 0 if it isnt already.

?> Our date format is `YYYY-MM-DD`, based on UTC Time, so make sure you follow that format, and not your local timezone. The date for right now is <utcdate></utcdate>

Lastly, at the top of the JSON, there is a list of VRChat usernames for contributers. In newer versions, this will be scrolling on the tablet and will also give you special effects in worlds.
```json
{
  "Contributers": ["Happyrobot33", "Mattshark89", "DatGek", "Heather May", "mackandelius", "Morghus", "Krazen", "fundale", "Literally Marty", "Roimu", "Daernaro", "IlChiporiAlbino", "Darizard", "-Jinxy-", "OrcaToaster"],
}
```

After doing this, save the file using `Ctrl+S`, and then use the key combination `Ctrl+Shift+G` to access source control. Once there, fill in the message field with `[Avatar Addition] Avatar Name`, replacing `Avatar Name` with the name(s) of the avatar(s) you are adding. Once completed, select Commit & Push, and push as a fork repository.

!> If you are adding multiple avatars, make sure you list all of their names in the message.
