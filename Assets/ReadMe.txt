
****************************************
Overview
****************************************

We have all seen game with beutiful rolling backgrounds, now you can have them too!

Making a racing game? Then no problem with Mega-Shapes. Draw a path for the road, draw a cross section and then loft a road. Want more detail? Then Loft a barrier and it will automatically conform to the road surface. Slide the barriers in or out, scale them, twist them, you imagination is the only limitation.

Want more? Then use the powerful rail clone feature to repeat and position sets of objects along your road, fences, powerlines, trees whatever you like.

Still not enough? For those that want even more Mega-Shapes gives you a scatter system that will take any objects you choose and scatter them along your road and have them auotmatically conform to the existing meshes surface.

With Mega-Shapes you can create beutiful levels quickly and easily.


MegaShapes is a system that builds meshed from splines, this can be anything from simple lines, ribbons using shapes as the paths to complex lofts of multiple spline cross sections along a spline path. Combined with this is an advanced layer system for adding deltails to the lofts, with layers that allow cloning of objects, as well as scattering objects over a loft to a full rule based system for building detailed meshes along a path or mapped to a loft surface.

All aspects of the system can be edited in realtime with layers conforming automatically to any changes in the base shapes or surfaces allowing for very easy editing and updating.

MegaShapes current features:
Full spline bezier based spline system.
Turn shapes into meshes with uv mapping and extrusion options
Controllers for moving objects along paths
3ds max exporter to get paths and animated paths easily into the system (soon to be extended to other editors as well as support for vector based files)
Simple loft system to extrude any shape along another to build roads, tubes, surfaces etc
Advanced loft system to allow any number of cross sections to be used to build a loft surface
Loft layers to add further detail to surfaces, layers currently included (more to come):
	Clone - Take any mesh and duplicate it along a surface with options for start and end meshes
	Clone Spline - Clone a mesh along a shape again with start end mesh options
	Scatter - randomly scatter meshes onto a loft surface
	Scatter Spline - randomly scatter meshes along a spline
	Clone Rules - Adanced rule based mesh builder, current rules are:
		Start	- Object to be used at the start
		End		- Object to be used at the end
		Filler	- Main filler object
		Regular	- Object to appear every Nth object
		Placed	- Object to appear at specific location
		
		You can have any number of rules and where more than one of the same type is found a weighting value is used to choose which to use.
	Clone Spline - Like above but along a spline.

Features to come in updates:
Fast Rope and chain system
Scatter objects inside splines
Animated breaking Wave simulator
Animated Tank Tracks system
Move advanced controllers for moving objects along splines
Hose system for connecting objects with rubber type hoses
Move loft layers and enhancements to the current system
Move rules for the rule based layers

All code is copyright Chris West 2012 and any use outside of the Unity engine is not permitted.

****************************************
How to use:
****************************************
More information on the system can be found on the website at http://www.west-racing.com/mf

****************************************
Changes:
****************************************
v1.31
Fixed bug in Helix shape with one of the knot handles being wrong.
Fixed exceptions in the alpha rope code
Added custom inspectors for the rope system.
Added a Verlet solver to the rope system.
Added first test of self collide to rope system.
Added start of a new scene for rope testing.
Added a new rope meshing option, deform mesh to rope spline.

v1.30
Added a Create MegaShape Prefab option to the GameObject menu so you can easily create prefabs out of lofts or shape meshes.
Removed some debug logging messages when loading SVG and SXL files.
Rope and chain code added in an alpha state ie not fully tested or with all features present

v1.28
Support added for SXL file import, sxl is a xml based spline format allowing users to easily write their own spline exporters.
Bezier curve Exporter for Maya available

v1.27
Up value used correctly for complex loft layers.
Editing of splines works correctly for scaled and rotated splines
SVG import now uses Axis value on import for orientation.

v1.26
Deleting a layer will now rebuild mesh if realtime is off.
Fixed strange results when typing in values for Offset values on Clone Layer.

v1.25
Added Ribbon option to mesh types when converting Spline to a mesh.
Fixed MeshRenderer being added to object when it wasn't required.
Added Late Update option to shapes.
Pivot offset value now works for tube, box and ribbon meshing options.
Added UV offset param to tube, box and ribbon meshing options.

