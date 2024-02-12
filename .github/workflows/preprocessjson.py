#YES, I KNOW that this could be a single line JQ command,
#but MAN that would be extemely complicated and unreadable, and im not that
#smart, so python script it is.

import json
import sys
import os

def __main__(argv):
    # Load the JSON file
    # with open(sys.argv[1], 'r') as file:
    #    data = json.load(file)

    # Load the json file relative to packages
    path = "C:/Users/Matthew/Documents/OpenFlight-VRC/Packages/com.mattshark.openflight/Runtime/data.json"
    with open(path, encoding="utf-8") as file:
        data = json.load(file)

    # Create a new dictionary to store the hash table
    hashTable = {}

    # Loop through the bases
    bases = data["Bases"]
    for base in bases:
        for variant in bases[base]:
            variantData = bases[base][variant]
                
            # Get all the hashes needed
            hashes = variantData["Hash"]
            for hash in hashes:
                # Remove the hash key from a deep copy of the variant data
                variantData = variantData.copy()
                if "Hash" in variantData:
                    del variantData["Hash"]

                hashTable[hash] = variantData

    #nest the hash table
    hashTable = {"HashTable": hashTable}

    ## add/override the hash table with the new data
    data = {**data, **hashTable}

    # Write the new JSON file
    with open(path, 'w') as file:
        json.dump(data, file, indent=4)

if __name__ == "__main__":
    __main__(sys.argv)
