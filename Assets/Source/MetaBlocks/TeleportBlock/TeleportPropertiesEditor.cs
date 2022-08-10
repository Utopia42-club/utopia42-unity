using System;
using System.Collections.Generic;
using Source.Model;
using Source.Reactive.Producer;
using Source.Service;
using Source.Service.Auth;
using Source.Ui;
using Source.Ui.LoadingLayer;
using Source.Ui.SearchField;
using Source.Ui.Snack;
using Source.UtopiaException;
using UnityEngine.UIElements;

namespace Source.MetaBlocks.TeleportBlock
{
    public class TeleportPropertiesEditor : UxmlElement
    {
        private readonly DropdownField network;
        private readonly SearchField contract;
        private readonly TextField posX;
        private readonly TextField posY;
        private readonly TextField posZ;
        private readonly Dictionary<string, NetworkData> networks = new();
        private readonly Dictionary<int, string> netIdToNetKey = new();
        private int? lastSetNetworkId;

        public TeleportPropertiesEditor() : base("Ui/PropertyEditors/TeleportBlockEditor")
        {
            network = this.Q<DropdownField>("network");
            contract = this.Q<SearchField>("contract")
                .WithDataLoader(LoadContracts);
            network.RegisterValueChangedCallback(e => NetworkChanged());
            NetworkChanged();
            posX = this.Q<TextField>("x");
            posY = this.Q<TextField>("y");
            posZ = this.Q<TextField>("z");

            network.choices.Clear();
            var loading = LoadingLayer.Show(this);
            World.INSTANCE.StartCoroutine(MultiverseService.Instance.GetAllNetworks((loadedNetworks) =>
            {
                loading.Close();
                foreach (var loadedNetwork in loadedNetworks)
                {
                    if (networks.ContainsKey(loadedNetwork.name))
                    {
                        var storedNetwork = networks[loadedNetwork.name];
                        networks.Remove(loadedNetwork.name);
                        var key = $"{storedNetwork.name} ({storedNetwork.id})";
                        networks[key] = storedNetwork;
                        netIdToNetKey[storedNetwork.id] = key;
                        key = $"{loadedNetwork.name} ({loadedNetwork.id})";
                        networks[key] = loadedNetwork;
                        netIdToNetKey[loadedNetwork.id] = key;
                    }
                    else
                    {
                        networks[loadedNetwork.name] = loadedNetwork;
                        netIdToNetKey[loadedNetwork.id] = loadedNetwork.name;
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

        private void NetworkChanged()
        {
            var net = GetNetworkId();
            if (!net.HasValue || contract.GetItem<MetaverseContract>()?.network?.id != net.Value)
            {
                contract.value = null;
                contract.SetItem(null);
            }

            contract.SetEnabled(net.HasValue);
        }

        private Observable<List<object>> LoadContracts(string f)
        {
            return new CoroutineObservable<List<object>>((n, e) =>
            {
                var net = GetNetworkId();
                if (!net.HasValue)
                    throw new IllegalStateException("Network must be selected");
                return MultiverseService.Instance.GetContracts(net.Value, 5, f,
                    r => n(new List<object>(r)), () => e(new Exception()));
            });
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
                !string.IsNullOrWhiteSpace(posZ.value) && contract.GetItem<MetaverseContract>() != null
                && !string.IsNullOrWhiteSpace(network.value))
            {
                return new TeleportBlockProperties
                {
                    destination = new[]
                    {
                        int.Parse(posX.text.Trim()), int.Parse(posY.text.Trim()),
                        int.Parse(posZ.text.Trim())
                    },
                    networkId = GetNetworkId().Value,
                    contractAddress = contract.GetItem<MetaverseContract>().id.ToLower()
                };
            }

            return null;
        }

        private int? GetNetworkId()
        {
            if (string.IsNullOrEmpty(network.value))
                return null;
            if (networks.TryGetValue(network.value, out var net))
                return net.id;
            return null;
        }

        public void SetValue(TeleportBlockProperties value)
        {
            if (value == null)
            {
                TrySetNetwork(AuthService.Instance.CurrentContract.network.id);
                contract.SetItem(AuthService.Instance.CurrentContract);
                return;
            }

            if (value.destination != null)
            {
                posX.value = value.destination[0].ToString();
                posY.value = value.destination[1].ToString();
                posZ.value = value.destination[2].ToString();
                if (value.contractAddress != null)
                {
                    var loading = LoadingLayer.Show(this);
                    Observables.FromCoroutine<MetaverseContract>((n, e) => MultiverseService.Instance.GetContract(
                            value.networkId, value.contractAddress, n,
                            () => e(new Exception())))
                        .Subscribe(n =>
                        {
                            contract.SetItem(n);
                            loading.Close();
                        }, e => loading.Close(), loading.Close);
                }

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
        }
    }
}