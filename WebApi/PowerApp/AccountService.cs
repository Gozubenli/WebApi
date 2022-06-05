using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RentReadyWebApi.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RentReadyWebApi.Service
{
    public class AccountService
    {
        public async Task<List<Account>> GetAccountList(string text)
        {
            List<Account> list = new List<Account>();
            try
            {
                string connectionString = "Url=https://org2906d5f0.crm4.dynamics.com;Username=huseyin@gozubenli.onmicrosoft.com;Password=dynamics_1.;";

                using (HttpClient client = Helper.GetHttpClient(connectionString, Helper.clientId, Helper.redirectUrl))
                {
                    var response = await client.GetAsync("accounts?$select=accountid,accountnumber,name,statecode,statuscode,address1_stateorprovince,address2_stateorprovince&$top=100&$filter=contains(name,'" + text+ "') or contains(accountnumber,'" + text + "')");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonBody = response.Content.ReadAsStringAsync().Result;
                        var o = JsonConvert.DeserializeObject<JObject>(jsonBody);

                        JObject body = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        JArray jArray = JArray.Parse(body["value"].ToString());
                        foreach (var arr in jArray.Children())
                        {                            
                            list.Add(new Account()
                            {
                                accountid = arr["accountid"].ToString(),
                                accountnumber = arr["accountnumber"].ToString(),
                                name = arr["name"].ToString(),
                                stateOrProvince = arr["address1_stateorprovince"].ToString()?? arr["address2_stateorprovince"].ToString(),                                
                                statecode = Convert.ToInt32(arr["statecode"].ToString()),
                            });                            
                        }
                    }
                    else
                    {
                        Console.WriteLine("The request failed with a status of '{0}'", response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.DisplayException(ex);
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
            }
            return list;
        }
    }
}
