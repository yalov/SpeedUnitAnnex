## ChangeLog:

### Version 1.4.1.1
 * fix new option added in v1.4.1

### Version 1.4.1
 * new option: in the Target mode, split speed into 2 components only if RCS is enabled

### Version 1.4.0
 * new option: in the Target mode, while retrograde to target, the color of speed become red.
 * new option: in the Target mode, split speed into 2 components: 
    * signed projection on the direction to target 
    * unsigned leftover (speed on the surface orthogonal to line to the target)
 * restructure settings in 3 columns

### Version 1.3.9
 * fix log spam
 * new option: in the Surface mode, split speed to horizontal and vertical components

### Version 1.3.8
 * new option: in the Surface mode, while descending the color of speed become red.
   (idea from the Vertikal Speed Indicator)

### Version 1.3.7
 * Updated Russian localization (Sooll3)
 * Fixed nullref

### Version 1.3.6
 * recompiled for ksp 1.8.0
 * targeted .NET 4.7.2

### Version 1.3.5
 * Merge FAR compatibility (supports at least FAR v0.15.10.0 "Lundgren")
 * Options for disable Orbit Ap/Pe or Time readouts separately
 * Recompile for 1.7
 * Fix Radar Altimeter: landed on a splashed vessel
 * Added all missing SI prefixes, which KSP has: 
   peta-, exa-, zetta-, yotta-

### Version 1.3.4 (Dev)
 * fix FAR compatibility: take IAS from the FAR
   (supports at least FAR v0.15.10.0 "Lundgren")

### Version 1.3.3 (Dev)
 * FAR compatibility
   (supports at least FAR v0.15.10.0 "Lundgren")

### Version 1.3.2
 * surface: multiple choices for rover and airplane speedometers settings
   * added km/h and mph for airplanes
 * target: relative docking port angles
   * only a roll angle
   * yaw, pitch and roll angles
   * integer or one decimal digit precision
   * option for disabling target name
 * recompile for ksp 1.6.1
 * fix settings-esc bug
 * fix targeting .NET version to 3.5

### Version 1.2.6
 * recompile for ksp 1.6.0

### Version 1.2.5
 * show a kerbal trait in the EVA
 * better kerbal name cropping
 * optimization
 * fixed AGL in the sea: distance to bottom for every vessel below -20m  

### Version 1.2.0.1/1.2.1.1
 * for KSP 1.4.5/1.5.1, respectively
 * fix a nullref bug

### Version 1.2.1
 * recompile for KSP 1.5.0

### Version 1.2.0
 * Surface: Indicated AirSpeed (IAS) for airplanes
 * Orbit: Ap/Pe become a little transparent if they are unstable: smaller than atmosphere height or larger than SOI-radius
 * Chinese localization (thanks to Yanchen) * 
 * localization updates

### Version 1.1.8
 * recompiled for KSP 1.4.3
 * fix URL in the .version file

### Version 1.1.7
 * recompiled for KSP 1.4.2
 * French localization (thanks to Valens)
 * Portuguese localization (draft)

### Version 1.1.6
 * Localization update (de-de, en-us, es-es, it-it, ru)

### Version 1.1.5
 * Italian localization (thanks to Simog)
 * surface EVA AGL fix,
 * hide "Orb" for Orbit-EVA with enabled "Time to the next Ap/Pe"

### Version 1.1.3/1.1.4
 * recompiled for KSP 1.3.1/1.4.1, respectively
 * Ap/Pe become a little transparent if they are negative or larger than SOI-radius
 * new orbit mode option: time to closest Ap/Pe (disable by default)
 * speed in the target mode has 2 digit after decimal for < 2 m/s

### Version 1.0.0
 * recompiled for KSP 1.4.0

### Version 0.9.10
 * Add Spanish localization (thanks to Fitiales)

### Version 0.9.9
 * Add kerbal name for kerbals in orbit mode.

### Version 0.9.8
 * add knots for airplanes (Mach number or knots in the setting)
 * fix bugs

### Version 0.9.7.1
 * German localization (thanks to Isabelle.V.Fuchs)

### Version 0.9.7
 * Released on 2018-01-15
