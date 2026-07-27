using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    // Installs a "trap on move" effect on a pathway: when the pathway activates, it traps
    // individuals at the source instead of moving them.
    public interface IPathwayTrappable
    {
        public bool TryTrapPathway(int amt);
    }
}
