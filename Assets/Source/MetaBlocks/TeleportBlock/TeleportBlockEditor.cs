using System;
using System.Collections.Generic;
using Source.Model;
using Source.Service;
using Source.Service.Auth;
using Source.Ui.LoadingLayer;
using Source.Ui.Snack;
using Source.Ui.Utils;
using UnityEngine.UIElements;

namespace Source.MetaBlocks.TeleportBlock
{
    public class TeleportBlockEditor
    {
        private readonly DropdownField network;
        private readonly TextField contract;
        private readonly TextField posX;
        private readonly TextField posY;
        private readonly TextField posZ;
        private readonly Dictionary<string, MetaverseNetwork> networks = new();
        private readonly Dictionary<int, string> netIdToNetKey = new();
        private int? lastSetNetworkId;

        public TeleportBlockEditor(Action<TeleportBlockProperties> onSave, int instanceID)
        {
            var root = PropertyEditor.INSTANCE.Setup("Ui/PropertyEditors/TeleportBlockEditor",
                "Teleport Block Properties", () =>
                {
                    onSave(GetValue());
                    PropertyEditor.INSTANCE.Hide();
                }, instanceID);
            network = root.Q<DropdownField>("network");
            contract = root.Q<TextField>("contract");
            posX = root.Q<TextField>("x");
            posY = root.Q<TextField>("y");
            posZ = root.Q<TextField>("z");
            TextFields.RegisterUiEngagementCallbacksForTextField(contract);
            TextFields.RegisterUiEngagementCallbacksForTextField(posX);
            TextFields.RegisterUiEngagementCallbacksForTextField(posY);
            TextFields.RegisterUiEngagementCallbacksForTextField(posZ);
            network.choices.Clear();
            var loading = LoadingLayer.Show(root);
            World.INSTANCE.StartCoroutine(MultiverseService.Instance.GetAllNetworks((nets) =>
            {
                loading.Close();
                foreach (var network in nets)
                {
                    if (networks.ContainsKey(network.networkName))
                    {
                        var net = networks[network.networkName];
                        networks.Remove(network.networkName);
                        var key = $"{net.networkName} ({net.networkId})";
                        networks[key] = net;
                        netIdToNetKey[net.networkId] = key;
                        key = $"{network.networkName} ({network.networkId})";
                        networks[key] = network;
                        netIdToNetKey[network.networkId] = key;
                    }
                    else
                    {
                        networks[network.networkName] = network;
                        netIdToNetKey[network.networkId] = network.networkName;
                    }
                }

                network.choices.AddRange(networks.Keys);
                if (lastSetNetworkId.HasValue)
                    TrySetNetwork(lastSetNetworkId.Value);
            }, () =>
            {
                loading.Close();
                new Toast("Failed to load available networks", Toast.ToastType.Error).Show();
            }));
        }

        private void TrySetNetwork(int id)
        {
            if (netIdToNetKey.TryGetValue(id, out var n))
                network.value = n;
            lastSetNetworkId = id;
        }

        public TeleportBlockProperties GetValue()
        {
            if (HasValue(posX) && HasValue(posY) && HasValue(posZ)
                && HasValue(contract) && !string.IsNullOrWhiteSpace(network.value))
            {
                return new TeleportBlockProperties
                {
                    destination = new[]
                    {
                        int.Parse(posX.text.Trim()), int.Parse(posY.text.Trim()),
                        int.Parse(posZ.text.Trim())
                    },
                    networkId = networks[network.value].networkId,
                    contractAddress = contract.value.Trim().ToLower()
                };
            }

            return null;
        }

        public void SetValue(TeleportBlockProperties value)
        {
            if (value == null)
            {
                TrySetNetwork(AuthService.Instance.CurrentContract.networkId);
                contract.value = AuthService.Instance.CurrentContract.address;
                return;
            }

            if (value.destination != null)
            {
                posX.value = value.destination[0].ToString();
                posY.value = value.destination[1].ToString();
                posZ.value = value.destination[2].ToString();
                contract.value = value.contractAddress;
                TrySetNetwork(value.networkId);
            }
        }

        public void Show()
        {
            PropertyEditor.INSTANCE.Show();
        }

        private bool HasValue(TextField f)
        {
            return !string.IsNullOrWhiteSpace(f.text);
        }
    }
}