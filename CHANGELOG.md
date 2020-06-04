# Changelog

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
  - For documentation, visit https://github.com/Athlon007/MOP/wiki/Rule-Files-Documentation
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
