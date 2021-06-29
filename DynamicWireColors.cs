using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using static WireTool;
using static IOEntity;

namespace Oxide.Plugins
{
    [Info("Dynamic Wire Colors", "WhiteThunder", "1.0.0")]
    [Description("Temporarily changes the color of wires and hoses while they are providing insufficient power or fluid.")]
    internal class DynamicWireColors : CovalencePlugin
    {
        #region Fields

        private Configuration _pluginConfig;

        private const string PermissionUse = "dynamicwirecolors.use";

        #endregion

        #region Hooks

        private void Init()
        {
            permission.RegisterPermission(PermissionUse, this);
        }

        private void OnServerInitialized()
        {
            foreach (var entity in BaseNetworkable.serverEntities)
            {
                var ioEntity = entity as IOEntity;
                if (ioEntity == null)
                    continue;

                FixWireColors(ioEntity);
                ProcessSourceEntity(ioEntity);
            }
        }

        private void Unload()
        {
            foreach (var entity in BaseNetworkable.serverEntities)
            {
                var ioEntity = entity as IOEntity;
                if (ioEntity == null)
                    continue;

                foreach (var destinationSlot in ioEntity.inputs)
                {
                    var sourceEntity = destinationSlot.connectedTo.Get();
                    if (sourceEntity == null)
                        continue;

                    var sourceSlot = sourceEntity.outputs[destinationSlot.connectedToSlot];
                    ChangeSlotColor(sourceEntity, sourceSlot, destinationSlot.wireColour);
                }
            }
        }

        private void OnOutputUpdate(IOEntity sourceEntity)
        {
            NextTick(() =>
            {
                if (sourceEntity == null)
                    return;

                ProcessSourceEntity(sourceEntity);
            });
        }

        #endregion

        #region Helper Methods

        private bool ChangeColorWasBlocked(IOEntity ioEntity, IOSlot slot, WireColour color)
        {
            object hookResult = Interface.CallHook("OnDynamicWireColorChange", ioEntity, slot, color);
            return hookResult is bool && (bool)hookResult == false;
        }

        private void FixWireColors(IOEntity sourceEntity)
        {
            // This fixes an issue where loading a save does not restore the input slot colors.
            // This workaround updates the input slot colors to match the output colors when the plugin loads.
            // Without this workaround, we can't use the input colors to know which color to revert back to.
            foreach (var sourceSlot in sourceEntity.outputs)
            {
                var destinationEntity = sourceSlot.connectedTo.Get();
                if (destinationEntity == null)
                    continue;

                var destinationSlot = destinationEntity.inputs[sourceSlot.connectedToSlot];
                if (destinationSlot.wireColour == WireColour.Default)
                    destinationSlot.wireColour = sourceSlot.wireColour;
            }
        }

        private bool EntityHasPermission(IOEntity ioEntity)
        {
            if (!_pluginConfig.RequiresPermission)
                return true;

            if (ioEntity.OwnerID == 0)
                return false;

            return permission.UserHasPermission(ioEntity.OwnerID.ToString(), PermissionUse);
        }

        private bool EitherEntityHasPermission(IOEntity ioEntity1, IOEntity ioEntity2, string perm) =>
            EntityHasPermission(ioEntity1) || EntityHasPermission(ioEntity2);

        private void ChangeSlotColor(IOEntity sourceEntity, IOSlot sourceSlot, WireColour color)
        {
            if (sourceSlot.wireColour == color || ChangeColorWasBlocked(sourceEntity, sourceSlot, color))
                return;

            sourceSlot.wireColour = color;
            sourceEntity.SendNetworkUpdate();
        }

        private bool HasOtherMainInput(IOEntity ioEntity, IOSlot currentSlot)
        {
            foreach (var slot in ioEntity.inputs)
            {
                if (slot.type == currentSlot.type && slot.mainPowerSlot && slot != currentSlot)
                    return true;
            }

            return false;
        }

        private bool SufficientPowerOrFluid(IOEntity destinationEntity, IOSlot destinationSlot, int inputAmount)
        {
            if (inputAmount == 0)
                return false;

            // Only electrical entities have the concept of "sufficient" power (could be wrong).
            if (destinationSlot.type != IOType.Electric)
                return true;

            if (inputAmount >= destinationEntity.ConsumptionAmount())
                return true;

            // If not providing sufficient power, only change color if there are no other main power inputs.
            // This avoids dynamically coloring a toggle input, for example, unless that input is providing exactly 0.
            return HasOtherMainInput(destinationEntity, destinationSlot);
        }

