<h1 align="center">Welcome to Lightspeed 👋</h1>
<p>
  <a href="https://www.apache.org/licenses/LICENSE-2.0.txt" target="_blank">
    <img alt="License: Apache--2.0" src="https://img.shields.io/badge/License-Apache--2.0-yellow.svg" />
  </a>
  <a href="https://github.com/Rhinox-Training/rhinox-lightspeed/pulls"><img src="https://camo.githubusercontent.com/39f0f598b3d29424cde0684dbfe69f5a313b84bc2bf9e72f9c2cede374c82d80/68747470733a2f2f696d672e736869656c64732e696f2f62616467652f5052732d77656c636f6d652d626c75652e737667" alt="PRs Welcome" data-canonical-src="https://img.shields.io/badge/PRs-welcome-blue.svg" style="max-width: 100%;"></a>
  <a href="https://openupm.com/packages/com.rhinox.open.lightspeed/" target="_blank">
    <img alt="openupm" src="https://img.shields.io/npm/v/com.rhinox.open.lightspeed?label=openupm&registry_uri=https://package.openupm.com" />
  </a>
</p>

> Coding extensions library: New basic datatypes, static helper methods and extensions on Unity datatypes

## Install

This package is being hosted on [openupm](https://openupm.com/packages/com.rhinox.open.lightspeed/).
If you have their CLI installed you can use the following command.

```sh
openupm install com.rhinox.open.lightspeed
```

If not, you can manually edit the manifest.json (as described on the openupm page) or install it as a git package using the following url:
`https://github.com/Rhinox-Training/rhinox-lightspeed?path=/Assets/Lightspeed`

## Content

Lightspeed offers shortcuts and extensions for coding in C# for Unity, divided into two main parts: Core and Unity. The Core section contains C# functions that work independently of Unity, while the Unity section is tailored for Unity-specific tasks. These tools are designed to make coding faster and more intuitive and work as the basis of our other core packages: [GuiUtils](https://github.com/Rhinox-Training/rhinox-guiutils) and [Utilities](https://github.com/Rhinox-Training/rhinox-utilities).

Please explore the code for a proper overview but here's a quick sample:
#### Core

- A variety of extensions for
  - `collections` Except, GetOrDefault, AddUnique, DistinctBy, IsNullOrEmpty, ...
  - `Array/List` TakeSegment, SortStable, SortBy, ...
  - `ValueTypes` float.LossyEquals, float.Map, Enum.IsSingleFlag, ...
  - `string` ContainsOneOf, ReplaceFirst, GetCommonPrefix, SplitCamelCase, ...
  - `Type` InheritsFrom, HasInterfaceType, ImplementsOpenGenericClass, ...
- `FileHelper` ClearDirectoryContentsIfExists, GetRelativePath, MoveFolder, CreateInstance, ...
- - Static `Utility` class with a bunch of useful methods like:
  - `ReadCsv` parses csv from string
  - `ResizeArrayGeneric` Resize unknown array type
  - `JoinArrays` & `AddToArray`, ...
- static `ReflectionUtility` class with a bunch of useful methods like: 
  - `GetAllFields`\\`Properties`\\`Events` which goes down the Type hierarchy to fetch everything
  - `FindTypes` with a predicate
  - `GetTypesInheritingFrom` 
  - `FindTypeExtensively` uses the type name and finds it across assemblies and namespaces if it has changed.
    - You can mark these changes with attributes like `RefactoringOldNamespace`
  - `GetMemberValue` which fetches a property/field/etc value by string
  - `InvokeMethod` invokes a method by string
- A variety of useful collection types like:
  - `LazyTree` which sorts things into a tree like structure that resolves as you need it.
  - `PriorityQueue` where you can associate items with a priority and it auto sorts when needed
- And many more nice things

#### Unity
- Extension of the `FileHelper` class which handles paths nicely between android, windows, mac, etc
  - i.e. it will use WebRequests to fetch files on android (which is required)
- A variety of extensions for
  - `Bounds` a suite of useful bounds extensions such as `GameObject.GetObjectBounds`, `GameObject.CenterBoundsOnPosition`, etc.
  - `IsWithinFrustum` for Renderer/Vector3/mesh
  - `GameObject` AddComponentWithInit, GetAllChildren, TryGetComponentsInParent, ...
  - `Component` GetOrAddComponent, CopyDataFrom, AlignWith, AlignParentTo, ...
  - `GetMatrixRelativeTo` and other more readable matrix math functions
  - `Quaternion` LossyEquals, AngleTo, Difference, ApplyDifference
  - `Texture` MakeSquare, Pad, InsetBorder, ...
  - `GetClosestTo` / `GetFurthestFrom` from a list of components / positions
  - `Rect` AlignLeft, AlignCenter, and variety of other rect manipulation methods
  - `Vector` With(x: 5), DivideBy, DistanceTo, RotateAround, Clamp, IsColinear, ...
- Static `Utility` class with a bunch of useful methods like:
  - `FindAssets` gets assets from AssetDatabase without the hassle
  - `ProjectOnLine` and various other line tools to find intersections
  - `IsPointInPolygon` is point within the given 2D points
  - `GetKey` & variants which works for both the new and old input system
  - `WrapAngle`, `GetClosestAngle`, ...
  - `Destroy` which switches function depending on editor (immediate), etc
  - `FindSceneObjectsOfTypeAll` get all loaded scene objects of some type
  - `GetColorTexture` Gets you a 1 pixel size texture with a certain color
  - `CopyTextureCPU` copies a texture into CPU memory
- Lots of tiny classes like:
  - `TimeDelay` / `TickDelay` just call `Tick` on it and it will return true every X seconds/ticks
    - Saves you from having to keep track of multiple variables and updating them
  - `TransformState` simple serializable state of a transform, just reapply it with `Restore` / `RestoreRelative`
  - `SceneReferenceData` keeps track of a scene asset that will still work at runtime
  - `Axis` simple enum describing X,Y,Z & s variety of methods to manipulate Vectors with it. i.e. `Vector3.one.With(3, Axis.Z)`
- Static `SeparatingAxesTheorem` class with an implementation of this algorithm.
- Some addressable usage enhancements
- And many more nice things

## Optional dependencies

- com.unity.mathematics (Native extensions)
- com.unity.textmeshpro (TMP Extensions)
- com.unity.addressables (Addressable Extensions)

## Contribution guidelines

- This library contains lightweight, efficient and generic helpers.
- It contains a few Editor utilities but most of that kind of utilities can be found in [GuiUtils](https://github.com/Rhinox-Training/rhinox-guiutils).
- It is intended to not execute anything, only provide helper functions / data objects.
  - So no editor windows
  - No hooks into the editor
  - Not even inheriting from monobehaviour

See the [Utilities](https://github.com/Rhinox-Training/rhinox-utilities) package if you are interested in contributing something along those lines.


## Show your support

- Feel free to make an issue if you find any problems
- Feel free to make a pull request if you wish to contribute
- Give a ⭐️ if this project helped you!

## 📝 License

This project is [Apache--2.0](https://www.apache.org/licenses/LICENSE-2.0.txt) licensed.

Parts of the code come from external authors or are being reused from other open source projects:
- [Deform](https://github.com/keenanwoodall/Deform) - (c) Keenan Woodall (MIT License)
- [SceneReference](https://github.com/JohannesMP/unity-scene-reference) - (c) JohannesMP 2017/(c) S. Tarık Çetin 2019 (MIT License)
