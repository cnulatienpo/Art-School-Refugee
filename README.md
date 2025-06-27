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

## Mess Hall Intro Screen

Display a short welcome the first time a player enters the Mess Hall.

1. Inside your Mess Hall UI canvas create a full screen panel with an `Image` and `CanvasGroup`.
2. Add a `Text` element for the message body, a `Toggle` labelled **Don't show this again**, and a `Button` labelled **Start Drawing**.
3. Attach `MessHallIntroScreen` to the panel and assign the `panel`, `messageText`, `dontShowAgainToggle` and `startDrawingButton` fields.
4. Tweak the text area to be wide with top-left alignment and fade the panel's `CanvasGroup` from 0 to 1 when the Mess Hall opens.

The script checks a `PlayerPrefs` key named `messhall_seen`. When the button is pressed it hides the panel and, if the toggle is enabled, saves the preference so the screen will not appear again.

## Level Strip Manager

Create a small panel that docks to the left side of the screen and displays
buttons for quickly changing between play modes.

1. Under your main `Canvas` create an empty `GameObject` named
   **LevelStrip** and anchor it to the left stretch (min `(0,0)` max `(0,1)`).
   Give it a fixed width and add a **Vertical Layout Group** component so
   children are stacked top to bottom.
2. Inside the strip add three **Button** objects in this order:
   - **Level 1** &ndash; text `"1"`.
   - **Mess Hall** &ndash; replace the button image with your sketchbook icon.
   - **Level 2** &ndash; text `"2"`.
3. Attach `LevelStripManager` to the **LevelStrip** object and assign the three
   button references in the inspector.
4. Ensure a `GameManager` exists in the scene with methods `EnterLevel1()`,
   `EnterMessHall()` and `EnterLevel2()`. It should expose a boolean
   `IsLevel2Unlocked` and an `ActiveMode` enum used by the script.
5. Play the scene. Clicking a button calls the corresponding `GameManager`
   method. Level 2 remains disabled until `IsLevel2Unlocked` becomes `true` and
   the currently active mode is highlighted.

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
