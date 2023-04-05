using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Model
{
    public class Party
    {
        public Mnemonic Mnemonic;
        public string Name;
        public ExtPubKey AccountExtPubKey;
        public RootedKeyPath AccountKeyPath;
        public ExtKey RootExtKey;
        public Party(Mnemonic mnemonic, string password, KeyPath keyPath)
        {
            Mnemonic = mnemonic;
            Name = password;
            RootExtKey = mnemonic.DeriveExtKey(password);
            AccountExtPubKey = RootExtKey.Derive(keyPath).Neuter();
            AccountKeyPath = new RootedKeyPath(RootExtKey.GetPublicKey().GetHDFingerPrint(), keyPath);
        }
    }


    public class SaveDetails
    {
        public string ExtKey { get; set; }
        public string ExtPubKey { get; set; }
        public string KeyPath { get; set; }
        public string UserId { get; set; }
    }
}
