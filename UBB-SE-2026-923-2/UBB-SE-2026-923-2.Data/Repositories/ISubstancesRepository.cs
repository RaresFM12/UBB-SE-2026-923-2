namespace UBB_SE_2026_923_2.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UBB_SE_2026_923_2.Models;

    public interface ISubstancesRepository
    {
        void AddSubstance(string name, float lethalDose, string description);

        void RemoveSubstanceByName(string name);

        Substance GetSubstanceByName(string name);

        List<Substance> GetAllSubstances();

        void UpdateSubstanceByName(Substance substance);

        bool SubstanceExists(string name);

        public Dictionary<string, int> GetTop30Substances();
    }
}
