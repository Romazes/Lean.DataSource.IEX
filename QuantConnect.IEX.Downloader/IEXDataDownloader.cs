/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using QuantConnect.Data;
using QuantConnect.Securities;
using QuantConnect.Data.Market;

namespace QuantConnect.IEX.Downloader
{
    public class IEXDataDownloader : IDataDownloader, IDisposable
    {
        private readonly IEXDataQueueHandler _handler;

        public IEXDataDownloader()
        {
            _handler = new IEXDataQueueHandler();
        }

        public void Dispose()
        {
            _handler.Dispose();
        }

        /// <summary>
        /// Get historical data enumerable for a single symbol, type and resolution given this start and end time (in UTC).
        /// </summary>
        /// <param name="dataDownloaderGetParameters">model class for passing in parameters for historical data</param>
        /// <returns>Enumerable of base data for this symbol</returns>
        public IEnumerable<BaseData> Get(DataDownloaderGetParameters dataDownloaderGetParameters)
        {
            var symbol = dataDownloaderGetParameters.Symbol;
            var resolution = dataDownloaderGetParameters.Resolution;
            var startUtc = dataDownloaderGetParameters.StartUtc;
            var endUtc = dataDownloaderGetParameters.EndUtc;
            var tickType = dataDownloaderGetParameters.TickType;

            if (tickType != TickType.Trade)
            {
                yield break;
            }

            if (!(resolution == Resolution.Daily || resolution == Resolution.Minute))
                throw new NotSupportedException("Resolution not available: " + resolution);

            if (endUtc < startUtc)
                throw new ArgumentException("The end date must be greater or equal than the start date.");

            var historyRequests = new[] {
                new HistoryRequest(startUtc,
                                   endUtc,
                                   typeof(TradeBar),
                                   symbol,
                                   resolution,
                                   SecurityExchangeHours.AlwaysOpen(TimeZones.NewYork),
                                   TimeZones.NewYork,
                                   resolution,
                                   true,
                                   false,
                                   DataNormalizationMode.Raw,
                                   TickType.Trade)
            };

            foreach (var slice in _handler.GetHistory(historyRequests, TimeZones.EasternStandard))
            {
                yield return slice[symbol];
            }
        }
    }
}