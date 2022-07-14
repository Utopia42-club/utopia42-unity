using System.Collections.Generic;
using Source.Model;
using Source.Service;
using Source.Service.Auth;
using Source.Ui;
using Source.Ui.LoadingLayer;
using Source.Ui.Snack;
using Source.Ui.Utils;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.MetaBlocks.TeleportBlock
{
    public class TeleportPropertiesEditor : UxmlElement
    {
        private readonly DropdownField network;
        private readonly TextField contract;
        private readonly TextField posX;
        private readonly TextField posY;
        private readonly TextField posZ;
        private readonly Dictionary<string, MetaverseNetwork> networks = new();
        private readonly Dictionary<int, string> netIdToNetKey = new();
        private int? lastSetNetworkId;

        public TeleportPropertiesEditor() : base("Ui/PropertyEditors/TeleportBlockEditor")
        {
            network = this.Q<DropdownField>("network");
            contract = this.Q<TextField>("contract");
            posX = this.Q<TextField>("x");
            posY = this.Q<TextField>("y");
            posZ = this.Q<TextField>("z");
            TextFields.RegisterUiEngagementCallbacksForTextField(contract);
            TextFields.RegisterUiEngagementCallbacksForTextField(posX);
            TextFields.RegisterUiEngagementCallbacksForTextField(posY);
            TextFields.RegisterUiEngagementCallbacksForTextField(posZ);
            network.choices.Clear();
            var loading = LoadingLayer.Show(this);
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
            if (!string.IsNullOrWhiteSpace(posX.value) && !string.IsNullOrWhiteSpace(posY.value) &&
                !string.IsNullOrWhiteSpace(posZ.value) && !string.IsNullOrWhiteSpace(contract.value)
                && !string.IsNullOrWhiteSpace(network.value))
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

        public void SetEditable(bool editable)
        {
            posX.isReadOnly = !editable;
            posY.isReadOnly = !editable;
            posZ.isReadOnly = !editable;
            contract.isReadOnly = !editable;
            network.SetEnabled(editable);
            network.style.opacity = 1;
        }

        public void SetDestinationLabel(string text)
        {
            this.Q<Label>("destinationLabel").text = text;
            var l = new List<VisualElement>(new List<VisualElement>(network.Children())[1].Children());
            Debug.Log(string.Join(", ", new List<string>(l[1].GetClasses())));
        }
        
    }
}