using System.Collections.Concurrent;

namespace URLShortenerAPI.Utility.SignalR
{
    internal class UserConnectionMapping
    {
        private readonly ConcurrentDictionary<string, HashSet<string>> _connections = new();

        public void AddConnection(string key, string connectionId)
        {
            _connections.AddOrUpdate(
                key,
                _ => new HashSet<string> { connectionId }, // If user doesn't exist, create a new HashSet
                (_, existingConnections) =>
                {
                    existingConnections.Add(connectionId);
                    return existingConnections;
                });
        }

        public void RemoveConnection(string key, string connectionId)
        {
            if (_connections.TryGetValue(key, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _connections.TryRemove(key, out _);
                }
            }
        }

        public List<string> GetConnections(string key)
        {
            if (_connections.TryGetValue(key, out var connections))
            {
                return connections.ToList(); // Return a copy to avoid modifying the original set
            }

            return new List<string>();
        }
    }

}
