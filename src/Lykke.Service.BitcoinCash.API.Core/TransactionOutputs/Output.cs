using NBitcoin;

namespace Lykke.Service.BitcoinCash.API.Core.TransactionOutputs
{

    public class Output : IOutput
    {
        public Output(OutPoint outpoint)
        {
            TransactionHash = outpoint.Hash.ToString();
            N = (int)outpoint.N;
        }

        public string TransactionHash { get; }
        public int N { get; }
    }
}
