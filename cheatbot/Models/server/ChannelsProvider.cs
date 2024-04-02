using Avalonia.Controls;
using cheatbot.Database.models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Models.server
{
    public class ChannelsProvider : IChannelsProvider
    {

        #region vars
        string url;
        string token;
        ServiceCollection serviceCollection;
        IHttpClientFactory httpClientFactory;
        #endregion

        public ChannelsProvider(string url, string token) {
            this.url = url;
            this.token = token; 
            serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();
            httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        }

        #region private
        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddHttpClient();
        }
        #endregion

        public async Task<List<ChannelModel>> GetChannels()
        {
            List<ChannelModel> res = new();

            var addr = $"{url}/v1/telegram/geolocations?type_id=1&status_code=active&sort_by=+code";
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await httpClient.GetAsync(addr);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<channelsResponseDto>(result);

                if (resp.success)
                {
                    foreach (var item in resp.data)
                    {
                        res.Add(new ChannelModel()
                        {
                            geotag = item.code,
                            tg_id = long.Parse(item.entity_details.entity_source_id),
                            link = item.entity_details.url_no_moderation
                        });
                    }
                }
                else
                    throw new Exception($"sucess=false");
            }
            catch (Exception ex)
            {
                throw new Exception($"GetFollowerState {ex.Message}");
            }

            return res;                
        }
    }

    public class channelsResponseDto { 
        public bool success { get; set; }
        public int? total_pages { get; set; }
        public int? total_items { get; set; }
        public List<channelDataDto> data { get; set; } = new();
    }

    public class channelDataDto
    {
        public int? id { get; set; } 
        public string? code { get; set; }    
        public int? office_id { get; set; }
        public int? type_id { get; set; }
        public entityDetailsDto? entity_details { get; set; }

    }

    public class entityDetailsDto
    {
        public int id { get; set; }
        public string? url_no_moderation { get; set; }
        public string? url_with_moderation { get; set; }        
        public string? entity_source_id { get; set; }
        public channelStatusDto? status { get; set; }
        public channelTypeDto? type { get; set; }
    }

    public class channelStatusDto
    {
        public int id { get; set; }    
        public string? status_code { get; set; }
    }

    public class channelTypeDto
    {
        public int id { get; set; } 
        public string? type_code { get; set; }
    }

}
