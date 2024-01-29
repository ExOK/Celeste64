### Setting up Trenchbroom for Level Editing:
 - Use a "Generic" from the game type selection, under `View -> Preferences`
 - Set the `Content` folder as the `Game Path`, so that it can find Textures.
 - Selected `File -> Reload Texture Collections`, to view all the surfaces.
 - Select `Celeste64.fgd` as the Entity Definitions, under the `Entity` tab.

### Loading Levels / Maps
 - Add new Levels to the `Levels.json` by copying an existing entry and modifying it.
 - `"ID"` should be entirely unique for your level - it's what is used to track save data.
 - Place your Trenchbroom `.map` files in the `Maps` folder, and then select your entry Map by modying `"Map"` in `Levels.json`.
 - Restart the game, and it will appear on the Overworld Level select.
 - Press `Ctrl + R` at any time during Gameplay to reload your map (and all other assets) to test while playing.