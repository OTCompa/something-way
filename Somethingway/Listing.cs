using Dalamud.Game.Gui.PartyFinder.Types;
using System.Collections.Generic;
using System.Linq;

namespace Somethingway
{
    internal class Listing
    {
        public uint Id;
        
        public string Name;
        public string Description;
        public uint CurrentWorld;  // World that PF lead is currently on/created the PF on
        public uint HomeWorld;  // PF leads' home world
        
        public ushort Category;
        public ushort Duty;
        public ushort DutyType;
        public bool BeginnersWelcome;
        public ushort SecondsRemaining;
        public ushort MinimumItemLevel;
        public byte NumParties;  // Number of parties that pf is recruiting for (alliance raids = 3, DRS = 6, etc)
        public byte SlotsAvailable;  // Number of slots in PF listing available
        public List<uint> Slots;
        public ObjectiveFlags Objective;
        public ConditionFlags Conditions;
        public DutyFinderSettingsFlags DutyFinderSettings;
        public LootRuleFlags LootRules;
        public SearchAreaFlags SearchArea;
        public List<int> JobsPresent;

        public Listing(IPartyFinderListing pfListing)
        {
            Id = pfListing.Id;
            Name = pfListing.Name.ToString();
            Description = pfListing.Description.ToString();
            CurrentWorld = pfListing.CurrentWorld.Value.RowId;
            HomeWorld = pfListing.HomeWorld.Value.RowId;
            Category = (ushort)pfListing.Category;
            Duty = pfListing.RawDuty;
            DutyType = (ushort)pfListing.DutyType;
            BeginnersWelcome = pfListing.BeginnersWelcome;
            SecondsRemaining = pfListing.SecondsRemaining;
            MinimumItemLevel = pfListing.MinimumItemLevel;
            NumParties = pfListing.Parties;
            SlotsAvailable = pfListing.SlotsAvailable;
            Slots = condenseSlots(pfListing.Slots);
            Objective = pfListing.Objective;
            Conditions = pfListing.Conditions;
            DutyFinderSettings = pfListing.DutyFinderSettings;
            LootRules = pfListing.LootRules;
            SearchArea = pfListing.SearchArea;
            JobsPresent = pfListing.RawJobsPresent.Select(x => (int)x).ToList();
        }

        // condenses an IReadOnlyCollection of job flags into a single uint
        private List<uint> condenseSlots(IReadOnlyCollection<PartyFinderSlot> slots)
        {
            List<uint> acceptedJobs = new List<uint>();
            foreach (var slot in slots)
            {
                uint jobs = 0;
                foreach (var job in slot.Accepting)
                {
                    jobs |= (uint)job;
                }
                acceptedJobs.Add(jobs);
            }
            return acceptedJobs;
        }
    }
}