v1.24
Walk Loft now has an alpha mode and a distance mode for positioning object.
Shape Follow now has an alpha mode and a distance mode for positioning object.
Added Late Update option to Walk Loft
Added Late Update option to Shape Follow
Added Help page for Shape Follow
Added Help page for Walk Loft
When loading SPL or SVG files you now have the option to Replace existing splines or add to the splines making the shape.
Add example script MegaMovePlayer.cs to show how to control character moving on a spline.

v1.23
Added Elipse support to SVG importer.
Fixed splines not showing up when selected in Unity 4.0
Added a Reverse spline option to the Shapes inspector, will reverse the currently selected spline
Added an Apply Scaling button to the inspector so if you scale the shape using the transform click this to correctly scale the splines
Added an Outline spline system, you can now ask the system to make a new spline that outlines the current one with control over the outline distance.

v1.22
SVG importer rewritten and greatly improved.
CursorPos on shapes now works correctly for the selected curve.

v1.21
Fixed an exception error if Complex Layer missing a path.
Complex Loft layer now handles non closed splines properly for our of range values ie alpha < 0 or > 1

v1.19
Fixed the flip not working on the Complex Loft layer.
Added error checking code to Complex Loft so check for valid state before trying to draw. NB Complex loft needs at least 2 cross sections before mesh builds.
Added start of SVG import support.
Optimized some of the core maths for faster updating of loft meshes, more to come.
Added a rotate value to the walk loft script.
Fixed bug in Duplicate Layer code, now works for all layer types.
Added an offset and modifier param to each target in Shape Follow.
Jump and tree assets added for turoial scenes

v1.18
Made some changes to how the twist works on tube and box meshes.
Fixed bug on Duplicate Layer which caused an exception.
Update help page for Loft Object component.
Changed how the system deals with transforms, now allows objects to be moved, scaled and rotated and layers will follow correctly (if realtime set), you may need to adjust rotations on clone layers and offsets depending on base object rotations etc. Please backup before updating.
When copying a Loft Object layers will now use the newly created Loft Surface instead of the original one if the Loft Object is the one being copied.

v1.17
Walk Loft works correctly with transformed loft objects.
Added limits for the 'Dist' and 'CDist' params to the Loft Object inspector.

v1.16
Added Box type to meshing options for splines.
Added a Vertical offset value to the Walk Loft helper script.
Fixed inpsector gui bug
Add Mega Shape Follow to allow gameobjects to follow a shape or multiple shapes.

v1.15
Added 'Duplicate' button to layer inspector to easily make a copy of a layer on a loft object.
Added 'Delete' button to layer inspector to easily remove a layer.
Added a smooth value to Walk Loft helper script.
Added 'tangent' value to Loft Object to control the accuracy of the forward calculations in the lofter.
Added new meshing option to splines, 'Tube' allows multistranded tubes to be made.

v1.14
Added ability to copy loft objects easily

v1.13
Added autosmooth option to shapes.
Added BuildSpline method.
Added various methods to aid in spline building via script.
Added option for different handle types on Shapes as some people reported big slowdowns in 3.5.3

v1.12
Adjusted sensitivity of some Editor GUI values

v1.11
Issues a warning if a Loft Object will generate too many vertices.
Fixed bug in uv mapping calculation on Simple Loft Layer.
Added a global Offset value to Clone Layer
Added a global Offset value to Clone Simple Layer
Added a global Offset value to Clone Spline Layer
Changed the up calculation to make it consitant between when up is calculated and when its not, may need to change your rotations if you see old clones rotated.
Fixed bug where you could close Main Params foldout

v1.10
Bug fixed of shapes not being created when active scene port wasnt selected.
Shape cursor position now respects the selected curve in multi curve shapes
Adjusted error checking.

v1.09
Bug fixed that stopped scatter layers moving if surface object rotated or moved.
Error checking when objects without meshes are selected added.
Added error dialog when trying to select objects with no meshes.

v1.08
Bug fix for Clone layer not being created from the Layer Create Window fixed.
Removed unused layer types.

v1.07
Fixed exception errors when copying loft objects between scenes, you will need to select paths again after copy.
Folldouts now have colored buttons to make it easier to see and use.
Rebuild Button on Loft object changed to a toggle value.

v1.06
Scatter Layers no longer update it their animation Speed is 0 or the Layer is disabled

v1.04
Added support for Lightmapping of finished loft Objects, a Build Lightmap button as been added to the Loft Object inspector.

v1.03
Modbut class renamed to MegaModBut to avoid naming conflicts.
Added missing button to create first cross section on Complex Loft.

v1.0
First release

