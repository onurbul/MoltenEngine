[b4b07048]
Added 0.2 Changelog 

[dfaa667f]
[UI] Added UITextSegment.Seek() helper method 
[UI] More improvements to UITextBox caret navigation

[477ce55d]
[UI] Added home/end key support to UITextBox 

[82f0fc89]
[UI] Added caret right move functionality 
[UI] Further improvements to UITextBox caret navigation
[UI] Cleaned up UITextSegment

[c6a49d73]
[UI] Further improvements to UITextCaret.Move() 

[81dfbd5d]
[UI] Several fixes to WIP UITextBox caret navigation 
[UI] Moved UITextCaret.CaretPoint.SelectedChar class into CaretPoint
[UI] Removed UITextCaret.CaretPoint.EndOffset property
[UI] Streamlined UITextCaret char index tracking

[65eeba71]
[Input] Fixed KeyboardDevice.OnKeyDown not triggering when a key press is held 
[Win] Fixed missing timestamp on Keyboard key release actions
[Input] Added IPIckable<T>.OnKeyDown() and .OnKeyUp() methods
[UI] Added UIElement.OnKeyDown() and .OnKeyUp() virtual methods
[UI] UITextBox caret will now move correctly when arrow key is held down

[134a05a2]
Regenerated documentation with HTMLDocGenerator 

[9bf2a272]
[UI] Initial work on UITextBox caret navigation 

[c503ba66]
[UI] Don't draw white icon square if UIWindow icon is not set 

[cfe8c911]
[UI] Further improvements to UITextBox caret 
[UI] Fixed positioning of UITextBox carret after a line break
[UI] Fixed UITextBox caret not visible when line is clicked beyond it's end width

[1385bcb9]
[UI] Added missing summaries in UITextSegment 
[UI] Removed UITextChunk.NewNext() - Unused

[234b61a9]
[UI] Fixed first half of split segment disappearing during UITextLine.Split() 
[UI] Added UITextLinkable abstract class
[UI] Unified UITextBox linking logic of chunks, lines and segments via UITextLinkable

[c78759c5]
[UI] Fixed line duplication during line break 
[UI] Further improvements to UITextBox chunk (un)linking

[902f5c60]
[UI] Fixes to UITextBox chunk resizing. 
[UI] UITextBox.AppendLine() now uses InsertLine() internally
[UI] Cleaned up old UITextBox methods

[79da626c]
[UI] Fixed UITextBox mid-segment line break moving the whole segment 

[4bf0b75f]
[Android] Fixed build error in Molten.Engine.Android 

[ec12a4e2]
[UI] Fixed UITextBox line breaks when caret is between two segments (char index 0) 

[cc17c66a]
Set input service window when focusing a native example window 

