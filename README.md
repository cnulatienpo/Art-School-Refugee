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

## Mess Hall Manager

1. Create an empty GameObject in your scene and add the `MessHallManager` script.
2. Assign the Mess Hall UI canvas, the prompt bar GameObject, and the scrollable drawing canvas to the `messHallUI`, `promptBar`, and `scrollableCanvas` fields.
3. Connect your sketchbook button to the `EnterMessHall` method so clicking it opens the Mess Hall.
4. Add a close or back button inside the Mess Hall and hook its `onClick` event to `ExitMessHall`.

When entering the Mess Hall, all three objects are activated. Exiting hides them again.

## Symmetry Handler

Attach the `SymmetryHandler` component anywhere in your scene and assign the
`SketchbookToolPanel` so it can read the `mirrorSymmetry` toggle. Use
`GetMirroredPosition` when drawing a brush stroke to obtain the reflected
location across the center of the drawing area.

To convert pointer coordinates from screen space to the drawing area's local
space use:

```csharp
Vector2 localPos;
RectTransformUtility.ScreenPointToLocalPointInRectangle(
    drawingArea, Input.mousePosition, canvas.worldCamera, out localPos);
```

Pass `localPos` to `GetMirroredPosition` to retrieve the mirrored position.

## Layer Manager

Use `LayerManager` to organize three numbered layers in your sketchbook scenes.

1. Create three `RawImage` objects or RenderTextures for Layer 1, Layer 2 and
   Layer 3.
2. Add the `LayerManager` component to any GameObject and drag the textures into
   the `layers` array.
3. Call `SetActiveLayer(index)` from input scripts to choose which layer the
   `BrushEngine` draws to, and use `GetActiveTexture()` to retrieve that
   texture.
4. When it's time to export or flatten the drawing call `MergeAllLayers()` which
   blends the three layers from bottom to top and returns a single `Texture2D`.
   If the `disableLayers` parameter is true the original layer references are
   cleared so they no longer receive strokes.

`MergeAllLayers` copies pixel data from each layer, including RenderTextures,
before performing standard alpha blending. The result can be saved as PNG or
used elsewhere in your project.

## Sketchbook Exporter

Attach the `SketchbookExporter` component to any GameObject and assign its
`layerManager` field. Call `ExportMergedDrawing` to save the combined layers
as either PNG or JPG.

The exporter writes files to `Application.persistentDataPath` by default. Enable
`saveToDesktop` to store images on the user's desktop instead. Filenames follow
the pattern `messhall_sketch_###.png` or `.jpg` and automatically increment so
existing files are not overwritten.

## Session Upload Server

A small Node.js + Express server receives session data from the Mess Hall and
stores it in MongoDB Atlas.

1. Copy `.env.example` to `.env` and fill in your `MONGO_URI`.
2. Run `npm install` to install dependencies.
3. Start the server with `node server.js`.

The server listens on `/api/messhall/upload` and returns `{ "status": "ok" }`
when data is successfully stored.
