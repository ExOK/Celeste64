# Working with Versioned Save Data

## Context
The original Celeste 64 did not support any sort of versioning with its save data, or other persisted data that gets loaded from files. This meant new properties could be added, but we didn't have much control over upgrading save data, if things ever needed to get migrated to another file, or if we needed a way to upgrade the existing properties in the save data in some way.

To solve this, we have implemented a versioning persisted data system that allows us to upgrade various pieces of persisted data when needed. 

## Structure
- Persisted Data Classes: 
	- Persisted Data Classes define all the available properties for a given version of serialized data. This is where all the persisted data lives and gets loaded to. This can also define an `UpgradeFrom` function to upgrade from a previous version.
	- An example is the Save_V02.cs class
	- All Persisted Data Classes need to implement the Version property to determine which version of the file they are. This is a number starting from `1` that should increment with every version.
	- If this is the first version of the file, it should extend the `PersistedData` class, which does not require it to implement an upgrade path
	- If this is not the first version of the file, it should implement the `VersionedPersistedData` class, which requires it to implement an UpgradeFrom function, and passing in the previous version as a generic type.
- Data Model Classes:
	- This is a wrapper for the persisted data classes that is used to interface with and manage the data in the latest version of the related persisted data object.
	- An example is the Save.cs class, which manages the Save_VXX data classes.
	- This is technically optional, but is recommended for things that get referenced in a lot of places throughout the code base.
	- The reason for this being separate is to simplify the upgrade process. You could reference the persisted data object directly, but it would mean any time we do an upgrade, every reference throughout the whole codebase would need to be updated. By using a wrapper like this, we can isolate a majority of the upgrades to exist in just the one model class.
	- This also is a good place to put functions and properties for interfacing with the persisted data, to avoid needing to have the exact same copies of the function on every version of the persisted data class.

## When to perform a version upgrade
If all you need to do is add a new property to the persisted data class, you do NOT need to perform a full version upgrade. You can simply add the property to the latest version of the class, and add a default value, and save files will automatically get that default when they are loaded. If you add a new property, you should also consider updating the related Model class as well if applicable.

If you just need to delete a property, you should not need to do a version upgrade either.

Upgrades should be reserved for cases where we need to perform more involved upgrades to the overarching save file structure. One example of this is that the original game had settings stored as part of the save file, but we performed an upgrade that split the settings out into its own settings file so it could be shared between all save file (See the `UpgradeFrom` function in `Save_V02`.) This required more control over how the data got migrated, so this was a good example of needing to upgrade.

Another reason to upgrade would be if you needed to mutate the existing save data in some way. Like for example, if you need to convert a list of ints into a list of strings, or if you needed to append something to the end of an existing string property. 

If you have nested persisted data classes, and the inner class gets upgraded, you'll need to upgrade the version of the class that uses it too, and the entire chain up to the base data class. For example, the `Save_V02` class references the `LevelRecord_V01` class. If `LevelRecord_V01` gets upgraded to `LevelRecord_V02`, you'll need to upgrade `Save_V02` to `Save_V03` as well.

## Creating a new Persisted Data Class
To make a new persisted data class, make a new class that extends PersistedData, and the implement the Version property. The first version should be set as 1. 

Implement the JsonSerializerContext for this class. You'll need to make a new context for every version.

Implement the GetTypeInfo function. This should generally return {your JsonSerializerContext}.Default.{your persisted data class name}

Consider making a data model class that wraps your persisted data class. This should make it easier to upgrade in the future, because you can just update the data model class references, instead of everything across the whole codebase.


## Upgrading a Persisted Data Class and making a new version
To upgrade a persisted data class, you'll want to copy the existing latest Persisted Data Class file, and use that as a base. Rename the file with an incremented version, and increment the Version property in Code as well. Then change the new class to implement VersionedPersistedData<{The last version class before this one}>, so Save_V02 should implement VersionedPersistedData<Save_V01>. Also update the JsonSerializerContext and the getTypeInfo function as well.

Next you'll want to implement the UpgradeFrom function. This is where we should actually be doing the bulk of the upgrade work. Here, you're going to want to take the previous version of the object, and copy all of the data over to the new version. Then you can also set it up to perform any extra upgrade steps you need to do.

After you have the persisted data class set up, you'll need to update any references from the old version to the new version. Ths should mostly be contained to the model/wrapper class, but you may need to update a few places outside of that too.

If you upgraded a persisted data class that is used by another persisted data class, make sure to upgrade the one that uses it too.

Finally, make sure to test both that your update works, and that the data can be saved and loaded correctly after the update. You may also want to make a backup of your save too before doing this, just to be safe, and to make it easier to test the upgrade multiple times if you need to.