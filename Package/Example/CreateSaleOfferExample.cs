using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HeathenEngineering.BGSDK.API;
using HeathenEngineering.BGSDK.DataModel;
using HeathenEngineering.BGSDK.Engine; //for BG SDK Setting


public class CreateSaleOfferExample : MonoBehaviour
{

	enum Secrect
    {
		MATIC = 0,
		ETHEREUM
	}

	[Header("Demo Contruct")]
	[SerializeField] public Contract contract;
	[SerializeField] public List<Token> tokens;

	[Header("Wallet Info")]
	[SerializeField] string walletAddress;
	[SerializeField] string walletId;
	[SerializeField] string pinCode;

	[Header("Secrect Info")]
	[SerializeField] Secrect chain;

	[Header("Output Console")]
	public Text console;

	bool isInProgress = false;





	// Start is called before the first frame update
	void Start()
    {
		isInProgress = false;
	}

    // Update is called once per frame
    void Update()
    {
        
    }
	/*
	 * Starts sale process chain starting with CreateTokenType()
	 * 
	 */
    public void InitOffer()
	{
		//if (isInProgress) return;

		int id = UnityEngine.Random.Range(0, 2);
		if (id < tokens.Count)
		{
			Token arkaneToken = tokens[id];
			arkaneToken.contract = contract;
			isInProgress = true;
			StartCoroutine(Server.Privileged.MintNonFungibleToken(arkaneToken, new string[] { this.walletAddress }, mintTokenResult));
		}
	}

	void CreateTokenResult(DefineTokenTypeResult result)
	{
		console.text = result.message;
		return;

		Debug.Log(result.message);
		if (!result.hasError)
		{
			TokenResponceData token_info = result.result;
			Token arkaneToken = ScriptableObject.CreateInstance<Token>();
			arkaneToken.name = token_info.name;
			arkaneToken.Set(token_info);
			arkaneToken.UpdatedFromServer = true;
			arkaneToken.UpdatedOn = DateTime.Now.ToBinary();
			contract.tokens.Add(arkaneToken);

			//Run validation here
			arkaneToken.contract = contract;
			StartCoroutine(Server.Privileged.MintNonFungibleToken(arkaneToken, new string[] { this.walletAddress }, mintTokenResult));
		}
	}

	void mintTokenResult(Server.Privileged.MintResult result)
	{

		if (!result.hasError)
		{
			System.Threading.Thread.Sleep(2000);
			console.text = result.message;
			StartCoroutine(Server.Market.CreateOffer(result.tokenIds[0].ToString(), contract.Address, this.walletAddress, this.chain.ToString(), CreateOfferResult));
		}
	}

	void CreateOfferResult(OfferResult result)
	{
		if (result != null && !result.hasError)
		{
			System.Threading.Thread.Sleep(2000);
			console.text = result.message;
			OfferId = result.result.id;
			StartCoroutine(Server.Market.SignOffer(this.walletId, result.result.dataToSign, pinCode, this.chain.ToString(), SignOfferResult));
		}
	}

	void SignOfferResult(OfferSignatureResult result)
	{
		if (result != null && !result.hasError)
		{
			System.Threading.Thread.Sleep(2000);
			console.text = result.message;
			StartCoroutine(Server.Market.Sign(result.result.signature, OfferId));
		}

		isInProgress = false;
	}

	string OfferId = "";
}
