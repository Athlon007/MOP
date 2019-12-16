# Changelog

## Beta 1.1.1 (16.12.2019)

### Added

- Added "Very Far" active distance (4x the Normal value)

### Changes

- Removed landfill from the list of disabled objects, in order to fix issue with empty plastic containers not respwaning on the landfill
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
- Fxied an issue with car inspected status not being saved after the save, if the player just passed the inspection

### Removed

- Removed Rally from list of disabled objects, because it broke it, and the performance improvement wasn't that significant
- Removed Jokke (Kilju Guy) from the list of disabled objects, for the same reason as the rally issues

## Beta 1.1 (15.12.2019)

### WARNING: The mop is now named zzzMOP.dll. Please remove the old MOP.dll first

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
