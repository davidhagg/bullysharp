using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BullySharp.Core.Logging;
using NetMQ;

namespace BullySharp.Core
{
    public class LeaderElection
    {
        public bool IsLeaderProcess;

        private readonly IList<Peer> peers;
        private readonly Peer localPeer;
        private readonly object lockObject;
        private readonly ILogger logger;
        private readonly NetMQContext context;

        public LeaderElection(IList<Peer> peers, object lockObject, NetMQContext context, ILogger logger)
        {
            this.peers = peers;
            this.lockObject = lockObject;
            this.context = context;
            this.logger = logger;

            localPeer = this.peers.First(c => c.IsLocal);
        }

        public void Run(CancellationToken cancellationToken)
        {
            InitLeaderElection();

            while (!cancellationToken.IsCancellationRequested)
            {
                SendPing(peers);
                Thread.Sleep(Settings.PingInterval);
            }
        }

        private void InitLeaderElection()
        {
            try
            {
                logger.Log($"ELECT NEW LEADER INIT ({localPeer.Id})");

                lock (lockObject)
                {
                    // Todo: check if there is a master already
                    ElectNewMaster(localPeer, peers);

                    if (IsLeaderProcess)
                    {
                        logger.Log($"I AM NEW LEADER: {localPeer.Id}");
                        BroadcastVictory(peers);
                    }
                }

                logger.Log($"ELECT NEW LEADER END ({localPeer.Id})");
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message);
            }
        }

        public void ElectNewMaster(Peer thisPeer, IList<Peer> allPeers)
        {
            if (!thisPeer.IsLocal)
            {
                throw new Exception("ElectNewMaster: thisPeer is not local. Must initiate election message from my list of higher peers.");
            }

            // If there are no higher peers, I am the winner
            if (!thisPeer.HigherPeers.Any())
            {
                IsLeaderProcess = true;
                return;
            }

            // If any higher thisPeer responds to my election message with Ok
            // I stop bothering them since they outrank me
            if (thisPeer.HigherPeers.OrderByDescending(i => i).Any(id => SendElection(allPeers[id])))
            {
                IsLeaderProcess = false;
                return;
            }

            IsLeaderProcess = true;
        }

        private bool SendElection(Peer peer)
        {
            if (peer.IsLocal)
            {
                throw new Exception("SendElection: peer is local. Cannot send election message to my self.");
            }

            string reply;

            logger.Log($"Sending election message to {peer.ElectionSocket}");

            bool didAnswer;

            using (var client = context.CreateRequestSocket())
            {
                client.Connect(peer.ElectionSocket);

                // Send election message to higher peer
                client.TrySendFrame(TimeSpan.FromMilliseconds(1000), Message.Election);

                // Wait for reply OK/NOK or timeout
                didAnswer = client.TryReceiveFrameString(TimeSpan.FromMilliseconds(1000), out reply);

                client.Close();
            }

            if (didAnswer)
            {
                return reply == Message.Ok;
            }

            return false;
        }

        private void BroadcastVictory(IList<Peer> allPeers)
        {
            foreach (var peer in allPeers)
            {
                logger.Log($"SENDING VICTORY MSG TO: {peer.Id}");

                using (var client = context.CreateRequestSocket())
                {
                    client.Connect(peer.LeaderSocket);
                    var couldSend = client.TrySendFrame(Settings.VictoryMessageTimeout, localPeer.Id.ToString());
                    logger.Log($"COULD SEND: {couldSend}");
                    client.Close();
                }
            }
        }

        private void SendPing(IList<Peer> allPeers)
        {
            var nodesDown = new List<Peer>();
            foreach (var peer in allPeers.Where(x => !x.IsLocal))
            {
                logger.Log($"PINGING: {peer.PingSocket}");

                using (var client = context.CreateRequestSocket())
                {
                    client.Connect(peer.PingSocket);
                    client.TrySendFrame(Settings.PingTimeout, localPeer.Id.ToString());

                    string pingReply;
                    var replied = client.TryReceiveFrameString(Settings.PingTimeout, out pingReply);
                    client.Close();

                    logger.Log($"PING RESPONSE FROM: {peer.PingSocket} {replied} {pingReply}");

                    if (!replied || pingReply != Message.Ok)
                    {
                        nodesDown.Add(peer);
                    }
                }
            }

            if (nodesDown.Any())
            {
                InitLeaderElection();
            }
        }
    }
}