using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenSeaApi : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class AssetContract
{
    public string address { get; set; }
    public string asset_contract_type { get; set; }
    public DateTime created_date { get; set; }
    public string name { get; set; }
    public object nft_version { get; set; }
    public string opensea_version { get; set; }
    public int owner { get; set; }
    public string schema_name { get; set; }
    public string symbol { get; set; }
    public object total_supply { get; set; }
    public object description { get; set; }
    public object external_link { get; set; }
    public object image_url { get; set; }
    public bool default_to_fiat { get; set; }
    public int dev_buyer_fee_basis_points { get; set; }
    public int dev_seller_fee_basis_points { get; set; }
    public bool only_proxied_transfers { get; set; }
    public int opensea_buyer_fee_basis_points { get; set; }
    public int opensea_seller_fee_basis_points { get; set; }
    public int buyer_fee_basis_points { get; set; }
    public int seller_fee_basis_points { get; set; }
    public object payout_address { get; set; }
}

[System.Serializable]
public class PaymentToken
{
    public int id { get; set; }
    public string symbol { get; set; }
    public string address { get; set; }
    public string image_url { get; set; }
    public string name { get; set; }
    public int decimals { get; set; }
    public double? eth_price { get; set; }
    public double? usd_price { get; set; }
}

[System.Serializable]
public class Traits
{
    public string trait_type { get; set; }
    public string value { get; set; }
    public object display_type { get; set; }
    public object max_value { get; set; }
    public int trait_count { get; set; }
    public object order { get; set; }
}

[System.Serializable]
public class Stats
{
    public double one_day_volume { get; set; }
    public double one_day_change { get; set; }
    public double one_day_sales { get; set; }
    public double one_day_average_price { get; set; }
    public double seven_day_volume { get; set; }
    public double seven_day_change { get; set; }
    public double seven_day_sales { get; set; }
    public double seven_day_average_price { get; set; }
    public double thirty_day_volume { get; set; }
    public double thirty_day_change { get; set; }
    public double thirty_day_sales { get; set; }
    public double thirty_day_average_price { get; set; }
    public double total_volume { get; set; }
    public double total_sales { get; set; }
    public double total_supply { get; set; }
    public double count { get; set; }
    public int num_owners { get; set; }
    public double average_price { get; set; }
    public int num_reports { get; set; }
    public double market_cap { get; set; }
    public int floor_price { get; set; }
}

[System.Serializable]
public class DisplayData
{
    public string card_display_style { get; set; }
}

[System.Serializable]
public class Collection
{
    public List<PaymentToken> payment_tokens { get; set; }
    public List<object> primary_asset_contracts { get; set; }
    public Traits traits { get; set; }
    public Stats stats { get; set; }
    public string banner_image_url { get; set; }
    public object chat_url { get; set; }
    public DateTime created_date { get; set; }
    public bool default_to_fiat { get; set; }
    public string description { get; set; }
    public string dev_buyer_fee_basis_points { get; set; }
    public string dev_seller_fee_basis_points { get; set; }
    public object discord_url { get; set; }
    public DisplayData display_data { get; set; }
    public object external_url { get; set; }
    public bool featured { get; set; }
    public string featured_image_url { get; set; }
    public bool hidden { get; set; }
    public string safelist_request_status { get; set; }
    public string image_url { get; set; }
    public bool is_subject_to_whitelist { get; set; }
    public string large_image_url { get; set; }
    public object medium_username { get; set; }
    public string name { get; set; }
    public bool only_proxied_transfers { get; set; }
    public string opensea_buyer_fee_basis_points { get; set; }
    public string opensea_seller_fee_basis_points { get; set; }
    public string payout_address { get; set; }
    public bool require_email { get; set; }
    public object short_description { get; set; }
    public string slug { get; set; }
    public object telegram_url { get; set; }
    public object twitter_username { get; set; }
    public object instagram_username { get; set; }
    public object wiki_url { get; set; }
    public bool is_nsfw { get; set; }
}

[System.Serializable]
public class User
{
    public string username { get; set; }
}

[System.Serializable]
public class Owner
{
    public User user { get; set; }
    public string profile_img_url { get; set; }
    public string address { get; set; }
    public string config { get; set; }
}

[System.Serializable]
public class Creator
{
    public User user { get; set; }
    public string profile_img_url { get; set; }
    public string address { get; set; }
    public string config { get; set; }
}

[System.Serializable]
public class TopOwnership
{
    public Owner owner { get; set; }
    public string quantity { get; set; }
}

[System.Serializable]
public class Root
{
    public int id { get; set; }
    public int num_sales { get; set; }
    public object background_color { get; set; }
    public string image_url { get; set; }
    public string image_preview_url { get; set; }
    public string image_thumbnail_url { get; set; }
    public object image_original_url { get; set; }
    public object animation_url { get; set; }
    public object animation_original_url { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public object external_link { get; set; }
    public AssetContract asset_contract { get; set; }
    public string permalink { get; set; }
    public Collection collection { get; set; }
    public object decimals { get; set; }
    public object token_metadata { get; set; }
    public bool is_nsfw { get; set; }
    public Owner owner { get; set; }
    public object sell_orders { get; set; }
    public Creator creator { get; set; }
    public List<Traits> traits { get; set; }
    public object last_sale { get; set; }
    public object top_bid { get; set; }
    public object listing_date { get; set; }
    public bool is_presale { get; set; }
    public object transfer_fee_payment_token { get; set; }
    public object transfer_fee { get; set; }
    public List<object> related_assets { get; set; }
    public object orders { get; set; }
    public List<object> auctions { get; set; }
    public bool supports_wyvern { get; set; }
    public List<TopOwnership> top_ownerships { get; set; }
    public object ownership { get; set; }
    public object highest_buyer_commitment { get; set; }
    public string token_id { get; set; }
}
