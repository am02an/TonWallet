
using UnityEngine;
using UnityEngine.UI;
using System;
using UnitonConnect.Core;
using UnitonConnect.Core.Data;

public sealed class ConnectWalletExample : MonoBehaviour
{
    [SerializeField, Space] private Button _connectButton;

    private UnitonConnectSDK _unitonConnect;

    private void OnDisable()
    {
        _connectButton.onClick.RemoveListener(Connect);

        _unitonConnect.OnInitiliazed -= SdkInitialized;

        _unitonConnect.OnWalletConnected -= WalletConnectionFinished;
        _unitonConnect.OnWalletConnectFailed -= WalletConnectionFailed;
    }

    private void Start()
    {
        _unitonConnect = UnitonConnectSDK.Instance;
        Debug.Log("_unitonConnect" + " " + _unitonConnect == null);
        _connectButton.interactable = false;
        _connectButton.onClick.AddListener(Connect);

        _unitonConnect.OnInitiliazed += SdkInitialized;

        _unitonConnect.OnWalletConnected += WalletConnectionFinished;
        _unitonConnect.OnWalletConnectFailed += WalletConnectionFailed;
    }

    private void Connect()
    {
        if (!_unitonConnect.IsInitialized)
        {
            Debug.LogWarning("Sdk is not initialized, connection canceled");

            return;
        }

        _unitonConnect.Connect();
    }

    private void SdkInitialized(bool isSuccess)
    {
        _connectButton.interactable = isSuccess;
    }

    private void WalletConnectionFinished(WalletConfig wallet)
    {
        var userAddress = wallet.Address;

        var successConnectMessage = $"Wallet is connected, " +
            $"Address: {userAddress}, Public Key: {wallet.PublicKey}";

        var shortAddress = _unitonConnect.Wallet.ToShort(6);

        Debug.Log(successConnectMessage);
        Debug.Log($"Parsed short address: {shortAddress}");
    }

    private void WalletConnectionFailed(string errorMessage)
    {
        Debug.LogError("Failed to connect wallet, reason: {errorMessage}");
    }
}