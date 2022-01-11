#if UNITY_SERVER || UNITY_EDITOR || UNITY_ANDROID
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using HeathenEngineering.BGSDK.DataModel;
using HeathenEngineering.BGSDK.Engine;
using System.Collections.Generic;
using System.Text;

namespace HeathenEngineering.BGSDK.API
{
    public static class Server
    {

        public static string Info;

        /// <summary>
        /// A wrapper around BGSDK's Token Management API
        /// </summary>
        /// <remarks>
        /// <para>
        /// For more detrails see <see href="https://docs.venly.io/pages/token-management.html#_token_management">https://docs.venly.io/pages/token-management.html#_token_management</see>.
        /// All functions of this class and child classes are designed to be used with Unity's StartCoroutine method.
        /// All funcitons of this class will take an Action as the final paramiter which is called when the process completes.
        /// Actions can be defined as a funciton in the calling script or can be passed as an expression.
        /// </para>
        /// <code>
        /// StartCoroutine(API.Tokens.GetContract(Identity, contract, HandleGetContractResults));
        /// </code>
        /// <para>
        /// or
        /// </para>
        /// <code>
        /// StartCoroutine(API.Tokens.GetContract(Identity, contract, (resultObject) => 
        /// {
        ///     //TODO: handle the resultObject
        /// }));
        /// </code>
        /// <para>
        /// Additional code samples can be found in the Samples provided with the package.
        /// </para>
        /// </remarks>
        public static class Tokens
        {
            /// <summary>
            /// Fetch details about a specific contract for the current app Id
            /// </summary>
            /// <param name="contract">The contract to get</param>
            /// <param name="callback">The method to call back into with the results.</param>
            /// <returns>The Unity routine enumerator</returns>
            /// <remarks>
            /// <para>
            /// For more information please see <see href="https://docs.venly.io/pages/token-management.html#_get_contract">https://docs.venly.io/pages/token-management.html</see>
            /// </para>
            /// </remarks>
            /// <example>
            /// <para>
            /// How to call:
            /// </para>
            /// <code>
            /// StartCoroutine(API.Tokens.GetContract(Identity, contract, HandleGetContractResults));
            /// </code>
            /// </example>
            public static IEnumerator GetContract(Engine.Contract contract, Action<ContractResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new ContractResult() { hasError = true, message = "Attempted to call BGSDK.Tokens.GetContract with no BGSDK.Settings object applied.", result = null });
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        callback(new ContractResult() { hasError = true, message = "BGSDKIdentity required, null identity provided.\nPlease initalize the Settings.user variable before calling GetContract", result = null });
                        yield return null;
                    }
                    else
                    {
                        UnityWebRequest www = UnityWebRequest.Get(BGSDKSettings.current.ContractUri + "/" + contract.Id);
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            string resultContent = www.downloadHandler.text;
                            var results = new ContractResult();
                            results.result = JsonUtility.FromJson<ListContractsResult.RecieveContractModel>(resultContent)?.ToContractData();
                            results.message = "Get Contract complete.";
                            results.httpCode = www.responseCode;
                            callback(results);
                        }
                        else
                        {
                            callback(new ContractResult() { hasError = true, message = "Error:" + (www.isNetworkError ? " a network error occured while requesting the contract." : " a HTTP error occured while requesting the contract."), result = null, httpCode = www.responseCode });
                        }
                    }
                }
            }

            /// <summary>
            /// <para>Returns the list of available token types for the indicated contract</para>
            /// <see href="https://docs.venly.io/pages/token-management.html">https://docs.venly.io/pages/token-management.html</see>
            /// </summary>
            /// <param name="contract">The contract to get</param>
            /// <param name="callback">The method to call back into with the results.</param>
            /// <returns>The Unity routine enumerator</returns>
            /// <example>
            /// <para>
            /// How to call:
            /// </para>
            /// <code>
            /// StartCoroutine(API.Tokens.ListTokenTypes(Identity, contract, HandleListTokenTypeResults));
            /// </code>
            /// </example>
            public static IEnumerator ListTokenTypes(Contract contract, Action<ListTokenTypesResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new ListTokenTypesResult() { hasError = true, message = "Attempted to call BGSDK.Tokens.ListTokenTypes with no BGSDK.Settings object applied.", result = null });
                    yield return null;
                }
                else if (BGSDKSettings.user == null)
                {
                    callback(new ListTokenTypesResult() { hasError = true, message = "BGSDKIdentity required, null identity provided.\nPlease initalize the Settings.user variable before calling ListTokenTypes", result = null });
                    yield return null;
                }
                else
                {
                    UnityWebRequest www = UnityWebRequest.Get(BGSDKSettings.current.GetTokenUri(contract));
                    www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);

                    var co = www.SendWebRequest();
                    while (!co.isDone)
                        yield return null;

                    if (!www.isNetworkError && !www.isHttpError)
                    {
                        string resultContent = www.downloadHandler.text;
                        var results = new ListTokenTypesResult();
                        results = JsonUtility.FromJson<ListTokenTypesResult>(Utilities.JSONArrayWrapper(resultContent));
                        results.message = "List Token Types complete.";
                        results.httpCode = www.responseCode;
                        callback(results);
                    }
                    else
                    {
                        callback(new ListTokenTypesResult() { hasError = true, message = "Error:" + (www.isNetworkError ? " a network error occured while requesting the list of available token types." : " a HTTP error occured while requesting the list of availabel token types."), result = null, httpCode = www.responseCode });
                    }
                }
            }

            /// <summary>
            /// <para>Returns the definition of the indicated token</para>
            /// <see href="https://docs.venly.io/pages/token-management.html">https://docs.venly.io/pages/token-management.html</see>
            /// </summary>
            /// <param name="contract">The contract to get</param>
            /// <param name="tokenId">The id of the token to fetch</param>
            /// <param name="callback">The method to call back into with the results.</param>
            /// <returns>The Unity routine enumerator</returns>
            /// <example>
            /// <para>
            /// How to call:
            /// </para>
            /// <code>
            /// StartCoroutine(API.Tokens.GetTokenType(Identity, contract, tokenId, HandleGetTokenTypeResults));
            /// </code>
            /// </example>
            public static IEnumerator GetTokenType(Contract contract, string tokenId, Action<DataModel.DefineTokenTypeResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new DataModel.DefineTokenTypeResult() { hasError = true, message = "Attempted to call BGSDK.Tokens.GetTokenType with no BGSDK.Settings object applied.", result = null });
                    yield return null;
                }
                else if (BGSDKSettings.user == null)
                {
                    callback(new DefineTokenTypeResult() { hasError = true, message = "BGSDKIdentity required, null identity provided.\nPlease initalize the Settings.user variable before calling GetTokenType", result = null });
                    yield return null;
                }
                else
                {
                    UnityWebRequest www = UnityWebRequest.Get(BGSDKSettings.current.GetTokenUri(contract) + "/" + tokenId);
                    www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);

                    var co = www.SendWebRequest();
                    while (!co.isDone)
                        yield return null;

                    if (!www.isNetworkError && !www.isHttpError)
                    {
                        string resultContent = www.downloadHandler.text;
                        var results = new DataModel.DefineTokenTypeResult();
                        results.result = JsonUtility.FromJson<DataModel.TokenResponceData>(resultContent);
                        results.message = "List Token Types complete.";
                        results.httpCode = www.responseCode;
                        callback(results);
                    }
                    else
                    {
                        callback(new DataModel.DefineTokenTypeResult() { hasError = true, message = "Error:" + (www.isNetworkError ? " a network error occured while requesting the definition of token " + tokenId + "." : " a HTTP error occured while requesting the definition of token " + tokenId + "."), result = null, httpCode = www.responseCode });
                    }
                }
            }


            /*public static IEnumerator ListClients(Action<ListWalletResult> callback)
            {

                if (BGSDKSettings.current == null)
                {
                    Debug.Log("Attempted to call BGSDK.Wallets.List with no BGSDK.Settings object applied.");
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        Debug.Log("BGSDKSettings.user required, null Settings.user provided.");
                        yield return null;
                    }
                    else
                    {
                        Debug.Log(BGSDKSettings.current.ClientsUri);
                        UnityWebRequest www = UnityWebRequest.Get(BGSDKSettings.current.ClientsUri);
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);
                        Debug.Log("Response code: " + www.result);
                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;


                        Debug.Log("Response code: " + www.downloadHandler.text);
                        Debug.Log("Result: " + www.responseCode);
                        //INFO = www.downloadHandler.text;

                    }
                }

            }*/


            public static IEnumerator CreateTokenType(Contract contract, Action<DefineTokenTypeResult> callback)
            {

                if (BGSDKSettings.current == null)
                {
                    callback(new DefineTokenTypeResult() { hasError = true, message = "Attempted to call BGSDK.Tokens.CreateTokenType with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {

                    if (BGSDKSettings.user == null)
                    {
                        callback(new DefineTokenTypeResult() { hasError = true, message = "BGSDKSettings.user required, null Settings.user provided.\n Please initalize the Settings.user before calling CreateTokenType" });
                        yield return null;
                    }
                    else
                    {
                        TokenDefinition farm_type = new TokenDefinition
                        {
                            name = "Hammer",
                            description = "Hammer v-" + DateTime.Now.Millisecond,
                            fungible = false,
                            burnable = true,
                            externalUrl = "https://static.wikia.nocookie.net/parody/images/4/42/74915084_10162764640400387_6139958579186106368_o.jpg",
                            image = "https://static.wikia.nocookie.net/parody/images/4/42/74915084_10162764640400387_6139958579186106368_o.jpg",
                            currentSupply = 0,
                            maxSupply = 10000,

                            attributes = new TokenAttributes[] {
                                new TokenAttributes(){
                                type = "property",
                                name = "Farm Power",
                                value = UnityEngine.Random.Range(10, 100).ToString(),
                                maxValue = 100
                            },

                            new TokenAttributes(){
                                type = "property",
                                name = "Level",
                                value = UnityEngine.Random.Range(0, 100).ToString(),
                                maxValue = 100
                            },
                            new TokenAttributes(){
                                type = "property",
                                name = "Price",
                                value = UnityEngine.Random.Range(1, 10).ToString(),
                                maxValue = 0
                            },

                            }
                        };

                        TokenDefinition weapon_type = new TokenDefinition
                        {
                            name = "Axe ",
                            description = "Axe v-" + DateTime.Now.Millisecond,
                            fungible = false,
                            burnable = true,
                            externalUrl = "https://static.wikia.nocookie.net/parody/images/4/42/74915084_10162764640400387_6139958579186106368_o.jpg",
                            image = "https://static.wikia.nocookie.net/parody/images/4/42/74915084_10162764640400387_6139958579186106368_o.jpg",
                            currentSupply = 0,
                            maxSupply = 10000,
                            attributes = new TokenAttributes[] {
                                new TokenAttributes(){
                                type = "property",
                                name = "Attack Damage",
                                value = UnityEngine.Random.Range(10, 100).ToString(),
                                maxValue = 100
                            },
                            new TokenAttributes(){
                                type = "property",
                                name = "Defense",
                                value = UnityEngine.Random.Range(10, 100).ToString(),
                                maxValue = 100
                            },
                            new TokenAttributes(){
                                type = "property",
                                name = "Level",
                                value = UnityEngine.Random.Range(0, 100).ToString(),
                                maxValue = 100
                            },
                            new TokenAttributes(){
                                type = "property",
                                name = "Price",
                                value = UnityEngine.Random.Range(1, 10).ToString(),
                                maxValue = 0
                            }

                            }
                           
                        };

                        
                        //defination builder
                        TokenDefinition[] random_create_list = new TokenDefinition[] { farm_type, weapon_type, farm_type, weapon_type };
                        TokenDefinition tkndef = random_create_list[UnityEngine.Random.Range(0, 3)];

                        //now use this def to body
                        //TokenProperties<string> str_prop = new TokenProperties<string>();

                        string jsonString = tkndef.ToJson();  // defination interface


                        UnityWebRequest www = UnityWebRequest.Put(BGSDKSettings.current.DefineTokenTypeUri(contract), jsonString);
                        www.method = UnityWebRequest.kHttpVerbPOST;
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);
                        www.uploadHandler.contentType = "application/json;charset=UTF-8";
                        www.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {

                            //WebResults<TokenResponceData> results = new WebResults<TokenResponceData>(www);
                            var results = new DefineTokenTypeResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                TokenDefinition def = JsonUtility.FromJson<TokenDefinition>(resultContent);
                                results.result = (TokenResponceData)JsonUtility.FromJson<TokenResponceData>(resultContent);
                                results.result.contractAddress = contract.data.address;
                                results.result.contractTypeId = new System.Numerics.BigInteger(int.Parse(contract.data.id));// ;

                                TokenDefinition result_def = results.result as TokenDefinition;

                                if(result_def != null)
                                {
                                    result_def = def;
                                }
                                results.message = "Create Token type complete.";
                                results.success = true;
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);
                                //Info = resultContent;
                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }

                        }

                    }


                }

            }

            public class MinTokenBody
            {
                public string typeId;
                public string[] destinations = new string[] { "0xe3A801b655dCe11CCfD5EF7aF9EC4EEAB62d3A04" };
            }

            public static IEnumerator MinTokenType(Contract contract, string typeid,  Action<TokenDefinitionResult> callback)
            {

                if (BGSDKSettings.current == null)
                {
                    callback(new TokenDefinitionResult() { hasError = true, message = "Attempted to call BGSDK.Tokens.CreateTokenType with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {

                    if (BGSDKSettings.user == null)
                    {
                        callback(new TokenDefinitionResult() { hasError = true, message = "BGSDKSettings.user required, null Settings.user provided.\n Please initalize the Settings.user before calling CreateTokenType" });
                        yield return null;
                    }
                    else
                    {
   
                        //now use this def to body
                        var jsonString = JsonUtility.ToJson(new MinTokenBody() {typeId = typeid });

                        UnityWebRequest www = UnityWebRequest.Put(BGSDKSettings.current.MintTokenUri(contract) + "/non-fungible/", jsonString);
                        www.method = UnityWebRequest.kHttpVerbPOST;
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);
                        www.uploadHandler.contentType = "application/json;charset=UTF-8";
                        www.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            var results = new TokenDefinitionResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                results = JsonUtility.FromJson<TokenDefinitionResult>(resultContent);
                                results.message = "Create Token type complete.";
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);
                                //Info = resultContent;
                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }

                        }

                    }


                }

            }




            /// <summary>
            /// Returns tokens for given token type
            /// </summary>
            /// <param name="token">The token type to query</param>
            /// <param name="callback">The callback to invoke with the results</param>
            /// <returns></returns>
            public static IEnumerator GetTokens(Token token, Action<Token.ResultList> callback) => token.Get(callback);
        }

        /// <summary>
        /// Wraps the BGSDK interface for wallets incuding User, App and Whitelable wallets.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Wallet funcitonality is discribed in the <see href="https://docs.venly.io/pages/reference.html">https://docs.venly.io/pages/reference.html</see> documentation.
        /// All functions of this class and child classes are designed to be used with Unity's StartCoroutine method.
        /// All funcitons of this class will take an Action as the final paramiter which is called when the process completes.
        /// Actions can be defined as a funciton in the calling script or can be passed as an expression.
        /// </para>
        /// <code>
        /// StartCoroutine(API.Wallets.Get(Settings.user, walletId, HandleResults));
        /// </code>
        /// <para>
        /// or
        /// </para>
        /// <code>
        /// StartCoroutine(API.Wallets.Get(Settings.user, walletId, (resultObject) => 
        /// {
        ///     //TODO: handle the resultObject
        /// }));
        /// </code>
        /// <para>
        /// Additional code samples can be found in the Samples provided with the package.
        /// </para>
        /// </remarks>
        public static class Wallets
        {
            [HideInInspector]
            public class CreateWalletModel
            {
                public string walletType;
                public string secretType;
                public string identifier;
                public string pincode;
            }

            public enum Type
            {
                WHITE_LABEL,
                UNRECOVERABLE_WHITE_LABEL
            }

            /// <summary>
            /// Create a new white label style wallet for the user
            /// </summary>
            /// <remarks>
            /// See <see href="https://docs.venly.io/api/api-products/wallet-api/create-wallet"/> for more details
            /// </remarks>
            /// <param name="pincode">[Required] The pin that will encrypt and decrypt the wallet</param>
            /// <param name="identifier">[Optional] An identifier that can be used to query or group wallets</param>
            /// <param name="description">[Optional] A description to describe the wallet.</param>
            /// <param name="chain">The blockchain on which to create the wallet</param>
            /// <param name="type">Define if the wallet is recoverable or unrecoverable</param>
            /// <param name="callback"></param>
            /// <returns></returns>
            public static IEnumerator Create(string pincode, string identifier, SecretType chain, Action<ListWalletResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new ListWalletResult() { hasError = true, message = "Attempted to call BGSDK.Wallets.CreateWhitelabelWallet with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {

                    if (BGSDKSettings.user == null)
                    {
                        callback(new ListWalletResult() { hasError = true, message = "BGSDKSettings.user required, null Settings.user provided.\n Please initalize the Settings.user before calling CreateWhitelableWallet", result = null });
                        yield return null;
                    }
                    else
                    {
                        var walletModel = new CreateWalletModel
                        {
                            walletType = "WHITE_LABEL",
                            identifier = identifier,
                            pincode = pincode,
                        };

                        switch (chain)
                        {
                            case SecretType.AVAC:
                                walletModel.secretType = "AVAC";
                                break;
                            case SecretType.BSC:
                                walletModel.secretType = "BSC";
                                break;
                            case SecretType.ETHEREUM:
                                walletModel.secretType = "ETHEREUM";
                                break;
                            case SecretType.MATIC:
                                walletModel.secretType = "MATIC";
                                break;
                        }

                        var jsonString = JsonUtility.ToJson(walletModel);

                        UnityWebRequest www = UnityWebRequest.Put(BGSDKSettings.current.WalletUri, jsonString);
                        www.method = UnityWebRequest.kHttpVerbPOST;
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);
                        www.uploadHandler.contentType = "application/json;charset=UTF-8";
                        www.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            var results = new ListWalletResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                results.result = new System.Collections.Generic.List<Wallet>();
                                results.result.Add(JsonUtility.FromJson<Wallet>(Utilities.JSONArrayWrapper(resultContent)));
                                results.message = "Create wallet complete.";
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);
                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }
                        }
                        else
                        {
                            callback(new ListWalletResult() { hasError = true, message = "Error:" + (www.isNetworkError ? " a network error occured while attempting creat wallet." : " a HTTP error occured while attempting to creat wallet."), result = null, httpCode = www.responseCode });
                        }
                    }
                }
            }

            /// <summary>
            /// Gets the user wallets available to the authorized Settings.user
            /// </summary>
            /// <param name="callback">A method pointer to handle the results of the query</param>
            /// <returns>The Unity routine enumerator</returns>
            /// <remarks>
            /// <see href="https://docs.venly.io/pages/reference.html#_list_wallets_arkane_api">https://docs.venly.io/pages/reference.html#_list_wallets_arkane_api</see>
            /// </remarks>
            public static IEnumerator List(Action<ListWalletResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new ListWalletResult() { hasError = true, message = "Attempted to call BGSDK.Wallets.List with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        callback(new ListWalletResult() { hasError = true, message = "BGSDKSettings.user required, null Settings.user provided.", result = null });
                        yield return null;
                    }
                    else
                    {
                        UnityWebRequest www = UnityWebRequest.Get(BGSDKSettings.current.WalletUri);
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            var results = new ListWalletResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                results = JsonUtility.FromJson<ListWalletResult>(Utilities.JSONArrayWrapper(resultContent));
                                results.message = "Wallet refresh complete.";
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);

                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }
                        }
                        else
                        {
                            callback(new ListWalletResult() { hasError = true, message = "Error:" + (www.isNetworkError ? " a network error occured while requesting the user's wallets." : " a HTTP error occured while requesting the user's wallets."), result = null, httpCode = www.responseCode });
                        }
                    }
                }
            }

            /// <summary>
            /// Gets a user wallet as available to the authorized Settings.user
            /// </summary>
            /// <param name="Settings.user">The Settings.user to query for</param>
            /// <param name="callback">A method pointer to handle the results of the query</param>
            /// <returns>The Unity routine enumerator</returns>
            /// <remarks>
            /// <see href="https://docs.venly.io/pages/reference.html#get-specific-user-wallet">https://docs.venly.io/pages/reference.html#get-specific-user-wallet</see>
            /// </remarks>
            public static IEnumerator Get(string walletAddress, Action<ListWalletResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new ListWalletResult() { hasError = true, message = "Attempted to call BGSDK.Wallets.UserWallet.Get with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        callback(new ListWalletResult() { hasError = true, message = "BGSDKSettings.user required, null Settings.user provided.", result = null });
                        yield return null;
                    }
                    else
                    {
                        UnityWebRequest www = UnityWebRequest.Get(BGSDKSettings.current.WalletUri + "/" + walletAddress);
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            var results = new ListWalletResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                results.result = new System.Collections.Generic.List<Wallet>();
                                results.result.Add(JsonUtility.FromJson<Wallet>(resultContent));
                                results.message = "Wallet refresh complete.";
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);
                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }
                        }
                        else
                        {
                            callback(new ListWalletResult() { hasError = true, message = "Error:" + (www.isNetworkError ? " a network error occured while requesting a user's wallet." : " a HTTP error occured while requesting a user's wallet."), result = null, httpCode = www.responseCode });
                        }
                    }
                }
            }

            /// <summary>
            /// Endpoint that allows updating the details of a wallet (ex. pincode).
            /// </summary>
            /// <remarks>
            /// <para>
            /// For more information please see <see href="https://docs-staging.venly.io/pages/whitelabel.html#_update_wallet_arkane_api">https://docs-staging.venly.io/pages/whitelabel.html#_update_wallet_arkane_api</see>
            /// </para>
            /// </remarks>
            /// <param name="walletAddress"></param>
            /// <param name="currentPincode"></param>
            /// <param name="newPincode"></param>
            /// <param name="callback"></param>
            /// <returns>The Unity routine enumerator</returns>
            public static IEnumerator UpdatePincode(string walletAddress, string currentPincode, string newPincode, Action<ListWalletResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new ListWalletResult() { hasError = true, message = "Attempted to call BGSDK.Wallets.CreateWhitelabelWallet with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {

                    if (BGSDKSettings.user == null)
                    {
                        callback(new ListWalletResult() { hasError = true, message = "BGSDKSettings.user required, null Settings.user provided.", result = null });
                        yield return null;
                    }
                    else
                    {
                        WWWForm form = new WWWForm();
                        form.AddField("pincode", currentPincode);
                        form.AddField("newPincode", newPincode);

                        UnityWebRequest www = UnityWebRequest.Post(BGSDKSettings.current.WalletUri + "/" + walletAddress + "/security", form);
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            var results = new ListWalletResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                results.result = new System.Collections.Generic.List<Wallet>();
                                results.result.Add(JsonUtility.FromJson<Wallet>(Utilities.JSONArrayWrapper(resultContent)));
                                results.message = "Update wallet complete.";
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);
                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }
                        }
                        else
                        {
                            callback(new ListWalletResult() { hasError = true, message = "Error:" + (www.isNetworkError ? " a network error occured while attempting creat a whitelable wallet." : " a HTTP error occured while attempting to creat a whitelable wallet."), result = null, httpCode = www.responseCode });
                        }
                    }
                }
            }

            /// <summary>
            /// Returns the "native" balance for a wallet. This is the balance of the native token used by the chain. Ex. ETH for Ethereum.
            /// </summary>
            /// <remarks>
            /// For more information see <see href="https://docs.venly.io/pages/reference.html#_native_balance_arkane_api">https://docs.venly.io/pages/reference.html#_native_balance_arkane_api</see>
            /// </remarks>
            /// <param name="walletAddress"></param>
            /// <param name="callback"></param>
            /// <returns>The Unity routine enumerator</returns>
            public static IEnumerator Balance(string walletAddress, Action<BalanceResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new BalanceResult() { hasError = true, message = "Attempted to call BGSDK.Wallets.UserWallet.NativeBalance with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        callback(new BalanceResult() { hasError = true, message = "BGSDKSettings.user required, null Settings.user provided.", result = null });
                        yield return null;
                    }
                    else
                    {
                        UnityWebRequest www = UnityWebRequest.Get(BGSDKSettings.current.WalletUri + "/" + walletAddress + "/balance");
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            var results = new BalanceResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                results = JsonUtility.FromJson<BalanceResult>(resultContent);
                                results.message = "Wallet balance updated.";
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);
                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }
                        }
                        else
                        {
                            callback(new BalanceResult() { hasError = true, message = "Error:" + (www.isNetworkError ? " a network error occured while requesting a user's wallet." : " a HTTP error occured while requesting a user's wallet."), result = null, httpCode = www.responseCode });
                        }
                    }
                }
            }

            /// <summary>
            /// Returns the balance of all tokens currently supported by BGSDK.
            /// </summary>
            /// <remarks>
            /// <para>
            /// For more details see <see href="https://docs.venly.io/pages/reference.html#_token_balances_arkane_api">https://docs.venly.io/pages/reference.html#_token_balances_arkane_api</see>
            /// </para>
            /// </remarks>
            /// <param name="walletAddress"></param>
            /// <param name="callback"></param>
            /// <returns>The Unity routine enumerator</returns>
            public static IEnumerator TokenBalance(string walletAddress, Action<TokenBalanceResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new TokenBalanceResult() { hasError = true, message = "Attempted to call BGSDK.Wallets.UserWallet.TokenBalance with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        callback(new TokenBalanceResult() { hasError = true, message = "BGSDKSettings.user required, null Settings.user provided.", result = null });
                        yield return null;
                    }
                    else
                    {
                        UnityWebRequest www = UnityWebRequest.Get(BGSDKSettings.current.WalletUri + "/" + walletAddress + "/balance/tokens");
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            var results = new TokenBalanceResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                results = JsonUtility.FromJson<TokenBalanceResult>(Utilities.JSONArrayWrapper(resultContent));
                                results.message = "Fetch token balance complete.";
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);

                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }
                        }
                        else
                        {
                            callback(new TokenBalanceResult() { hasError = true, message = "Error:" + (www.isNetworkError ? " a network error occured while requesting the token balance from a wallet." : " a HTTP error occured while requesting the token balance from a wallet."), result = null, httpCode = www.responseCode });
                        }
                    }
                }
            }

            /// <summary>
            /// Returns the token balance for a specified token (this can be any token).
            /// </summary>
            /// <remarks>
            /// <para>
            /// For more details see <see href="https://docs.venly.io/pages/reference.html#_specific_token_balance_arkane_api">https://docs.venly.io/pages/reference.html#_specific_token_balance_arkane_api</see>
            /// </para>
            /// </remarks>
            /// <param name="walletAddress"></param>
            /// <param name="tokenAddress"></param>
            /// <param name="callback"></param>
            /// <returns>The Unity routine enumerator</returns>
            public static IEnumerator SpecificTokenBalance(string walletAddress, string tokenAddress, Action<TokenBalanceResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new TokenBalanceResult() { hasError = true, message = "Attempted to call BGSDK.Wallets.UserWallet.TokenBalance with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        callback(new TokenBalanceResult() { hasError = true, message = "BGSDKSettings.user required, null Settings.user provided.", result = null });
                        yield return null;
                    }
                    else
                    {
                        UnityWebRequest www = UnityWebRequest.Get(BGSDKSettings.current.WalletUri + "/" + walletAddress + "/balance/tokens/" + tokenAddress);
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            var results = new TokenBalanceResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                results.result = JsonUtility.FromJson<TokenBalance>(resultContent);
                                results.message = "Fetch token balance complete.";
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);

                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }
                        }
                        else
                        {
                            callback(new TokenBalanceResult() { hasError = true, message = "Error:" + (www.isNetworkError ? " a network error occured while requesting the token balance from a wallet." : " a HTTP error occured while requesting the token balance from a wallet."), result = null, httpCode = www.responseCode });
                        }
                    }
                }
            }

            /// <summary>
            /// NFTs can be queried either by wallet ID or by wallet address, if required multiple NFT contract addresses can be passed as a query parameter to act as a filter. 
            /// </summary>
            /// <remarks>
            /// For more information please see <see href="https://docs.venly.io/api/api-products/wallet-api/retrieve-non-fungible-tokens"/>
            /// </remarks>
            /// <param name="walletAddress"></param>
            /// <param name="optionalContractAddresses">List of contract addresses to filter for, if empty or null all will be returned. Can be null</param>
            /// <param name="callback"></param>
            /// <returns>The Unity routine enumerator</returns>
            public static IEnumerator NFTs(string walletAddress, SecretType chain, List<string> optionalContractAddresses, Action<NFTBalanceResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new NFTBalanceResult()
                    {
                        hasError = true,
                        message = "Attempted to call BGSDK.Wallets.UserWallet.ListNFTs with no BGSDK.Settings object applied."
                    });
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        callback(new NFTBalanceResult()
                        {
                            hasError = true,
                            message = "BGSDKSettings.user required, null Settings.user provided."
                        });
                        yield return null;
                    }
                    else
                    {
                        string address = BGSDKSettings.current.WalletUri + "/" + chain.ToString() + "/" + walletAddress + "/nonfungibles";

                        if (optionalContractAddresses != null && optionalContractAddresses.Count > 0)
                        {
                            address += "?";
                            for (int i = 0; i < optionalContractAddresses.Count; i++)
                            {
                                if (i == 0)
                                    address += "contract-addresses=" + optionalContractAddresses[i];
                                else
                                    address += "&contract-addresses=" + optionalContractAddresses[i];
                            }
                        }

                        UnityWebRequest www = UnityWebRequest.Get(address);
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            var results = new NFTBalanceResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                results = JsonUtility.FromJson<NFTBalanceResult>(resultContent);
                                results.message = "List NFTs complete.";
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);

                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }
                        }
                        else
                        {
                            callback(new NFTBalanceResult()
                            {
                                hasError = true,
                                message = "Error:" + (www.isNetworkError ? " a network error occured while requesting NFTs." : " a HTTP error occured while requesting NFTs."),
                                httpCode = www.responseCode
                            });
                        }
                    }
                }
            }
        }




        public static class Market
        {
            public static IEnumerator CreateOffer(string id_nft, string contract_address, string walletAddress, Action<OfferResult> callback)
            {

                if (BGSDKSettings.current == null)
                {
                    //callback(new DefineTokenTypeResult() { hasError = true, message = "Attempted to call BGSDK.Tokens.CreateTokenType with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        //callback(new DefineTokenTypeResult() { hasError = true, message = "BGSDKSettings.user required, null Settings.user provided.\n Please initalize the Settings.user before calling CreateTokenType" });
                        yield return null;
                    }
                    else
                    {

                        NFT nft = new NFT()
                        {
                            address = contract_address,
                            tokenId = id_nft,
                            chain = "MATIC"
                        };

                        OfferBody body = new OfferBody()
                        {
                            nft = nft,
                            price = UnityEngine.Random.Range(0.001f, 1.0f).ToString("f4") + "",
                            sellerAddress = walletAddress
                        };

                        //now use this def to body
                        var jsonString = JsonUtility.ToJson(body);

                        UnityWebRequest www = UnityWebRequest.Put(BGSDKSettings.current.offerUrl, jsonString);
                        www.method = UnityWebRequest.kHttpVerbPOST;
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);
                        www.uploadHandler.contentType = "application/json;charset=UTF-8";
                        www.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            var results = new OfferResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                results = JsonUtility.FromJson<OfferResult>(resultContent);
                                results.message = "Create Offer complete.";
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);
                                //Info = resultContent;
                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }

                        }

                    }
                }
            }

            public static IEnumerator SignOffer(string walletId, string data, string pincode, Action<OfferSignatureResult> callback)
            {

                if (BGSDKSettings.current == null)
                {
                    Debug.Log("Attempted to call BGSDK.Wallets.List with no BGSDK.Settings object applied.");
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        Debug.Log("BGSDKSettings.user required, null Settings.user provided.");
                        yield return null;
                    }
                    else
                    {
                        OfferSignature signature = new OfferSignature
                        {
                            pincode = pincode,
                            signatureRequest = new SignatureRequest
                            {
                                data = data,
                                walletId = walletId
                            }
                        };

                        var jsonString = JsonUtility.ToJson(signature);

                        UnityWebRequest www = UnityWebRequest.Put(BGSDKSettings.current.api[BGSDKSettings.current.useStaging] + "/api/signatures", jsonString);
                        www.method = UnityWebRequest.kHttpVerbPOST;
                        www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);
                        www.uploadHandler.contentType = "application/json;charset=UTF-8";
                        www.SetRequestHeader("Content-Type", "application/json;charset=UTF-8");

                        var co = www.SendWebRequest();
                        while (!co.isDone)
                            yield return null;

                        if (!www.isNetworkError && !www.isHttpError)
                        {
                            var results = new OfferSignatureResult();
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                results = JsonUtility.FromJson<OfferSignatureResult>(resultContent);
                                results.message = "Sign Offer complete.";
                                results.httpCode = www.responseCode;
                                Debug.Log(resultContent);
                                //Info = resultContent;
                            }
                            catch (Exception ex)
                            {
                                results = null;
                                results.message = "An error occured while processing JSON results, see exception for more details.";
                                results.exception = ex;
                                results.httpCode = www.responseCode;
                            }
                            finally
                            {
                                callback(results);
                            }

                        }

                    }
                }

            }


           public  static IEnumerator Sign(string signature, string offerid)
            {
                //now use this def to body
                var jsonString = "{\"signature\":\"" + signature + "\" }";

                var www = new UnityWebRequest(BGSDKSettings.current.offerUrl + "/" + offerid + "/signature", "PATCH");
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);
                www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                www.uploadHandler.contentType = "application/json";
                www.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);
                www.SetRequestHeader("Content-Type", "application/json");
              



                var co = www.SendWebRequest();
                while (!co.isDone)
                    yield return null;

                if (!www.isNetworkError && !www.isHttpError)
                {
                    var results = new OfferResult();
                    try
                    {
                        string resultContent = www.downloadHandler.text;
                        results = JsonUtility.FromJson<OfferResult>(resultContent);
                        results.message = "Initiating Offer complete.";
                        results.httpCode = www.responseCode;
                        Debug.Log(resultContent);
                        //Info = resultContent;
                    }
                    catch (Exception ex)
                    {
                        results = null;
                        results.message = "An error occured while processing JSON results, see exception for more details.";
                        results.exception = ex;
                        results.httpCode = www.responseCode;
                    }


                }
            }


        }





        #region privileged

        public class Privileged
        {

            private class MintNonFungibleRequest
            {
                public string typeId;
                public string[] destinations;
            }

            private class MintFungibleRequest
            {
                public int[] amounts;
                public string[] destinations;
            }

            public class MintResult : DataModel.BGSDKBaseResult
            {
                public List<ulong> tokenIds;
            }

            /// <summary>
            /// Create a NFT for each destination provided
            /// </summary>
            /// <remarks>
            /// See <see href="https://docs.venly.io/api/api-products/nft-api/mint-nft"/> for details
            /// </remarks>
            /// <param name="token"></param>
            /// <param name="destinations">The wallets to add the token to</param>
            /// <param name="callback"></param>
            /// <returns></returns>
            public static IEnumerator MintNonFungibleToken(Token token, string[] destinations, Action<MintResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new MintResult() { hasError = true, message = "Attempted to call BGSDK.Tokens.MintNonFungibleToken with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        callback(new MintResult() { hasError = true, message = "BGSDKIdentity required, null identity provided.\nPlease initalize Settings.user before calling Privileged.MintNonFungibleToken" });
                        yield return null;
                    }
                    else
                    {
                        var data = new MintNonFungibleRequest()
                        {
                            typeId = token.Id.ToString(),
                            destinations = destinations
                        };

                        var request = new UnityWebRequest(BGSDKSettings.current.MintTokenUri(token.contract) + "/non-fungible", "POST");
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
                        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                        request.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);
                        request.SetRequestHeader("Content-Type", "application/json");
                        var async = request.SendWebRequest();

                        while (!async.isDone)
                            yield return null;

                        if (!request.isNetworkError && !request.isHttpError)
                        {
                            //Debug json deserialize - requires triming mint result json
                            string mint_result_json = request.downloadHandler.text.Trim(new char[] { '[', ']' });
                            MintResult mint_result_wrapper = JsonUtility.FromJson<MintResult>(mint_result_json);
                            mint_result_wrapper.httpCode = request.responseCode;
                            mint_result_wrapper.message = "Mint NFT complete";
                            callback(mint_result_wrapper);
                        }
                        else
                        {
                            callback(new MintResult() { hasError = true, message = "Error:" + (request.isNetworkError ? " a network error occured while attempting to mint a token." : " a HTTP error occured while attempting to mint a token."), httpCode = request.responseCode });
                        }
                    }
                }
            }

            /// <summary>
            /// Create a fungible token for each destination provided
            /// </summary>
            /// <remarks>
            /// See <see href="https://docs.venly.io/api/api-products/nft-api/mint-fungible-nft"/> for details
            /// </remarks>
            /// <param name="token"></param>
            /// <param name="amounts">The amount of tokens to add to each wallet</param>
            /// <param name="destinations">The wallets to add the tokens to</param>
            /// <param name="callback"></param>
            /// <returns></returns>
            public static IEnumerator MintFungibleToken(Token token, int[] amounts, string[] destinations, Action<BGSDKBaseResult> callback)
            {
                if (BGSDKSettings.current == null)
                {
                    callback(new DataModel.BGSDKBaseResult() { hasError = true, message = "Attempted to call BGSDK.Tokens.MintFungibleToken with no BGSDK.Settings object applied." });
                    yield return null;
                }
                else
                {
                    if (BGSDKSettings.user == null)
                    {
                        callback(new DataModel.BGSDKBaseResult() { hasError = true, message = "BGSDKIdentity required, null identity provided.\nPlease initalize Settings.user before calling Privileged.MintFungibleToken" });
                        yield return null;
                    }
                    else
                    {
                        var data = new MintFungibleRequest()
                        {
                            amounts = amounts,
                            destinations = destinations
                        };

                        var request = new UnityWebRequest(BGSDKSettings.current.MintTokenUri(token.contract) + "/fungible/" + token.TypeId.ToString(), "POST");
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
                        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                        request.SetRequestHeader("Authorization", BGSDKSettings.user.authentication.token_type + " " + BGSDKSettings.user.authentication.access_token);
                        request.SetRequestHeader("Content-Type", "application/json");
                        var async = request.SendWebRequest();

                        while (!async.isDone)
                            yield return null;

                        if (!request.isNetworkError && !request.isHttpError)
                        {
                            callback(new DataModel.BGSDKBaseResult() { hasError = false, message = "Successful request to mint token!", httpCode = request.responseCode });
                        }
                        else
                        {
                            callback(new DataModel.BGSDKBaseResult() { hasError = true, message = "Error:" + (request.isNetworkError ? " a network error occured while attempting to mint a token." : " a HTTP error occured while attempting to mint a token."), httpCode = request.responseCode });
                        }
                    }
                }
            }

            #endregion
        }


    }
}
#endif