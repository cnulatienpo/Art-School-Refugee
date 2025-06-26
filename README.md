# Art-School-Refugee
Unity Game

## Desktop build support

The project now includes configuration for Windows and macOS builds with the
Unity Input System enabled. A new `DesktopSettings` script ensures canvas
scaling matches the current resolution and provides stylus pressure/tilt
fallback using the mouse.

## Mess Hall Setup

To create the Mess Hall drawing interface:

1. Create a new `Canvas` and add a `ScrollRect`.
2. Inside the ScrollRect, create a `Viewport` with a `Mask` and an empty `Content` RectTransform.
3. Attach `MessHallCanvas` to any GameObject and assign:
   - `scrollRect` to your ScrollRect component.
   - `content` to the Content RectTransform.
   - `sheetPrefab` to an `Image` prefab tinted to a light tan color or using a textured sprite.
   - `newSheetButton` to a UI button positioned outside the ScrollRect.
4. Optionally add a prompt bar at the top of the screen as a separate UI element so it stays docked while the content scrolls.

Press **New Sheet** to extend the paper without clearing previous drawings. The `ExportImage` method can be called to save the entire drawing as a PNG; use a PDF library to convert the image to PDF if needed.