[6a80b323]
Merge pull request #124 from Syncaidius/dependabot/nuget/Magick.NET-Q8-AnyCPU-12.2.2 
Bump Magick.NET-Q8-AnyCPU from 12.2.1 to 12.2.2
[dce0b197]
Bump Magick.NET-Q8-AnyCPU from 12.2.1 to 12.2.2 
Bumps [Magick.NET-Q8-AnyCPU](https://github.com/dlemstra/Magick.NET) from 12.2.1 to 12.2.2.
- [Release notes](https://github.com/dlemstra/Magick.NET/releases)
- [Commits](https://github.com/dlemstra/Magick.NET/compare/12.2.1...12.2.2)

---
updated-dependencies:
- dependency-name: Magick.NET-Q8-AnyCPU
  dependency-type: direct:production
  update-type: version-update:semver-patch
...

Signed-off-by: dependabot[bot] <support@github.com>
[0fef6ac6]
[Input] Fixed every second click getting ignored spam-clicking 
[Input] Removed InputAction.DoublePressed
[Input] Correctly make use of InputActionType.Double for double presses
[Win] Fixed WinformsSurface.NextFrame() retrieving Win32 messages for all windows, instead of just itself

[0970f72e]
[UI] Insert new lines in the current UITextBox chunk, not last 

[b7db6221]
[UI] Resize chunks after inserting lines in UITextBox 

[abcccc44]
[UI] First pass on line breaking in UITextBox 
[UI] Merged UITextElement back into UITextBox
[Input] Renamed KeyCode.Back to KeyCode.Backspace
[UI] Refactored line linking in UITextBox

[26a83979]
[UI] Removed UITextSegmentType 
[UI] UITextBox now supports typing on empty lines

[92857fd9]
[UI] Improved accuracy of caret in UITextLine.Pick() 
[Input] Added KeyboardDevice parameter to KeyboardDevice.OnCharacterKey event
[UI] Added support for keyboard and char-based input. Closes #85
[Input] Added IInputReceiver.InitializeInput() and .DeinitializeInput()
[input] Added IInputReceiver.IsFirstInput()
CameraComponent now implements IInputReceiver<KeyboardDevice>
Improved input handling in SceneManager
[UI] Added UIElement.OnKeyboardInput()
[UI] Added UIElement.OnKeyboardChar()
Added SpriteFont.MeasureChar() and MeasureCharWidth()

[6a1b6ba4]
Expanded scene component type tracking system 
Added SceneLayer.Track<T>()
Added SceneLayer.RemoveTrackCallbacks();
Added SceneLayer.Untrack<T>()
Added SceneLayer.GetTracked<T>()
IInputReceiver is now IInputReceiver<T> to receive specific types of InputDevice input
By default, IPickable<Vector2F>, Ipickable<Vector3F> and IInputReceiver<KeyboardDevice> are tracked
Added directional movement via left analogue stick in examples

[43cb778e]
Added support for double-clicking/tapping 
[Input] Added InputSettings.DoubleClickInterval
[Input] Added missing summaries for InputAction enum
[Input] Added InputAction.DoublePressed
[Input] Added PointingDevice.OnDoublePressed event
[Input] Added detection of double-press actions to PointingDevice
[UI] Added UIElement.DoublePressed event
[UI] Added UIElement.OnDoublePressed protected method
Examples can now be opened by double-clicking items in the example list

[3bd85937]
Added missing summaries in IPickable<T> 

[60e80fe9]
[UI] Left clicking UITextBox will now clear it's selection and move the caret 
[UI] Dragging the mouse on UITextBox will now update text selection accordingly

[99a84c94]
[UI] Further improvements to UITextCaret selection handling 

[925fd95b]
[UI] Fixed incorrect selection rendering when end point comes before start point 

[f9f53d3f]
[Math] Improved T4 generation of multi-dimensional types 
[Math] Matrix2/3/4 fields are now T4 templated
[Math] Added callback for custom summaries to  T4 method TypeGenerator.GenerateFields()

[85e7f7f5]
[Math] Further improvements to T4 operator generation 
[Math] Added scalar operators to T4-generated vector, angle, quaternion and color types

[84905c1a]
[Math] Fixed T4 floating-point vector template 

[049a351d]
[Math] Fixed summaries referring to "add" for all T4-generated operators 

[dd2723d8]
[Math] Further improvements to T4 templating 
[Math] Updated Matrix2 T4 template
[Math] Updated Matrix3 T4 template
[Math] Updated Matrix4 T4 template

[4c5f31ef]
[Math] Updated T4 quaternion template 
[Math] Added static ref-based operator methods for quaternion types

[ab621405]
[Math] Automated argument name generation 
[Math] Arguments are no longer needed in type definition files

[181afe4d]
Remove changelog generator - revisit later 

[61ee64ff]
docs(CHANGELOG): Updated 0.2 release notes 

[2d3f45cd]
Disable changelog generator running on commit/PR 

[ab974d22]
Update changelog generator access token 

[66cc35b3]
Attempt to fix changelog generator 

[279a040a]
docs(CHANGELOG): Updated {{ github.event.inputs.release_version }} release notes 

[776a4440]
Updated changelog yaml 

[8db4e243]
Updated changelog generator main.yaml 

[0b289085]
Attempt to auto-generate changelogs 

[9313219a]
[Math] Updated T4 rectangle template 
[Math] Added summaries to T4-generated cast operators
[Math] Added array-value constructor to rectangle types
[Math] Added unsafe pointer constructor to rectangle types
[Math] Improved rectangle cast operators

[53956587]
[Math] Refactored and updated all vector T4 sub-templates 

[7b4c5683]
[Math] Updated AngleF and AngleD T4 template 
[Math] AngleF/D Radians property is now a field
[Math] Removed private AngleF/D.radiansInt
[Math] Updated AngleF/D.GetHashCode()

[2a85ac4e]
[Math] Refactored main T4 vector template 
[Math] Added GenerateUpgradeConstructors() T4 helper method
[Math] Improved T4 operator generation
[Math] Improved T4 summary generation
[Math] Updated vector type definitions

[68226c28]
[Math] Simplified static Clamp() in T4 color template 

[e13a3a57]
[Math] Finished T4 template for color types 
[Math] Added ref and operator divide methods to all color types
[Math] Renamed ToBgra() to PackBGRA() on all color types
[Math] Renamed ToRgba() to PackRGBA() on all color types
[Math] Added instanced Clamp() method to all color types
[Math] Added Dot() to Color3 and Color3D
[Math] Added Swizzle() to Color3 and Color3D

[95be30ec]
[Math] Full refactor of type generation 
[Math] Added definition file for primitive data type information
[Math] Simplified type definitions
[Math] Field and arg names are now defined in type definitions
[Math] Added helper method for generating standard constructors to t4_header.tt
[Math] Added helper method for generating fields
[Math] Added helper methods for generating arg and parameter lists
[Math] Color3, Color4, Color3D and Color4D are now T4 templated

[da0eca82]
[Math] Merged T4 matrix definitions 

[28fb9927]
[Math] Matrix4F and Matrix4D are now T4 templated 
[Math] Matrix4D now has the correct struct pack size

[ce10ebd4]
[Math] Refactored T4 template chain to reduce size of type definitions 

[c1940a91]
[Math] Fix Matrix2/3 summaries 

[91984f24]
[Math] Matrix3F and Matrix3D are now T4 templated 
[Math] Fixed incorrect struct layout pack value for Matrix3D
[Math] Fixed incorrect data-type for Matrix3D array constructor

[75a600b7]
[Math] Matrix2x2F and Matrix2x2D are now T4 templated 
[Math] Added missing struct layout attribute to Matrix2x2F and Matrix2x2D

[f742c331]
Use switch inside SceneComponent.RegisterOnLayer() and UnregisterFromLayer() 

[dab9076c]
Merged IPickable2D into IPickable<T> 

[80e70e95]
Replaced IPickable2D with IPickable<T> 
[Math] Added IVector<T>
[Math} T4-generated vector types now implement IVector<T>
[Math] Added IPickable.Pick3D()

[743f968a]
[Input] Refactored object picking via CameraComponent 
[Input] Added IPickable and IPickable2D
[UI] Refactored UIElement picking to implement IPickable2D
Renamed UIPointerTracker to CameraInputTracker
Added Scene.FocusedCamera property
[UI] Merged most UIManagerComponent functionality into CameraComponent

[d5938ce7]
[UI] UITextElement now has a visible caret 
[UI] Added caret blinking functionality
[UI] UITextBox positions caret based on selection

[03905315]
[UI] Fixed selection not working if no start segment was picked 

[f2d68e55]
[UI] UITextBox can now select at a per-character level 

[2c7fbe07]
[Graphics] Added SpriteFont.MeasureWidth() 
[Graphics] Added missing summaries in SpriteFont
[Math] Prefer use of primitive constants - e.g. float.Pi and double.Pi - over Math and MathF constants

[76050eda]
[IO] Cleanup and improvements to Logger class 

[df26ed9c]
[IO] Improved inner-exception handling in Logger.Error() 

[fb76afc6]
[Math] Rectangle types are now generated from a T4 template 
[Math] Added long/ulong RectangleL and RectangleUL
[Math] Removed implicit rectangle cast operators
[Math] Added consistent explicit cast operators between rectangle types

[8e68e82c]
Merge pull request #106 from Syncaidius/dependabot/nuget/Newtonsoft.Json-13.0.2 
Bump Newtonsoft.Json from 13.0.1 to 13.0.2
[39707cde]
Bump Newtonsoft.Json from 13.0.1 to 13.0.2 
Bumps [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) from 13.0.1 to 13.0.2.
- [Release notes](https://github.com/JamesNK/Newtonsoft.Json/releases)
- [Commits](https://github.com/JamesNK/Newtonsoft.Json/compare/13.0.1...13.0.2)

---
updated-dependencies:
- dependency-name: Newtonsoft.Json
  dependency-type: direct:production
  update-type: version-update:semver-patch
...

Signed-off-by: dependabot[bot] <support@github.com>
[484c4f62]
[Math] Added ref overload of Clamp() to all vector types 
[Math] Added aggressive inlining to static vector methods, where appropriate

[462a55ff]
[Math] AngleD and AngleF are now T4 template-generated 
[Math] Added ref overloads of Min() and Max() to AngleF/AngleD
[Math] AngleF/AngleD Add(), Subtract(), Divide() and Multiply() now expect ref parameters. Use operators for non-ref operations

[b75e66eb]
[Math] Updated T4 templates to no longer use MathHelperDP 

[e128d1b7]
[Math] MathHelper degree/radian/gradian/revolution conversion methods are now generic 
[Math] Merged remaining non-generic MathHelperDP methods back into MathHelper
[Math] Removed MathHelperDP
[Math] Added MathHelper.Constants<T> static class for generic floating-point constants
[Math] Fixed usage of MathF in QuaternionF

[68c2daae]
Use MathF instead of Math where applicable 

[c76b0b8c]
[Math] Missing vector T4 parameter summaries for ref methods Min(), Max() and Clamp() 

[122ce692]
[Math] Use MathF instead of Math in Matrix3F and Matrix4F 

[238f3ecd]
[Math] Fixed incorrect name of new MathHelper.Lerp<T>() method 
[Math] Removed MathHelperDP.Lerp()

[fc87c493]
[Math] Moved MathHelper.Epsilon and BigEpsilon into CollisionHelper 
[Math] Removed MathHelperDP.Epsilon and BigEpsilon

[b3df0129]
[Math] MathHelper.WrapAngle() is now a generic math method 
[Math] Removed MathHelperDP.WrapAngle()
[Math] Replaced MathHelper.Wrap() integer overload with a generic math implementation - MathHelper.WrapI()
[Math] MathHelper.Mod2PI() is now a generic math method
[Math] Removed MathHelperDP.Mod2PI()
[Math] Removed MathHelper.Pi, TwoPi and Tau constants in favour of float.Pi and Tau
[Math] Removed MathHelperDP.Pi, TwoPi and Tau constants in favour of double.Pi and Tau
[Math] Improved AngleD and AngleF.Wrap() implementation

[e3176924]
[Math] Removed NearEqual from integer vector types 
[Math] Updated T4 vector templates
[Math] MathHelper.NearEqual() is now a generic math method
[Math] Removed MathHelperDP.NearEqual()

[cabaeaf1]
Math Refactor 
[Math] Fixed metadata size not being set before T4 GenerateTypes() filter is called
[Math] Fixed whitespace at the start of T4-generated type files
[Math] Refactored the rest of the T4 vector templates to use GenerateTypes()
[Math] Moved MathHelper.BarycentricCoordinates() to Vector3F and Vector3D
[Math] Moved MathHelper.GetVelocityOfPoint() to Vector3F and Vector3D
[Math] MathHelper.Median() is now a generic math method
[Math] MathHelper.Min() is now a generic math method
[Math] MathHelper.Max() is now a generic math method
[Math] MathHelper.NonZeroSign() is now a generic math method
[Math] MathHelper.RoundToNearest() is now a generic math method
[Math] Removed MathHelperDP.NonZeroSign()
[Math] Removed MathHelperDP.RoundToNearest()
[Math] Removed MathHelperDP.Median()
[Math] Optimized MathHelper.Max(T,T, params T)
[Math] Optimized MathHelper.Min(T,T, params T)
[Math] Added generic MathHelper.Lerp() floating-point overload
[Math] MathHelper.Clamp(T value) is now generic math method
[Math] Removed MathHelperDP.Clamp(double value)
[Math] MathHelper.Mod(T value) is now generic math method
[Math] Removed MathHelperDP.Mod(double value)

[4e80bfd9]
[Math] Fixes and improvements to t4 template helper methods 
[Math] Updated floating-point Vector3 t4 template

[095be8d5]
[Math] Updated t4 quaternion template 

[7189dcfe]
[Math] Refactored and optimised vector t4 template 

[d78ca861]
[Math] [Breaking change] Split math types in to precision-based namespaces 
[Math] Moved all double-precision types into Molten.DoublePrecision namespace
[Math] Moved all half-precision types into Molten.HalfPrecision
This change is to reduce bloat in the main Molten namespace

[e40420b3]
Added generic math benchmark tests 
Cleaned up whitespace in floating-point vector types

[62fda86e]
[Math] Corrected summary for new Abs() vector method 

[096dc75a]
[Math] Removed Shape_Old class 

[f416bbcb]
[Math] Added Abs() to all floating-point vector types 

[c711bdd2]
[Math] Fixed Equals() implementation for integer vectors 
[Math] Correct use of Math and MathF for floating-point vectors
[Math] Removed some floating-point methods from integer vectors

[ade34ee0]
[Math] Updated AngleF and AngleD summaries 
[Math] Updated summaries for Half2/3/4
[DX11] Removed old SharpDX interop code

[7facedd1]
[UI] Further implementation of text selection functionality 
[UI] Renamed UITextElement.MaxCharacters to MaxLength
[UI] UITextElement.SetText() now respects MaxLength property
[UI] Added UITextSegment.IsSelected property - set by a UITextCaret internally
[UI] Added UITextCaret.Clear()
[UI] Removed obsolete UITextChunk.ChunkPickResult struct

[6e9e3873]
[UI] Refactored UITextElement selection functionality 
[UI] Added UITextCaret class
[UI] Separated UITextBox.Chunk into UITextChunk class

[e8979eff]
[UI] Fixes to UITextBox.AppendLine() and .NewLine() 
[UI] Added UITextElement.Recalculate()
[UI] Added UITextElement.Clear()
[UI] Fixed UITextSegment not measuring whitespace
[UI] Fixes to UITextParser

[478c7c4c]
[UI] Added UITextElement.Parser property 
[UI] Fixed more build errors
[UI] Added missing summaries

[82359e1c]
[UI] Switched UITextBox and UITextLine to linked-list 

[8cc0795a]
[UI] Implemented a generic UITextParser 

[34602762]
[UI] Added UITextElement base class for text-based UI elements 
[UI] Added UITextElement.MaxLength property
[UI] Separated Line class from UITextBox into UITextLine
[UI] Separated Segment class from UITextBox into UITextSegment
[UI] Removed UITextBox.RuleSet class - Replaced by customizable parser system
[UI] Renamed UITextSegment.AddNextNode() to .InsertSegment()
[UI] Exposed segment-insert functionality in UITextSegment
[UI] Added missing summaries to UITextLine
[UI] Added UISettings.DefaultTextParser property (non-serialized)
[UI] Removed UITextBox.SetText()
[UI] Added UITextElement.AppendLine() and .AppendSegment() methods

[7dd92055]
[UI] UITextbox line and segment selector now support scrolling 

[ef584085]
[UI] Fixed UITextbox text getting mixed up after a new chunk is added 

[e1e17436]
[UI] Refactored UITextBox rendering 
[UI] UITextBox line numbers now scroll correctly
[UI] UITextBox now internally splits text into chunks of lines for easier culling
[UI] UITextBox now only renders visible line chunks
[Util] Added ThreadedList.UnsafeCount
[Util] ThreadedList.Count is now thread-safe
[Util] Added ThreadedList.AddRange() overload for copying items directly from another ThreadedList

[2fc510a5]
[UI] Started refactor of UITextbox line management 

[cdc008fd]
[UI] Margin system now functional 
[UI] Added UIMargin.FitToParent()
[UI] Removed UIMargin.SideMode.Auto
[UI] Added UIMargin.SideMode.Percent

[0fb1a942]
Updated build config 

[16574ae9]
Added benchmark project - uses BenchmarkDotNet 
Added benchmark for Vector4 SIMD testing

[248cada6]
[Math] Removed MathHelper.IsPowerOf2() in favour of primitive type .IsPow2() 
DDSWriter now uses uint.IsPow2()

[387cb7b6]
Removed MathHelper.Sign() and MathHelper.DP.Sign in favour of primitive type Sign() methods 
Shape.Contour now uses double.Sign()

[60676605]
[Math] Use type constants for float and double 

[491bee8e]
Switched to float.Clamp() and int.Clamp() 
[Math] Removed MathHelper.Clamp(value, min, max)

[370a7a78]
[Math] Cleaned up MathHelper and MathHelperDP 

[b19ee533]
[Math] Removed Hermite() and CatmullRom() from integer vector types 

[3495f416]
[Math] Added tuple (de)composers (cast operators) to Vector types 

[f2f009e5]
Merge pull request #97 from Syncaidius/dependabot/nuget/Magick.NET-Q8-AnyCPU-12.2.1 
Bump Magick.NET-Q8-AnyCPU from 12.2.0 to 12.2.1
[66301e05]
Merge pull request #98 from Syncaidius/dependabot/nuget/System.Management-7.0.0 
Bump System.Management from 6.0.0 to 7.0.0
[09e59bb1]
Bump Magick.NET-Q8-AnyCPU from 12.2.0 to 12.2.1 
Bumps [Magick.NET-Q8-AnyCPU](https://github.com/dlemstra/Magick.NET) from 12.2.0 to 12.2.1.
- [Release notes](https://github.com/dlemstra/Magick.NET/releases)
- [Commits](https://github.com/dlemstra/Magick.NET/compare/12.2.0...12.2.1)

---
updated-dependencies:
- dependency-name: Magick.NET-Q8-AnyCPU
  dependency-type: direct:production
  update-type: version-update:semver-patch
...

Signed-off-by: dependabot[bot] <support@github.com>
[c7ba6157]
Bump System.Management from 6.0.0 to 7.0.0 
Bumps [System.Management](https://github.com/dotnet/runtime) from 6.0.0 to 7.0.0.
- [Release notes](https://github.com/dotnet/runtime/releases)
- [Commits](https://github.com/dotnet/runtime/commits)

---
updated-dependencies:
- dependency-name: System.Management
  dependency-type: direct:production
  update-type: version-update:semver-major
...

Signed-off-by: dependabot[bot] <support@github.com>
[185834ab]
[Math] Replaced Molten.Half with System.Half 

[fa2ce58d]
Updated build yml configs 

[576e7474]
Updated to .NET 7.0 

[f947e5ce]
Refactored UIPadding class 
Renamed UISpacing to UIPadding
Added UIMargin class [WIP]
Removed UIAnchorFlags

[eabd377f]
[Input] Remove debug message 

[1d6e3b4a]
[UI] Completed mouse wheel support on UITextBox 

[f9bdcaf5]
Update WinMouseDevice.cs 

[1b1f6df9]
[Input] Normalized InputScrollWheel.Delta 
[Input] Removed InputScrollWheel.Increment property

[1e062112]
Merge branch 'master' of https://github.com/Syncaidius/MoltenEngine 

[022d8c69]
[UI] Added support for scrolling using mouse wheels 
[Input] Fixed horizontal mouse wheel updates being interpreted as vertical wheel
[Win] Fixed mouse wheel data being parsed incorrectly
Updated NuGet package tags for several projects
[Input] Added InputScrollWheel.Increment - default is 120

[924bb037]
Update pages.yml 

[76f48b5f]
Updated nuget package config 

[d451face]
[Docs] Improvements to method pages 
[Docs] Constructors now have pages
[Docs] Ref/in/out parameters now labelled accordingly
[Docs] Regenerated with the latest HtmlDocGenerator

[4e71d60b]
Update README.md 

[e7bf0c29]
Merge pull request #81 from Syncaidius/dependabot/nuget/Magick.NET-Q8-AnyCPU-12.2.0 
Bump Magick.NET-Q8-AnyCPU from 12.1.0 to 12.2.0
[62ee9a7d]
Bump Magick.NET-Q8-AnyCPU from 12.1.0 to 12.2.0 
Bumps [Magick.NET-Q8-AnyCPU](https://github.com/dlemstra/Magick.NET) from 12.1.0 to 12.2.0.
- [Release notes](https://github.com/dlemstra/Magick.NET/releases)
- [Commits](https://github.com/dlemstra/Magick.NET/compare/12.1.0...12.2.0)

---
updated-dependencies:
- dependency-name: Magick.NET-Q8-AnyCPU
  dependency-type: direct:production
  update-type: version-update:semver-minor
...

Signed-off-by: dependabot[bot] <support@github.com>
[e9433e31]
Update README.md 

[ee223b6b]
[Docs] Regenerated documentation 
[Docs] Updated to the latest HtmlDocGenerator

[01935bb9]
Corrected nuget package author field 

[bf4baf29]
Added missing alpha nuget version tag 

[1feb35bc]
Added nuget package config to all project files 
Removed old Molten.Platform project files - not used