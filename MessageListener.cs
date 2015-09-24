using System;
using System.Threading;
using BullySharp.Core.Logging;
using NetMQ;

namespace BullySharp.Core
{
    public class MessageListener
    {
        private readonly ILogger logger;
        private readonly NetMQContext context;

        public event EventHandler<int> LeaderChanged; 

        public MessageListener(ILogger logger, NetMQContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        public void Listen(CancellationToken cancellationToken)
        {
            using (var electionServer = context.CreateResponseSocket())
            using (var leaderServer = context.CreateResponseSocket())
            using (var pingServer = context.CreateResponseSocket())
            using (var poller = new Poller())
            {
                electionServer.Bind(Settings.ElectionListenerEndpoint);
                leaderServer.Bind(Settings.LeaderListenerEndpoint);
                pingServer.Bind(Settings.PingListenerEndpoint);

                logger.Log($"ElectionListenerEndpoint: {Settings.ElectionListenerEndpoint}");
                logger.Log($"LeaderListenerEndpoint: {Settings.LeaderListenerEndpoint}");
                logger.Log($"PingListenerEndpoint: {Settings.PingListenerEndpoint}");

                poller.AddSocket(electionServer);
                poller.AddSocket(leaderServer);
                poller.AddSocket(pingServer);

                // Listen for new messages on the electionServer socket
                electionServer.ReceiveReady += (s, a) =>
                {
                    var msg = a.Socket.ReceiveFrameString();

                    logger.Log($"ELECTION MESSAGE: {msg}");
                    if (msg == Message.Election)
                    {
                        a.Socket.SendFrame(msg == Message.Election
                            ? Message.Ok
                            : Message.Fail);
                    }
                };

                // Listen for new messages on the leaderServer socket
                leaderServer.ReceiveReady += (s, a) =>
                {
                    var winnerMessage = a.Socket.ReceiveFrameString();
                    OnLeaderChanged(winnerMessage);
                    logger.Log($"NEW LEADER MESSAGE RECEIVED: {winnerMessage}");
                };

                // Listen for pings
                pingServer.ReceiveReady += (s, a) =>
                {
                    a.Socket.ReceiveFrameString();
                    a.Socket.SendFrame(Message.Ok);
                };

                logger.Log("-----------------------------------");

                poller.PollTillCancelled();
            }
        }

        protected virtual void OnLeaderChanged(string leaderId)
        {
            var newLeader = int.Parse(leaderId);
            LeaderChanged?.Invoke(this, newLeader);
        }
    }
}