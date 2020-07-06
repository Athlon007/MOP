# Ye Olde MOP FAQ

## How can I improve the framerate even more

- Set the Active Distance to Close
- Enable 'Destroy Empty Beer Bottles' option in the MOP settings
- Enable 'Disable empty items' in the MOP settings
- Removed mods that you don't really use
- Try using DirectX11, by adding the "-force-d3d11" to launch options, or OpenGL: "-force-opengl" (may introduce graphical bugs)
- Try to limit the number of items in your house
- Try lowering the resolution
- Disable some options in the game:
  - Antialasing
  - Bloom
  - Reflections
  - Sunshafts
  - Sun Shadows
  - House Light Shadows
  - AI Car Dust Trails
  - Car Mirrors
  - Show FPS (MSC's built in FPS counter decreases the framerate by up to 10 FPS, depending on the machine. I recommend using Steam built in FPS meter, the Windows 10 built in (under Win+G), or any other external software). It is also innacurate, never going below 20 FPS
- Move the vehicles or items that you don't use out of the places where you experience lag the most
- Lower the shadow rendering distance in the MOP settings
- Limit the game's framerate to fixed value

## What does it disable

- All vehicles in the game
- Many static elements of the world (such as buildings, static objects, AI traffic, NPCs, etc.)
- Items (such as sausages, beer cases, wheels, etc.)

All of them are either turned off by player approaching those items, leaving house area, or entering sectors.

Besides that, MOP fixes some more or less known bugs of vanilla My Summer Car. Next section explains what are those.

## What vanilla MSC bugs does MOP fix

- Seats that sometimes happen to detach themselves while trying to mount them into the car (2.6.1)
- Car hood and fiberglass hood detaching themselves on the game load (2.6.3)
- Improves the stock hood closing mechanic (2.8.3)
- Rear bumper detaching itself on the game load (2.9)
- Multiple Z-fighting bugs:
  - Satsuma license plates Z fighting with the bumper and bootlid (2.9.3)
  - Slot machine glass element inside of the store (2.9.3)
  - Wrist watch dials Z fighting with the watchface and themselves (2.9.3)
- Annoyingly small hitbox for the repair shop cash register (2.9.6)
- Missing collisions to the car parked at the Fleetari's repair shop (2.10)
- Negative battery terminal disappearing seemingly for no reason at all (2.11)
- Save files staying in the "read only" state (2.11)
- Satsuma dashboard needles Z-fighting (2.11.3)

## What are sectors and where are they located

Sectors tell MOP to disable some extra elements, such as trees, foliage, buildings, AI traffic, NPCs and other stuff. These are located at:

- Yard (one at the garage, two inside of home)
- Repair shop
- Store (one in the main section, one at the back)
- Jail
- Wood chopping machine hall
- Cottage
- Pigman's cabin

MOP is designed in such a way to not interfeer with the gameplay itself. You won't find MOP disabling the AI traffic when it should be on for instance.

Notice: some mods may include their own sectors in their respective rule files.

## My framerate is high, but it often jumps down which I don't like

You can enable the framerate limiter in the MOP settings under the Graphics section, or simply use V-Sync.

## When I walk close to the Satsuma, my framerate suddenly drops

Satsuma is the biggest resource hog of the entire game. There's literally nothing I can do about it.

The only reason why game doesn't lag so much if you get a bit further from the Satsuma, is because MOP uses some cleaver techniques, such as disabling parts of the car when they don't need to be enabled, but everything Satsuma related has to be ON, if you want to drive it.

## My game runs much slower when I'm at home

The more things you keep at home, the game **will run slower**, because every single individual item has it's own PlayMaker script, rigidbody and collider running on each frame - each of this element has to be calculated by the game engine, wasting system resources.

You can combat this by moving vehicles or items that you don't use out of the yard!

## There's a screen saying "Loading Modern Optimization Plugin" displayed for the few seconds and my input is frozen

This is an intended behaviour - it has been added so the player won't break his save by trying to pick up items and potentially breaking his game.

## MOP doesn't seem to work

Make sure you have the newest version of MSC Mod Loader and MOP installed. Additionally, check if you haven't disabled some options, and that Safe Mode is toggled off.

## Does it work with [INSERT MOD NAME HERE]

If it's listed on the Mod compatibility list - yes.

If it doesn't - perhaps, I don't know. You need to check it yourself. In most cases that mod will work. There are hundreds of mods for My Summer Car, many of which are very poorly coded.

## There's an issue with [INSERT MOD NAME HERE]

I cannot guarantee that all mods will work with MOP. But you can report what's the issue and I may look into it. Many mods do work, and some (listed on Compatibility list) received extra attention.

## Why MOP is not compatible with CheatBox

MOP cannot be compatible with CheatBox for one simple reason - in order to CheatBox teleporting to work, it looks for object using GameObject.Find(), unfortunately it doesn't work, if the object is disabled - a thing that MOP does by its design.

There is only 1 scenario where MOP and CheatBox would work together, with none of them loosing their core functionality:

- CheatBox author modifying the CheatBox itself, so instead of it using `GameObject.Find(objectName)`, it would use `Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == objectName)`

If you want to retain the item teleporting function, you can enable Safe Mode in the MOP settings (which will harm the performance), or you can create the Custom.txt rule file, with flags "ignore: objectName", for the items that you want to be able to teleport.

## How do I install rule files from the zip file

First navigate to the folder where your MSC Mods are. Then go to:

Config -> Mod Settings -> MOP

And unpack the content of the zip file here.

## After deleting the mod, a rule file corresponding to that mod is gone

This is an intended behaviour of MOP which goal is to reduce the load time and disk space usage.

If you wish for the MOP to not do that, you can disable it by going int Mod Settings -> MOP Settings -> Rule Files -> Check "Don't delete unused rule files".

**Note:** These rule files won't be loaded nontheless.

## Rule file that I made for my mod has been deleted by MOP

MOP by default tries to verify if the installed rule files are trusted, to avoid having any malicous ones potentially breaking the game or MOP itself.

If you're mod maker, you can disable that function by going into Mod Settings -> MOP Settings -> Rule Files -> Uncheck "Verify Rule Files".

## Something broke (ex. item fell through the ground) after I teleported it or self

I'm sorry, but I don't intend on fixing bugs involving cheat mods.

## I found an issue/MOP threw an error

Remember to always include output_log.txt and MOP_LOG.txt - you can paste both into Pastebin. If the file is too large, you may want to upload it to cloud, like Google Drive, OneDrive or Dropbox.

Also please describe how to reproduce the bug. Some of those bugs are really tricky to trigger!

If you can, please attach your save file too.

"MOP not work plz fix" won't help me at all - if anything, your report will be ignored!

## My game stopped working/computer blew up/dog died/I started thermonuclear war because of MOP

I'm sorry to hear that! But I'm not responsible for any damage done to your computer, save game, or anything related.

MOP is released as a free software with the hope of it being useful to others. I don't provide any guarantee that it will not cause issues, nor I can't guarantee that MOP will improve the performance in your case.

## My problem is not listed here, or nothing worked

Then you can contact me about it over GitHub, NexusMods or RaceDepartment.
