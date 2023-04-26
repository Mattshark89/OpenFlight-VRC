!> This documentation is incomplete!

# Adding Avatars to OpenFlight
OpenFlight is designed to be as simple as possible to add new avatars to, both from a world creator standpoint and as a contributer. This guide will walk you through the process of adding a new avatar to the system, so all OpenFlight worlds can detect it. For more information on how we know what avatar you are in, see the [Avatar Detection](TODO) page.

## Collecting the required information
To begin, its recommended to collect all of the information you will need to add the avatar to the system in a notepad or other text editor. This will make it easier to copy and paste the information into the system later on, and provide the relevant information in your subsequent pull request. Dont worry about sanitizing any fields, as standard UTF-8 characters are supported.
- Avatar Base Name
  - This is the name of the base itself. Some examples of this are Kitavali, Da'vali, etc.
- Avatar Creator
  - This is the name of the person who created the avatar base. If the base is an edit of another creators base, include the original creators name as well, seperated by a `/`. For example, `Happyrobot33 / Mattshark89`
- Introducer
  - This is you! This is the name that will be displayed in the system as the person who added the avatar to the systems list.
