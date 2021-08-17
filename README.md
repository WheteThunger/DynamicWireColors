## Features

- Changes the color of electrical **wires** while they are providing 0 or insufficient power (default: red)
- Changes the color of **hoses** while they are providing 0 fluid (default: red)
- Restores the original **wire** or **hose** color when sufficient power or fluid starts flowing

## Permissions

- `dynamicwirecolors.use` -- While the plugin is configured with `"RequiresPermission": true` (default), only wires and hoses connected to entities deployed by players with this permission will have dynamic colors.

## Configuration

Default configuration:

```json
{
  "InsufficientPowerColor": "Red",
  "InsufficientFluidColor": "Red",
  "RequiresPermission": true,
  "AppliesToStaticEntities": false
}
```

- `InsufficientPowerColor` -- Color to use for electrical wires that are providing 0 or insufficient power.
  - Allowed values: `"Default"` | `"Red"` | `"Green"` | `"Blue"` | `"Yellow"`
- `InsufficientFluidColor` -- Color to use while for hoses that are providing 0 fluid.
  - Allowed values: `"Default"` | `"Red"` | `"Green"` | `"Blue"` | `"Yellow"`
  - Note: `"Default"` is the same as `"Green"`
- `RequiresPermission` (`true` or `false`) -- While `true` (default), wires and hoses are dyanmically colored only if one or both of the connected entities was deployed by a player with the corresponding permission. While `false`, all players essentially have permission.
- `AppliesToStaticEntities` (`true` or `false`) -- While `true`, wires and hoses connected to static entities, such as those at monuments, will be dynamically colored. This definition of "static" technically means entities that have `OwnerID` set to `0`, so enabling this could potentially apply to entities spawned via plugins if those plugins do not assign an owner.

## Developer Hooks

#### OnDynamicWireColorChange

```csharp
bool? OnDynamicWireColorChange(IOEntity ioEntity, IOEntity.IOSlot slot, WireTool.WireColour color)
```

- Called when this plugin is about to change the wire or hose color of an entity's slot
- Returning `false` will prevent the wire color from being changed
- Returning `null` will allow the wire color to be changed
