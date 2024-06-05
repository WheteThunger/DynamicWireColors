## Features

- Changes the color of electrical **wires** while they are providing 0 or insufficient power (default: red)
- Changes the color of **hoses** while they are providing 0 fluid (default: red)
- Restores the original **wire** or **hose** color when sufficient power or fluid starts flowing

## Permissions

- `dynamicwirecolors.use` -- While the plugin is configured with `"RequiresPermission": true` (default), wires connecting player-owned entities will only be dynamically colored if at least one of the entities was deployed by a player with this permission.

## Configuration

Default configuration:

```json
{
  "InsufficientPowerColor": "Red",
  "InsufficientFluidColor": "Red",
  "RequiresPermission": true,
  "AppliesToUnownedEntities": false
}
```

- `InsufficientPowerColor` -- Color to use for electrical wires that are providing 0 or insufficient power.
  - Allowed values: `"Default"` | `"Red"` | `"Green"` | `"Blue"` | `"Yellow"` | `"LightBlue"` | `"Orange"` | `"Pink"` | `"Purple"` | `"White"`
- `InsufficientFluidColor` -- Color to use while for hoses that are providing 0 fluid.
  - Allowed values: `"Default"` | `"Red"` | `"Green"` | `"Blue"` | `"Yellow"` | `"LightBlue"` | `"Orange"` | `"Pink"` | `"Purple"` | `"White"`
  - Note: `"Default"` is the same as `"Green"`
- `RequiresPermission` (`true` or `false`) -- While `true` (default), wires connecting player-owned entities will only be dynamically colored if at least one of the entities was deployed by a player with the `dynamicwirecolors.use` permission. While `false`, wires connected to any player-owned entity will be dynamically colored, as though you had granted the permission to all players.
- `AppliesToUnownedEntities` (`true` or `false`) -- While `true`, wires and hoses connecting unowned entities (entities that have `OwnerID` set to `0`) will be dynamically colored. For example, this applies to entities at monuments, as well as to modular car tanker modules. While `false` (default), wires connecting unowned entities will not be affected by the plugin.

## Developer Hooks

#### OnDynamicWireColorChange

```csharp
object OnDynamicWireColorChange(IOEntity ioEntity, IOEntity.IOSlot slot, WireTool.WireColour color)
```

- Called when this plugin is about to change the wire or hose color of an entity's slot
- Returning `false` will prevent the wire color from being changed
- Returning `null` will allow the wire color to be changed
