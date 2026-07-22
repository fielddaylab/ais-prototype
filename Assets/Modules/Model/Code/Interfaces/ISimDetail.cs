using AIS.Narrative;
using BeauUtil;

namespace AIS.Intervene
{
    /// <summary>
    /// How much of a sim detail the player is allowed to see.
    /// A detail is locked until the evidence card that unlocks it is in the player's inventory;
    /// what "locked" looks like depends on the phase, hence two distinct locked states.
    /// </summary>
    public enum SimDetailDisplay
    {
        Hidden,   // not on the model at all -- the narrative phase's locked state
        Obscured, // on the model, but its data is replaced by a placeholder -- the intervene phase's locked state
        Revealed  // fully visible
    }

    public interface ISimDetail
    {
        StringHash32 Id();

        /// <summary>
        /// Applies a visibility state. When obscuring, lockSuit is the suit of the evidence card
        /// that would unlock this detail, so the placeholder can hint at what the player is missing.
        /// It is PlayerStatId.Invalid when there is no suit to show.
        /// </summary>
        void SetDisplay(SimDetailDisplay display, PlayerStatId lockSuit);
    }
}
