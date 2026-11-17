using UnityEngine;

namespace FoglightIsles
{
    /// <summary>World-side presentation and collection of one optional journal entry.</summary>
    public sealed class DiscoveryPickup : MonoBehaviour
    {
        private DiscoveryJournal _journal;
        private string _id;
        private string _title;

        public void Configure(DiscoveryJournal journal, string id, string title)
        {
            _journal = journal;
            _id = id;
            _title = title;
            if (_journal.IsDiscovered(_id)) gameObject.SetActive(false);
        }

        public bool Collect()
        {
            if (_journal == null || !_journal.Discover(_id, _title)) return false;
            gameObject.SetActive(false);
            return true;
        }
    }
}
