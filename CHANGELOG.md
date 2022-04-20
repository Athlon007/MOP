# Changelog

## 3.6.3 (TBA)

### Bug Fixes

- Fixed drag race disappearing when getting to the end of the drag strip on low toggle distance.

## 3.6.2 (16.04.2022)

### Added

- Added a debugging custom rule flag: "show_garbage_memory_usage":
  - Replaces FPS meter functionality, to show currently garbage collector memory and the rate at which the memory is being filled

### Bug Fixes

- Drastically reduced the number of lag spikes by code optimization and fixing memory leaks in MOP (6x less lag spikes!)
- Fixed a bug where if mod used primitive cubes on Satsuma, they would appear with MOP running

## 3.6.1 (15.04.2022)

### Added

- On beta builds, a warning will be shown at the top of the main menu and in the console

### Changes

- The MOP rule files message in the main menu has been moved back to the center of the screen
- Partial overhaul of rule files back-end
- Rule files for mods that are disabled won't be loaded anymore
- Improved saving of MOP's console output
- Many minor optimisation changes
- MOP will fail more gracefully, if there was some issue while checking if Satsuma is fully loaded

### Bug Fixes

- Fixed a bug which would make MOP unable to restart the function that controls enabling/disabling objects
- Fixed game restarting procedure not working correctly
- Fixed a bug in which sometimes certain engine parts would be invisible

## 3.6 (01.04.2022)

### Added

- Rule Files API: Added flag "change_parent: object_name new_parent_object_name"
  - Changes the object's root parent, indicated by object_name, to parent defined by new_parent_object_name
  - new_parent_object_name can be set to NULL, to make the object an orphan
- Added "Experimental" settings section:
  - "Fast Algorithm" - it decreases the time it takes for MOP to enable/disable objects
  - "Lazy Sector Updating" - it decreases the lag when disabling/enabling objects while entering the sector
- Added warning when disabling automatic rule updates
- Added warning in settings when attempting to change Mode while in-game

### Changes

- Adjusted what objects are not disabled when entering the sector:
  - Lake and trees are not being disabled when entering the garage (on Balanced quality mode and above)
  - Bus stop and bushes are not being disabled when entering Teimo's store
- Optimization in item disabling (should reduce the lag and delay when disabling objects)
- Mod Loader Pro: Adjusted the color of the DONATE button
- Very minor changes in exception throwing
- Removed unnecessary exceptions for certain sectors

### Bug Fixes

- Fixed a bug that could crash the MOP, if the MOP tried to enable an item that does not exist
- Fixed an empty gap between sectors of the main part of the home and bathroom/sauna area, which removed a lag caused by enabling and then disabling all objects corresponding to those areas
- Fixed identical gap between the kitchen and the storage closet
- Fixed a bug that would sometimes cause MOP to not load the sector if player quickly moved between one another
- Fixed bug reports not working when pressing "Yes" to include a bug report
- After generating a bug report and clicking "No" to include save file, the bug report folder will open again
- Fixed MOP not detecting rule file minimum mod version correctly

## 3.5.1 (28.02.2022)

Version bump, because I incorrectly signed the 3.5 :)

## 3.5 (28.02.2022)

A lot of people were no complaining that MOP 3.4 is not available for Mod Loader Pro, so guess what? MOP 3.5 is available for BOTH mod loaders!

### Added

- (Mod Loader Pro) Restored tooltips
- (Mod Loader Pro) Restored resolution changing
- Added a warning if you try to run MSCLoader version of MOP on Mod Loader Pro and vice-versa

## 3.4.3 (14.02.2022)

### Bug Fixes

- Possibly fixed issue related to MOP crashing on IsSatsumaLoadedCompletely()
- The grill will not be disabled if it's set on fire
- Items on the grill will not be disabled

## 3.4.2 (03.01.2022)

### Bug Fixes

- Fixed a bug which caused the game to not spawn bought items

## 3.4.1 (03.01.2022)

Version bump, because releasing updates on MSCLoader is stupid.

## 3.4 (03.01.2022)

This version REQUIRES the MSCLoader!

### Added

- Added three new console commands:
  - 'mop resolution' - Lists all available resolutions
  - 'mop resolution [nOfResolution]' - Sets the resolution from the 'mop resolution' list
  - 'mop force-load-restart [true/false]' - Forces the 'missing Satsuma parts' game reload to happen
- Rule Files API: Added "skip_fury_collider_fix" that makes MOP skip the repair shop missing Fury collider fix

### Changes

- MOP has been ported to MSCLoader by Piotrulos
- 'mop help' list is now shown alphabetically
- MopSave.json in bug report is now stored in Save folder

### Bug Fixes

- Fixed a few typos

### Removed

- Removed resolution changing in the settings (unable to be ported with how MSCLoader handles settings)

## 3.3.10 (28.12.2021)

### Bug Fixes

- Fixed a bug that caused MOP to detect rule files for MOP 2.X as outdated, causing them to not load

## 3.3.9 (22.12.2021)

### Added

- MOP report includes MOP config file and MopSave.xml (if it exists)
- Added "GitHub" button into the settings
- Added special welcome message
- Restored the "Force Update" button for rule files

### Changes

- Updated the MOP bug report instruction
- Rule files loader data is now saved into RulesInfo.json
- MopSave files are now saved into MopSave.json (instead of MopSave.xml)

### Bug Fixes

- Fixed rule file auto-update not working
- Fixed a bug that caused missing spark plugs detection to not work

## 3.3.8 (19.12.2021)

### Added

- MOP report now correctly recognizes Windows 11

### Changes

- Improved reading game data in MOP report
- Many changes in class structures
- Many minor optimization improvements
- Minor changes in phrasing of some dialogues
- Dance hall, cottage, and cabin are not disabled in Quality mode

### Bug Fixes

- Fixed MOP not calculating the timeDisabled for certain items.
- Fixed "Delete All Logs" button not working correctly
- Fixed an error if the user deleted unused rule files and then tried to do it again
- Fixed Satsuma being frozen if using Safe Mode

## 3.3.7 (30.11.2021)

### Added

- Added more detailed logging in ToggleAll function
- MOP reports now contain a transcript of MOP console messages

### Changes

- Rewrote Perajarvi game fixes script to be more reliable
- Satsuma should not be frozen, if the MOP failed to load
- Minor internal changes

## 3.3.6 (25.10.2021)

### Changes

