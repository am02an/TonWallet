using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Insatnce;
    public const string playerDataJson = @"
    {
      ""users"": [
        {
          ""id"": null,
          ""telegram_username"": null,
          ""payout_wallet"": null,
          ""airdrop_total_ton"": null,
          ""reward_total_ton"": null
        }
      ],

      ""games_paid"": [
        {
          ""id"": null,
          ""room_id"": null,
          ""risk_tier"": null,
          ""pool_address"": null,
          ""tx_hash"": null,
          ""entry_fee_ton"": null,
          ""created_at"": null
        }
      ],

      ""airdrop_log"": [
        {
          ""id"": null,
          ""user_id"": null,
          ""payout_wallet"": null,
          ""tx_hash"": null,
          ""amount_ton"": null,
          ""datetime"": null,
          ""mode"": null
        }
      ],

      ""reward_log"": [
        {
          ""id"": null,
          ""user_id"": null,
          ""payout_wallet"": null,
          ""tx_hash"": null,
          ""game_id"": null,
          ""amount_ton"": null,
          ""datetime"": null
        }
      ],

      ""pay_fee_log"": [
        {
          ""id"": null,
          ""user_id"": null,
          ""game_id"": null,
          ""amount_ton"": null,
          ""wallet_address"": null,
          ""tx_hash"": null,
          ""remaining_attempts"": null,
          ""datetime"": null
        }
      ],

      ""free_play_log"": [
        {
          ""id"": null,
          ""user_id"": null,
          ""score"": null,
          ""datetime"": null
        }
      ],

      ""paid_play_log"": [
        {
          ""id"": null,
          ""user_id"": null,
          ""game_id"": null,
          ""score"": null,
          ""datetime"": null
        }
      ],

      ""global_setting"": {
        ""airdrop_day_range"": null,
        ""reward_range_hours"": null,
        ""attempts"": null,
        ""entry_fee_ton"": null,
        ""airdrop_amount_ton"": null
      }
    }";
    public void Awake()
    {
        Insatnce = this;
    }
    public string GetPlayerdata()
    {
        return playerDataJson;
    }
}
