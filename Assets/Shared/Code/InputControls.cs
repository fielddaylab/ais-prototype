using FieldDay;
using FieldDay.HID;
using UnityEngine;

namespace AIS {
    static public class InputControls {
        static public bool CheckAdvanceInput() {
            if (Game.Input.IsKeyPressed(KeyCode.Return) || Game.Input.IsKeyPressed(KeyCode.Space)) {
                return true;
            }
            if (CursorUtility.IsCursorWithinGameWindow() && !Game.Input.IsPointerOverCanvas()) {
                return Game.Input.IsMousePressed(0);
            }
            return false;
        }
    }
}