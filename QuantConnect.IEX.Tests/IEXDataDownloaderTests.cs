﻿/*
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

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Plotly.NET.TraceObjects;
using QuantConnect.Data;
using QuantConnect.IEX.Downloader;

namespace QuantConnect.IEX.Tests
{
    public class IEXDataDownloaderTests
    {
        private IEXDataDownloader _downloader;

        [SetUp]
        public void SetUp()
        {
            _downloader = new IEXDataDownloader();
        }

        private static IEnumerable<TestCaseData> HistoricalDataTestCases => IEXDataQueueHandlerTests.TestParameters;

        [TestCaseSource(nameof(HistoricalDataTestCases))]
        public void DownloadsHistoricalData(Symbol symbol, Resolution resolution, TickType tickType, TimeSpan period, bool isEmptyResult)
        {
            var request = IEXDataQueueHandlerTests.CreateHistoryRequest(symbol, resolution, tickType, period);

            var parameters = new DataDownloaderGetParameters(symbol, resolution, request.StartTimeUtc, request.EndTimeUtc, tickType);

            var downloadResponse = _downloader.Get(parameters).ToList();

            if (!isEmptyResult)
            {
                Assert.IsEmpty(downloadResponse);
                return;
            }

            Assert.IsNotEmpty(downloadResponse);

            foreach (var baseData in downloadResponse)
            {
                IEXDataQueueHandlerTests.AssertTradeBar(symbol, resolution, baseData);
            }
        }
    }
}