        private void ProcessConnection(IOEntity sourceEntity, IOSlot sourceSlot, IOEntity destinationEntity, IOSlot destinationSlot)
        {
            var inputAmount = sourceEntity.GetPassthroughAmount(destinationSlot.connectedToSlot);

            if (SufficientPowerOrFluid(destinationEntity, destinationSlot, inputAmount))
                ChangeSlotColor(sourceEntity, sourceSlot, destinationSlot.wireColour);
            else if (EitherEntityHasPermission(sourceEntity, destinationEntity, PermissionUse))
                ChangeSlotColor(sourceEntity, sourceSlot, _pluginConfig.GetInsufficientColorForType(sourceSlot.type));
        }

        private void ProcessSourceEntity(IOEntity sourceEntity)
        {
            foreach (var sourceSlot in sourceEntity.outputs)
            {
                var destinationEntity = sourceSlot.connectedTo.Get();
                if (destinationEntity == null)
                    continue;

                var destinationSlot = destinationEntity.inputs[sourceSlot.connectedToSlot];
                ProcessConnection(sourceEntity, sourceSlot, destinationEntity, destinationSlot);
            }
        }

        #endregion

        #region Configuration

        private class Configuration : SerializableConfiguration
        {
            [JsonProperty("InsufficientPowerColor")]
            [JsonConverter(typeof(StringEnumConverter))]
            public WireColour InsufficientPowerColor = WireColour.Red;

            [JsonProperty("InsufficientFluidColor")]
            [JsonConverter(typeof(StringEnumConverter))]
            public WireColour InsufficientFluidColor = WireColour.Red;

            [JsonProperty("RequiresPermission")]
            public bool RequiresPermission = true;

            public WireColour GetInsufficientColorForType(IOType ioType)
            {
                return ioType == IOType.Fluidic
                    ? InsufficientFluidColor
                    : InsufficientPowerColor;
            }
        }

        private Configuration GetDefaultConfig() => new Configuration();

        #endregion

        #region Configuration Boilerplate

        private class SerializableConfiguration
        {
            public string ToJson() => JsonConvert.SerializeObject(this);

            public Dictionary<string, object> ToDictionary() => JsonHelper.Deserialize(ToJson()) as Dictionary<string, object>;
        }

        private static class JsonHelper
        {
            public static object Deserialize(string json) => ToObject(JToken.Parse(json));

            private static object ToObject(JToken token)
            {
                switch (token.Type)
                {
                    case JTokenType.Object:
                        return token.Children<JProperty>()
                                    .ToDictionary(prop => prop.Name,
                                                  prop => ToObject(prop.Value));

                    case JTokenType.Array:
                        return token.Select(ToObject).ToList();

                    default:
                        return ((JValue)token).Value;
                }
            }
        }

        private bool MaybeUpdateConfig(SerializableConfiguration config)
        {
            var currentWithDefaults = config.ToDictionary();
            var currentRaw = Config.ToDictionary(x => x.Key, x => x.Value);
            return MaybeUpdateConfigDict(currentWithDefaults, currentRaw);
        }

        private bool MaybeUpdateConfigDict(Dictionary<string, object> currentWithDefaults, Dictionary<string, object> currentRaw)
        {
            bool changed = false;

            foreach (var key in currentWithDefaults.Keys)
            {
                object currentRawValue;
                if (currentRaw.TryGetValue(key, out currentRawValue))
                {
                    var defaultDictValue = currentWithDefaults[key] as Dictionary<string, object>;
                    var currentDictValue = currentRawValue as Dictionary<string, object>;

                    if (defaultDictValue != null)
                    {
                        if (currentDictValue == null)
                        {
                            currentRaw[key] = currentWithDefaults[key];
                            changed = true;
                        }
                        else if (MaybeUpdateConfigDict(defaultDictValue, currentDictValue))
                            changed = true;
                    }
                }
                else
                {
                    currentRaw[key] = currentWithDefaults[key];
                    changed = true;
                }
            }

            return changed;
        }

        protected override void LoadDefaultConfig() => _pluginConfig = GetDefaultConfig();

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _pluginConfig = Config.ReadObject<Configuration>();
                if (_pluginConfig == null)
                {
                    throw new JsonException();
                }

                if (MaybeUpdateConfig(_pluginConfig))
                {
                    LogWarning("Configuration appears to be outdated; updating and saving");
                    SaveConfig();
                }
            }
            catch (Exception e)
            {
                LogError(e.Message);
                LogWarning($"Configuration file {Name}.json is invalid; using defaults");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig()
        {
            Log($"Configuration changes saved to {Name}.json");
            Config.WriteObject(_pluginConfig, true);
        }

        #endregion
    }
}
