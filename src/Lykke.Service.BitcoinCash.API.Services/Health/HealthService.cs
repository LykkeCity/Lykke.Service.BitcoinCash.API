﻿using System.Collections.Generic;
using Lykke.Service.BitcoinCash.API.Core.Domain.Health;
using Lykke.Service.BitcoinCash.API.Core.Services;

namespace Lykke.Service.BitcoinCash.API.Services.Health
{
    // NOTE: See https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring
    public class HealthService : IHealthService
    {
        public string GetHealthViolationMessage()
        {
            // TODO: Check gathered health statistics, and return appropriate health violation message, or NULL if service hasn't critical errors
            return null;
        }

        public IEnumerable<HealthIssue> GetHealthIssues()
        {
            var issues = new HealthIssuesCollection();

            // TODO: Check gathered health statistics, and add appropriate health issues message to issues

            return issues;
        }
    }
}
