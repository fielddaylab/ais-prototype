using BeauUtil;
using System.Collections;

namespace FieldDay.Scripting {
    /// <summary>
    /// Optional interface for dialogue printers that support shifting their
    /// layout (e.g. the {layout Left/Right/Center} tag).
    /// </summary>
    public interface IDialogueLayoutPrinter {
        /// <summary>
        /// Shifts the printer's layout to the alignment described by the given tag argument.
        /// </summary>
        IEnumerator ShiftLayout(StringSlice alignmentArg);

        /// <summary>
        /// Immediately applies the layout described by the given tag argument, without animating.
        /// </summary>
        void SnapLayout(StringSlice alignmentArg);
    }
}
