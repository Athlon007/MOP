# Changelog

## Release Candidate 2 (xx.01.2020)

### Changes

- Cash Register script optimization

### Bug Fixes

- Fixed bugs related to wristwatch guy in pub

## Releace Candidate 1 (20.01.2020)

Welcome to first stable release of MOP!

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
