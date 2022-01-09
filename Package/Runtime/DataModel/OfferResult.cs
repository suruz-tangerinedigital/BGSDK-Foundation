using System;

namespace HeathenEngineering.BGSDK.DataModel
{


    #region offer_body
    [Serializable]
    public class NFT
    {
        public string tokenId;
        public string address;
        public string chain;

        /*public NFT(string tokenId, string address, SecretType chain)
        {
            switch (chain)
            {
                case SecretType.AVAC:
                    this.chain = "AVAC";
                    break;
                case SecretType.BSC:
                    this.chain = "BSC";
                    break;
                case SecretType.ETHEREUM:
                    this.chain = "ETHEREUM";
                    break;
                case SecretType.MATIC:
                    this.chain = "MATIC";
                    break;
            }

            this.tokenId = tokenId;
            this.address = address;

        }*/
    }
  


    [Serializable]
    public class OfferBody
    {
        public  string type = "SALE";
        public  NFT nft;//= new NFT("", "", SecretType.MATIC);
        public  string sellerAddress = "0xdb7c22EA49EF93F753F2ed4c9E1A2589aC6E7690";
        public  string price;
    }
    #endregion


    #region offer_result

    [Serializable]
    public class NFTOfferd : NFT
    {
       public ContractData contract;
    }


    [Serializable]
    public class NFTDefination : NFTOfferd
    {

    }

    public class OfferResult : BGSDKBaseResult
    {
        public string id;
        NFTDefination nft;
    }

    #endregion
}
