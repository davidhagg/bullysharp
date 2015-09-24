using System;
using System.Collections.Generic;
using System.Linq;

namespace BullySharp.Core
{
    public static class PeerFactory
    {
        private const char ClusterMemberDelimiter = ',';

        public static List<Peer> CreatePeers()
        {
            var electionSockets = Settings.ElectionEndpoints.Split(ClusterMemberDelimiter).ToList();
            var leaderSockets = Settings.LeaderEndpoints.Split(ClusterMemberDelimiter).ToList();
            var pingSockets = Settings.PingEndpoints.Split(ClusterMemberDelimiter).ToList();

            var thisCandidateId = int.Parse(Settings.PeerId);
            var candidates = new List<Peer>();

            var index = 0;
            foreach (var electionUri in electionSockets)
            {
                candidates.Add(new Peer(index,
                                             electionUri,
                                             leaderSockets[index],
                                             pingSockets[index],
                                             index == thisCandidateId,
                                             GetMoreAuthoritativeCandidateIds(index, electionSockets)
                                             .Where(i => i >= 0).ToList()));
                index++;
            }

            return candidates;
        }

        private static IEnumerable<int> GetMoreAuthoritativeCandidateIds(int index, List<string> candidateUris)
        {
            return candidateUris.Select(n => {
                var i = Array.IndexOf<string>(candidateUris.ToArray(), n);
                if (i > index)
                {
                    return i;
                }

                return -1;
            });
        }
    }
}