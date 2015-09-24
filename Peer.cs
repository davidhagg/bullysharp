using System.Collections.Generic;

namespace BullySharp.Core
{
    public class Peer
    {
        public Peer(int id, string electionSocket, string leaderSocket, string pingSocket, bool isLocal, List<int> higherPeers)
        {
            Id = id;
            ElectionSocket = electionSocket;
            LeaderSocket = leaderSocket;
            PingSocket = pingSocket;
            IsLocal = isLocal;
            HigherPeers = higherPeers;
        }

        public int Id { get; set; }

        public string ElectionSocket { get; set; }

        public string LeaderSocket { get; set; }

        public string PingSocket { get; set; }

        public bool IsLocal { get; set; }

        public List<int> HigherPeers { get; set; }
    }
}