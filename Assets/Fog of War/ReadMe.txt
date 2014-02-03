--------------------------------------------------
             Tasharen Fog of War
 Copyright © 2012-2014 Tasharen Entertainment
                Version 1.1
http://www.tasharen.com/forum/index.php?topic=1291.0
--------------------------------------------------

Thank you for buying Tasharen Fog of War!

----------------------------------------------
 Overview and Usage
----------------------------------------------

Fog of War requires 3 things in order to work:
1. Fog of War system (FOWSystem) that will create a height map of your scene and perform all the updates.
2. Fog of War Revealer (FOWRevealer) on one or more game objects in the world.
3. Either a FOWImageEffect on your camera, or have your game objects use FOW-sampling shaders such as "Fog of War/Diffuse".

----------------------------------------------
 Package Structure
----------------------------------------------

Fog of War
|
+-FoW		-- Contains the actual Fog of War system in its entirety.
|
+-Examples	-- Contains example scenes, assets, scripts and shaders. Safe to delete.

The Examples folder contains two scenes:
- FOW 1 - Image Effect    -- Easiest way to add FoW by using a post-process effect, but requires Unity Pro.
- FOW 2 - Custom Shaders  -- More advanced way of doing FoW by using shaders on objects that sample the fog's textures.
                             This approach will be faster on hardware with limited fillrate and will work on Unity Free.

----------------------------------------------

If you have any questions, suggestions, comments or feature requests, please
drop by the forums, found here: http://www.tasharen.com/forum/index.php?topic=1291.0