- Improved the way foot spoilage is calculated (shouldn't cause the items to immediately spoil)

### Bug Fixes

- Fixed MOP showing error about not all parts being installed, if the cylinder head was installed, but the engine block was not
- Fixed item spoilage not working when the level has loaded and the item hasn't been initialized yet
- Fixed compatibility with older game versions that don't have the fuse box

## 3.3.5 (25.10.2021)

### Added

- Added support for fuses
- Added support for item spoiling - from now on, item spoiling is not frozen, if they are not loaded

### Changes

- Minor optimization improvements

### Bug Fixes

- Fixed MOP unable to locate terrace house colliders
- Fixed MOP unable to locate correct radio to patch
- Fixed fuses that were taken out from the house disappearing after player left the house

## 3.3.4 (11.10.2021)

### Added

- MOP will check if the Satsuma engine has been loaded succcessfully
  - If not, the scene will automatically be restarted once, in order to remove that issue
  - If that didn't work, you will be informed about it
  - It SHOULD also include spark plugs

### Bug Fixes

- Fixed a bug preventing the game from being saved
- Fixed MOP not being able to locate the FSM of RadiatorHose3 database

## 3.3.3 (25.08.2021)

### Added

- Save integrity check now checks if halfshafts and battery terminal bolt values are correct

### Changes

- The game will now check if TRAFFIC car is about to be hit by a traffic car in BALANCED mode
  - Previously that would happen only in QUALITY mode
  - This change does not affect PERFORMANCE mode

### Bug Fixes

- Fixed crash caused by MOP not being able to find the car stock radio
- Fixed bug causing music coming out of the radio to randomly go silent
- Fixed a bug which caused performance mode settings to not update, making you stuck in a single performance mode

## 3.3.2 (11.06.2021)

### Bug Fixes

- Even more attempts to fix the bloody kilju/empty plastic can bug

## 3.3.1 (07.06.2021)

### Added

- Added a custom item save script, that should always save the items in the state and position they are supposed to be at
- Added compatibility with Advanced Backpack mod

### Changes

- Minor under the hood improvements
- "Shadow Distance" slider won't be displayed, if "Adjust Shadows" toggle is not checked
- Improved the performance of enabling and disabling human NPCs
- Slightly adjusted the distance when the logwalls spawn

### Bug Fixes

- Fixed occasional lag after loading the game, caused by Satsuma rigidbody being enabled when it should not be
- Fixed crashing sounds coming from Satsuma and Jonnez when walking next to them
- Fixed empty plastic cans possibly being unpickable by the player
- Fixed MOP disabling empty items, even when "Disable Empty Items" checkbox was not ticked
- Fixed items despawning too aggressively, if player entered the driving mode and was at home, resulting in items despawing as soon as he entered that mode
- Definitive fix for the unemptying kilju bottles
- Fixed an error while paying for packages containing wheels
- Fixed wheels not spawning when unpacking them
- Fixed pub fighter occasionally despawning while leaving/entering the pub
- Fixed Uncle's ragdoll at the pub causing havoc inside by clipping into pub furnitures

### Removed

- Removed engine culling (disabling textures), as it was sometimes causing engine parts to turn invisible

## 3.3 (20.05.2021)

This version fully ditches MSCLoader. From now on, Mod Loader Pro is a requirement.

### Added

- Added integration with Save Slots
  - If you close Saves menu, the current save will be verified again
- After few restarts, MOP will remind you to fully restart the game. This is done in order to free up RAM, that the game did not clear

### Changes

- Eliminated the lag caused by spawning items, while purchasing, unpacking shopping bag, removing fish from the fish trap, etc.
- Minor changes under the hood
- The MOP rule files message in main menu is now displayed in top right corner
- Optimized spawning of Amis Auto packages

### Bug Fixes

- (My Summer Car Bug) Fixed vanilla bug, which on higher framerates, causing Satsuma to violently stop, if the rear bumper got detach
- Fixed empty bottles not spawning after consuming milk or beer
- Grammar fixes
- Fixed "Open Logs Folder" button saying that logs folder does not exist
- Addressed an issue, in which for some users MOP would crash, if they were in jail
- Fixed the fire extinguisher holder not detecting freshly bought fire extinguishers
- Fixed trigger fixing not being applied to inactive triggers
- Fixed rear bumper unable to be detached, while having all screws unbolted
- Fixed a bug which caused all Satsuma parts to be detached after quitting to main menu and loading game back few times, without fully closing the game

### Removed

- Removed "mop restore-save" command
- Removed "mop do-bumper-fix" command
- Removed rule file flag "experimental_satsuma_physics_fix"
- Removed brute-force method of fixing rear bumper detaching bug, as it's no longer needed
- Removed "ExternalExecuting" class

## 3.2.3 (10.05.2021)

### Bug Fixes

- (My Summer Car Bug) Addressed MSC Bug, in which the Kekmet trailer may be glued to the composter at the house backyard
- Fixed empty milk packs flying in air
- Fixed other items sometimes possibly being frozen mid-air
- Fixed text in one of the message boxes

## 3.2.2 (05.05.2021)

### Added

- Added an average framerate recorder (FPS meter has to be enabled)
- Added "mop resolution" command, that lets you set a custom resolution

### Bug Fixes

- Possibly addressed the issue with light bulbs and spark plugs
- Addressed an issue with empty beer bottles floating in air after drinking
- Mod Loader Pro: Fixed debug functions assigned to F5 and F6 being included in ProLoader build of MOP

## 3.2.1 (30.04.2021)

### Bug Fixes

- MSCLoader: Fixed mod crashing

## 3.2 (30.04.2021)

### Added

- MOP now comes with two variants! A version with the legacy Mod Loader, and a shiny new Mod Loader Pro support!
  - In order to fully utilize the new Mod Loader Pro, download "MOP.pro.zip" file
- (Mod Loader Pro) Added "Change Resolution" button in MOP settings
- Added NexusMods button

### Changes

- Improved Hayosiko disabling during early part of the game
- Changed how vehicle position is verified
- Reduced time which takes for fail-safe trigger of loadscreen to do its thing to 20 seconds
- Slightly reduced the object spawning/despawning time
- Rocks won't despawn when player is at home

### Bug Fixes

- Sold kilju should not respawn on the junkyard as full anymore
- Fixed a lag that was fixable by player having walk to the Satsuma
- More fixes to radiator hose3 (again)
- Fixed fuel line bolt resetting to default
- Fixed rare case of not yet found suitcase falling through the map and triggering murderer sequence
- Fixed fire extinguisher holder

### Removed

- Removed rule files flag: "experimental_save_optimization"

## 3.1.7 (06.04.2021)

### Changes

- Updated to MSCLoader 1.1.13
- Improved performance of places enabling and disabling (even by up to 12x!)

### Bug Fixes

- (My Summer Car Bug) Fixed MSC bug, which caused Satsuma parts that detached themselves from the car sometimes being not reattachable until after the game restart
- Fixed Satsuma door hinges getting jammed, which prevented doors from being opened
- Fixed Satsuma odometer resetting to an on load value
- Fixed possible bug that may have caused not all mail boxes to be counted towards the Teimo adverts job progress
- Fixed "radiator hose 3" not spawning on fresh game save

## 3.1.6 (01.04.2021)

### Added

- MOP will try to fix your save file, if it finds that the passenger bucket seat is missing from your game world, while you own driver bucket seat
- Added "Delete unused rules" button in MOP settings
- Added some more information to MOP log

### Changelog

- Updated to MSCLoader 1.1.12
- Changed the minimum bolts reset handling - now it's a mod console error, instead of an exception
- Changed the warning in "Logging" section of MOP settings
- Updated FAQ link to MOP wiki
- Improved MOP load time
- Optimization improvements

### Bug Fixes

- Fixed a bug in which crash log and bug reporting would fail, if it was generated before ItemsManager could load
- Fixed MOP not loading, if a passenger bucket seat was missing
- Fixed flatbed being stuck attached to the Kekmet
- Fixed "radiator hose 3" not having ItemBehaviour property added to it
- Fixed "radiator hose 3" spawning at default position, if it was detached from the car and the game was saved
- Fixed "radiator hose 3" possibly disappearing on detach
- Sauna simulation won't reset itself anymore

## 3.1.5 (09.02.2021)

### Added

- If MOP got stuck on load screen for over 30 seconds, load screen will be disabled and an error will be displayed

### Changes

- MOP now asks if you want to generate a mod report, even if MOP couldn't load into the game.

### Bug Fixes

- Fixed infinite load screen due to CanTrigger finding

## 3.1.4 (04.02.2021)

### Added

- Added some more info to MOP reports

### Bug Fixes

- Fixed Perajarvi construction site despawning
- Possibly fixed an error happening, where in some cases MOP couldn't find the CanTrigger object during game save
- Possibly fixed an error, where MOP tried to check if rigidbody of item is disabled, but the item didn't have rigidbody component

## 3.1.3 (29.01.2021)

### Added

- MOP Report:
  - Added the number of installed mods
  - Added game window width and height
- "I found a bug button" will now ask you to load the game fully at least once, before generating a bug report
- Added an instruction what to do, if an error occures

### Bug Fixes

- Fixed an error, if object that was SatsumaMassManager was hooking didn't have Remove FSM
- Fixed pathfinding of objects in error logs
- Fixed engine renderers not being enabled back on
- Fixed fighting between EngineCulling and ItemBehaviour; In some cases, EngineCulling tried to disable the renderer, while the ItemBehaviour tried to enable it back on. From now on, EngineCulling has a priority over ItemBehaviour

## 3.1.2 (26.01.2021)

### Added

- MOP will now ask, if you want to include your save file with bug report

### Bug Fixes

- Second attempt on fixing the bug, which left renderer and rigidbody of the object disabled
- Fixed occassional null exception when toggling Teimo shop
- Fixed an error in rule files loading, where if a "ignore" flag was set to TRAFFIC object, MOP would say that this object is not being disabled and would the ignore that flag
  - With that, fixed compatibility with CallYourCousin
- Fixed kilju bottles sold to Jokke being teleported to the junkyard with kilju still in them
- Fixed possible null reference exception in SatsumaSeatsManager class
- Fixed windshield repair job not fixing the windshield
- MOP will now ask, if you want to include your save file with bug report
- Fixed repair shop door self resetting
- Fixed cabin door self resetting

## 3.1.1 (22.01.2021)

### Bug Fixes

- Boombox is no longer disabled by MOP, if it's playing music
- Fixed MOP not being able to find the can trigger at Jokke
- In some cases physics and mesh renderer of the object could get stuck disabled, this is now fixed
- Fixed Kekmet and trailer not being connected together, if the game was saved with trailer being attached to the tractor
- Fixed items clipping through vehicles in Performance mode
- Fixed kilju bottles sometimes not being teleported to the landfill after selling to Jokke
- Fixed possible case of delegating toggling method of ItemBehaviour being null
- Fixed firewood logs not being hooked properly
- Fixed an error where if player stood in a single place for long enough, traffic cars would pass by with renderers disabled

### Removed

- Removed "lazy updating" when player wasn't moving, due to it causing some issues.

## 3.1 (18.01.2021)

### Added

- Traffic vehicle renderers are now being disabled when not needed
- Added "I found a bug" button in MOP settings
  - Generates a .zip file that contains output_log.txt, MOP report and all today MOP logs

### Changes

- Total rewrite of WorldObjects
- "Loading Modern Optimization Plugin" message is now fully capitalized
- Rule files that couldn't be verified are not deleted anymore, instead they are being skipped
- Improved load time
- Increased dance hall toggling distance
- Renamed "Open session log folder" to "Open log folder"

### Bug Fixes

- Fixed MSC bug, which caused a lag when the highway traffic was loading
- Fixed Jokke's furnitures getting disabled when they shouldn't be
- Fixed disappearing "radiator hose 3"
- Fixed not appearing firewood logs, if they were picked from wood carrier
- Fixed possible cases of two or more ItemBehaviours being attached to a single object
- Fixed "Open log folder" button not working
- Fixed "Delete all logs" button not working
- Fixed Satsuma doors occasionally having two FixedJoints, making the doors stuck
- Fixed MOP potentially getting stuck on the "Loading Modern Optimization Plugin" load screen
- Fixed Gifu possibly stalling, if hand throttle hasn't been adjusted
- Fixed hiking grandma animation playing while she's sitting in the car

### Removed

- Removed "mop open-logs" command

## 3.0.1 (11.01.2021)

### Added

- Firewood log prefab is now being hooked from the beginning, meaning the firewood logs will despawn accordingly

### Changes

- MOP logs are not saved into session ID folder, instead they are called by the date time they were created in order to make it less confusing
- Framerate limiter values now range from 20 to 200
- Framerate limiter values now jump by 10, instead of by 1

### Bug Fixes

- Fixed MOP not working properly, if player is using analog controller
- Freshly spawned firewood logs should despawn correctly
- Fixed "mop generate-list" and "mop load-rules" commands not working as intended
- Possible bug fix for player dying in Satsuma from low speed impacts
- Fixed cursor showing up on MOP load screen
- Fixed a harmless error appearing on game load, if the player didn't steal the slot machine
- Fixed log folder appearing inside of mysummercar_Data, instead of the root MSC folder
- Fixed a typo in ignore_full obsolete warning

### Removed

- Removed "mop sector-debug" command from the help list

## 3.0 (04.01.2020)

### Added

- Added Modes of how MOP operates:
  - Pick between Performance, Balanced, Quality or Safe operation modes
  - Performance mode disables elements right "in your face", but gets you a bit more FPS
  - Balanced mode is is intended to balance between obvious disabling of objects, and quality
  - Quality mode tries to hide how MOP disables things, at the cost of speed
- MOP will not execute any heavy operations, if the player is not moving.
- Added "Session ID":
  - GUID based identification system, which will mark this specific session
  - Used mainly for crash log system
- Added "mop open-logs" command
- Garage doors are now being disabled if player is not in the yard area.
- "mop help" command now supports search. You can search for command help by typing "mop help <command_name>", for instance "mop help version" will show you information about "mop version" command, or for instance, type "mop help generate-list" to show the info about "generate list" command
- Added "mop load-rules [true/false]", if set to false, MOP won't load any rule files.
- Rule Files API: Object names with space can be written using quotation marks, instead of using "%20", example: you can use ignore:"The Object" insted of ignore:The%20Object

### Changes

- Updated for MSCloader 1.1.8
- Renamed "Run in Background" to "Run Game in Background"
- Major overhaul of how world objects are handled
- Many improvements in Vehicles logic
- Optimized initialization of MOP
  - MOP will not crash, if one or more elements couldn't be loaded
  - You can keep using MOP and playing the game, even if something didn't finitialize
- Dynamic Draw Distance is now smoother
- The previously experimental optimization features are now enabled by default in Performance mode
- Improved the reliability of the script that reinstalls the rear bumper after game load
- Changed the welcome screen
- Improved the script that prevents the trailer to get stuck under floor
- Driveway sector now is enabled in the performance mode
- Complete overhaul of error logging system
  - Now mulitple error logs can be saved during the session (but only one for the error type)
  - Logs are now saved into My Summer Car installation path, into "MOP_Logs" path, which are then saved inside of the current session ID folder
  - Errors now are separated by critical and non critical errors. If non-critical error occures, the player can continue playing
- Minor changes to settings UI
- "mop open-folder" is now "mop open-config"
- Rule Files API: ignore_full is now obsolete, and has been replaced with "ignore: <object_name> fullignore"
- Renamed "Destroy Empty Beer Bottles" to "Destroy Empty Bottles"
  - "Destroy Empty Bottles" now works for booze, coffee, spirit, milk and vodka shots
- Many smaller optimization changes and improvements
- Changed "Don't delete unused rule files" to "Delete unused rule files" (disabled by default)
- If not in Performance mode, Perajarvi church is not disabled anymore, so it's always visible on the horizon
- Lake house in Perajarvi won't be disabled in Quality mode
- Satsuma is now being disabled, if left for repair works
- "mop wiki" command now shows you the prompt before opening the browser
- Major changes in the internal file structure
- File folder structure now represents the structure of namespaces
- Disabling and enabling objects is now 2x faster

### Bug Fixes

- Fixed "Critical error". Now if error happens will show where the issue happened specifically.
- Fixed a bug in which the vehicles would sometimes get frozen while driving
- Fixed a bug in which the MOP would potentially be stuck on load screen
- Mouse movement is now also disabled on load screen
- Fixed MOP not loading up the front hook of Kekmet
- Fixed a bug in which player could interract with objects on MOP load screen
- Fixed a bug in which the value "lightSelection" in Satsuma class would be null
- Fixed a bug in which the vehicle audio sound would stay in place
- Fixed engine renderers not showing up, if the car's hood got detached
- Fixed the engine cooldown ticking sound replaying after the player walks away from the car and walks back to it
- Fixed doors paint color potentially resetting
- Fixed berry picking skill resetting to default
- Fixed Gifu air pressure resetting to default
- Fixes "open output_log.txt" not working properly
- Fixed badly placed trigger for Shop Mod items
- Fixed Gifu hand throttle stopping to work
- Fixed Kekmet hand throttle stopping to work
- Fixed a bug in which vsync would get re-enabled if the game has been Alt+Tabbed with disabled running in background
- Fixed a bug in which the money amout in the suitcase would reset to the default value

### Removed

- Removed experimental Satsuma storage system
- Removed many scripts that are not needed anymore
- Removed much of now obsolete code
- Removed "mop sector-debug" command

## 2.12.2 (20.07.2020)

### Changes

- Reverted to pre-2.12 method of getting items position

## 2.12.1 (16.07.2020)

### Bug Fixes

- Fixed an error on game load, which would cause items to teleport to XYZ: 0,0,0

## 2.12 (15.07.2020)

### Added

- On the game load, MOP will not get the saved position of items, meaning less chance for items to fall through car, etc.
- Added a message when you try to enable Safe Mode in the game

### Changes

- Minor changes to the settings

### Bug Fixes

- Fixed CD cases not appearing, if the player loaded the game away from 0,0,0 position
- Fixed Satsuma potentially breaking on game save
- Improved "Open output_log.txt" button inner workings
- Fixed Satsuma getting wrecked, if played died in the car while it was moving

### Removed

- Removed unused code

## 2.11.4 (09.07.2020)

### Added

- CDs and CD cases are now hooked by MOP
- Rule Files API: "satsuma_ignore_rule" now affects Z-fighting fixes of Satsuma gauge neeldes
- Rule Files API: "ignore" flag now affect Satsuma childs and renderers objects

### Bug Fixes

- (My Summer Car Bug) Fixed Z fighting of RPM gauge and clock
- Fixed kilju not being hooked by MOP on game load
- Fixed the dashboard gauges Z fighting bug fix not working, if the gauges panel haven't been attached to the car on game load
- Fixed computer memory resetting to default value
- Fixed compatibility with ColorfulGauges3 (requires rule files update)
- Fixed odometer of Gifu, Hayosiko and Kekmet's hour meter resetting to the default values

## 2.11.3 (07.07.2020)

### Added

- Added a message, if player tries to open output log, or last MOP_LOG when there is none

### Changes

- Improved the performance of sectors

### Bug Fixes

- (My Summer Car Bug) Fixed Z fighting of Satsuma dashboard needles
- Fixed a bug in which the Satsuma could potentially be sent into the air, if the player Alt+Tabbed during MOP loading screen
- Fixed bus roll fix not working as intended
- Fixed a bug in which MOP would throw an error while trying to hook the save game action to the phone handle
- Fixing items falling out of the Satsuma's boot 2: The Collider Boogaloo
- Fixed a bug with infinitely burning garbage barrel fire
- Fixed MOP throwing error, if the pedastrian has been knocked out

## 2.11.2 (03.07.2020)

### Added

- Added loading screen

### Bug Fixes

- Probably fixed items falling through Satsuma (the game is such a mess, that I can't be 100% sure)
- Fixed more cases in which Satsuma would "gain on weight" for no reason
- Fixed doors at yard being closed after respawn
- Fixed some parts bolts seeming to be unbolted, even tho internally the part was fully bolted, making it impossible to detach the part (mostly applies for saves that were used pre-2.11 update. If you started your save after 2.11 update, you're good)

## 2.11.1 (29.06.2020)

### Bug Fixes

- Fixed MOP not working, if player hasn't bought the computer

## 2.11 (29.06.2020)

### Added

- Added "Disable empty items" option in the MOP settings, under Other section
  - Objects that are marked as "empty" by the game (like used coolant bottles) are automatically disabled
- Stolen machine slot now despawns
- Pedastrians NPCs now despawn
- Added new console command `mop generate-list [true/false]` - it generates the list of what items are despawned by MOP (useful for mod makers that want to create rule files for their mods)
- If the Custom.txt rule file is present, the content of it will be showed in MOP log and mod report
- Added experimental save file optimization (disabled by default)
  - WARNING: This is a highly experimental function, I recommend making your own backup of entire save folder! **USE ONLY AT YOUR OWN RISK!**
  - Can reduce the save file size, from 10% to 30% (depending on how old is the save file)
  - Should reduce the load time (especially for older saves)
  - You can enable it using flag "experimental_save_optimization" in the custom rule file
  - MOP generates a save (defaultES2File and items) backup on each save with ".mopbackup" suffix (ex. defaultES2File.txt.mopbackup)
  - If something wrong has happened to your save, you can issue command "mop restore-save", that restores last save backup

### Changes

- (My Summer Car) Restocking now works no matter the player's distance to the store (previously, if player was too close to the store, the restocking script wouldn't trigger)
- Optimized how anti bolt resetting script works
- Renamed "Open last log" to "Open last MOP log" in the MOP settings, to avoid confusion
- MOP will not check if the server is online on each menu opening, reducing the main menu load time if you're offline
- Minor performance enhancements
- Minor changes to console outputs

### Bug Fixes

- (My Summer Car Bug) Fixed save files staying in "read only" state, if the game crashed when trying to switch from the game scene to main menu after saving, in result causing saving to not work properly (yes, it was not MOP's bug...)
- (My Summer Car Bug) Fixed negative battery terminal disappearing seemingly for no reason at all
- Fixed MOP sometimes not initializing after starting a new game
- Fixed player's bedroom window wrap resetting to default value
- Fixed Satsuma engine not being re-enabled, if ToggleAll function has been called, potentially causing some parts to not save their bolt states
- Fixes pre game save actions not executing, if the Suski has called the player during the game ending
- Fixed floppy eject button on the computer not working
- Fixed a bug in which player would get the 60 seconds parc ferme penalty during the rally
- Fixed a bug in which Satsuma would get heavier and heavier after each respawn
- Fixed a bug in which there would be a sunlight shining through the floor inside of the home early in the morning (around 2-6 AM)
- Fixed MOP throwing an error, if the rule file couldn't be verified

### Removed

- Rule Files API: Removed obsolete flags
- Removed unused code

## 2.10 (13.06.2020)

### Added

- Added Dynamic Draw Distance
  - The draw distance will be doubled, if player is above certain level - so you can see entirity of the map
  - And it will be lowered, if you're in the building
  - You can enable it in the settings
- Added "Run in Background" toggle in the MOP settings, letting you choose if the game pauses on ALT+TAB
- Added CheatBox warning (it will NEVER be supported, stop messaging me about it, here's an explanation: <https://youtube.com/watch?v=dQw4w9WgXcQ>)
- Added info at the bottom of MOP settings
- If the incorrect flag has been found in rule file, the error line will be displayed

### Changes

- Tweaked Repair Shop sector to be less aggresive
- Improved the dialog upon clicking a link in the settings
- Minor changes in the settings
- Improved fuel tank level saving script
- Multiple rule files API changes
- Rule Files API: ignore_at_place is now obsolete! Please use ignore: [place] [object_name] instead
- Rule Files API: toggle_renderer is now obsolete! Please use "toggle: [object_name] renderer" instead
- Rule Files API: toggle_item is now obsolete! Please use "toggle: [object_name] item" instead
- Rule Files API: toggle_vehicle is now obsolete! Please use "toggle: [object_name] vehicle" instead
- Rule Files API: toggle_vehicle_physics_only is now obsolete! Please use "toggle: [object_name] vehicle_physics" instead
- Rule Files API: prevent_toggle_on_object is now obsolete! Please use ignore: [vehicle_name] [object_name] instead
- Overall improvements in rule files loading
- Code optimization and improvements

### Bug Fixes

- (My Summer Car Bug) Added missing collisions to the car parked at the Fleetari's repair shop
- Fixed saving potentially breaking, if the Safe Mode is on
- Fixed potential bug in PlayerTaxiManager class
- Fixed Safe Mode not initializing properly if the toggling routine failed, and getting stuck in constant "trying to restart" loop
- Fixed toggle ignoring on objects inside of vehicles working only for the first item in the rule file
- Fixed MOP rules loading not working, if the mod has been disabled and the game has been restarted
- Fixed MOP not working, if player stole the video poker machine
- Fixed enabling of all objects on MOP crash
- Fixed Teimo counting the broken windows multiple times, if you broke one, left the shop area and came back
- Fixed Gifu position and waste tank level sometimes not saving properly
- Fixed Satsuma bolts sometimes not saving their values
- Fixed the save game hook not working for the Rent Apartment mod save point

## 2.9.8 (09.06.2020)

Warning: My Summer Car update from June 2020 is now required!

### Added

- By default, Rule files are now verified if they are coming from trusted source
  - Untrusted rule file will be deleted
  - You can disable it in the MOP settings
- You can now make MOP to check rule files auto update on every restart
- Added "mop cat" command - it prints the content of a rule file

### Changes

- MOP now checks for rule files auto update every 2 days by default

### Bug Fixes

- Fixed video poker stealing object bugs
- Fixed steering assist being turned on, despite disabling it in the settings
- Probably fixed bouncy Satsuma bug, if it wasn't fully assembled
- Computer won't be disabled, if it is left on
- Fixed bouncy Jonnez bug
- Fixed missing new line in new custom rule file

## 2.9.7 (04.06.2020)

### Bug Fixes

- Fixed MOP not working, if the experimental_optimization flag was not set

## 2.9.6 (04.06.2020)

### Added

- Some element of Satsuma (such as expensive simulations) will be disabled, if the player is not in the car
- Added new commands:
  - mop delete [ModID]
  - mop open [ModID]

### Changes

- Renamed "Enable Shadow Adjusting" to "Adjust Shadows"
- Lowered the distance after which the haybales are re-enabled
- Overall performance enhancements

### Bug Fixes

- (My Summer Car Bug) Fixed repair shop cash register trigger
- Fixed steering rods resetting their value
- Fixed assemble sound playing on Satsuma respawn
- Fixed Jonnez kickstand resetting to default value on respawn
- Fixed Jonnez making collision noises
- Fixed Satsuma without wheels making collision noises
- Fixed shadow adjusting not saving correctly

### Removed

- Removed "SATSUMA: Toggle Physics Only" form the mod settings

## 2.9.5 (29.05.2020)

### Added

- You can now adjust the shadow distance
- Envelopes and lottery tickets physics is now toggled

### Changes

- Improved how MOP is loading, so it will not break completly, if something goes wrong during initialization
- Improved Rules class loading
- Refactored Places class
- Moved MOP rule files info label in the main menu to center, so it displays correctly on 4:3 displays
- Home chimeny is not toggled anymore, so objects wont fall through it

### Bug Fixes

- Fixed registry plates glowing bug
- Fixed "Open output_log" not working under Linux
- Fixed rule files potentially not working under Linux
- Fixed following items not toggling: macaron box, milk, potato chips, pizza
- Fixed bus randomly ending up on its side

## 2.9.4 (24.05.2020)

### Added

- Pikes are now toggled when removed from the fish trap
- Added spawn hook to fish trap

### Bug Fixes

- Fixed Teimo's cap and glasses disappearing, when he's riding on a bike
- Fixed not being able to save the game, if player didn't finish the Jokke moving job

## 2.9.3 (24.05.2020)

### Added

- MOP sectors are disabled when you're far away from them
- Command 'mop new' now can create mods rule files using 'mop new modID'
- Added 'mop open-folder' command that opens MOP config folder

### Changes

- Sector game objects are now parented to MOP_WorldManager

### Bug Fixes

- (My Summer Car Bug) Fixed Satsuma license plates Z fighting bug
- (My Summer Car Bug) Fixed Z fighting bug of a slot machine in store
- (My Summer Car Bug) Fixed Z fighting of wrist watch dials
- Fixed kilju bottles sometimes not teleporting to junkyard and still being full, after selling them to Jokke
- Car jack and floor jack doesn't despawn, if it's not in it's default position
- Fixed Flatbed being attatched to the Kekmet, even when both were far apart

## 2.9.2 (22.05.2020)

### Bug Fixes

- Fixed tire change job at the repair shop not working properly
- Fixed 'prevent_toggle_on_object' flag

## 2.9.1 (21.05.2020)

Really sorry for everyone that encountered the bug with some elements of yard and other places not loading correctly, but it only happpens to some specific people, at a specific occasion, which I yet have to figure out. 10 other people have been testing MOP 2.9 and nobody reported that issue.

### Bug Fixes

- Fixed places LOD not loading correctly for some people
- Fixed a bug in which Flettari would not greet player, and buying parts not working

## 2.9 (20.05.2020)

### Added

- All items in the game are now toggled
- Offset wheels are now also toggled
- Trunks now add weight to the car
- Added hook to car part boxes
- MSC's default LOD scripts for yard, store, repairshop, inspection and farm are now disabled, to save some processing power

### Changes

- Tweaked how vehicles physics is now toggled, eliminating the bug that would cause sudden FPS drop
- Changed how MSC fixes are applied
- Overall code optimization and improvements

### Bug Fixes

- (My Summer Car Bug) Fixed rear bumper sometimes detaching on game load
- Fixed a bug with a trailer being attached, while the log was still there
- Fixed a bug in which Custom.txt wouldn't be loaded, if no other rule file was present
- Fixed player dropping a helmet after putting it on
- Fixed 'mop reload' not resetting sectors
- Fixed how yard TV is loaded, which would prevent rule files not work with it
- Fixed rear view window grille paint resetting
- Fixed MOP throwing a bug related to uncle's beer case

## 2.8.4 (14.05.2020)

### Bug Fixes

- Fixed MOP not starting, if player didn't buy CD player

## 2.8.3 (14.05.2020)

### Added

- Added a data sending disclaimer, when starting MOP for the first time
- Experimental Trunk: Added support for multiple storages
  - Added a storage in a glovebox
- Added experimental optimization system, that disables Satsuma, if it's parked at the garage
  - You can enable it using custom rule file flag: experimental_optimization
- Added new rule file flag "min_ver", which tells MOP what's the minimum required version of MOP for that rule file to work

### Changes

- Improved hood fix script
- 'mop rules' command won't show empty categories anymore
- Changed max framerate limiter value to 144 FPS
- Partial code refactoring
- Minor changes in the mod settings

### Bug Fixes

- Fixed firewood carrier resetting to the default position
- Fixed compatiblity with CDPlayer Enhanced mod
- Fixed radio stopping playing, uppon Satsuma respawn
- Fixed car jack potentially resetting to the default position

### Removed

- Removed unused code

## 2.8.2 (10.05.2020)

### Bug Fixes

- Fixed MOP not working on the official version of the game
- Fixed kilju bucket getting disabled, resulting in a very long kilju brewing time

## 2.8.1 (09.05.2020)

### Added

- Added support for new car bulb lights
- Added sector at the cabin
- Added trigger under the cottage, which teleports the objects that are under it back up
- Added extra check for when the trailer support log gets stuck under the map
- Spark plugs are now toggled

### Changes

- Mattres at the old mansion aren't disabled anymore
- Tweaked sectors at store and repair shop, so it's not as "in your face" as it used to be
- Items that have fallen under the map will now respawn on the landfill
- Partially restored the old method of keeping position of Satsuma

### Bug Fixes

- Fixed wasp hives not saving their state
- Fixed fish trap not working
- Fixed coffee pan and jerry cans resetting its value
- Experimental trunk system will not disable objects, if they rear seat is not attached
- Fixed wrong slash for you lovely Linux users
- Fixed batteries not charging when leaving the yard
- Addressed an issue of flatbed support log getting stuck under the map
- I'm so done with this bs...
- Fixed spark plugs resetting to the default position

## 2.8 (07.05.2020)

### Added

- Items toggling overhaul
- Added 2 new flags to rule files:
  - ignore_mod_vehicles - works the same as the setting moved from Advanced settings
  - toggle_all_vehicles_physics_only - works the same as the setting moved from Advanced settings
- Junk cars are now toggled
- Mooses are now toggled
- Rally parts salesman is now toggled
- Haybales are now toggled
- Slightly changed the home sector
- Rule file flag "ignore" now affects items - it will now prevent disabling of an item, and only allows disabling of the physics and renderers (basically, the old fashioned way)
- Minor changes in how MOP checks if an item is stored in CarryMore inventory
- Added sector at the cottage
- Added sector at the storage room next to kitchen
- Added sector on the driveway (you can enable it only using custom rule file: add flag "driveway_sector")
- Added check if an item has fallen below the ground. If they did, they're gonna respawn at the yard
- Added a filter to rule files loading, that looks for prohibited items (related to MOP and MSCLoader)
- Added (experimental) Satsuma trunk system! You can enable it using custom rule file flag: experimental_satsuma_trunk
  - Uppon closing the trunk, all items inside of it will be disabled

### Changes

- Moved advanced settings into custom rule files

### Bug Fixes

- Fixed car body paint resetting to default
- Fixed a bug in which batteries would sometimes teleport back to the car, if it was left on charge
- Fixed oil fitlers, batteries, spark plugs and and alterantor belts not saving their values
- Fixed items falling through vehicles
- Fixed seats not being able to be attached, if they weren't attached before to the car
- Fixed MOP spaming output_log on game reload
- Fixed a bug in which MOP would sometimes not load on new game, or save and reload
- Machine hall next to Uncle's home will not appear and disappear when entering the sector
- Fixed area checks and sectors preventing user interaction with certain things
- Minor formatting fix to "mop rules" command output
- Fixed machine hall disappearing when leaving it
- Fixed wasp hives resetting to the on load state
- Fixed a bug, in which the framerate would drop, if player dropped held item

### Removed

- Removed advanced settings section in the mod settings

## 2.7.2 (02.05.2020)

### Bug Fixes

- Fixed alternator tightness level resetting to default after Satsuma respawn
- Fixed flatbed falling into the ground, if the tractor was left under certain angle, and trailer was on the flat surface

## 2.7.1 (29.04.2020)

### Changes

- Trailer is now fully disabled on distance
- Changed how MOP checks if server is online
- Minor changes in the mod settings

### Bug Fixes

- Fixed Ventii bet resetting to the default value
- Fixed a bug which would prevent player from detaching oil filters, batteries, spark plugs and fan belts
- Fixed batteries popping off on load
- Fixed an error message appearing with the GAZ 24 car
- Fixed rule files updating not working under GNU/Linux using Wine

### Removed

- Removed unused code

## 2.7 (27.04.2020)

Welcome to MOP 2.7. After 9 days of in development, hopefully, it will be worth the wait.

### Added

- Satsuma enabling and disabling overhaul
- Added support for user made rule files!
  - Simply create Custom.txt where rule files are located and do your magic!
  - For documentation, visit <https://github.com/Athlon007/MOP/wiki/Rule-Files-Documentation>
- Added new items to toggled elements:
  - Computer
  - Rykipohja
  - Mansion
  - TV
- Added CheckSteam to mod report
- Drunk guy new house is now toggled when not needed
- Expanded sector at home into the sauna and shower areas
- Unity car is now being disabled on game load
- Added notice to Advanced section of the settings
- Added sector in jail
- You can now prevent MOP from removing unused rule files
  - Note: unused rule files will not be loaded!
- Added system version info to mod report
- Added garbage collecting on menu load
- Added a small easter egg ;)

### Changes

- Increased toggling distance when Active Distance is set to 0 from x0.5 to x0.75
- Changes in how Satsuma position saving is operated
- Increased the toggle distance of repair shop from 200 to 300 units
- Server connection error won't bring up the console on load
- Increased server timeout to 20 seconds
- Development: Changelog elements marked as the development change are now marked orange

### Bug Fixes

- Fixed spare wheel falling through the ground
- Fixed driver mass being enabled when it should not be
- Fixed MOP trying to access the playmaker script of the item even tho it didn't exist
- Fixed Jail save not working
- Fixed Gifu shit tank restting to the on load value
- Fixed MOP trying to connect to the rule files server when offline with rule files auto update disabled
- Fixed repair shop getting disabled, when going to the far corner of it
- Fixed uncle's beer case hook not getting hooked, if the uncle is not present yet in the story
- Fixed Satsuma physics not toggling, if the left front wheel is not attached
- Fixed oil filters, batteries, spark plug boxes and alternator belts not getting hooked
- Fixed vehicles physics getting enabled back, if player was in the sector
- Fixed "levitating Satsuma" bug, which may also cause Satsuma ending up on the roof of the garage (yes, this is stupid)

### Removed

- Removed unused code

## 2.6.4 (18.04.2020)

### Added

- Added sector at home
- Added a warning message in case of experimental version of the game being detected

### Bug Fixes

- Fixed haybales resetting to their original position
- Fixed haybales despawning if taken to the yard
- Fixed MOP not activating objects, if the player died
- Fixed cottage ax disappearing when taken out of the cottage

## 2.6.3 (16.04.2020)

### Added

- You can now set the frequency of rule files auto update
- Added rule files settings into mod report

### Changes

- Vehicle physics is now not toggled, if the vehicle is moving (so you can push the car off the ski hill again!)
- Optimized seats script
- Updated mop-rules command output
- Minor changes in how mod is initialized

### Bug Fixes

- Fixed items not being able to be rotated in some (beer) cases
- Fixed MSC bug in which the hood would pop off on game load
- Fixed lag while entering sector

## 2.6.2 (14.04.2020)

### Changes

- Minor changes to how items are toggled
- Changed how toggling method is changed for vehicles, when the toggling mode is changed to only toggle the physics

### Bug Fixes

- Traffic is not toggled while entering pub or repair shop
- Fixed a bug in which the trailer would get stuck in ground, if the Kekmet has been toggled
- Fixed rules not affecting the combine harvester
- Fixed crash on Boat.ToggleBoatPhysics
- Items won't fall under the home pier, when player enters the garage
- Fixed possible memory leak

## 2.6.1 (13.04.2020)

Note: Expect items to fall through vehicles, if you're using some kind of teleportation mod, or NoClip. There's nothing I can do about it.

### Added

- Added check of Experimental versions in mod log
- Wheels that aren't attached to the car are now toggled
- Added some more bugs to fix later

### Changes

- Items are now toggled before vehicles
- Rewritten vehicles toggling to more reliable method
- Rewritten items toggling, in order to prevent items falling through the vehicles
- Updated some error codes
- Error codes now appear before exception info
- Changed how toggling works while player enters sectors, in order to prevent items from clipping through vehicles

### Bug Fixes

- Attempt to fix items clipping through vehicles (again...)
- Attempt to fix Satsuma getting flipped (topless plz fix your shit)
- Fixed safe mode bug
- Fixed My Summer Car bug in which seats would detach themselves
- Fixed MOP crashing if some engine part has been removed from Satsuma
- Fixed a bug which potentially would cause MOP to try to divide by 0
- Fixed boat fuel level resetting after respawn
- Satsuma deformation logic doesn't get reneabled by MOP
- Minor bug fixes

## 2.6 (09.04.2020)

### Added

- Farm is now toggled
- Combine is now toggled
- Added sector at machine hall on player's yard

### Changes

- Updated for MSC Mod Loader Version 1.1.7
- Sectors now work when using noclip mod
- Changed error codes
- Street lights won't be toggled when entering the store
- Moved the MOP message dialogue to upper right corner in main menu in order to not overlap the ModLoader messages
- Changed how Active Distance slider is displayed
- Minor internal changes
- Removed unused code

### Bug Fixes

- Fixed potential error, in which MOP would not load if the rule files class haven't been loaded
- Fixed house renderers disappearing, if the player entered the garage and the active distance was set to 0
- GIFU: Fixed shit tank resetting to empty, if the truck was respawned
- Items should not fall through the cars anymore
- SATSUMA: Fixed Fleetari not painting the car body
- House in front of Fleetari won't be disabled if the player enters the repair shop
- Bar fighter won't disappear when entering the bar
- Fixed Satsuma flipping over on certain occasions (needs testing)
- Fixed Safe Mode not kicking in correctly
- Fixed Farm and Combine job darts appearing on the yard map when they shouldn't
- Fixed PathsLayer turning on when it should not be available for the player
- Fixed Satsuma physics toggling right when car gets activated, alllowing items inside of it potentially flip the car
- Fixed macaron boxes not being hooked
- Removed empty shopping bags spawning at the backyard bug
- Fixed shopping bags that have been loaded on game save not being able to hook new items
- Fixed items from opened shopping bag teleporting back to cash register at store
- Fixed game crashing when player is driving the green Fittan or EDM to his house

## 2.5.2 (03.04.2020)

### Bug Fixes

- Fixed MOP not working if the Satsuma isn't built

## 2.5.1 (02.04.2020)

### Added

- Added "mop-rules" command
- Detailed lake is now also toggled while entering sector

### Changes

- Rule files downloading optimization
- From now on, MOP will only update rule files when the newer one is available on the server, instead of re-downloading all of them
- Optimized the script which is triggered when driving the Satsuma

## 2.5 (30.03.2020)

Note: First game start may take a little longer (depending on how many mods you have), because MOP has to download rule files.

### Added

- Added rule files system!
  - Added new section in settings: Rule Files
  - From now on, all mod compatibility is done via the text files with .mopconfig format, dwonloaded from the remote server
  - No need to update MOP in order to add mod compatibility! (At least in most cases)
  - Mod compatibility rules can be updated if needed! MOP checks for rules update every week (you can force update check in the settings)
  - You can disable rules updating in the MOP settings
- Satsuma renderers are now disabled, if not needed
- Satsuma engine renderers are now disabled, if player is in the car, and hood is attached
- Added disclaimer for pirated copies of the game
- Added "Open output log" button into settings
- Added more items to toggled objects
- Added extra sector at store

### Changes

- MOP now loads in the main menu
- Sectors are now enabled for everyone
- Improved how sectors work
- Some console messages are now colored
- Moved 'Open Last Log' and 'Generate Mod Report' to new category - Logging
- All game objects created by MOP will now have "MOP_" suffix in their name
- Changed some console messages to be more self explanatory
- Improved the readibility of changelog
- Code optimization and improvements

### Bug Fixes

- Boat will not disappear/teleport back to the spawn position on respawn
- Fixed doors at home seeming to be open, while in fact they were closed
- Increased toggling distance of water facility, so it doesn't clip on and off when player is at the junction next to school
- Fixed sector at Teimo being placed incorrectly, allowing player to experience "out of bounds" state while being outside
- Fixed possible index out of range bug while toggling items
- Fixed lake simple tile not being found by sector manager
- Fixed sectors not toggling off, if player is using noclip
- Fixed a bug in which Satsuma would remain disabled, if the player went and ordered a repairshop job without using Satsuma
- Fixed a bug in which Satsuma toggling script would be called every time, wasting resources

### Removed

- Removed swamp from toggled world objects

## 2.4.4 (19.03.2020)

This update is only for players who are playing on stable release. If you use Experimental, you don't have to update!

### Bug Fixes

- Fixed MOP not working on update 23.12.2019

## 2.4.3 (19.03.2020)

### Bug Fixes

- Fixed ItemHook being applied twice causing MOP to break and run slower
- Rewritten Item toggling to more reliable script

## 2.4.2 (18.03.2020)

### Bug Fixes

- Fixed MOP not working for some users

## 2.4.1 (16.03.2020)

### Added

- Added error codes to crash logs

### Bug Fixes

- Fixed some parts in Satsuma untighting after being installed and Satsuma was reloaded
- Fixed MOP not working if Enable Sectors was disabled
- Fixed error spam when some error happens in loop

## 2.4 (16.03.2020)

### Added

- Inspection is now toggled
- Added sectors
  - Uppon entering one, some elements will be disabled (such as some trees)
  - Also the toggling distance will be reduced to 30 unites
  - Disabled by default! You can enable it in the settings (Enable Sectors)
- Added dirt and highway bridges to toggled items
- Added compatibility with Supercharger and ECU mods

### Changes

- Moved "Ignore Mod Vehicles" to Others
- Renamed "Go to MOP wiki" to "MOP Wiki"
- Code optimization and improvements

### Bug Fixes

- Fixed "Destroy Empty Beer Bottles" not working with Uncle's beer case
- Fixed objects left at the inspection building sinking into the ground
- Fixed poker machine disappearing when towing it far from store

### Removed

- Removed Toggle Vehicle and Toggle Items

## 2.3.4 (08.03.2020)

### Changes

- Updated for MSC Mod Loader Version 1.1.6

### Bug Fixes

- Fixed a bug in which player couldn't use brochure at Fleetari repair shop

## 2.3.3 (07.03.2020)

### Added

- Added support for Tangerine Pickup
- Added extra check for when player ordered a thing in Repair Shop

### Bug Fixes

- Removed Inspection from toggling, in order to fix bugx related to the new periodic inspection system

## 2.3.2 (04.03.2020)

### Bug Fixes

- Fixed flatbed resetting to full, after player sold the logs and flatbed got unloaded
- Fixed ordered paintjobs resetting to default white, if they were ordered and player left the repair shop

## 2.3.1 (02.03.2020)

### Changes

- Improved how MOP checks if the rope to the vehicle has been hooked
- Changed how the on ground check is checked for Jonnez

### Bug Fixes

- Fixed Gifu being frozen when the Active Distance is set to 0
- Fixed MOP not starting if one of the diskettes from Jokke's apartment has been left in the computer

## 2.3 (29.02.2020)

### Added

- Added framerate limiter
- Vehicle physics will not be turned off, if the car is not grounded
- Added support for Fishing Mod

### Changes

- Increased Perajarvi toggling distance to 300 units
- Minor changes in the settings

### Bug Fixes

- (Mod) Ruscko Restoration Project: Fixed Ruscko key being visible on Fleetari's desk when it shouldn't be there

### Removed

- Removed "Temporarily Disable Physics Toggling" button

## 2.2.2 (28.02.2020)

### Changes

- 'Destroy Empty Beer Bottles' will not be activated, if Bottle Recycling mod is present
- Code optimization

### Bug Fixes

- Empty beer cases will now not fall through the vehicle
- Empty beer bottles will now not fall through the ground after being used
- Fixed Bottle Recycling mod not being detected by Compatibility Manager

## 2.2.1 (22.02.2020)

### Added

- Added support for BottleRecycling mod

## 2.2 (20.02.2020)

### Added

- Added compatiblity with the newest experimental update (20.02.2020)

### Bug Fixes

- Fixed new video poker machine being glitched out

### Removed

- Removed Occlusion Culling due to it being too unreliable and confusing for new users

## 2.1.5 (16.02.2020)

### Bug Fixes

- Fixed floppies at Jokke disappearing from disappearing
- Jokke's furnitures shouldn't clip through his house floor
- Fixed a bug in which the Hayosiko would despawn if the player didn't had keys for it
- Fixed vehicle doors getting stuck open after respawn

### Removed

- Removed unused code

## 2.1.4 (10.02.2020)

### Added

- Added support for KekmetAddons mod

### Bug Fixes

- Items left on chimney in the cottage will not fall through it anymore

## 2.1.3 (09.02.2020)

### Bug Fixes

- Fixed order items (ex. ratchet set) from disappearing when player bought them and left the shop
- Satsuma parts at Repair Shop will not appear when they shouldn't
- Sausage with fries in the microwave at Teimo's will not appear when it shouldn't
- Fixed Teimo's advertisement pile disappearing when taking it away from shop
- Fixed the wobbly bar fighter bug

### Removed

- Removed unused code

## 2.1.2 (06.02.2020)

### Changes

- Increased default strawberry field toggle distance

### Bug Fixes

- Strawberry field mailboxes will not be toggled anymore
- Fixed houses at strawberry field not appearing

## 2.1.1 (03.02.2020)

### Added

- Added support for CD Player Enhanced Mod
- CD Player Enhanced Mod: added check for when the CDs have been bought

### Changes

- Improved compatibility with CarryMore mod

### Bug Fixes

- CarryMore Mod: objects should not clip and teleport when taking out of the backpack

## 2.1 (31.01.2020)

### Added

- Added support for HayosikoColorfulGauges mod
- Added option to allow MOP to toggle vehicles physics only for vehicles
- Added option to ignore mod vehicles

### Changes

- Changed how time is displayed in crash logs and mod report to ISO 8601
- Code optimization and improvements

## 2.0 (24.01.2020)

Welcome to the first official stable release of MOP!

What's new compared to Beta 1.6:

### Added

- Added "Go to MOP wiki"
- Added a dialog window when opening an external link
- SATSUMA: Toggle Physics Only is now displayed in mod report
- Temporarily Disable Physics Toggling is now displayed in mod report
- Added "Advanced" settings
- Added support for Moonshine Still mod (thanks to Hawk031)

### Changes

- Changed the layout of mod report
- Minor changes related to occlusion culling
- Moved vehicles and shop items toggling to Advanced settings
- Compatibility Manager rewrite
- Cash Register script optimization
- Renamed "(Beta) Occlusion Culling" to "Occlusion Culling - Experimental"

### Bug Fixes

- Fixed bugs related to wristwatch guy the in pub
- Fixed possible future issues with MSCLoader

## Release Candidate 2 (21.01.2020)

### Changes

- Cash Register script optimization

### Bug Fixes

- Fixed bugs related to wristwatch guy in the pub

## Release Candidate 1 (20.01.2020)

This is the last Beta release and first Release Candidate!

Here's what's new compared to the last Beta:

### Added

- Added "Go to MOP wiki"
- Added a dialog window when opening an external link
- SATSUMA: Toggle Physics Only is now displayed in mod report
- Temporarily Disable Physics Toggling is now displayed in mod report
- Added "Advanced" settings
- Added support for Moonshine Still mod (thanks to Hawk031)

### Changes

- Changed the layout of mod report
- Minor changes related to occlusion culling
- Moved vehicles and shop items toggling to Advanced settings
- Compatibility Manager rewrite

## Beta 1.6 (13.01.2020)

### Added

- Added "Temporarily Disable Physics Toggling" button in the settings. If your vehicle gets stuck in the air, you can temporarily disable physics toggling. Physics toggling will be enabled back next time you restart the game
- Added the date of report generation to mod report
- Strawberry field now is disabled when player is far away from it
- Piers and railroad tunnels are also disabled

### Changes

- Changed the initialization delay to 2 seconds
- Code optimization and enchancements

### Bug Fixes

- Fixed battery charger cables connecting themselves, after leaving and getting back to home
- Changed how GT grille is checked on initial loading. GT grille should not disappear anymore (at least, I hope so)
- Fixed house toggling sometimes breaking and causing MOP to not turn on the yard back on
- Fixed Satsuma physics not toggling back on, after leaving it on the Fleetari's lift and going back to it after save
- Fixed lift at Fleetari resetting back after respawn

## Beta 1.5 (10.01.2020)

### Added

- A 1 second delay before mod initialization to mod loading, in order to fix bugs related to GT items and CD radio
- Added "SATSUMA: Toggle Physics Only" in the settings. Enabling it will prevent MOP from toggling car's body elements. Use it only if you experience issues with Satsuma
- Vehicle physics toggling now respect the Active Distance setting
- New mods supported:
  - Offroad Hayosiko mod
  - Jet Sky mod

### Bug Fixes

- Fixed towed cars not moving when trying to tow them
- Fixed in some cases disappearing RPM gauge and GT grille
- Fixed garage doors getting stuck (because yes)
- Garage doors will now work properly
- Fixed CD radio switches disappearing
- Fixed "remove empty beer bottles" setting not working properly

## Beta 1.4.1 (08.01.2020)

### Bug Fixes

- Fixed how Hayosiko despawning is managed. From now on, Uncle Stage has to be set to 5, in order for van to fully despawn. Else, only car's physics will get toggled
- Fixed Gifu diff lock, pump and rear axle being turned on after Gifu respawn
- Fixed flatbed firewood level being reset after it was filled and then the flatbed got respawned

## Beta 1.4 (06.01.2020)

### Added

- Empty beer bottles can now be removed automatically, right after you drink the beer. Wouldn't that be neat IRL? (You can enable it in the mod settings)
- Added support for GAZ 24 Volga mod, so you can now feel like KGB agent
- Added support for Police Ferndale mod, so you can now feel like police officer
- Added support for VHS Player mod, so you can watch movies about KGB agents and police officers

### Changes

- Improved save game hook, so now now it hooks into jail savegame, even when you're not in jail (which may eventually be a thing)
- Minor changes in the settings

### Bug Fixes

- Excluded Fury from Drivable Fury mod from occlusion culling
- Fixed two shy houses that would disappear when player tried to approach them. We gave them some confindence boost, so they won't disappear :)
- Fishes should now fish properly
- Occlusion shouldn't crash anymore

### Removed

- Removed legacy occlusion method, because it was sooo last season
- Does anyone read these? Hello?

## Beta 1.3 (01.01.2020)

Happy New Year and welcome in 2020!

### Added

- Added check for when the Occlusion Distance is lower than Minimum Occlusion Distance
- Added save game hook to jail save spot
- Jokke's furnitures are now toggled on and off
- Added spirit to the list of toggled items

### Changes

- Changed occlusion culling info
- Renamed "Occlusion Distance" to "Maximum Occlusion Distance"
- Improved emergency safe mode scrip
- Changes in settings order
- Improvements related to plastic cans/juice cointaners/kilju
- Unified the console messages
- Minor changes under the hood
  - Unified some variable names
  - Code optimization and improvements
  - Code refactoring

### Bug Fixes

- Fixed a typo in the settings
- Excluded Hayoshiko from the occlusion
- Fixed camera and fireworks bag from clipping through the cottage
- Fixed an issue with Hayosiko staying between tree walls even after it should be at the uncle's

### Removed

- Removed Occlusion Sample Delay from the settings

## Beta 1.2.1 (22.12.2019)

### Added

- Added error tracing to PreSaveGame function
- Added some Christmas Spirit

### Changes

- Fundamental changes in how store items are getting disabled and enabled
  - Items shouldn't now change their position, or teleport to each others after respawn
  - Items should now be saved correctly
  - Note: because of that, there FPS boost with the mod may be a bit smaller than it used to be

### Bug Fixes

- Fixed a bug in which there would spawn random money at Fleetari's

### Removed

- Removed unused code

## Beta 1.2 (19.12.2019)

### Added

- Added full support for Drivable Fury!
- Added support for Second Ferndale!
- Both vehicles get toggled - just vanilla vehicles!
- Added MSC Mod Loader info to "Generate mod info"
- Mod will check if the MSC Mod Loader version is newer than 1.1.5
- Added boat to the list of hidden objects
- Occlusion Hide Delay will now be automagically calculated
- Added car jack and warning triangle to list of toggled items

### Changes

- Simplified Occlusion Sample Detail settings - now it consists of 5 checkboxes
- Occlusion Culling leaves the experimental stage and now is in Beta!

### Bug Fixes

- Fixed some lights getting switched back on after house reload
- Fixed legacy culling throwing an error on game save
- Fixed an error related to uncle's beer case
- Fixed PreSaveHook not being triggered in some save points
- Fixed tires landing under the repair shop, when they were left for change
- Fixed Satsuma possibly clipping through the floor at repair shop
- Fixed tree collisions not working
- Fixed an issues related to saving the game

### Removed

- Removed electricity poles from the list of removed objects
- Removed unused code
- Removed Herobrine

## Beta 1.1.3 (17.12.2019)

### Bug Fixes

- Fix related to beta 1.1.2

## Beta 1.1.2 (17.12.2019)

### Bug Fixes

- Fixed beer cases falling through the ground

## Beta 1.1.1 (16.12.2019)

### Added

- MOP is now available on RaceDepartment!
- Added "Very Far" active distance (4x the Normal value)

### Changes

- Removed landfill from the list of disabled objects, in order to fix issue with empty plastic containers not respawning on the landfill
- General improvements in regard of shop items

### Bug Fixes

- The mod will not disable the Satsuma physics, when the car is in the inspection area
- Properly implemented SecondPassOnLoad (thanks piotrulos!)
- Fixed Teimo holding beer and sausages with fries in left palm after respawn
- Fixed second Teimo appearing on the bike, after store respawn
- Empty plastic containers now respawn at the landfill
- Fixed cottage items (like coffee pot) landing under the cottage, making them unnaccessible (you may need to use noclip if you used the earlier versions of the mod to get them back. Sorry!)
- Fixed some cottage items disappearing after being moved from the cottage and saving the game
- Fixed rally not working properly
- Shopping bags should despawn after being used
- Sauna items should not fall through the sauna benches anymore
- Items left on washing machine should not fall through it
- Items left on terrace should not fall through it
- Fixed a bug in which the player would suddenly stand up while driving cars and approaching the Satsuma
- Fixed an issue with car inspected status not being saved after the save, if the player just passed the inspection

### Removed

- Removed Rally from list of disabled objects, because it broke it, and the performance improvement wasn't that significant
- Removed Jokke (Kilju Guy) from the list of disabled objects, for the same reason as the rally issues

## Beta 1.1 (15.12.2019)

### Added

- The mod now requires MSC Mod Loader 1.1.5
- Two new occlusion methods
  - Chequered - the script instead of checking screen areas from 0 to max lineary (ex. area 0-1-2-3-4...), it will check the screen areas from 0 to max by first running even areas (0-2-4-6-8...), and then odd (1-3-5-7-9), reducing chance of objects disappearing when it is on screen
  - Double - runs two instances of the script, it should be the most accurate, but is slowest. One instance checks even areas, the other checks the odd areas
- Occlusion loop should restart automatically now, if it stops (temporary fix for the loop not working when opening the pause menu)
- Added error output. The errors will be saved into MOP_LOG.txt in MSC main folder.
- Added "Open last log" into the mod settings
- You can disable toggling vehicles and shop items
- Added "Safe Mode", which will only allow mod to toggle only objects that are known to not cause any issues
- Added "Generate mod report", which is used to dump the info about the mod

### Changes

- Mod file has been renamed from MOP.dll to zzzMOP.dll, to make it load as last one. PLEASE REMOVE THE OLD VERSION OF MOP.DLL FIRST!!!
- Mod will now be loaded as the last one. (Requires MSC Mod Loader 1.1.5)

### Bug Fixes

- Temporarily disabled occlusion from van, due to it disappearing when it shouldn't
- Fixed some errors for occlusion list loading
- Removed dragstrip from the occlusion list
- Fixed an issue with kilju bottles disappearing when using CarryMore mod
- Fixed (probably) the issue with Fury and Second Ferndale mods, because now the mod should load as the last
- Fixed Teimo not looking or talking to the player

## Beta 1.0.1 (12.12.2019)

### Bug Fixes

- Fixed a typo in the mod name
- Fixed incorrect format of manifest file

## Beta 1.0.0 (11.12.2019)

- Initial release
