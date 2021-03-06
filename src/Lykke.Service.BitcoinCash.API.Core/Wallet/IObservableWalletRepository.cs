﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.BitcoinCash.API.Core.Operation;

namespace Lykke.Service.BitcoinCash.API.Core.Wallet
{

    public interface IObservableWallet
    {
        string Address { get; }
    }

    public class ObservableWallet : IObservableWallet
    {
        public string Address { get; set; }

        public static ObservableWallet Create(string address)
        {
            return new ObservableWallet
            {
                Address = address
            };
        }
    }
    public interface IObservableWalletRepository
    {
        Task Insert(IObservableWallet wallet);
        Task<(IEnumerable<IObservableWallet> items, string ContinuationToken)> GetAll(int take, string continuationToken);
        Task<IEnumerable<IObservableWallet>> GetAll();
        Task Delete(string address);
        Task<IObservableWallet> Get(string address);
    }
}
