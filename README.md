## Features

- Automatically changes the color of electrical wires while they are providing 0 or insufficient power
- Automatically changes the color of hoses while they are providing 0 fluid
- Automatically restores the original wire or hose color when sufficient power or fluid is provided

## Permissions

- `dynamicwirecolors.use` -- While the plugin is configured with `"RequiresPermission": true`, only wires and hoses connected to entities deployed by players with this permission will have dynamic colors.

## Configuration

Default configuration:

```json
{
  "InsufficientPowerColor": "Red",
  "InsufficientFluidColor": "Red",
  "RequiresPermission": true
}
```

- `InsufficientPowerColor` -- Color to use for electrical wires that are providing 0 or insufficient power.
  - Allowed values: `"Default"` | `"Red"` | `"Green"` | `"Blue"` | `"Yellow"`
- `InsufficientFluidColor` -- Color to use while for hoses that are providing 0 fluid.
  - Allowed values: `"Default"` | `"Red"` | `"Green"` | `"Blue"` | `"Yellow"`
  - Note: `"Default"` is the same as `"Green"`
- `RequiresPermission` (`true` or `false`) -- While `true`, wires and hoses are dyanmically colored only if one or both of the connected entities was deployed by a player with the corresponding permission. While `false`, all wires and hoses are eligible for dynamic colors, regardless of who placed the connected entities.

## Developer Hooks

#### OnDynamicWireColorChange

```csharp
bool? OnDynamicWireColorChange(IOEntity ioEntity, IOEntity.IOSlot slot, WireTool.WireColour color)
```

- Called when this plugin is about to change the wire or hose color of an entity's slot
  - This is only done to the output slot of the source entity, since that is what the client uses to determine the render color
- Returning `false` will prevent the wire color from being changed
- Returning `null` will allow the wire color to be changed
