using System;
using UnityEngine;
using UnityEngine.UI;
using HeathenEngineering.BGSDK.API;
using HeathenEngineering.BGSDK.DataModel;
using HeathenEngineering.BGSDK.Engine; //for BG SDK Setting

using System.Collections.Generic;

public class GetNFTs : MonoBehaviour
{
    [Header("Demo Contruct")]
    [SerializeField] public Contract contract;

    [Header("Wallet Info")]
    [SerializeField] string walletAddress;
    [SerializeField] string walletId;
    [SerializeField] string pinCode;

    [Header("Secrect Info")]
    [SerializeField] SecretType chain;

    [Header("Output Console")]
    public Text console;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ListNFTs()
    {
        Debug.Log("ListNFTs");
        StartCoroutine(Server.Wallets.NFTs(walletAddress, chain, null, ListResult));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ListResult(NFTBalanceResult result)
    {
        if(result.hasError == false)
        {

            string nft_list = "";

            foreach (NFTBalanceResult.Token token in result.result)
            {
                if (token.attributes.Length > 0)
                {
                    TokenAttributes attrib = token.attributes[0];

                    string name = attrib.name;
                    string value = attrib.value;

                    nft_list += token.description + "\n";
                }
            }

            console.text = nft_list;
        }
    }
}
