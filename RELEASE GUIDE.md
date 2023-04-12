# Updating the latest release
if you are updating the latest release number, then all you need to do is manually trigger the Build Release action (Keep in mind anyone who already has the package in a project WONT get this new version since the version number is the same)

# New Release
if you are making a new release, increment the version key in the relevant package.json file, then manually trigger the Build Release action. This will create a new release with the new version number, and update the website with the new info aswell